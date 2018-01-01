using System;
using System.Collections.Generic;

namespace TrueSync
{
    /// <summary>
    /// 注意序列化的只有IputDataBase对象和tick
    /// </summary>
	[Serializable]
	public class SyncedData : ResourcePoolItem
	{
        /// <summary>
        /// 就是Stack<SyncedData>的一个pool，存取单位是SyncedData
        /// </summary>
		internal static ResourcePoolSyncedData pool = new ResourcePoolSyncedData();

        /// <summary>
        /// 就是Stack<List<SyncedData>>的一个pool,存取单位是List<SyncedData>
        /// </summary>
		internal static ResourcePoolListSyncedData poolList = new ResourcePoolListSyncedData();
        /// <summary>
        /// 用户输入数据
        /// </summary>
		public InputDataBase inputData;
        /// <summary>
        /// 逻辑帧数
        /// </summary>
		public int tick;

        /// <summary>
        /// fake英文释义为:伪造
        /// </summary>
		[NonSerialized]
		public bool fake;

		[NonSerialized]
		public bool dirty;

		[NonSerialized]
		public bool dropPlayer;

        /// <summary>
        /// 本机来说他是localID,对于远端数据来说，他是远端玩家Id
        /// </summary>
		[NonSerialized]
		public byte dropFromPlayerId;

		private static List<byte> bytesToEncode = new List<byte>();

		public SyncedData()
		{
			this.inputData = AbstractLockstep.instance.InputDataProvider();
		}

		public void Init(byte ownerID, int tick)
		{
			this.inputData.ownerID = ownerID;
			this.tick = tick;
			this.fake = false;
			this.dirty = false;
		}

        /// <summary>
        /// 处理头
        /// </summary>
        /// <param name="bytes"></param>
		public void GetEncodedHeader(List<byte> bytes)
		{
			Utils.GetBytes(this.tick, bytes);
			bytes.Add(this.inputData.ownerID);
			bytes.Add(this.dropFromPlayerId);
			bytes.Add((byte)(this.dropPlayer ? 1 : 0));
		}
        /// <summary>
        /// 处理每一个SyncedData
        /// </summary>
        /// <param name="bytes"></param>
		public void GetEncodedActions(List<byte> bytes)
		{
			this.inputData.Serialize(bytes);
		}

        /// <summary>
        /// 解码参考编码
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
		public static List<SyncedData> Decode(byte[] data)
		{
			List<SyncedData> pool = SyncedData.poolList.GetNew();
            pool.Clear();
			int i = 0;
			int tick = BitConverter.ToInt32(data, i);
			i += 4;
			byte ownerID = data[i++];
			byte dropFromPlayerId = data[i++];
			bool dropPlayer = data[i++] == 1;
			//int num2 = tick;
			while (i < data.Length)
			{
				SyncedData syncedData = SyncedData.pool.GetNew();
                syncedData.Init(ownerID, tick--);//这里不理解为啥要tick--
                syncedData.inputData.Deserialize(data, ref i);
                pool.Add(syncedData);
			}
			if (pool.Count > 0)
			{
                pool[0].dropPlayer = dropPlayer;
                pool[0].dropFromPlayerId = dropFromPlayerId;
			}
			return pool;
		}

        /// <summary>
        /// 消息描述:消息头+玩家1消息体+玩家2消息体+...
        /// 消息头:tick+OwnerID+drpoFromPlayerId+dropPlayer(来源于第一个SyncedData)
        /// 消息体:SyncedData(真实的是处理了SyncedData的InputDataBase)
        /// </summary>
        /// <param name="syncedData"></param>
        /// <returns></returns>
		public static byte[] Encode(SyncedData[] syncedData)
		{
			SyncedData.bytesToEncode.Clear();
			if (syncedData.Length != 0)
			{
				syncedData[0].GetEncodedHeader(SyncedData.bytesToEncode);//编码第一个syncedData的一些数据
                //编码每个syncedData包括第一个
                for (int i = 0; i < syncedData.Length; i++)
				{
					syncedData[i].GetEncodedActions(SyncedData.bytesToEncode);//执行的就是InputDataBase->Serialize，序列化的是InputData里的字典，字典都转成byte
                }
			}
            //new 相应大小的byte数组返回
			byte[] array = new byte[SyncedData.bytesToEncode.Count];
			int j = 0;
			int num = array.Length;
			while (j < num)
			{
				array[j] = SyncedData.bytesToEncode[j];
				j++;
			}
			return array;
		}

        /// <summary>
        /// 深复制SyncedData
        /// </summary>
        /// <returns></returns>
		public SyncedData clone()
		{
			SyncedData @new = SyncedData.pool.GetNew();
			@new.Init(this.inputData.ownerID, this.tick);
			@new.inputData.CopyFrom(this.inputData);
			return @new;
		}

        /// <summary>
        /// 比较的是SyncedData的inputDataBase是否相等
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
		public bool EqualsData(SyncedData other)
		{
			return this.inputData.EqualsData(other.inputData);
		}

		public void CleanUp()
		{
			this.inputData.CleanUp();
		}
	}
}
