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
        public TSVector2 farstart2;
        public TSVector2 farend2;
        public TSVector2 midPos;
        public TSVector2 normal;
        public TSVector2 edgeDir;
        public int ID;
        public tableEdge(TSVector2 _start, TSVector2 _end, int _ID=-1)
        {
            ID = _ID;
            start = _start;
            end = _end;
            edgeDir = end - start;
            midPos = start+ edgeDir.normalized*edgeDir.magnitude/2;
            var normal3D = (TSQuaternion.AngleAxis(90, TSVector.up) * new TSVector(edgeDir.x, 0, edgeDir.y).normalized);
            normal = new TSVector2(normal3D.x,normal3D.z);
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

        public void Init(TSVector2 _cur_pos, TSVector2 _next_pos, FP _radius)
        {
            cur_pos = _cur_pos;
            next_pos = _next_pos;
            radius = _radius;
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

    public enum HitType
    {
        None,
        Ball,
        Edge,
        Other,
    }

    public class BaseHit
    {
        public BaseHit()
        {
             hitType=HitType.None;
             t_percent=0;
             deltaTime=0;
             valid=true;//碰撞无效
             //process=false;//是否已经更新过了
    }
        public virtual FP CalHitTime()
        {
            return t_percent * deltaTime;
        }
        public virtual int GetMainBallID() { return -1; }
        public HitType hitType;
        public FP t_percent;
        public FP deltaTime;
        public bool valid;//碰撞无效
        public virtual bool Isprocess() { return false; }//是否已经更新过了
        public virtual void TagProcess() { }
    }

    public class fastBall: BaseHit
    {
        public fastBall(BallObj _ball)
        {
            ball = _ball;
            deltaTime = _ball.deltaTime;
            hitType = HitType.None;
        }
        public BallObj ball;
        //public FP deltaTime;
    }

    public class fastHitBall: BaseHit
    {
        public fastHitBall()
        {

        }
        public fastHitBall(BallObj _runballObj,BallObj _staticballObj, FP _t_percent)
        {
            runballObj = _runballObj;
            staticballObj = _staticballObj;
            t_percent = _t_percent;
            hitType = HitType.Ball;
        }

        public void Init(BallObj _runballObj, BallObj _staticballObj, FP _t_percent)
        {
            runballObj = _runballObj;
            staticballObj = _staticballObj;
            t_percent = _t_percent;
            hitType = HitType.Ball;
        }

        public FP CalRunBallHitTime()
        {
            return runballObj.deltaTime * t_percent;
        }
        public FP CalStaticBallHitTime()
        {
            return staticballObj.deltaTime * t_percent;
        }
        public override FP CalHitTime()
        {
            return CalRunBallHitTime();
        }

        public override int GetMainBallID()
        {
            return runballObj.ID;
        }
        public override bool Isprocess()
        {
            return runballObj.lockcheck && staticballObj.lockcheck;
        }
        public override void TagProcess()
        {
            runballObj.lockcheck = true;
            staticballObj.lockcheck = true;
        }
        public BallObj runballObj;
        public BallObj staticballObj;
        //public FP t_percent;
    }

    public class fastEdge: BaseHit
    {
        public fastEdge()
        {

        }
        public fastEdge(BallObj _ball,tableEdge _tbe, FP _t_percent)
        {
            ball = _ball;
            tbe = _tbe;
            t_percent = _t_percent;
            deltaTime = ball.deltaTime;
            hitType = HitType.Edge;
        }

        public void Init(BallObj _ball, tableEdge _tbe, FP _t_percent,TSVector2 _hitNormal)
        {
            ball = _ball;
            tbe = _tbe;
            t_percent = _t_percent;
            deltaTime = ball.deltaTime;
            hitType = HitType.Edge;
            hitNormal = _hitNormal;
        }

        public override int GetMainBallID()
        {
            return ball.ID;
        }
        public override bool Isprocess()
        {
            return ball.lockcheck;
        }
        public override void TagProcess()
        {
            ball.lockcheck = true;
        }
        public BallObj ball;
        public tableEdge tbe;
        public TSVector2 hitNormal;
        //public FP t_percent;
    }

}
