using UnityEngine;
using System.Collections;
using TrueSync;
using System.Collections.Generic;

namespace PoolEngine
{

    public class tableEdge
    {
        public TSVector2 start;
        public TSVector2 end;
        public TSVector2 farstart;
        public TSVector2 farend;
        public tableEdge(TSVector2 _start, TSVector2 _end)
        {
            start = _start;
            end = _end;
            var dir = end - start;
            farstart = start - dir * 100;
            farend = end + dir * 100;
        }
    }

    //public class PoolCollision
    //{
    //    public TSVector2 point;
    //    public TSVector2 normal;
    //}

    /// <summary>
    /// 动态数据，表示圆正在运动的数据。当前速度和位置，下一帧的位置
    /// </summary>
    public class CircleRunData
    {
        public CircleRunData(TSVector2 _cur_pos, TSVector2 _next_pos, FP _radius)
        {
            cur_pos = _cur_pos;
            next_pos = _next_pos;
            radius = _radius;
        }
        public CircleRunData()
        {

        }
        public TSVector2 cur_pos;
        public TSVector2 next_pos;
        public FP radius;
    }

    /// <summary>
    /// 静态圆表示
    /// </summary>
    public class CircleData
    {
        public TSVector2 pre_pos;//上次真是停留的位置
                                 //public TSVector2 predict_pos;//基于pre_pos和速度预测的位置
        public TSVector2 cur_pos;//基于碰撞检测,当前停留的真实位置
        public FP radius;
    }

    public class testData
    {
        public tableEdge tbg;
        public TSVector2 prehitPos;//记录撞击前的位置
        public TSVector2 hitpos;//记录撞击的位置
        public TSVector2 PremoveDir;
        public TSVector2 AftmoveDir;
        public BallObj ball;
    }

    public class fastEdgeCompare : Comparer<fastEdge>
    {
        public override int Compare(fastEdge x, fastEdge y)
        {
            if (x.t_percent > y.t_percent)
            {
                return 1;
            }
            else
            {
                return -1;
            }
        }
    }

    public class fastHitBall
    {
        public fastHitBall(BallObj _runballObj,BallObj _staticballObj, FP _t_percent)
        {
            runballObj = _runballObj;
            staticballObj = _staticballObj;
            t_percent = _t_percent;
        }
        public BallObj runballObj;
        public BallObj staticballObj;
        public FP t_percent;
    }

    public class fastEdge
    {
        public fastEdge(tableEdge _tbe, FP _t_percent)
        {
            tbe = _tbe;
            t_percent = _t_percent;
        }
        public tableEdge tbe;
        public FP t_percent;
    }

}
