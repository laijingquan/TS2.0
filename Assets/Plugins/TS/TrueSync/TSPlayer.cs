using System;
using System.Collections.Generic;
using UnityEngine;

namespace TrueSync
{
	[Serializable]
	public class TSPlayer
	{
		[SerializeField]
		public TSPlayerInfo playerInfo;

		[NonSerialized]
		public int dropCount;

		[NonSerialized]
		public bool dropped;

        /// <summary>
        /// 相当于玩家是否已经准备好了标志，本地玩家是否准备好是可以直接知道的，远程玩家是要远端发来准备好的消息
        /// </summary>
		[NonSerialized]
        public bool sentSyncedStart;

		[SerializeField]
		internal SerializableDictionaryIntSyncedData controls;

		private int lastTick;

		public byte ID
		{
			get
			{
				return this.playerInfo.id;
			}
		}

		internal TSPlayer(byte id, string name)
		{
			this.playerInfo = new TSPlayerInfo(id, name);
			this.dropCount = 0;
			this.dropped = false;
			this.controls = new SerializableDictionaryIntSyncedData();
		}

        /// <summary>
        /// 有数据并且不是伪造的
        /// </summary>
        /// <param name="tick"></param>
        /// <returns></returns>
		public bool IsDataReady(int tick)
		{
			return this.controls.ContainsKey(tick) && !this.controls[tick].fake;
		}

		public bool IsDataDirty(int tick)
		{
			bool flag = this.controls.ContainsKey(tick);
			return flag && this.controls[tick].dirty;
		}

		public SyncedData GetData(int tick)
		{
			SyncedData result;
            //判断当前tick是否能在controls里找到
			if (!this.controls.ContainsKey(tick))
			{
				SyncedData syncedData;
                //如果没找到,那么尝试找上一个tick
				if (this.controls.ContainsKey(tick - 1))
				{
					syncedData = this.controls[tick - 1].clone();//深拷贝一份数据出来
					syncedData.tick = tick;
				}
                //如果连上一个tick都没找到,那么就从缓存池里尝试去取出一个新的初始化ID和tick
				else
				{
					syncedData = SyncedData.pool.GetNew();
					syncedData.Init(this.ID, tick);
				}
				syncedData.fake = true;//伪造标志为true,证明该数据为是伪造的
				this.controls[tick] = syncedData;
				result = syncedData;
			}
			else
			{
				result = this.controls[tick];
			}
			return result;
		}

        /// <summary>
        /// 如果controls含有该key,那么就添加到SyncedDataPool相当于被回收了,没有该key那么就添加到controls
        /// </summary>
        /// <param name="data"></param>
		public void AddData(SyncedData data)
		{
			int tick = data.tick;
			if (this.controls.ContainsKey(tick))
			{
				SyncedData.pool.GiveBack(data);
			}
			else
			{
				this.controls[tick] = data;
				this.lastTick = tick;
			}
		}

		public void AddData(List<SyncedData> data)
		{
			for (int i = 0; i < data.Count; i++)
			{
				this.AddData(data[i]);
			}
		}

		public void RemoveData(int refTick)
		{
			bool flag = this.controls.ContainsKey(refTick);
			if (flag)
			{
				SyncedData.pool.GiveBack(this.controls[refTick]);
				this.controls.Remove(refTick);
			}
		}

		public void AddDataProjected(int refTick, int window)
		{
			SyncedData syncedData = this.GetData(refTick);
			for (int i = 1; i <= window; i++)
			{
				SyncedData data = this.GetData(refTick + i);
				bool fake = data.fake;
				if (fake)
				{
					SyncedData syncedData2 = syncedData.clone();
					syncedData2.fake = true;
					syncedData2.tick = refTick + i;
					bool flag = this.controls.ContainsKey(syncedData2.tick);
					if (flag)
					{
						SyncedData.pool.GiveBack(this.controls[syncedData2.tick]);
					}
					this.controls[syncedData2.tick] = syncedData2;
				}
				else
				{
					bool dirty = data.dirty;
					if (dirty)
					{
						data.dirty = false;
						syncedData = data;
					}
				}
			}
		}

		public void AddDataRollback(List<SyncedData> data)
		{
			for (int i = 0; i < data.Count; i++)
			{
				SyncedData data2 = this.GetData(data[i].tick);
				bool fake = data2.fake;
				if (fake)
				{
					bool flag = data2.EqualsData(data[i]);
					if (!flag)
					{
						data[i].dirty = true;
						SyncedData.pool.GiveBack(this.controls[data[i].tick]);
						this.controls[data[i].tick] = data[i];
						break;
					}
					data2.fake = false;
					data2.dirty = false;
				}
				SyncedData.pool.GiveBack(data[i]);
			}
		}

		public bool GetSendDataForDrop(byte fromPlayerId, SyncedData[] sendWindowArray)
		{
			bool result;
			if (this.controls.Count == 0)
			{
				result = false;
			}
			else
			{
				this.GetDataFromTick(this.lastTick, sendWindowArray);
				sendWindowArray[0] = sendWindowArray[0].clone();//不影响controls里面的数据,这里深拷贝一份出来
				sendWindowArray[0].dropFromPlayerId = fromPlayerId; //可以理解为是谁判断了玩家掉线，需要发送给其他远端玩家知道
                 sendWindowArray[0].dropPlayer = true;
				result = true;
			}
			return result;
		}

		public void GetSendData(int tick, SyncedData[] sendWindowArray)
		{
			this.GetDataFromTick(tick, sendWindowArray);
		}

        /// <summary>
        /// 因为sendWindowArray容量为1,所以就获取到一个SyncedData数据
        /// </summary>
        /// <param name="tick"></param>
        /// <param name="sendWindowArray"></param>
		private void GetDataFromTick(int tick, SyncedData[] sendWindowArray)
		{
			for (int i = 0; i < sendWindowArray.Length; i++)
			{
				sendWindowArray[i] = this.GetData(tick - i);
			}
		}
	}
}
