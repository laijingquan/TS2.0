using System;
using System.Collections.Generic;

namespace TrueSync
{
    /// <summary>
    /// ע�����л���ֻ��IputDataBase�����tick
    /// </summary>
	[Serializable]
	public class SyncedData : ResourcePoolItem
	{
        /// <summary>
        /// ����Stack<SyncedData>��һ��pool����ȡ��λ��SyncedData
        /// </summary>
		internal static ResourcePoolSyncedData pool = new ResourcePoolSyncedData();

        /// <summary>
        /// ����Stack<List<SyncedData>>��һ��pool,��ȡ��λ��List<SyncedData>
        /// </summary>
		internal static ResourcePoolListSyncedData poolList = new ResourcePoolListSyncedData();
        /// <summary>
        /// �û���������
        /// </summary>
		public InputDataBase inputData;
        /// <summary>
        /// �߼�֡��
        /// </summary>
		public int tick;

        /// <summary>
        /// fakeӢ������Ϊ:α��
        /// </summary>
		[NonSerialized]
		public bool fake;

		[NonSerialized]
		public bool dirty;

		[NonSerialized]
		public bool dropPlayer;

        /// <summary>
        /// ������˵����localID,����Զ��������˵������Զ�����Id
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
        /// ����ͷ
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
        /// ����ÿһ��SyncedData
        /// </summary>
        /// <param name="bytes"></param>
		public void GetEncodedActions(List<byte> bytes)
		{
			this.inputData.Serialize(bytes);
		}

        /// <summary>
        /// ����ο�����
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
                syncedData.Init(ownerID, tick--);//���ﲻ���ΪɶҪtick--
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
        /// ��Ϣ����:��Ϣͷ+���1��Ϣ��+���2��Ϣ��+...
        /// ��Ϣͷ:tick+OwnerID+drpoFromPlayerId+dropPlayer(��Դ�ڵ�һ��SyncedData)
        /// ��Ϣ��:SyncedData(��ʵ���Ǵ�����SyncedData��InputDataBase)
        /// </summary>
        /// <param name="syncedData"></param>
        /// <returns></returns>
		public static byte[] Encode(SyncedData[] syncedData)
		{
			SyncedData.bytesToEncode.Clear();
			if (syncedData.Length != 0)
			{
				syncedData[0].GetEncodedHeader(SyncedData.bytesToEncode);//�����һ��syncedData��һЩ����
                //����ÿ��syncedData������һ��
                for (int i = 0; i < syncedData.Length; i++)
				{
					syncedData[i].GetEncodedActions(SyncedData.bytesToEncode);//ִ�еľ���InputDataBase->Serialize�����л�����InputData����ֵ䣬�ֵ䶼ת��byte
                }
			}
            //new ��Ӧ��С��byte���鷵��
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
        /// ���SyncedData
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
        /// �Ƚϵ���SyncedData��inputDataBase�Ƿ����
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
