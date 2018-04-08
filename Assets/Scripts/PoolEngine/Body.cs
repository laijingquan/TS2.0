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

        protected FP decreaseV;

        //protected TSVector2 _w;
        //protected TSVector2 _v;
        //protected TSVector2 _wx;
        //protected TSVector2 _wy;
        //protected TSVector2 _vx;
        //protected TSVector2 _vy;

        protected TSVector2 cur_pos;

        protected FP airDrag=0.05;//影响线速度[0~1]
        protected FP angularDrag=0.2;//影响角速度[0~1]

        protected bool isSleep = false;

        public Body(TSVector2 _cur_pos)
        {
            cur_pos = _cur_pos;
        }

        public Body(TSVector2 _cur_pos, TSVector2 _v, TSVector2 _w)
        {
            Init(_cur_pos, _v, _w);
        }

        public void Init(TSVector2 _cur_pos, TSVector2 _v, TSVector2 _w)
        {
            v = _v;
            w = _w;
            cur_pos = _cur_pos;
        }

        protected void Active(TSVector2 _v, TSVector2 _w)
        {
            v = _v;
            w = _w;
            decreaseV = v.magnitude * airDrag;//线速度固定的衰减
            isSleep = false;
        }

        public void Sleep()
        {
            isSleep = true;
        }

        public bool IsSleep()
        {
            return isSleep;
        }

        public void Reset()
        {

        }

        /// <summary>
        /// 线速度衰减
        /// </summary>
        protected TSVector2 LinearSpeedReduce(FP deltatime)
        {
            //var decreaseV = v.magnitude - v.magnitude * airDrag;
            //var speed = v.normalized * decreaseV;
            //if (speed.magnitude<=0)
            //    speed = TSVector2.zero;
            //return speed;
            var _decreaseV = v.magnitude - decreaseV * deltatime;//线速度的衰减值是恒定的
            if (v.magnitude <= 0 || _decreaseV<=0)
            {
                v = TSVector2.zero;
                Sleep();
            }
            else
                v = v.normalized * _decreaseV;
            return v*deltatime;
        }
        /// <summary>
        /// 角速度衰减
        /// </summary>
        protected void AngularSpeedReduce()
        {

        }

        public TSVector2 NextPos(FP deltatime)
        {
            //v = LinearSpeedReduce(deltatime);
            cur_pos += LinearSpeedReduce(deltatime);
            return cur_pos;
        }

        public TSVector2 PredictNextPos(FP deltatime)
        {
            return cur_pos + LinearSpeedReduce(deltatime);
        }

        public TSVector2 UpdateMoveDir(TSVector2 _moveDir)
        {
            v = _moveDir;
            Active(_moveDir,TSVector2.zero);
            return v;
        }

        public TSVector2 GetMoveDir()
        {
            return v;
        }

        public TSVector2 GetPos()
        {
            return cur_pos;
        }
    }
}
