using UnityEngine;
using System.Collections;
using TrueSync;
namespace PoolEngine
{
    public class Body
    {
        protected TSVector2 w;
        protected TSVector2 v;
        protected TSVector2 wx;
        protected TSVector2 wy;
        protected TSVector2 vx;
        protected TSVector2 vy;

        protected TSVector2 _w;
        protected TSVector2 _v;
        protected TSVector2 _wx;
        protected TSVector2 _wy;
        protected TSVector2 _vx;
        protected TSVector2 _vy;

        public void Active()
        {

        }

        public void Reset()
        {

        }

        public TSVector2 Move(TSVector2 moveDir)
        {

            return moveDir;
        }
    }
}
