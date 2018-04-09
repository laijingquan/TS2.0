using UnityEngine;
using System.Collections;
using TrueSync;
using System;
using System.Collections.Generic;
using System.Linq;
namespace PoolEngine
{
    public class PoolPhycisEngine
    {
        private static PoolPhycisEngine _instance=null;
        public static PoolPhycisEngine instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new PoolPhycisEngine();
                }
                return _instance;
            }
        }

        public void Awake()
        {
            CreateTable();
            if (debug)
            {
                CreateDebugerBalls();
                DebugUpdatePhysicStep();
            }
            else
                CreateBalls();
            
        }

        void CreateDebugerBalls()
        {
            balls = PoolDataTool.Load();
        }


        void RandCreateBalls()
        {
            TSRandom.Init();
            FP halfx = TableWidth/2-2*radius;
            FP halfy = TableHeight / 2-2*radius;
            for(int i =0;i<30;i++)
            {
                var ball = new BallObj(i, new TSVector2(TSRandom.Range((int)(-halfx), (int)halfx), TSRandom.Range((int)(-halfy), (int)halfy)), new TSVector2(TSRandom.Range(-1, 1), TSRandom.Range(-1, 1)).normalized, TSRandom.Range(1,2), radius);
                //var ball = new BallObj(i, new TSVector2(TSRandom.Range((int)(-halfx), (int)halfx), TSRandom.Range((int)(-halfy), (int)halfy)), radius);
                balls.Add(ball);
                if (ball.ID == 0)
                    mainBall = ball;
            }
        }

        void CreateBalls()
        {
            RandCreateBalls();
            return;
            var ballObj = new BallObj(1, new TSVector2(0, 2), new TSVector2(0, -1).normalized,1,0.5);
            balls.Add(ballObj);

            ballObj = new BallObj(2, new TSVector2(-4, 0), new TSVector2(1, 0).normalized, 3, 0.5);
            balls.Add(ballObj);

            ballObj = new BallObj(3, new TSVector2(4, 0), new TSVector2(-1, 0).normalized, 5, 0.5);
            balls.Add(ballObj);

            ballObj = new BallObj(4, new TSVector2(2, 1), new TSVector2(-0.5, 0.3).normalized, 2, 0.5);
            balls.Add(ballObj);

            ballObj = new BallObj(5, new TSVector2(2, 1), new TSVector2(-0.5, 0.6).normalized, 2, 0.5);
            balls.Add(ballObj);

            ballObj = new BallObj(6, new TSVector2(2, 1), new TSVector2(0.5, 0.3).normalized, 2, 0.5);
            balls.Add(ballObj);

            ballObj = new BallObj(7, new TSVector2(2, 1), new TSVector2(-0.5, -0.3).normalized, 2, 0.5);
            balls.Add(ballObj);

            ballObj = new BallObj(8, new TSVector2(2, 1), new TSVector2(-0.5, -0.3).normalized, 2, 0.5);
            balls.Add(ballObj);

            ballObj = new BallObj(9, new TSVector2(2, 1), new TSVector2(-0.5, -0.3).normalized, 2, 0.5);
            balls.Add(ballObj);

            ballObj = new BallObj(10, new TSVector2(2, 1), new TSVector2(-0.5, -0.3).normalized, 2, 0.5);
            balls.Add(ballObj);

            ballObj = new BallObj(11, new TSVector2(2, 1), new TSVector2(-0.5, -0.3).normalized, 2, 0.5);
            balls.Add(ballObj);

            ballObj = new BallObj(12, new TSVector2(2, 1), new TSVector2(-0.5, -0.3).normalized, 2, 0.5);
            balls.Add(ballObj);

            ballObj = new BallObj(13, new TSVector2(2, 1), new TSVector2(-0.5, -0.3).normalized, 2, 0.5);
            balls.Add(ballObj);

            ballObj = new BallObj(14, new TSVector2(2, 1), new TSVector2(-0.5, -0.3).normalized, 2, 0.5);
            balls.Add(ballObj);

            ballObj = new BallObj(15, new TSVector2(2, 1), new TSVector2(-0.5, -0.3).normalized, 2, 0.5);
            balls.Add(ballObj);

            ballObj = new BallObj(16, new TSVector2(2, 1), new TSVector2(-0.5, -0.3).normalized, 2, 0.5);
            balls.Add(ballObj);

            ballObj = new BallObj(17, new TSVector2(2, 1), new TSVector2(-0.5, -0.3).normalized, 2, 0.5);
            balls.Add(ballObj);

            ballObj = new BallObj(18, new TSVector2(2, 1), new TSVector2(-0.5, -0.3).normalized, 2, 0.5);
            balls.Add(ballObj);

            ballObj = new BallObj(19, new TSVector2(2, 1), new TSVector2(-0.5, -0.3).normalized, 2, 0.5);
            balls.Add(ballObj);

            ballObj = new BallObj(20, new TSVector2(2, 1), new TSVector2(-0.5, -0.3).normalized, 2, 0.5);
            balls.Add(ballObj);

            ballObj = new BallObj(21, new TSVector2(2, 1), new TSVector2(-0.5, -0.3).normalized, 2, 0.5);
            balls.Add(ballObj);

            ballObj = new BallObj(22, new TSVector2(2, 1), new TSVector2(-0.5, -0.3).normalized, 2, 0.5);
            balls.Add(ballObj);

            ballObj = new BallObj(23, new TSVector2(2, 1), new TSVector2(-0.5, -0.3).normalized, 2, 0.5);
            balls.Add(ballObj);

            ballObj = new BallObj(24, new TSVector2(2, 1), new TSVector2(-0.5, -0.3).normalized, 2, 0.5);
            balls.Add(ballObj);
            ballObj = new BallObj(25, new TSVector2(2, 1), new TSVector2(-0.5, -0.3).normalized, 2, 0.5);
            balls.Add(ballObj);

            ballObj = new BallObj(26, new TSVector2(2, 1), new TSVector2(-0.5, -0.3).normalized, 2, 0.5);
            balls.Add(ballObj);
            ballObj = new BallObj(27, new TSVector2(2, 1), new TSVector2(-0.5, -0.3).normalized, 2, 0.5);
            balls.Add(ballObj);

            ballObj = new BallObj(28, new TSVector2(2, 1), new TSVector2(-0.5, -0.3).normalized, 2, 0.5);
            balls.Add(ballObj);
            ballObj = new BallObj(29, new TSVector2(2, 1), new TSVector2(-0.5, -0.3).normalized, 2, 0.5);
            balls.Add(ballObj);

            ballObj = new BallObj(30, new TSVector2(2, 1), new TSVector2(-0.5, -0.3).normalized, 2, 0.5);
            balls.Add(ballObj);
        }
        void CreateTable()
        {

            //上
            //tableEdges[0].start = new TSVector2(-tableWidth/2, tableHeight/2);
            //tableEdges[0].end = new TSVector2(tableWidth/2, tableHeight/2);
            tableEdges[0] = new tableEdge(new TSVector2(-tableWidth / 2, tableHeight / 2), new TSVector2(tableWidth / 2, tableHeight / 2));


            //下
            //tableEdges[1].start = new TSVector2(-tableWidth / 2, -tableHeight / 2);
            //tableEdges[1].end = new TSVector2(tableWidth / 2, -tableHeight / 2);
            tableEdges[1] = new tableEdge(new TSVector2(-tableWidth / 2, -tableHeight / 2), new TSVector2(tableWidth / 2, -tableHeight / 2));

            //左
            //tableEdges[2].start = tableEdges[1].start;
            //tableEdges[2].end = tableEdges[0].start;
            tableEdges[2] = new tableEdge(new TSVector2(-tableWidth / 2, -tableHeight / 2), new TSVector2(-tableWidth / 2, tableHeight / 2));

            //右
            //tableEdges[3].start = tableEdges[1].end;
            //tableEdges[3].end = tableEdges[0].end;
            tableEdges[3] = new tableEdge(new TSVector2(tableWidth / 2, -tableHeight / 2), new TSVector2(tableWidth / 2, tableHeight / 2));
        }

        public void Shot(TSVector2 movedir)
        {
            if(mainBall!=null)
            {
                mainBall.UpdateMoveDir(movedir);
            }
        }


        private bool control = true;
        private FP fixedTime=0;
        private FP stepTime = 0.01;
        // Update is called once per frame
        public void Update(FP deltaTime)
        {
            if (debug)
                return;
            if (IsAllSleep())
                return;
            //UpdatePhysicStep(deltaTime);//逻辑层
            fixedTime += deltaTime;
            while (fixedTime >= stepTime)
            {
                fixedTime -= stepTime;
                if (control)
                    UpdatePhysicStep(stepTime);//逻辑层
            }
        }

        private static int testnumber = 0;
        private List<testData> td = new List<testData>();
        public static Dictionary<int, List<BallObj>> record = new Dictionary<int, List<BallObj>>();

        public static List<BallObj> GetCurrentTestData()
        {
            foreach(var obj in record)
            {
                return obj.Value;
            }
            return null;
        }

        void AddTestData(tableEdge tbg, TSVector2 prepos, TSVector2 hitpos, TSVector2 premoveDir, TSVector2 aftmoveDir)
        {
            td.Add(new testData() { tbg = tbg, prehitPos = prepos, hitpos = hitpos, PremoveDir = premoveDir, AftmoveDir = aftmoveDir });
        }

        public void AddTestData(int step,List<BallObj >objs)
        {
            var temp = new List<BallObj>();
            for (int i = 0; i < objs.Count; i++)
            {
                temp.Add(new BallObj(objs[i]));
            }

            record.Add(step, temp);
        }

        void ClearTestData()
        {
            record.Clear();
        }


        void updateDirAndTimeByEdge(FP _percent, tableEdge _tbe, BallObj ball)
        {
            ball.UpdateBallPos(ball.deltaTime * _percent);//先更新到撞击点
            //ball.deltaTime = ball.deltaTime - ball.deltaTime * _percent;//更新剩余时间
            var curReflcDir = Detection.CheckCircle_LineCollision(_tbe, ball.GetPos(), ball.GetRadius(), ball.GetMoveDir());//计算碰撞响应
            //AddTestData(_tbe, ball.pre_pos, ball.cur_pos, ball.moveDir, curReflcDir);
            //ball.moveDir = curReflcDir;//更新实时方向
            ball.UpdateMoveDir(curReflcDir);//更新实时方向
        }
        void updateDirAndTimeByBall(FP _percent,BallObj runball,BallObj staticball)
        {
            runball.UpdateBallPos(runball.deltaTime * _percent);//先更新到撞击点
            staticball.UpdateBallPos(staticball.deltaTime*_percent);
            //runball.deltaTime  = runball.deltaTime - runball.deltaTime * _percent;//更新剩余时间
            //staticball.deltaTime = staticball.deltaTime - staticball.deltaTime * _percent;

            var runcrd = new CircleRunData(runball.GetPos(), runball.PredictPos(), runball.GetRadius());
            var staticcrd = new CircleRunData(staticball.GetPos(), staticball.PredictPos(), staticball.GetRadius());
            var curReflcDir = Detection.CheckCircle_CircleCollision(runball.GetPos(),runball.GetMoveDir(), staticball.GetPos(), staticball.GetMoveDir());//计算碰撞响应
            //AddTestData(_tbe, ball.pre_pos, ball.cur_pos, ball.moveDir, curReflcDir);
            //runball.moveDir = curReflcDir[0];//更新实时方向
            //staticball.moveDir = curReflcDir[1];
            runball.UpdateMoveDir(curReflcDir[0]);
            staticball.UpdateMoveDir(curReflcDir[1]);
        }

        void checkCollisionBall()
        {

        }

        void checkCollisionEdge()
        {

        }

        private List<BallObj> testData;
        void DebugUpdatePhysicStep()
        {
            UpdatePhysicStep(0.01);
        }

        void UpdatePhysicStep(FP _deltaTime)
        {
            step++;
            AddTestData(step,balls);

            for (int k = 0; k < balls.Count; k++)
            {
                var ball = balls[k];
                ball.deltaTime = _deltaTime;
                ball.lockcheck = false;
            }
            testnumber = 0;
            while (true)
            {
                testnumber++;
                if (testnumber > 100)
                {
                    if(testnumber>200)
                    {
                        Debug.Log("防止死循环");
                        break;
                    }
                }

                for (int k = 0; k < balls.Count; k++)
                {
                    var ball = balls[k];
                    if (ball.deltaTime<=0)
                        continue;
                    var deltaTime = ball.deltaTime;//每个球的剩余的时长是不一样的
                    List<BaseHit> fastHitBalls = new List<BaseHit>();
                    #region 球碰撞检测
                    CircleRunData run_crd = new CircleRunData(ball.GetPos(), ball.PredictPos(), ball.GetRadius());
                    for (int j = 0; j < balls.Count; j++)
                    {
                        var otherball = balls[j];
                        if (otherball == ball) continue;
                        FP _percent = 0;
                        CircleRunData static_crd = new CircleRunData(otherball.GetPos(), otherball.PredictPos(), otherball.GetRadius());
                        if (Detection.CheckCircle_CircleContact(run_crd, static_crd, ball.deltaTime, ref _percent))
                        {
                            fastHitBalls.Add(new fastHitBall(ball, otherball, _percent));
                        }
                    }

                    if (fastHitBalls.Count > 0)
                    {
                        CollectBallPairList(fastHitBalls);
                    }

                    #endregion
                    #region 边检测
                    FP t_percent = 0;

                    TSVector2 predictEndPos = ball.GetPos() + ball.GetMoveDir()*100;

                    //bool isflag = false;
                    List<BaseHit> fastedges = new List<BaseHit>();
                    //在当前速度下,预测圆最先和哪条边碰撞
                    for (int i = 0; i < tableEdges.Length; i++)
                    {
                        //if (Detection.CheckSegement_Contact(ball.cur_pos, predictEndPos, tableEdges[i].farstart, tableEdges[i].farend))//这个检测是去掉在挨着边但运动方向相反的情况
                        if(Detection.CheckCloseEdge(tableEdges[i].start,tableEdges[i].end,ball.GetPos(),predictEndPos))
                        {

                            if (Detection.CheckCircle_LineContact(tableEdges[i], run_crd, ref t_percent))
                            {
                                fastedges.Add(new fastEdge(ball,tableEdges[i], t_percent));
                            }
                       }
                    }
                    //如果和边和球都有碰撞集合,找到最先的碰撞点
                    if (fastedges.Count > 0)
                    {
                        CollectBallPairList(fastedges);
                    }
                    fastHitBalls.Clear();
                    fastedges.Clear();
                }
                #endregion
                if(ballPairHit.Count>0)
                    ProcessHitData();
                else
                {
                    //没有碰撞了,检查所有球是否有剩余时间，直接走完跳出
                    for (int i = 0; i < balls.Count; i++)
                    {
                        var nothitBall = balls[i];
                        if (nothitBall.deltaTime > 0)
                            nothitBall.UpdateBallPos(nothitBall.deltaTime);
                    }
                    break;
                }
            }

            ClearTestData();
        }

        public bool IsAllSleep()
        {
            for (int k = 0; k < balls.Count; k++)
            {
                if (!balls[k].IsSleep())
                    return false;
            }
            return true;
        }

        void UnLockBalls()
        {
            for (int k = 0; k < balls.Count; k++)
            {
                var ball = balls[k];
                ball.lockcheck = false;
            }
        }
        void CollectBallPairList(List<BaseHit> baseHitList)
        {
            for(int i =0; i < baseHitList.Count;i++)
            {
                CollectBallPairSingle(baseHitList[i]);
            }
        }

        void CollectBallPairSingle(BaseHit baseHit)
        {
            if(baseHit.hitType==HitType.Ball)
            {
                var _baseHit = baseHit as fastHitBall;
                if(ballPairHit.ContainsKey(_baseHit.runballObj.ID))
                {
                    ballPairHit[_baseHit.runballObj.ID].Add(_baseHit);
                }
                else
                {
                    ballPairHit.Add(_baseHit.runballObj.ID, new List<BaseHit>() { baseHit});
                }
            }
            else if(baseHit.hitType==HitType.Edge)
            {
                var _baseHit = baseHit as fastEdge;
                if (ballPairHit.ContainsKey(_baseHit.ball.ID))
                {
                    ballPairHit[_baseHit.ball.ID].Add(_baseHit);
                }
                else
                {
                    ballPairHit.Add(_baseHit.ball.ID, new List<BaseHit>() { baseHit });
                }
            }
        }
        /// <summary>
        /// 处理所有的碰撞记录
        /// </summary>
        void ProcessHitData()
        {
            foreach(var ballpair in ballPairHit)//每个球对应的碰撞记录集合
            {
                int ID = ballpair.Key;
                var baseHits = ballpair.Value;
                for(int i =0; i < baseHits.Count;i++)//其中一个球的碰撞记录集合
                {
                    var baseHit = baseHits[i];
                    if(baseHit.hitType==HitType.Ball)
                    {
                        var _baseHit = baseHit as fastHitBall;
                        int otherID = _baseHit.staticballObj.ID;
                        if (ballPairHit.ContainsKey(otherID))
                        {
                            var other_baseHits = ballPairHit[otherID];
                            for(int j = 0; j<other_baseHits.Count;j++)
                            {
                                var otherHit = other_baseHits[j];
                                if(otherHit.hitType==HitType.Ball)
                                {
                                    var _otherHit = otherHit as fastHitBall;
                                    if (_baseHit.runballObj.ID == _otherHit.staticballObj.ID)
                                    {
                                        continue;
                                    }
                                    else
                                    {
                                        if (/*_baseHit.CalHitTime()*/_baseHit.t_percent * _baseHit.staticballObj.deltaTime > _otherHit.CalHitTime())
                                        {
                                            _baseHit.valid = false;
                                        }
                                        else
                                            _otherHit.valid = false;
                                    }
                                }
                                else
                                {
                                    var _otherHit = otherHit as fastEdge;
                                    if (/*_baseHit.CalHitTime()*/ _baseHit.t_percent * _baseHit.staticballObj.deltaTime > _otherHit.CalHitTime())
                                    {
                                        _baseHit.valid = false;
                                    }
                                    else
                                        _otherHit.valid = false;
                                }
                            }
                        }
                        else
                            Debug.Log("一般碰撞对都是成对记录出现,不可能走到这!");
                    }                   
                }
            }

            bool isFlag = false;
            foreach (var ballpair in ballPairHit)
            {
                int ID = ballpair.Key;
                var baseHits = ballpair.Value.OrderBy((m)=>m.CalHitTime()).ToList();//按时间来排序

                BaseHit baseHit=null;
                for(int i = 0; i < baseHits.Count;i++)
                {
                    if (baseHits[i].valid == false) continue;
                    baseHit = baseHits[i];
                    break;
                }


                if (baseHit!=null&&baseHit.valid == true && !baseHit.Isprocess())
                {
                    isFlag = true;
                    baseHit.TagProcess();
                    if (baseHit.hitType == HitType.Ball)
                    {
                        var _baseHit = baseHit as fastHitBall;
                        _baseHit.runballObj.CalBallPos(_baseHit.t_percent * _baseHit.runballObj.deltaTime);
                        _baseHit.staticballObj.CalBallPos(_baseHit.t_percent * _baseHit.staticballObj.deltaTime);
                        updateDirAndTimeByBall(_baseHit.t_percent, _baseHit.runballObj, _baseHit.staticballObj);
                    }
                    else if (baseHit.hitType == HitType.Edge)
                    {
                        var _baseHit = baseHit as fastEdge;
                        _baseHit.ball.CalBallPos(_baseHit.t_percent * _baseHit.ball.deltaTime);
                        updateDirAndTimeByEdge(_baseHit.t_percent, _baseHit.tbe, _baseHit.ball);
                    }
                }
            }

            if(isFlag==false)
            {
                Debug.Log("虽然有碰撞记录,但是没有执行碰撞,这里可能会导致死循环的情况,因为一直检测到有碰撞,却不执行");
            }
            UnLockBalls();
            ballPairHit.Clear();
        }

        public bool CheckBound(TSVector2 pos)
        {
            var x = TSMath.Abs(pos.x);
            var y = TSMath.Abs(pos.y);
            if (x+TSMath.Epsilon>tableWidth/2.0||y+TSMath.Epsilon>TableHeight/2.0)
            {
                return true;
            }
            return false;
        }
        #region 事件
        public Action OnBeforStartRound;
        public Action OnStartRound;
        public Action OnAfterStartRound;
        public Action OnNextRound;
        public Action OnShot;
        #endregion
        #region 数据
        private int step = 0;
        private bool debug = false;
        private tableEdge[] tableEdges = new tableEdge[4];
        private  FP tableWidth = 10;
        private   FP tableHeight = 5;
        private FP radius = 0.5;
        private BallObj mainBall;
        private List<BallObj> balls = new List<BallObj>();
        private Dictionary<int, BallObj> ballsDict = new Dictionary<int, BallObj>();

        private Dictionary<int, List<BaseHit>> ballPairHit = new Dictionary<int, List<BaseHit>>();

        public FP TableWidth
        {
            get
            {
                return tableWidth;
            }
        }

        public FP TableHeight
        {
            get
            {
                return tableHeight;
            }
        }

        public bool EngineDebug
        {
            set
            {
                debug = value;
            }
            get
            {
                return debug;
            }
        }


        public tableEdge[] TableEdges
        {
            get
            {
                return tableEdges;
            }
        }

        public List<BallObj> Ball
        {
            get
            {
                return balls;
            }
        } 
    }
    #endregion
}
