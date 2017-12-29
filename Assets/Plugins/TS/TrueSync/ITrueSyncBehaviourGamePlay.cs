using System;

namespace TrueSync
{
    /// <summary>
    /// 和玩家输入相关的接口
    /// </summary>
	public interface ITrueSyncBehaviourGamePlay : ITrueSyncBehaviour
	{
		void OnPreSyncedUpdate();

		void OnSyncedInput();

		void OnSyncedUpdate();
	}
}
