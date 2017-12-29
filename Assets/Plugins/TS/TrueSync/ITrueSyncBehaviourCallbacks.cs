using System;

namespace TrueSync
{
    /// <summary>
    /// ��Ϸ�������ڵĽӿ�,��ʼ ��ͣ ���� ���� ��
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
