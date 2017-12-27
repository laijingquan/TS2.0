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

		internal Dictionary<byte, TSPlayer> players;

		internal List<TSPlayer> activePlayers;

		internal List<SyncedData> auxPlayersSyncedData;

		internal List<InputDataBase> auxPlayersInputData;

		internal int[] auxActivePlayersIds;

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
			bool flag = this.simulationState == AbstractLockstep.SimulationState.NOT_STARTED;
			if (flag)
			{
				this.simulationState = AbstractLockstep.SimulationState.WAITING_PLAYERS;
			}
			else
			{
				bool flag2 = this.simulationState == AbstractLockstep.SimulationState.WAITING_PLAYERS || this.simulationState == AbstractLockstep.SimulationState.PAUSED;
				if (flag2)
				{
					bool flag3 = this.simulationState == AbstractLockstep.SimulationState.WAITING_PLAYERS;
					if (flag3)
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
			bool flag = this.simulationState == AbstractLockstep.SimulationState.RUNNING;
			if (flag)
			{
				this.OnGamePaused();
				this.simulationState = AbstractLockstep.SimulationState.PAUSED;
			}
		}

		private void End()
		{
			bool flag = this.simulationState != AbstractLockstep.SimulationState.ENDED;
			if (flag)
			{
				this.OnGameEnded();
				bool flag2 = this.replayMode == ReplayMode.RECORD_REPLAY;
				if (flag2)
				{
					ReplayRecord.SaveRecord(this.replayRecord);
				}
				this.simulationState = AbstractLockstep.SimulationState.ENDED;
			}
		}

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
                    if (this.CheckGameIsReady() && this.IsStepReady(syncedDataTick))
					{
						this.compoundStats.Increment("simulated_frames");//simulate_frames++;
                        this.UpdateData();//收集本地输入，通过服务器发送给其他玩家
                        this.elapsedPanicTicks = 0;
						int refTick = this.GetRefTick(syncedDataTick);//对于defaultLookStep,直接返回syncedDataTick
                        //每100tick做一次刚体同步校验
                        if (refTick > 1 && refTick % 100 == 0)
						{
							this.SendInfoChecksum(refTick);
						}
						this._lastSafeTick = refTick;
						this.BeforeStepUpdate(syncedDataTick, refTick);
						List<SyncedData> tickData = this.GetTickData(syncedDataTick);//获取所有玩家在该syncedDataTick下的输入数据
                        this.ExecutePhysicsStep(tickData, syncedDataTick);
						if (this.replayMode == ReplayMode.RECORD_REPLAY)
						{
							this.replayRecord.AddSyncedData(this.GetTickData(refTick));
                        }
						this.AfterStepUpdate(syncedDataTick, refTick);//从TSPlayer.controls清除该refTick的数据
                        this.ticks++;
					}
					else
					{
						if (this.ticks >= this.totalWindow)
						{
							if (this.replayMode == ReplayMode.LOAD_REPLAY)
							{
								this.End();
							}
							else
							{
								this.compoundStats.Increment("missed_frames");
								this.elapsedPanicTicks++;
								if (this.elapsedPanicTicks > this.panicWindow)
								{
									this.compoundStats.Increment("panic");
									if (this.compoundStats.globalStats.GetInfo("panic").count >= 5L)
									{
										this.End();
									}
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

		private bool CheckGameIsReady()
		{
			bool flag = this.GameIsReady != null;
			bool result;
			if (flag)
			{
				Delegate[] invocationList = this.GameIsReady.GetInvocationList();
				for (int i = 0; i < invocationList.Length; i++)
				{
					Delegate @delegate = invocationList[i];
					bool flag2 = (bool)@delegate.DynamicInvoke(new object[0]);
					bool flag3 = !flag2;
					if (flag3)
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
			this.SyncedArrayToInputArray(data);//data塞到auxPlayersInputData
            this.StepUpdate(this.auxPlayersInputData);//TrueSyncManager.OnStepUpdate
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

		internal void UpdateActivePlayers()
		{
			this.playersIdsAux.Clear();
			int i = 0;
			int count = this.activePlayers.Count;
			while (i < count)
			{
                //bool flag = localPlayer == null || localPlayer.ID != activePlayers[i].ID;
                //过滤了本地玩家,那么playersIdsAus存的都是除本地玩家之外的所有玩家
                bool flag = this.localPlayer == null || this.localPlayer.ID != this.activePlayers[i].ID;
				if (flag)
				{
					this.playersIdsAux.Add((int)this.activePlayers[i].ID);//List
                }
				i++;
			}
			this.auxActivePlayersIds = this.playersIdsAux.ToArray();
		}

		private void CheckGameStart()
		{
			bool flag = this.replayMode == ReplayMode.LOAD_REPLAY;
			if (flag)
			{
				this.RunSimulation(false);
			}
			else
            { //检测所有玩家是否已经准备好了
                bool flag2 = true;
				int i = 0;
				int count = this.activePlayers.Count;
				while (i < count)
				{
					flag2 &= this.activePlayers[i].sentSyncedStart;
					i++;
				}
                //所有玩家已经准备好了
                bool flag3 = flag2;
				if (flag3)
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
			this.RaiseEvent(SIMULATION_CODE, new byte[1], true, this.auxActivePlayersIds);
		}

		public void RunSimulation(bool firstRun)
		{
			this.Run();
            //bool flag = !firstRun;
            //firstRun=true的时候 不给其他玩家发送消息,auxActivePlayersIds存的都是除了本地玩家之外的所有玩家id
            bool flag = !firstRun;
			if (flag)
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
			bool flag = this.bodiesToDestroy.ContainsKey(refTick);
			if (flag)
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
			bool flag2 = this.playersDisconnect.ContainsKey(refTick);
			if (flag2)
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
			int refTick = this.GetRefTick(this.GetSyncedDataTick());
			bool flag = refTick >= 0;
			if (flag)
			{
				int i = 0;
				int count = this.activePlayers.Count;
				while (i < count)
				{
					TSPlayer tSPlayer = this.activePlayers[i];
					bool flag2 = !tSPlayer.IsDataReady(refTick);
					if (flag2)
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
				this.CheckDrop(p);
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
        /// 收集本地输入，通过服务器发送给其他玩家
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
				SyncedData @new = SyncedData.pool.GetNew();
				@new.Init(this.localPlayer.ID, this.ticks);
				this.GetLocalData(@new.inputData);//调用OnSyncedInput();输入数据给到@new.inputData里面
                this.localPlayer.AddData(@new);//数据塞给TSPlayer的controls
                bool flag2 = this.communicator != null;
				if (flag2)
				{
					this.localPlayer.GetSendData(this.ticks, this._syncedDataCacheUpdateData);//从TSplayer的controls里取数据
                    this.communicator.OpRaiseEvent(SEND_CODE, SyncedData.Encode(this._syncedDataCacheUpdateData), true, this.auxActivePlayersIds);
				}
				result = @new;
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
			bool flag = eventCode == SEND_CODE;
			if (flag)
			{
				byte[] data = content as byte[];
				List<SyncedData> list = SyncedData.Decode(data);//只有list[0]是携带玩家ownerID数据,剩余的list都是该玩家的数据
                bool flag2 = list.Count > 0;
				if (flag2)
				{
					TSPlayer tSPlayer = this.players[list[0].inputData.ownerID];
					bool flag3 = !tSPlayer.dropped;
					if (flag3)
					{
						this.OnSyncedDataReceived(tSPlayer, list);//调用TSPlayer.addData添加数据
                        //满足三个条件,dropCount才可以增加
                        //网络数据的dropPlayer必须为true,网络过来的玩家不是本地玩家，提取网络数据中的dropFromPlayerId，查找players,其对应的dropped为false
                        //这里有dropPlayer dropped dropFromPlayerId 需要理解区分

                        bool flag4 = list[0].dropPlayer && tSPlayer.ID != this.localPlayer.ID && !this.players[list[0].dropFromPlayerId].dropped;
						if (flag4)
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
					bool flag6 = eventCode == SIMULATION_CODE;
					if (flag6)
					{
						byte[] array = content as byte[];
						bool flag7 = array.Length != 0;
						if (flag7)
						{
							bool flag8 = array[0] == 0;
							if (flag8)
							{
								this.Pause();
							}
							else
							{
								bool flag9 = array[0] == 1;
								if (flag9)
								{
									this.Run();
								}
								else
								{
									bool flag10 = array[0] == 3;
									if (flag10)
									{
										this.End();
									}
								}
							}
						}
					}
					else
					{
						bool flag11 = eventCode == SYNCED_GAME_START_CODE;
						if (flag11)
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
			bool flag = this.replayMode == ReplayMode.RECORD_REPLAY;
			if (flag)
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
                if (p.dropCount >= num)
				{
					this.compoundStats.globalStats.GetInfo("panic").count = 0L;
					p.dropped = true;
					this.activePlayers.Remove(p);
					this.UpdateActivePlayers();
					Debug.Log("Player dropped (stopped sending input)");
					int key = this.GetSyncedDataTick() + 1;//下一帧为掉线玩家
                    bool flag3 = !this.playersDisconnect.ContainsKey(key);
					if (flag3)
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
