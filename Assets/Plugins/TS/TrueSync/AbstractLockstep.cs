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
        /// ������� ����������� dict�洢
        /// </summary>
		internal Dictionary<byte, TSPlayer> players;
        /// <summary>
        /// ������� ����������� list�洢
        /// </summary>
		internal List<TSPlayer> activePlayers;

		internal List<SyncedData> auxPlayersSyncedData;

		internal List<InputDataBase> auxPlayersInputData;

        /// <summary>
        /// ��ȥ������ҵ�����
        /// </summary>
		internal int[] auxActivePlayersIds;
        /// <summary>
        /// �������
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
        /// ���ǳ���panicWindow��û���յ�Զ����ҵ�input��ô�ý�֮�Ƴ�����̫���ˣ�
        /// </summary>
        private int panicWindow;
        /// <summary>
        /// this is the size of the input queue;���ǻ����������ݵĴ��ڴ�С
        /// Lets say a game client has a ping (round trip time) of 60ms to Photon Cloud servers, and we're using the default locked step time of 0.02 (20ms time per frame).

        //In a lockstep game, we need the input queue to compensate that lag from the remote players by adding an equally big latency to local input.

        //This means that the sync window size shall be at least 3, so we add 60ms(3 frames X 20ms per frame) to all input
        //���Ǳ��ص�����ȴ�Զ�̵����룬Ϊ�˸��õ�ͬ��
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
        /// ��ȥ������ҵ� list һ����ת����
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
        /// this.ticks - this.syncWindow;syncWindow�������Ϊ���洰��,һֱ�ñ���Ԥ�Ȼ���һ��Զ�˹��������ݣ������ݸ���ƽ��
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
        /// TrueSyncBehaviour:FixedUpdate()��������
        /// </summary>
		public void Update()
		{
            //����������Ѿ����ڵȴ�������ҵ�״̬,��ô��Ӧ��ȥ�����������Ƿ��Ѿ�׼������
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
                    //��ÿ����ҽ��м�����
                    while (i < num)
					{
                        //�Ƿ����
                        if (this.CheckDrop(this.activePlayers[i]))
                        {
                            //����ǵ���,��ô����һ��activePlayers�б���ɾ��,Ϊ���ܹ���ȷ�ļ���������,����i--,num--
                            i--;
							num--;
						}
						i++;
					}
					int syncedDataTick = this.GetSyncedDataTick();//syncedDataTickָ����ǻ�����еĵ�һ��
                    if (this.CheckGameIsReady() && this.IsStepReady(syncedDataTick)) //�൱����ѯ��������ҵ�controls�Ƿ������ݣ�this.activePlayers[i].IsDataReady(syncedDataTick));
                    {
						this.compoundStats.Increment("simulated_frames");//simulate_frames++;
                        this.UpdateData();//�ռ�������ҵ����룬��ͨ�����������͸��������
                        this.elapsedPanicTicks = 0;
                        //����defaultLookStep refTick==syncedDataTick
						int refTick = this.GetRefTick(syncedDataTick);//����defaultLookStep,ֱ�ӷ���syncedDataTick
                        //ÿ100tick��һ�θ���ͬ��У��
                        if (refTick > 1 && refTick % 100 == 0)
						{
							this.SendInfoChecksum(refTick);
						}
						this._lastSafeTick = refTick;
						this.BeforeStepUpdate(syncedDataTick, refTick);//ȥ������Body,�Ͷ��ߵ����
						List<SyncedData> tickData = this.GetTickData(syncedDataTick);//��ȡ��������ڸ�syncedDataTick�µ���������
                        this.ExecutePhysicsStep(tickData, syncedDataTick);
						if (this.replayMode == ReplayMode.RECORD_REPLAY)
						{
							this.replayRecord.AddSyncedData(this.GetTickData(refTick));
                        }
						this.AfterStepUpdate(syncedDataTick, refTick);//��TSPlayer.controls�����refTick������,��Pool����
                        this.ticks++;
					}
					else
					{
                        //��ticks������ĳ�����޻�û�յ�Զ�˵����ݣ���ô���Ƕ���Ϊmissed_frames
						if (this.ticks >= this.totalWindow)
						{
                            //LOAD_REPLAYģʽֱ�ӽ�����Ϸ
							if (this.replayMode == ReplayMode.LOAD_REPLAY)
							{
								this.End();
							}
							else
							{
								this.compoundStats.Increment("missed_frames");
								this.elapsedPanicTicks++;
                                //��˼��˵missed_frames����Ԥ���panicWindow
								if (this.elapsedPanicTicks > this.panicWindow)
								{
									this.compoundStats.Increment("panic");
                                    //missed_frames����Ԥ���panicWindow��������ֳ������,��ôֱ�ӽ�����Ϸ
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
        /// ��ʱûע�ᣬ�ض�����true
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
			this.SyncedArrayToInputArray(data);//data����auxPlayersInputData�Ա�StepUpdate�����ݷַ������е�TrueSyncBehaviour
            this.StepUpdate(this.auxPlayersInputData);//TrueSyncManager.OnStepUpdate->���е�TrueSyncBehaviour.OnStepUpdate
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
        /// Ϊ����auxActivePlayersIds���˱������
        /// </summary>
		internal void UpdateActivePlayers()
		{
			this.playersIdsAux.Clear();
			int i = 0;
			int count = this.activePlayers.Count;
			while (i < count)
			{
                //�����˱������,��ôplayersIdsAus��Ķ��ǳ��������֮����������
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
            //Ĭ����ReplayMode.NO_REPLAY
            if (this.replayMode == ReplayMode.LOAD_REPLAY)
			{
				this.RunSimulation(false);
			}
			else
            { //�����������Ƿ��Ѿ�׼������
                bool flag = true;
				int i = 0;
				int count = this.activePlayers.Count;
				while (i < count)
				{
					flag &= this.activePlayers[i].sentSyncedStart;
					i++;
				}
                //��������Ѿ�׼������
				if (flag)
				{
					this.RunSimulation(false);
					SyncedData.pool.FillStack(this.activePlayers.Count * (this.syncWindow + this.rollbackWindow));
				}
                //������һ�û��׼����,����196�¼�
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
            //firstRun=true��ʱ�� ����������ҷ�����Ϣ,auxActivePlayersIds��Ķ��ǳ��˱������֮����������id
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
        /// Lag:��� �ӳ�
        /// </summary>
        private void DropLagPlayers()
		{
			List<TSPlayer> list = new List<TSPlayer>();
			int refTick = this.GetRefTick(this.GetSyncedDataTick());//����defaultLookStep,ֱ�ӷ���syncedDataTick
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
				this.CheckDrop(p);//�鿴Զ������Ƿ����
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
        /// �ռ�������ҵ����룬��ͨ�����������͸��������
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
				this.GetLocalData(syncedData.inputData);//TrueSyncManager.GetLocalData, ����OnSyncedInput();
                this.localPlayer.AddData(syncedData);//��������ֱ������������ң����滹��Ҫ�ѱ������ݷ��͸�Զ��
				if (this.communicator != null)
				{
                    //��Ϊ_syncedDataCacheUpdateDataֻ��һ��,����ֻ��ȡ��һ��SyncedData
                    this.localPlayer.GetSendData(this.ticks, this._syncedDataCacheUpdateData);//��TSplayer��controls��ȡ����
                    //�ѱ�����ҵ��������ݷ���Զ�����
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
        /// �Ը���body��λ�ú���ת���ݵ�һ��ͬ��У��
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
				List<SyncedData> list = SyncedData.Decode(data);//ֻ��list[0]��Я�����ownerID����,ʣ���list���Ǹ���ҵ�����
                //ע��list����������
                if (list.Count > 0)
				{
                    //tSPlayer�Ǳ������ݣ�����Ҫ�͸�����������ͬ����
					TSPlayer tSPlayer = this.players[list[0].inputData.ownerID];
					if (!tSPlayer.dropped)
					{
						this.OnSyncedDataReceived(tSPlayer, list);//����TSPlayer.controls�������
                        //������������,dropCount�ſ�������
                        //�������ݵ�dropPlayer����Ϊtrue,�����������Ҳ��Ǳ�����ң���ȡ���������е�dropFromPlayerId������players,���Ӧ��droppedΪfalse
                        //������dropPlayer dropped dropFromPlayerId ��Ҫ�������
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
                    //197�¼�������ֻ��3�֣�0:pause,1:Run,3:End
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
                    //196�¼� Զ�����׼�����¼�
					else
					{
                        //Զ�����׼�����¼�
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
        /// ��ȡ��������ڸ�tick�µ���������
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
        /// ����Ƿ��е������
        /// </summary>
        /// <param name="p"></param>
        /// <returns></returns>
        private bool CheckDrop(TSPlayer p)
		{
			bool result;
			if (p != this.localPlayer && !p.dropped && p.dropCount > 0)
			{
				int num = this.activePlayers.Count - 1;
                //���dropCount>=�������,��ô����Ϊ����ҵ�����,
                //�����������,dropCount������4��Ϊֻ������4����Ҷ�û�յ�����ҵ����ݣ�������ĵ����ˡ�
                if (p.dropCount >= num)
				{
					this.compoundStats.globalStats.GetInfo("panic").count = 0L;
					p.dropped = true;
					this.activePlayers.Remove(p);
					this.UpdateActivePlayers();
					Debug.Log("Player dropped (stopped sending input)");
					int key = this.GetSyncedDataTick() + 1;//��һ֡Ϊ�������
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
