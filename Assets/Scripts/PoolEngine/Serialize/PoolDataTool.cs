using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using TrueSync;
namespace PoolEngine
{
    public class PoolData : ScriptableObject
    {
        public List<BallObj> datas;
    }

    public class PoolDataTool
    {
        public const string path = "Assets/Resources/PoolEngine/Serialize/poolData.asset";

        public static void Save(List<BallObj> _datas)
        {
            var poolData = ScriptableObject.CreateInstance<PoolData>();
            foreach(var data in _datas)
            {
                data.DoSerialize();
            }
            poolData.datas = _datas;
            UnityEditor.AssetDatabase.CreateAsset(poolData, path);
            UnityEditor.EditorApplication.isPaused = true;
        }

        public static List<BallObj> Load()
        {
            var poolData = UnityEditor.AssetDatabase.LoadAssetAtPath<PoolData>(path);
            foreach (var data in poolData.datas)
            {
                data.DoDeserialize();
            }
            return poolData.datas;
        }
    }
}
