using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace PoolEngine
{
    public class DebugEngine : MonoBehaviour
    {

        // Use this for initialization
        void Start()
        {
            PoolDataTool.Save(new List<BallObj>());
        }

        // Update is called once per frame
        void Update()
        {

        }
    }
}
