using System;

namespace TrueSync
{
    /// <summary>
    /// 游戏声明周期的接口,开始 暂停 结束 掉线 等
    /// </summary>
	public interface ITrueSyncBehaviourCallbacks : ITrueSyncBehaviour
	{
		void OnSyncedStart();

		void OnSyncedStartLocalPlayer();

		void OnGamePaused();

		void OnGameUnPaused();

		void OnGameEnded();

		void OnPlayerDisconnection(int playerId);
	}
}
