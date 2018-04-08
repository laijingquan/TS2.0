using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using TrueSync;
namespace PoolEngine
{
    public class DebugEngine : MonoBehaviour
    {
        public bool shot;
        public Vector2 shotDir;
        // Use this for initialization
        void Start()
        {
            //PoolDataTool.Save(new List<BallObj>());
        }

        // Update is called once per frame
        void Update()
        {
            if(shot)
            {
                shot = false;
                PoolPhycisEngine.instance.Shot(shotDir.ToTSVector2());
            }
        }
    }
}
