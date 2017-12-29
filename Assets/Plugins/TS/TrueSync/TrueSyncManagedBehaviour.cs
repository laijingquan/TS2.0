using System;
using System.Collections.Generic;

namespace TrueSync
{
	public class TrueSyncManagedBehaviour : ITrueSyncBehaviourGamePlay, ITrueSyncBehaviour, ITrueSyncBehaviourCallbacks
	{
		public ITrueSyncBehaviour trueSyncBehavior;

		[AddTracking]
		public bool disabled;

		public TSPlayerInfo localOwner;

		public TSPlayerInfo owner;

		public TrueSyncManagedBehaviour(ITrueSyncBehaviour trueSyncBehavior)
		{
			StateTracker.AddTracking(this);
			StateTracker.AddTracking(trueSyncBehavior);
			this.trueSyncBehavior = trueSyncBehavior;//基本是TrueSyncBehaviour
		}

        /// <summary>
        /// 调用TrusSyncBehaviour:OnPreSyncedUpdate
        /// </summary>
		public void OnPreSyncedUpdate()
		{
			if (this.trueSyncBehavior is ITrueSyncBehaviourGamePlay)
			{
				((ITrueSyncBehaviourGamePlay)this.trueSyncBehavior).OnPreSyncedUpdate();
			}
		}
        /// <summary>
        /// 调用TrusSyncBehaviour:OnSyncedInput
        /// </summary>
        public void OnSyncedInput()
		{
			if (this.trueSyncBehavior is ITrueSyncBehaviourGamePlay)
			{
				((ITrueSyncBehaviourGamePlay)this.trueSyncBehavior).OnSyncedInput();
			}
		}
        /// <summary>
        /// 调用TrusSyncBehaviour:OnSyncedUpdate
        /// </summary>
        public void OnSyncedUpdate()
		{
			if (this.trueSyncBehavior is ITrueSyncBehaviourGamePlay)
			{
				((ITrueSyncBehaviourGamePlay)this.trueSyncBehavior).OnSyncedUpdate();
			}
		}
        /// <summary>
        /// 调用TrusSyncBehaviour:SetGameInfo
        /// </summary>
        public void SetGameInfo(TSPlayerInfo localOwner, int numberOfPlayers)
		{
			this.trueSyncBehavior.SetGameInfo(localOwner, numberOfPlayers);
		}
        /// <summary>
        /// 调用TrusSyncBehaviour:OnSyncedStart
        /// </summary>
        public void OnSyncedStart()
		{
			if (this.trueSyncBehavior is ITrueSyncBehaviourCallbacks)
			{
				((ITrueSyncBehaviourCallbacks)this.trueSyncBehavior).OnSyncedStart();
				if (this.localOwner.Id == this.owner.Id)
				{
					((ITrueSyncBehaviourCallbacks)this.trueSyncBehavior).OnSyncedStartLocalPlayer();
				}
			}
		}
        /// <summary>
        /// 调用TrusSyncBehaviour:OnGamePaused
        /// </summary>
        public void OnGamePaused()
		{
			if (this.trueSyncBehavior is ITrueSyncBehaviourCallbacks)
			{
				((ITrueSyncBehaviourCallbacks)this.trueSyncBehavior).OnGamePaused();
			}
		}
        /// <summary>
        /// 调用TrusSyncBehaviour:OnGameUnPaused
        /// </summary>
        public void OnGameUnPaused()
		{
			if (this.trueSyncBehavior is ITrueSyncBehaviourCallbacks)
			{
				((ITrueSyncBehaviourCallbacks)this.trueSyncBehavior).OnGameUnPaused();
			}
		}
        /// <summary>
        /// 调用TrusSyncBehaviour:OnGameEnded
        /// </summary>
        public void OnGameEnded()
		{
			if (this.trueSyncBehavior is ITrueSyncBehaviourCallbacks)
			{
				((ITrueSyncBehaviourCallbacks)this.trueSyncBehavior).OnGameEnded();
			}
		}
        /// <summary>
        /// 调用TrusSyncBehaviour:OnPlayerDisconnection
        /// </summary>
        public void OnPlayerDisconnection(int playerId)
		{
			if (this.trueSyncBehavior is ITrueSyncBehaviourCallbacks)
			{
				((ITrueSyncBehaviourCallbacks)this.trueSyncBehavior).OnPlayerDisconnection(playerId);
			}
		}

        /// <summary>
        /// 有可能反编译错误
        /// </summary>
		public void OnSyncedStartLocalPlayer()
		{
			throw new NotImplementedException();
		}

        /// <summary>
        /// 通知每个trueSyncBehviour 有玩家掉线(TrueSyncManagedBehaviour:OnPlayerDisconnection->trueSyncBehavior.OnPlayerDisconnection)
        /// </summary>
        /// <param name="generalBehaviours">属于场景中的TrueSyncBehaviour</param>
        /// <param name="behaviorsByPlayer">属于玩家的TrueSyncBehaviour</param>
        /// <param name="playerId">掉线的玩家id</param>
		public static void OnPlayerDisconnection(List<TrueSyncManagedBehaviour> generalBehaviours, Dictionary<byte, List<TrueSyncManagedBehaviour>> behaviorsByPlayer, byte playerId)
		{
			int i = 0;
			int count = generalBehaviours.Count;
			while (i < count)
			{
				generalBehaviours[i].OnPlayerDisconnection((int)playerId);
				i++;
			}
			Dictionary<byte, List<TrueSyncManagedBehaviour>>.Enumerator enumerator = behaviorsByPlayer.GetEnumerator();
			while (enumerator.MoveNext())
			{
				KeyValuePair<byte, List<TrueSyncManagedBehaviour>> current = enumerator.Current;
				List<TrueSyncManagedBehaviour> value = current.Value;
				int j = 0;
				int count2 = value.Count;
				while (j < count2)
				{
					value[j].OnPlayerDisconnection((int)playerId);
					j++;
				}
			}
		}
        /// <summary>
        /// 通知每个trueSyncBehviour 游戏开始(TrueSyncManagedBehaviour:OnGameStarted->trueSyncBehavior.OnGameStarted)
        /// </summary>
        /// <param name="generalBehaviours">属于场景中的TrueSyncBehaviour</param>
        /// <param name="behaviorsByPlayer">属于玩家的TrueSyncBehaviour</param>
        public static void OnGameStarted(List<TrueSyncManagedBehaviour> generalBehaviours, Dictionary<byte, List<TrueSyncManagedBehaviour>> behaviorsByPlayer)
		{
			int i = 0;
			int count = generalBehaviours.Count;
			while (i < count)
			{
				generalBehaviours[i].OnSyncedStart();
				i++;
			}
			Dictionary<byte, List<TrueSyncManagedBehaviour>>.Enumerator enumerator = behaviorsByPlayer.GetEnumerator();
			while (enumerator.MoveNext())
			{
				KeyValuePair<byte, List<TrueSyncManagedBehaviour>> current = enumerator.Current;
				List<TrueSyncManagedBehaviour> value = current.Value;
				int j = 0;
				int count2 = value.Count;
				while (j < count2)
				{
					value[j].OnSyncedStart();
					j++;
				}
			}
		}
        /// <summary>
        /// 通知每个trueSyncBehviour 游戏暂停(TrueSyncManagedBehaviour:OnGamePaused->trueSyncBehavior.OnGamePaused)
        /// </summary>
        /// <param name="generalBehaviours">属于场景中的TrueSyncBehaviour</param>
        /// <param name="behaviorsByPlayer">属于玩家的TrueSyncBehaviour</param>
        public static void OnGamePaused(List<TrueSyncManagedBehaviour> generalBehaviours, Dictionary<byte, List<TrueSyncManagedBehaviour>> behaviorsByPlayer)
		{
			int i = 0;
			int count = generalBehaviours.Count;
			while (i < count)
			{
				generalBehaviours[i].OnGamePaused();
				i++;
			}
			Dictionary<byte, List<TrueSyncManagedBehaviour>>.Enumerator enumerator = behaviorsByPlayer.GetEnumerator();
			while (enumerator.MoveNext())
			{
				KeyValuePair<byte, List<TrueSyncManagedBehaviour>> current = enumerator.Current;
				List<TrueSyncManagedBehaviour> value = current.Value;
				int j = 0;
				int count2 = value.Count;
				while (j < count2)
				{
					value[j].OnGamePaused();
					j++;
				}
			}
		}
        /// <summary>
        /// 通知每个trueSyncBehviour 游戏取消暂停(TrueSyncManagedBehaviour:OnGameUnPaused->trueSyncBehavior.OnGameUnPaused)
        /// </summary>
        /// <param name="generalBehaviours">属于场景中的TrueSyncBehaviour</param>
        /// <param name="behaviorsByPlayer">属于玩家的TrueSyncBehaviour</param>
        public static void OnGameUnPaused(List<TrueSyncManagedBehaviour> generalBehaviours, Dictionary<byte, List<TrueSyncManagedBehaviour>> behaviorsByPlayer)
		{
			int i = 0;
			int count = generalBehaviours.Count;
			while (i < count)
			{
				generalBehaviours[i].OnGameUnPaused();
				i++;
			}
			Dictionary<byte, List<TrueSyncManagedBehaviour>>.Enumerator enumerator = behaviorsByPlayer.GetEnumerator();
			while (enumerator.MoveNext())
			{
				KeyValuePair<byte, List<TrueSyncManagedBehaviour>> current = enumerator.Current;
				List<TrueSyncManagedBehaviour> value = current.Value;
				int j = 0;
				int count2 = value.Count;
				while (j < count2)
				{
					value[j].OnGameUnPaused();
					j++;
				}
			}
		}
        /// <summary>
        /// 通知每个trueSyncBehviour 游戏结束(TrueSyncManagedBehaviour:OnGameEnded->trueSyncBehavior.OnGameEnded)
        /// </summary>
        /// <param name="generalBehaviours">属于场景中的TrueSyncBehaviour</param>
        /// <param name="behaviorsByPlayer">属于玩家的TrueSyncBehaviour</param>
        public static void OnGameEnded(List<TrueSyncManagedBehaviour> generalBehaviours, Dictionary<byte, List<TrueSyncManagedBehaviour>> behaviorsByPlayer)
		{
			int i = 0;
			int count = generalBehaviours.Count;
			while (i < count)
			{
				generalBehaviours[i].OnGameEnded();
				i++;
			}
			Dictionary<byte, List<TrueSyncManagedBehaviour>>.Enumerator enumerator = behaviorsByPlayer.GetEnumerator();
			while (enumerator.MoveNext())
			{
				KeyValuePair<byte, List<TrueSyncManagedBehaviour>> current = enumerator.Current;
				List<TrueSyncManagedBehaviour> value = current.Value;
				int j = 0;
				int count2 = value.Count;
				while (j < count2)
				{
					value[j].OnGameEnded();
					j++;
				}
			}
		}
	}
}
