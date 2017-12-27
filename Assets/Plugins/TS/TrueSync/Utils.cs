using System;
using System.Collections.Generic;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;

namespace TrueSync
{
    /// <summary>
    /// β�˷��ڵص�ַ����β��=С��
    /// </summary>
	public class Utils
	{
        /// <summary>
        /// ��ȡ��Type���ֶκ�����
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
		public static List<MemberInfo> GetMembersInfo(Type type)
		{
			List<MemberInfo> list = new List<MemberInfo>();
			list.AddRange(type.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic));
			list.AddRange(type.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic));
			return list;
		}

        /// <summary>
        /// int32ת��С��
        /// </summary>
        /// <param name="value"></param>
        /// <param name="bytes"></param>
		public static void GetBytes(int value, List<byte> bytes)
		{
			bytes.Add((byte)value);
			bytes.Add((byte)(value >> 8));
			bytes.Add((byte)(value >> 16));
			bytes.Add((byte)(value >> 24));
		}

        /// <summary>
        /// long64ת��С��
        /// </summary>
        /// <param name="value"></param>
        /// <param name="bytes"></param>
		public static void GetBytes(long value, List<byte> bytes)
		{
			bytes.Add((byte)value);
			bytes.Add((byte)(value >> 8));
			bytes.Add((byte)(value >> 16));
			bytes.Add((byte)(value >> 24));
			bytes.Add((byte)(value >> 32));
			bytes.Add((byte)(value >> 40));
			bytes.Add((byte)(value >> 48));
			bytes.Add((byte)(value >> 56));
        }
        /// <summary>
        /// C# ToString("x2")�����
        ///1).ת��Ϊ16���ơ�
        ///2).��дX:ToString("X2")��ת��Ϊ��д��16���ơ�
        ///3).Сдx:ToString("x2")��ת��ΪСд��16���ơ�
        ///4).2��ʾ�����λ�������2λ��ǰ�油0,�� 0x0A ���û��2,��ֻ�����0xA
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static string GetMd5Sum(string str)
		{
			Encoder encoder = Encoding.Unicode.GetEncoder();
			byte[] array = new byte[str.Length * 2];//��ΪUnicode����̶�Ϊһ���ַ���Ҫ2���ֽ�,����������Ҫ2������
			encoder.GetBytes(str.ToCharArray(), 0, str.Length, array, 0, true);//���ַ���ת��byte����array
			MD5 mD = new MD5CryptoServiceProvider();
			byte[] array2 = mD.ComputeHash(array);
			StringBuilder stringBuilder = new StringBuilder();
			for (int i = 0; i < array2.Length; i++)
			{
				stringBuilder.Append(array2[i].ToString("X2"));
			}
			return stringBuilder.ToString();
		}
	}
}
