using System;
using System.Collections.Generic;
using UnityEngine;

namespace TrueSync
{
	public abstract class AbstractLockstep
	{
		private enum SimulationState
		{
			NOT_STARTED,
			WAITING_PLAYERS,
			RUNNING,
			PAUSED,
			ENDED
		}

		private const int INITIAL_PLAYERS_CAPACITY = 4;

		private const byte SYNCED_GAME_START_CODE = 196;

		private const byte SIMULATION_CODE = 197;

		private const byte CHECKSUM_CODE = 198;

		private const byte SEND_CODE = 199;

		private const byte SIMULATION_EVENT_PAUSE = 0;

		private const byte SIMULATION_EVENT_RUN = 1;

		private const byte SIMULATION_EVENT_END = 3;

		private const int MAX_PANIC_BEFORE_END_GAME = 5;

		private const int SYNCED_INFO_BUFFER_WINDOW = 3;
        /// <summary>
        /// 所有玩家 包括本地玩家 dict存储
        /// </summary>
		internal Dictionary<byte, TSPlayer> players;
        /// <summary>
        /// 所有玩家 包括本地玩家 list存储
        /// </summary>
		internal List<TSPlayer> activePlayers;

		internal List<SyncedData> auxPlayersSyncedData;

		internal List<InputDataBase> auxPlayersInputData;

        /// <summary>
        /// 除去本地玩家的数组
        /// </summary>
		internal int[] auxActivePlayersIds;
        /// <summary>
        /// 本地玩家
        /// </summary>
		internal TSPlayer localPlayer;

		protected TrueSyncUpdateCallback StepUpdate;

		private TrueSyncInputCallback GetLocalData;

		internal TrueSyncInputDataProvider InputDataProvider;

		private TrueSyncEventCallback OnGameStarted;

		private TrueSyncEventCallback OnGamePaused;

		private TrueSyncEventCallback OnGameUnPaused;

		private TrueSyncEventCallback OnGameEnded;

		private TrueSyncPlayerDisconnectionCallback OnPlayerDisconnection;

		public TrueSyncIsReady GameIsReady;

		protected int ticks;
        /// <summary>
        /// maximum amount of missed frames/ticks before a remote player is removed from simulation due to being unresponsive(not sending input values anymore)
        /// 就是超过panicWindow还没有收到远程玩家的input那么久将之移除（他太卡了）
        /// </summary>
        private int panicWindow;
        /// <summary>
        /// this is the size of the input queue;就是缓存输入数据的窗口大小
        /// Lets say a game client has a ping (round trip time) of 60ms to Photon Cloud servers, and we're using the default locked step time of 0.02 (20ms time per frame).

        //In a lockstep game, we need the input queue to compensate that lag from the remote players by adding an equally big latency to local input.

        //This means that the sync window size shall be at least 3, so we add 60ms(3 frames X 20ms per frame) to all input
        //就是本地的输入等待远程的输入，为了更好的同步
        /// </summary>
        protected int syncWindow;

		private int elapsedPanicTicks;

		private AbstractLockstep.SimulationState simulationState;

		internal int rollbackWindow;

		internal ICommunicator communicator;

		protected IPhysicsManagerBase physicsManager;

		private GenericBufferWindow<SyncedInfo> bufferSyncedInfo;

		protected int totalWindow;

		public bool checksumOk;

		public CompoundStats compoundStats;

		public float deltaTime;

		public int _lastSafeTick = 0;

		protected Dictionary<int, List<IBody>> bodiesToDestroy;

		protected Dictionary<int, List<byte>> playersDisconnect;

		private ReplayMode replayMode;

		private ReplayRecord replayRecord;

		internal static AbstractLockstep instance;

        /// <summary>
        /// 除去本地玩家的 list 一个中转容器
        /// </summary>
		private List<int> playersIdsAux = new List<int>();

		private SyncedData[] _syncedDataCacheDrop = new SyncedData[1];

		private SyncedData[] _syncedDataCacheUpdateData = new SyncedData[1];

		public List<TSPlayer> ActivePlayers
		{
			get
			{
				return this.activePlayers;
			}
		}

		public IDictionary<byte, TSPlayer> Players
		{
			get
			{
				return this.players;
			}
		}

		public TSPlayer LocalPlayer
		{
			get
			{
				return this.localPlayer;
			}
		}

		public int Ticks
		{
			get
			{
				return this.GetSimulatedTick(this.GetSyncedDataTick()) - 1;
			}
		}

		public int LastSafeTick
		{
			get
			{
				bool flag = this._lastSafeTick < 0;
				int result;
				if (flag)
				{
					result = -1;
				}
				else
				{
					result = this._lastSafeTick - 1;
				}
				return result;
			}
		}

		private ReplayMode ReplayMode
		{
			set
			{
				this.replayMode = value;
				bool flag = this.replayMode == ReplayMode.RECORD_REPLAY;
				if (flag)
				{
					this.replayRecord = new ReplayRecord();
				}
			}
		}

		public ReplayRecord ReplayRecord
		{
			set
			{
				this.replayRecord = value;
				bool flag = this.replayRecord != null;
				if (flag)
				{
					this.replayMode = ReplayMode.LOAD_REPLAY;
					this.replayRecord.ApplyRecord(this);
				}
			}
		}

		public static AbstractLockstep NewInstance(float deltaTime, ICommunicator communicator, IPhysicsManagerBase physicsManager, int syncWindow, int panicWindow, int rollbackWindow, TrueSyncEventCallback OnGameStarted, TrueSyncEventCallback OnGamePaused, TrueSyncEventCallback OnGameUnPaused, TrueSyncEventCallback OnGameEnded, TrueSyncPlayerDisconnectionCallback OnPlayerDisconnection, TrueSyncUpdateCallback OnStepUpdate, TrueSyncInputCallback GetLocalData, TrueSyncInputDataProvider InputDataProvider)
		{
			bool flag = rollbackWindow <= 0 || communicator == null;
			AbstractLockstep result;
			if (flag)
			{
				result = new DefaultLockstep(deltaTime, communicator, physicsManager, syncWindow, panicWindow, rollbackWindow, OnGameStarted, OnGamePaused, OnGameUnPaused, OnGameEnded, OnPlayerDisconnection, OnStepUpdate, GetLocalData, InputDataProvider);
			}
			else
			{
				result = new RollbackLockstep(deltaTime, communicator, physicsManager, syncWindow, panicWindow, rollbackWindow, OnGameStarted, OnGamePaused, OnGameUnPaused, OnGameEnded, OnPlayerDisconnection, OnStepUpdate, GetLocalData, InputDataProvider);
			}
			return result;
		}

		public AbstractLockstep(float deltaTime, ICommunicator communicator, IPhysicsManagerBase physicsManager, int syncWindow, int panicWindow, int rollbackWindow, TrueSyncEventCallback OnGameStarted, TrueSyncEventCallback OnGamePaused, TrueSyncEventCallback OnGameUnPaused, TrueSyncEventCallback OnGameEnded, TrueSyncPlayerDisconnectionCallback OnPlayerDisconnection, TrueSyncUpdateCallback OnStepUpdate, TrueSyncInputCallback GetLocalData, TrueSyncInputDataProvider InputDataProvider)
		{
			AbstractLockstep.instance = this;
			this.deltaTime = deltaTime;
			this.syncWindow = syncWindow;
			this.panicWindow = panicWindow;
			this.rollbackWindow = rollbackWindow;
			this.totalWindow = syncWindow + rollbackWindow;
			this.StepUpdate = OnStepUpdate;
			this.OnGameStarted = OnGameStarted;
			this.OnGamePaused = OnGamePaused;
			this.OnGameUnPaused = OnGameUnPaused;
			this.OnGameEnded = OnGameEnded;
			this.OnPlayerDisconnection = OnPlayerDisconnection;
			this.GetLocalData = GetLocalData;
			this.InputDataProvider = InputDataProvider;
			this.ticks = 0;
			this.players = new Dictionary<byte, TSPlayer>(4);
			this.activePlayers = new List<TSPlayer>(4);
			this.auxPlayersSyncedData = new List<SyncedData>(4);
			this.auxPlayersInputData = new List<InputDataBase>(4);
			this.communicator = communicator;
			bool flag = communicator != null;
			if (flag)
			{
				this.communicator.AddEventListener(new OnEventReceived(this.OnEventDataReceived));
			}
			this.physicsManager = physicsManager;
			this.compoundStats = new CompoundStats();
			this.bufferSyncedInfo = new GenericBufferWindow<SyncedInfo>(3);
			this.checksumOk = true;
			this.simulationState = AbstractLockstep.SimulationState.NOT_STARTED;
			this.bodiesToDestroy = new Dictionary<int, List<IBody>>();
			this.playersDisconnect = new Dictionary<int, List<byte>>();
			this.ReplayMode = ReplayRecord.replayMode;
		}
        /// <summary>
        /// this.ticks - this.syncWindow;syncWindow可以理解为缓存窗口,一直让本地预先缓存一下远端过来的数据，让数据更加平滑
        /// </summary>
        /// <returns></returns>
		protected int GetSyncedDataTick()
		{
			return this.ticks - this.syncWindow;
		}

		protected abstract int GetRefTick(int syncedDataTick);

		protected virtual void BeforeStepUpdate(int syncedDataTick, int referenceTick)
		{
		}

		protected virtual void AfterStepUpdate(int syncedDataTick, int referenceTick)
		{
			int i = 0;
			int count = this.activePlayers.Count;
			while (i < count)
			{
				this.activePlayers[i].RemoveData(referenceTick);
				i++;
			}
		}

		protected abstract bool IsStepReady(int syncedDataTick);

		protected abstract void OnSyncedDataReceived(TSPlayer player, List<SyncedData> data);

		protected abstract string GetChecksumForSyncedInfo();

		protected abstract int GetSimulatedTick(int syncedDataTick);

		private void Run()
		{
			if (this.simulationState == AbstractLockstep.SimulationState.NOT_STARTED)
			{
				this.simulationState = AbstractLockstep.SimulationState.WAITING_PLAYERS;
			}
			else
			{
				if (this.simulationState == AbstractLockstep.SimulationState.WAITING_PLAYERS || this.simulationState == AbstractLockstep.SimulationState.PAUSED)
				{
					if (this.simulationState == AbstractLockstep.SimulationState.WAITING_PLAYERS)
					{
						this.OnGameStarted();
					}
					else
					{
						this.OnGameUnPaused();
					}
					this.simulationState = AbstractLockstep.SimulationState.RUNNING;
				}
			}
		}

		private void Pause()
		{
			if (this.simulationState == AbstractLockstep.SimulationState.RUNNING)
			{
				this.OnGamePaused();
				this.simulationState = AbstractLockstep.SimulationState.PAUSED;
			}
		}

		private void End()
		{
			if (this.simulationState != AbstractLockstep.SimulationState.ENDED)
			{
				this.OnGameEnded();
                if (this.replayMode == ReplayMode.RECORD_REPLAY)
				{
					ReplayRecord.SaveRecord(this.replayRecord);
				}
				this.simulationState = AbstractLockstep.SimulationState.ENDED;
			}
		}
        /// <summary>
        /// TrueSyncBehaviour:FixedUpdate()调过来的
        /// </summary>
		public void Update()
		{
            //当本地玩家已经处于等待其他玩家的状态,那么久应该去检测其他玩家是否已经准备好了
            if (this.simulationState == AbstractLockstep.SimulationState.WAITING_PLAYERS)
			{
				this.CheckGameStart();
			}
			else
			{
				if (this.simulationState == AbstractLockstep.SimulationState.RUNNING)
				{
					this.compoundStats.UpdateTime(this.deltaTime);
					if (this.communicator != null)
					{
						this.compoundStats.AddValue("ping", (long)this.communicator.RoundTripTime());
					}
					if (this.syncWindow == 0)
					{
						this.UpdateData();
					}
					int i = 0;
					int num = this.activePlayers.Count;
                    //对每个玩家进行检测掉线
                    while (i < num)
					{
                        //是否掉线
                        if (this.CheckDrop(this.activePlayers[i]))
                        {
                            //如果是掉线,那么该玩家会从activePlayers列表中删除,为了能够正确的检测其他玩家,必须i--,num--
                            i--;
							num--;
						}
						i++;
					}
					int syncedDataTick = this.GetSyncedDataTick();//syncedDataTick指向的是缓存队列的第一个
                    if (this.CheckGameIsReady() && this.IsStepReady(syncedDataTick)) //相当于是询问所有玩家的controls是否有数据，this.activePlayers[i].IsDataReady(syncedDataTick));
                    {
						this.compoundStats.Increment("simulated_frames");//simulate_frames++;
                        this.UpdateData();//收集本地玩家的输入，并通过服务器发送给其他玩家
                        this.elapsedPanicTicks = 0;
                        //对于defaultLookStep refTick==syncedDataTick
						int refTick = this.GetRefTick(syncedDataTick);//对于defaultLookStep,直接返回syncedDataTick
                        //每100tick做一次刚体同步校验
                        if (refTick > 1 && refTick % 100 == 0)
						{
							this.SendInfoChecksum(refTick);
						}
						this._lastSafeTick = refTick;
						this.BeforeStepUpdate(syncedDataTick, refTick);//去除无用Body,和断线的玩家
						List<SyncedData> tickData = this.GetTickData(syncedDataTick);//获取所有玩家在该syncedDataTick下的输入数据
                        this.ExecutePhysicsStep(tickData, syncedDataTick);
						if (this.replayMode == ReplayMode.RECORD_REPLAY)
						{
							this.replayRecord.AddSyncedData(this.GetTickData(refTick));
                        }
						this.AfterStepUpdate(syncedDataTick, refTick);//从TSPlayer.controls清除该refTick的数据,用Pool回收
                        this.ticks++;
					}
					else
					{
                        //当ticks大于了某个界限还没收到远端的数据，那么我们定义为missed_frames
						if (this.ticks >= this.totalWindow)
						{
                            //LOAD_REPLAY模式直接结束游戏
							if (this.replayMode == ReplayMode.LOAD_REPLAY)
							{
								this.End();
							}
							else
							{
								this.compoundStats.Increment("missed_frames");
								this.elapsedPanicTicks++;
                                //意思是说missed_frames大于预设的panicWindow
								if (this.elapsedPanicTicks > this.panicWindow)
								{
									this.compoundStats.Increment("panic");
                                    //missed_frames大于预设的panicWindow的情况出现超过五次,那么直接结束游戏
                                    if (this.compoundStats.globalStats.GetInfo("panic").count >= 5L)
									{
										this.End();
									}
                                    //
									else
									{
										this.elapsedPanicTicks = 0;
										this.DropLagPlayers();
									}
								}
							}
						}
						else
						{
							this.compoundStats.Increment("simulated_frames");
							this.physicsManager.UpdateStep();
							this.UpdateData();
							this.ticks++;
						}
					}
				}
			}
		}

        /// <summary>
        /// 暂时没注册，必定返回true
        /// </summary>
        /// <returns></returns>
		private bool CheckGameIsReady()
		{
			bool result;
			if (this.GameIsReady != null)
			{
				Delegate[] invocationList = this.GameIsReady.GetInvocationList();
				for (int i = 0; i < invocationList.Length; i++)
				{
					Delegate @delegate = invocationList[i];
					bool flag = (bool)@delegate.DynamicInvoke(new object[0]);
					if (!flag)
					{
						result = false;
						return result;
					}
				}
			}
			result = true;
			return result;
		}

		protected void ExecutePhysicsStep(List<SyncedData> data, int syncedDataTick)
		{
			this.ExecuteDelegates(syncedDataTick);
			this.SyncedArrayToInputArray(data);//data塞到auxPlayersInputData以便StepUpdate有数据分发给所有的TrueSyncBehaviour
            this.StepUpdate(this.auxPlayersInputData);//TrueSyncManager.OnStepUpdate->所有的TrueSyncBehaviour.OnStepUpdate
            this.physicsManager.UpdateStep();
		}

		private void ExecuteDelegates(int syncedDataTick)
		{
			syncedDataTick++;
			bool flag = this.playersDisconnect.ContainsKey(syncedDataTick);
			if (flag)
			{
				List<byte> list = this.playersDisconnect[syncedDataTick];
				int i = 0;
				int count = list.Count;
				while (i < count)
				{
					this.OnPlayerDisconnection(list[i]);
					i++;
				}
			}
		}
        /// <summary>
        /// 为了让auxActivePlayersIds过滤本地玩家
        /// </summary>
		internal void UpdateActivePlayers()
		{
			this.playersIdsAux.Clear();
			int i = 0;
			int count = this.activePlayers.Count;
			while (i < count)
			{
                //过滤了本地玩家,那么playersIdsAus存的都是除本地玩家之外的所有玩家
				if (this.localPlayer == null || this.localPlayer.ID != this.activePlayers[i].ID)
				{
					this.playersIdsAux.Add((int)this.activePlayers[i].ID);//List
                }
				i++;
			}
			this.auxActivePlayersIds = this.playersIdsAux.ToArray();
		}

		private void CheckGameStart()
		{
            //默认是ReplayMode.NO_REPLAY
            if (this.replayMode == ReplayMode.LOAD_REPLAY)
			{
				this.RunSimulation(false);
			}
			else
            { //检测所有玩家是否已经准备好了
                bool flag = true;
				int i = 0;
				int count = this.activePlayers.Count;
				while (i < count)
				{
					flag &= this.activePlayers[i].sentSyncedStart;
					i++;
				}
                //所有玩家已经准备好了
				if (flag)
				{
					this.RunSimulation(false);
					SyncedData.pool.FillStack(this.activePlayers.Count * (this.syncWindow + this.rollbackWindow));
				}
                //其他玩家还没有准备好,发送196事件
                else
                {
					this.RaiseEvent(SYNCED_GAME_START_CODE, SyncedInfo.Encode(new SyncedInfo
					{
						playerId = this.localPlayer.ID
					}));
				}
			}
		}

		protected void SyncedArrayToInputArray(List<SyncedData> data)
		{
			this.auxPlayersInputData.Clear();
			int i = 0;
			int count = data.Count;
			while (i < count)
			{
				this.auxPlayersInputData.Add(data[i].inputData);
				i++;
			}
		}

		public void PauseSimulation()
		{
			this.Pause();
			this.RaiseEvent(SIMULATION_CODE, new byte[1], true, this.auxActivePlayersIds);//content:0
        }

		public void RunSimulation(bool firstRun)
		{
			this.Run();
            //bool flag = !firstRun;
            //firstRun=true的时候 不给其他玩家发送消息,auxActivePlayersIds存的都是除了本地玩家之外的所有玩家id
			if (!firstRun)
			{
				this.RaiseEvent(SIMULATION_CODE, new byte[]
				{
					1
				}, true, this.auxActivePlayersIds);
			}
		}

		public void EndSimulation()
		{
			this.End();
			this.RaiseEvent(SIMULATION_CODE, new byte[]
			{
				3
			}, true, this.auxActivePlayersIds);
		}

		public void Destroy(IBody rigidBody)
		{
			rigidBody.TSDisabled = true;
			int key = this.GetSimulatedTick(this.GetSyncedDataTick()) + 1;
			bool flag = !this.bodiesToDestroy.ContainsKey(key);
			if (flag)
			{
				this.bodiesToDestroy[key] = new List<IBody>();
			}
			this.bodiesToDestroy[key].Add(rigidBody);
		}

		protected void CheckSafeRemotion(int refTick)
		{
			if (this.bodiesToDestroy.ContainsKey(refTick))
			{
				List<IBody> list = this.bodiesToDestroy[refTick];
				foreach (IBody current in list)
				{
					bool tSDisabled = current.TSDisabled;
					if (tSDisabled)
					{
						this.physicsManager.RemoveBody(current);
					}
				}
				this.bodiesToDestroy.Remove(refTick);
			}
			if (this.playersDisconnect.ContainsKey(refTick))
			{
				this.playersDisconnect.Remove(refTick);
			}
		}
        /// <summary>
        /// Lag:落后， 延迟
        /// </summary>
        private void DropLagPlayers()
		{
			List<TSPlayer> list = new List<TSPlayer>();
			int refTick = this.GetRefTick(this.GetSyncedDataTick());//对于defaultLookStep,直接返回syncedDataTick
			if (refTick >= 0)
			{
				int i = 0;
				int count = this.activePlayers.Count;
				while (i < count)
				{
					TSPlayer tSPlayer = this.activePlayers[i];
					if (!tSPlayer.IsDataReady(refTick))
					{
						tSPlayer.dropCount++;
						list.Add(tSPlayer);
					}
					i++;
				}
			}
			int j = 0;
			int count2 = list.Count;
			while (j < count2)
			{
				TSPlayer p = list[j];
				this.CheckDrop(p);//查看远端玩家是否掉线
				bool sendDataForDrop = list[j].GetSendDataForDrop(this.localPlayer.ID, this._syncedDataCacheDrop);
				if (sendDataForDrop)
				{
					this.communicator.OpRaiseEvent(SEND_CODE, SyncedData.Encode(this._syncedDataCacheDrop), true, null);
					SyncedData.pool.GiveBack(this._syncedDataCacheDrop[0]);
				}
				j++;
			}
		}
        /// <summary>
        /// 收集本地玩家的输入，并通过服务器发送给其他玩家
        /// </summary>
        /// <returns></returns>
        private SyncedData UpdateData()
		{
			SyncedData result;
			if (this.replayMode == ReplayMode.LOAD_REPLAY)
			{
				result = null;
			}
			else
			{
				SyncedData syncedData = SyncedData.pool.GetNew();
                syncedData.Init(this.localPlayer.ID, this.ticks);
				this.GetLocalData(syncedData.inputData);//TrueSyncManager.GetLocalData, 调用OnSyncedInput();
                this.localPlayer.AddData(syncedData);//本地数据直接塞给本地玩家，下面还需要把本地数据发送给远端
				if (this.communicator != null)
				{
                    //因为_syncedDataCacheUpdateData只有一个,所以只获取了一个SyncedData
                    this.localPlayer.GetSendData(this.ticks, this._syncedDataCacheUpdateData);//从TSplayer的controls里取数据
                    //把本地玩家的输入数据发给远端玩家
                    this.communicator.OpRaiseEvent(SEND_CODE, SyncedData.Encode(this._syncedDataCacheUpdateData), true, this.auxActivePlayersIds);
				}
				result = syncedData;
			}
			return result;
		}

		public InputDataBase GetInputData(int playerId)
		{
			return this.players[(byte)playerId].GetData(this.GetSyncedDataTick()).inputData;
		}
        /// <summary>
        /// 对刚体body的位置和旋转数据的一个同步校验
        /// </summary>
        /// <param name="tick"></param>
        private void SendInfoChecksum(int tick)
		{
			bool flag = this.replayMode == ReplayMode.LOAD_REPLAY;
			if (!flag)
			{
				SyncedInfo syncedInfo = this.bufferSyncedInfo.Current();
				syncedInfo.playerId = this.localPlayer.ID;
				syncedInfo.tick = tick;
				syncedInfo.checksum = this.GetChecksumForSyncedInfo();
				this.bufferSyncedInfo.MoveNext();
				this.RaiseEvent(CHECKSUM_CODE, SyncedInfo.Encode(syncedInfo));
			}
		}

		private void RaiseEvent(byte eventCode, object message)
		{
			this.RaiseEvent(eventCode, message, true, null);
		}

		private void RaiseEvent(byte eventCode, object message, bool reliable, int[] toPlayers)
		{
			bool flag = this.communicator != null;
			if (flag)
			{
				this.communicator.OpRaiseEvent(eventCode, message, reliable, toPlayers);
			}
		}

		private void OnEventDataReceived(byte eventCode, object content)
		{
			if (eventCode == SEND_CODE)
			{
				byte[] data = content as byte[];
				List<SyncedData> list = SyncedData.Decode(data);//只有list[0]是携带玩家ownerID数据,剩余的list都是该玩家的数据
                //注意list是网络数据
                if (list.Count > 0)
				{
                    //tSPlayer是本地数据，是需要和根据网络数据同步的
					TSPlayer tSPlayer = this.players[list[0].inputData.ownerID];
					if (!tSPlayer.dropped)
					{
						this.OnSyncedDataReceived(tSPlayer, list);//调用TSPlayer.controls添加数据
                        //满足三个条件,dropCount才可以增加
                        //网络数据的dropPlayer必须为true,网络过来的玩家不是本地玩家，提取网络数据中的dropFromPlayerId，查找players,其对应的dropped为false
                        //这里有dropPlayer dropped dropFromPlayerId 需要理解区分
						if (list[0].dropPlayer && tSPlayer.ID != this.localPlayer.ID && !this.players[list[0].dropFromPlayerId].dropped)
						{
							tSPlayer.dropCount++;
						}
					}
					else
					{
						int i = 0;
						int count = list.Count;
						while (i < count)
						{
							SyncedData.pool.GiveBack(list[i]);
							i++;
						}
					}
					SyncedData.poolList.GiveBack(list);
				}
			}
			else
			{
				bool flag5 = eventCode == CHECKSUM_CODE;
				if (flag5)
				{
					byte[] infoBytes = content as byte[];
					this.OnChecksumReceived(SyncedInfo.Decode(infoBytes));
				}
				else
				{
                    //197事件，内容只有3种：0:pause,1:Run,3:End
					if (eventCode == SIMULATION_CODE)
					{
						byte[] array = content as byte[];
						if (array.Length != 0)
						{
                            switch (array[0])
                            {
                                case 0:
                                    this.Pause();
                                    break;
                                case 1:
                                    Run();
                                    break;
                                case 3:
                                    End();
                                    break;
                            }
                        }
					}
                    //196事件 远程玩家准备好事件
					else
					{
                        //远程玩家准备好事件
						if (eventCode == SYNCED_GAME_START_CODE)
						{
							byte[] infoBytes2 = content as byte[];
							SyncedInfo syncedInfo = SyncedInfo.Decode(infoBytes2);
							this.players[syncedInfo.playerId].sentSyncedStart = true;
						}
					}
				}
			}
		}

		private void OnChecksumReceived(SyncedInfo syncedInfo)
		{
			bool dropped = this.players[syncedInfo.playerId].dropped;
			if (!dropped)
			{
				this.checksumOk = true;
				SyncedInfo[] buffer = this.bufferSyncedInfo.buffer;
				for (int i = 0; i < buffer.Length; i++)
				{
					SyncedInfo syncedInfo2 = buffer[i];
					bool flag = syncedInfo2.tick == syncedInfo.tick && syncedInfo2.checksum != syncedInfo.checksum;
					if (flag)
					{
						this.checksumOk = false;
						break;
					}
				}
			}
		}
        /// <summary>
        /// 获取所有玩家在该tick下的输入数据
        /// </summary>
        /// <param name="tick"></param>
        /// <returns></returns>
        protected List<SyncedData> GetTickData(int tick)
		{
			this.auxPlayersSyncedData.Clear();
			int i = 0;
			int count = this.activePlayers.Count;
			while (i < count)
			{
				this.auxPlayersSyncedData.Add(this.activePlayers[i].GetData(tick));
				i++;
			}
			return this.auxPlayersSyncedData;
		}

		public void AddPlayer(byte playerId, string playerName, bool isLocal)
		{
			TSPlayer tSPlayer = new TSPlayer(playerId, playerName);
			this.players.Add(tSPlayer.ID, tSPlayer);
			this.activePlayers.Add(tSPlayer);
			if (isLocal)
			{
				this.localPlayer = tSPlayer;
				this.localPlayer.sentSyncedStart = true;
			}
			this.UpdateActivePlayers();
			if (this.replayMode == ReplayMode.RECORD_REPLAY)
			{
				this.replayRecord.AddPlayer(tSPlayer);
			}
		}
        /// <summary>
        /// 检查是否有掉线玩家
        /// </summary>
        /// <param name="p"></param>
        /// <returns></returns>
        private bool CheckDrop(TSPlayer p)
		{
			bool result;
			if (p != this.localPlayer && !p.dropped && p.dropCount > 0)
			{
				int num = this.activePlayers.Count - 1;
                //如果dropCount>=玩家数量,那么就认为该玩家掉线了,
                //如果有五个玩家,dropCount至少是4因为只有其他4个玩家都没收到该玩家的数据，才是真的掉线了。
                if (p.dropCount >= num)
				{
					this.compoundStats.globalStats.GetInfo("panic").count = 0L;
					p.dropped = true;
					this.activePlayers.Remove(p);
					this.UpdateActivePlayers();
					Debug.Log("Player dropped (stopped sending input)");
					int key = this.GetSyncedDataTick() + 1;//下一帧为掉线玩家
					if (!this.playersDisconnect.ContainsKey(key))
					{
						this.playersDisconnect[key] = new List<byte>();
					}
					this.playersDisconnect[key].Add(p.ID);
					result = true;
					return result;
				}
			}
			result = false;
			return result;
		}
	}
}
