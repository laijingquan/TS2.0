using System;

namespace TrueSync
{
    /// <summary>
    /// �����������صĽӿ�
    /// </summary>
	public interface ITrueSyncBehaviourGamePlay : ITrueSyncBehaviour
	{
		void OnPreSyncedUpdate();

		void OnSyncedInput();

		void OnSyncedUpdate();
	}
}
