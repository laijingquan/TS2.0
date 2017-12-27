using System;
using System.Collections.Generic;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;

namespace TrueSync
{
    /// <summary>
    /// 尾端放在地地址：底尾端=小端
    /// </summary>
	public class Utils
	{
        /// <summary>
        /// 获取该Type的字段和属性
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
        /// int32转成小端
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
        /// long64转成小端
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
        /// C# ToString("x2")的理解
        ///1).转化为16进制。
        ///2).大写X:ToString("X2")即转化为大写的16进制。
        ///3).小写x:ToString("x2")即转化为小写的16进制。
        ///4).2表示输出两位，不足的2位的前面补0,如 0x0A 如果没有2,就只会输出0xA
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static string GetMd5Sum(string str)
		{
			Encoder encoder = Encoding.Unicode.GetEncoder();
			byte[] array = new byte[str.Length * 2];//因为Unicode编码固定为一个字符需要2个字节,所以这里需要2倍容量
			encoder.GetBytes(str.ToCharArray(), 0, str.Length, array, 0, true);//将字符串转成byte赋给array
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
