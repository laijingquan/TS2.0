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
        ObjPool<CircleRunData> run_crdPool;
        List<BaseHit> baseHits;
        ObjPool<fastHitBall> fastHitBallPool;
        ObjPool<fastEdge> fastEdgePool;
        public void Init()
        {
            run_crdPool = new ObjPool<CircleRunData>(10);
            fastHitBallPool = new ObjPool<fastHitBall>(20);
            fastEdgePool = new ObjPool<fastEdge>(20);
            baseHits = new List<BaseHit>();
        }

        public void Awake()
        {
            Init();
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
            FP halfx = TableWidth/2-2*radius-5;
            FP halfy = TableHeight / 2-2*radius-5;
            for(int i =0;i<20;i++)
            {
                var ball = new BallObj(i, new TSVector2(TSRandom.Range((int)(-halfx), (int)halfx), TSRandom.Range((int)(-halfy), (int)halfy)), new TSVector2(TSRandom.Range(-1, 1), TSRandom.Range(-1, 1)).normalized, TSRandom.Range(10,100), radius);
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
            var ballObj = new BallObj(1, new TSVector2(tableWidth/2+0.4, 0), new TSVector2(0, 1).normalized,1,0.5);
            balls.Add(ballObj);

            //ballObj = new BallObj(2, new TSVector2(-4, 0), new TSVector2(1, 0).normalized, 3, 0.5);
            //balls.Add(ballObj);

            //ballObj = new BallObj(3, new TSVector2(4, 0), new TSVector2(-1, 0).normalized, 5, 0.5);
            //balls.Add(ballObj);

            //ballObj = new BallObj(4, new TSVector2(2, 1), new TSVector2(-0.5, 0.3).normalized, 2, 0.5);
            //balls.Add(ballObj);

            //ballObj = new BallObj(5, new TSVector2(2, 1), new TSVector2(-0.5, 0.6).normalized, 2, 0.5);
            //balls.Add(ballObj);

            //ballObj = new BallObj(6, new TSVector2(2, 1), new TSVector2(0.5, 0.3).normalized, 2, 0.5);
            //balls.Add(ballObj);

            //ballObj = new BallObj(7, new TSVector2(2, 1), new TSVector2(-0.5, -0.3).normalized, 2, 0.5);
            //balls.Add(ballObj);

            //ballObj = new BallObj(8, new TSVector2(2, 1), new TSVector2(-0.5, -0.3).normalized, 2, 0.5);
            //balls.Add(ballObj);

            //ballObj = new BallObj(9, new TSVector2(2, 1), new TSVector2(-0.5, -0.3).normalized, 2, 0.5);
            //balls.Add(ballObj);

            //ballObj = new BallObj(10, new TSVector2(2, 1), new TSVector2(-0.5, -0.3).normalized, 2, 0.5);
            //balls.Add(ballObj);

            //ballObj = new BallObj(11, new TSVector2(2, 1), new TSVector2(-0.5, -0.3).normalized, 2, 0.5);
            //balls.Add(ballObj);

            //ballObj = new BallObj(12, new TSVector2(2, 1), new TSVector2(-0.5, -0.3).normalized, 2, 0.5);
            //balls.Add(ballObj);

            //ballObj = new BallObj(13, new TSVector2(2, 1), new TSVector2(-0.5, -0.3).normalized, 2, 0.5);
            //balls.Add(ballObj);

            //ballObj = new BallObj(14, new TSVector2(2, 1), new TSVector2(-0.5, -0.3).normalized, 2, 0.5);
            //balls.Add(ballObj);

            //ballObj = new BallObj(15, new TSVector2(2, 1), new TSVector2(-0.5, -0.3).normalized, 2, 0.5);
            //balls.Add(ballObj);

            //ballObj = new BallObj(16, new TSVector2(2, 1), new TSVector2(-0.5, -0.3).normalized, 2, 0.5);
            //balls.Add(ballObj);

            //ballObj = new BallObj(17, new TSVector2(2, 1), new TSVector2(-0.5, -0.3).normalized, 2, 0.5);
            //balls.Add(ballObj);

            //ballObj = new BallObj(18, new TSVector2(2, 1), new TSVector2(-0.5, -0.3).normalized, 2, 0.5);
            //balls.Add(ballObj);

            //ballObj = new BallObj(19, new TSVector2(2, 1), new TSVector2(-0.5, -0.3).normalized, 2, 0.5);
            //balls.Add(ballObj);

            //ballObj = new BallObj(20, new TSVector2(2, 1), new TSVector2(-0.5, -0.3).normalized, 2, 0.5);
            //balls.Add(ballObj);

            //ballObj = new BallObj(21, new TSVector2(2, 1), new TSVector2(-0.5, -0.3).normalized, 2, 0.5);
            //balls.Add(ballObj);

            //ballObj = new BallObj(22, new TSVector2(2, 1), new TSVector2(-0.5, -0.3).normalized, 2, 0.5);
            //balls.Add(ballObj);

            //ballObj = new BallObj(23, new TSVector2(2, 1), new TSVector2(-0.5, -0.3).normalized, 2, 0.5);
            //balls.Add(ballObj);

            //ballObj = new BallObj(24, new TSVector2(2, 1), new TSVector2(-0.5, -0.3).normalized, 2, 0.5);
            //balls.Add(ballObj);
            //ballObj = new BallObj(25, new TSVector2(2, 1), new TSVector2(-0.5, -0.3).normalized, 2, 0.5);
            //balls.Add(ballObj);

            //ballObj = new BallObj(26, new TSVector2(2, 1), new TSVector2(-0.5, -0.3).normalized, 2, 0.5);
            //balls.Add(ballObj);
            //ballObj = new BallObj(27, new TSVector2(2, 1), new TSVector2(-0.5, -0.3).normalized, 2, 0.5);
            //balls.Add(ballObj);

            //ballObj = new BallObj(28, new TSVector2(2, 1), new TSVector2(-0.5, -0.3).normalized, 2, 0.5);
            //balls.Add(ballObj);
            //ballObj = new BallObj(29, new TSVector2(2, 1), new TSVector2(-0.5, -0.3).normalized, 2, 0.5);
            //balls.Add(ballObj);

            //ballObj = new BallObj(30, new TSVector2(2, 1), new TSVector2(-0.5, -0.3).normalized, 2, 0.5);
            //balls.Add(ballObj);
        }
        void CreateTable2()
        {
            int big = 99999;
            tableWidth = 0;
            tableHeight = 0;
            int factor = 10;

            FP[] edges = new FP[] {-1.39,0.46,-1.44,0.53,   -1.36,0.63,-1.31,0.59,  -0.08,0.59,-0.07,0.64,  0.07,0.64,0.08,0.59,    1.31,0.59,1.36,0.63,    1.44,0.53,1.39,0.46,
                                                 1.39,-0.46,1.44,-0.53,1.36,    -0.63,1.31,-0.59,0.08,  -0.59,0.07,-0.64,-0.07, -0.64,-0.08,-0.59,-1.31,    -0.59,-1.36,-0.63,-1.44,    -0.53,-1.39,-0.46 ,-1.39,0.46};
            for(int j =0,i=0;j<edges.Length-2;i++)
            {
                tableEdges.Add(new tableEdge(new TSVector2(edges[j]* factor, edges[j+1]* factor), new TSVector2(edges[j+2]* factor, edges[j+3]* factor),i));
                var w = TSMath.Abs(edges[j] * factor);
                var h = TSMath.Abs(edges[j + 1] * factor);
                if (tableWidth<w)
                {
                    tableWidth = w;
                }
                if(tableHeight<h)
                {
                    tableHeight = h;
                }
                w = TSMath.Abs(edges[j+2] * factor);
                h = TSMath.Abs(edges[j + 3] * factor);
                if (tableWidth < w)
                {
                    tableWidth = w;
                }
                if (tableHeight < h)
                {
                    tableHeight = h;
                }
                j += 2;
            }
            tableWidth *= 2;
            tableHeight *= 2;
        }
        void CreateTable()
        {
            CreateTable2(); return;
            //上
            //tableEdges[0].start = new TSVector2(-tableWidth/2, tableHeight/2);
            //tableEdges[0].end = new TSVector2(tableWidth/2, tableHeight/2);
            tableEdges.Add( new tableEdge(new TSVector2(-tableWidth / 2, tableHeight / 2), new TSVector2(tableWidth / 2, tableHeight / 2)));


            //下
            //tableEdges[1].start = new TSVector2(-tableWidth / 2, -tableHeight / 2);
            //tableEdges[1].end = new TSVector2(tableWidth / 2, -tableHeight / 2);
            tableEdges.Add ( new tableEdge(new TSVector2(tableWidth / 2, -tableHeight / 2), new TSVector2(-tableWidth / 2, -tableHeight / 2)));

            //左
            //tableEdges[2].start = tableEdges[1].start;
            //tableEdges[2].end = tableEdges[0].start;
            tableEdges.Add( new tableEdge(new TSVector2(-tableWidth / 2, -tableHeight / 2), new TSVector2(-tableWidth / 2, tableHeight / 2)));

            //右
            //tableEdges[3].start = tableEdges[1].end;
            //tableEdges[3].end = tableEdges[0].end;
            tableEdges.Add(new tableEdge(new TSVector2(tableWidth / 2, tableHeight / 2), new TSVector2(tableWidth / 2, -tableHeight / 2)));
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

        void _updateDirAndTimeByEdge(FP _percent, TSVector2 hitNormal, BallObj ball, FP _deltaTime)
        {
            ball.UpdateBallPos(_deltaTime * _percent);//先更新到撞击点
            var curReflcDir = Detection.CheckCircle_EdgeCollision(hitNormal, ball.GetMoveDir());//计算碰撞响应
            ball.UpdateMoveDir(curReflcDir);//更新实时方向
        }

        void updateDirAndTimeByEdge(FP _percent, tableEdge _tbe, BallObj ball,FP _deltaTime)
        {
            ball.UpdateBallPos(_deltaTime * _percent);//先更新到撞击点
            var curReflcDir = Detection.CheckCircle_LineCollision(_tbe, ball.GetPos(), ball.GetRadius(), ball.GetMoveDir());//计算碰撞响应
            ball.UpdateMoveDir(curReflcDir);//更新实时方向
        }
        void updateDirAndTimeByBall(FP _percent,BallObj runball,BallObj staticball, FP _deltaTime)
        {
            runball.UpdateBallPos(_deltaTime * _percent);//先更新到撞击点
            staticball.UpdateBallPos(_deltaTime * _percent);


            //var runcrd = new CircleRunData(runball.GetPos(), runball.PredictPos(_deltaTime), runball.GetRadius());
            //var staticcrd = new CircleRunData(staticball.GetPos(), staticball.PredictPos(_deltaTime), staticball.GetRadius());
            var curReflcDir = Detection.CheckCircle_CircleCollision(runball.GetPos(),runball.GetMoveDir(), staticball.GetPos(), staticball.GetMoveDir());//计算碰撞响应

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
            //AddTestData(step,balls);

            for (int k = 0; k < balls.Count; k++)
            {
                var ball = balls[k];
                ball.deltaTime = _deltaTime;
                ball.lockcheck = false;
            }
            testnumber = 0;

            FP leftSyncTime = _deltaTime;
            while(true)
            {
                testnumber++;
                if (testnumber > 100)
                {
                    if (testnumber > 200)
                    {
                        Debug.Log("防止死循环");
                        break;
                    }
                }
                //List<BaseHit> baseHits = new List<BaseHit>();
                baseHits.Clear();
                FP _percent = 0;
                for (int i =0;i<balls.Count-1;i++)
                {
                    var ball = balls[i];
                    //CircleRunData run_crd = new CircleRunData(ball.GetPos(), ball.PredictPos(leftSyncTime), ball.GetRadius());
                    CircleRunData run_crd = run_crdPool.New();
                    run_crd.Init(ball.GetPos(), ball.PredictPos(leftSyncTime), ball.GetRadius());
                    for (int j =i+1;j<balls.Count;j++)
                    {
                        var otherball = balls[j];
                        //CircleRunData static_crd = new CircleRunData(otherball.GetPos(), otherball.PredictPos(leftSyncTime), otherball.GetRadius());
                        CircleRunData static_crd = run_crdPool.New();
                        static_crd.Init(otherball.GetPos(), otherball.PredictPos(leftSyncTime), otherball.GetRadius());
                        if (Detection.CheckCircle_CircleContact(run_crd, static_crd, ball.deltaTime, ref _percent))
                        {
                            var obj = fastHitBallPool.New();
                            obj.Init(ball, otherball, _percent);
                            baseHits.Add(obj);
                            //baseHits.Add(new fastHitBall(ball, otherball, _percent));
                        }
                    }
                }

                for(int ii=0;ii<balls.Count;ii++)
                {
                    var ball = balls[ii];
                    TSVector2 predictEndPos = ball.GetPos() + ball.GetMoveDir() * 100;
                    //CircleRunData run_crd = new CircleRunData(ball.GetPos(), ball.PredictPos(leftSyncTime), ball.GetRadius());
                    CircleRunData run_crd = run_crdPool.New();
                    run_crd.Init(ball.GetPos(), ball.PredictPos(leftSyncTime), ball.GetRadius());
                    //在当前速度下,预测圆最先和哪条边碰撞
                    for (int jj = 0; jj < tableEdges.Count; jj++)
                    {
                        TSVector2 predictCirclePos = TSVector2.zero;
                        if (Detection.CheckCloseSegement(tableEdges[jj],ball.moveDir))
                        //if (Detection.CheckCloseEdge(tableEdges[jj].start, tableEdges[jj].end, ball.GetPos(), predictEndPos))
                        {
                            //预测是否和边所在平面碰撞
                            if (Detection.CheckCircle_LineContact(tableEdges[jj], run_crd, ref _percent) /*|| Detection.CheckCircle_tableEdgeEndContact(run_crd, tableEdges[jj], ref _percent)*/)
                            {
                                predictCirclePos = ball.PredictPos(leftSyncTime * _percent);
                                if (Detection.CheckCircle_SegementContact(predictCirclePos, tableEdges[jj], ball.radius))//然后检测离真实线段最近的点是否符合要求
                                {
                                    var obj = fastEdgePool.New();
                                    obj.Init(ball, tableEdges[jj], _percent, tableEdges[jj].normal);
                                    baseHits.Add(obj);
                                }
                                else
                                    _percent = 0;
                            }
                            else
                            {
                                //TSVector2 nearestPos = TSVector2.zero;
                                //if(Detection.CheckCircle_tableEdgeEndContact(run_crd, tableEdges[jj], ref _percent,ref nearestPos))//检测是否和线段的端点产生了碰撞
                            }
                        }

                        if (Detection._CheckCircle_tableEdgeEndContact(run_crd, tableEdges[jj].start, ref _percent))//检测是否和线段的左端点产生了碰撞
                        {
                            predictCirclePos = ball.PredictPos(leftSyncTime * _percent);
                            if (Detection.CheckCircle_SegementContact(predictCirclePos, tableEdges[jj], ball.radius))//然后检测离真实线段最近的点是否符合要求
                            {
                                var obj = fastEdgePool.New();
                                obj.Init(ball, tableEdges[jj], _percent, (predictCirclePos - tableEdges[jj].start).normalized);
                                baseHits.Add(obj);
                            }
                            else
                                _percent = 0;
                        }
                        else if (Detection._CheckCircle_tableEdgeEndContact(run_crd, tableEdges[jj].end, ref _percent))//检测是否和线段的右端点产生了碰撞
                        {
                            predictCirclePos = ball.PredictPos(leftSyncTime * _percent);
                            if (Detection.CheckCircle_SegementContact(predictCirclePos, tableEdges[jj], ball.radius))//然后检测离真实线段最近的点是否符合要求
                            {
                                var obj = fastEdgePool.New();
                                obj.Init(ball, tableEdges[jj], _percent, (predictCirclePos - tableEdges[jj].end).normalized);
                                baseHits.Add(obj);
                            }
                            else
                                _percent = 0;
                        }
                        else
                            _percent = 0;
                    }
                }



                if(baseHits.Count>0)
                {
                    var closedHit = baseHits.OrderBy((m) => m.t_percent).First() ;
                    closedHit.TagProcess();
                    var syncTime = closedHit.t_percent * leftSyncTime;
                    leftSyncTime -= syncTime;
                    if (leftSyncTime <= 0)
                        leftSyncTime = 0;
                    if (closedHit.hitType == HitType.Ball)
                    {
                        var _baseHit = closedHit as fastHitBall;
                        //_baseHit.runballObj.CalBallPos(_baseHit.t_percent * _baseHit.runballObj.deltaTime);
                        //_baseHit.staticballObj.CalBallPos(_baseHit.t_percent * _baseHit.staticballObj.deltaTime);
                        updateDirAndTimeByBall(_baseHit.t_percent, _baseHit.runballObj, _baseHit.staticballObj, syncTime);
                    }
                    else if (closedHit.hitType == HitType.Edge)
                    {
                        var _baseHit = closedHit as fastEdge;
                        //_baseHit.ball.CalBallPos(_baseHit.t_percent * _baseHit.ball.deltaTime);
                        //updateDirAndTimeByEdge(_baseHit.t_percent, _baseHit.tbe, _baseHit.ball, syncTime);
                        _updateDirAndTimeByEdge(_baseHit.t_percent, _baseHit.hitNormal, _baseHit.ball, syncTime);
                    }
                    for (int m = 0; m < balls.Count; m++)
                    {
                        var nothitBall = balls[m];
                        if (nothitBall.lockcheck==false)
                            nothitBall.UpdateBallPos(syncTime);
                    }
                }
                else
                {
                    for (int n = 0; n < balls.Count; n++)
                    {
                        var nothitBall = balls[n];
                        //if (nothitBall.deltaTime > 0)
                            nothitBall.UpdateBallPos(leftSyncTime);
                    }
                    break;
                }
                run_crdPool.ResetAll();
                fastEdgePool.ResetAll();
                fastHitBallPool.ResetAll();
                UnLockBalls();
            }
            //ClearTestData();
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
        //void ProcessHitData()
        //{
        //    foreach(var ballpair in ballPairHit)//每个球对应的碰撞记录集合
        //    {
        //        int ID = ballpair.Key;
        //        var baseHits = ballpair.Value;
        //        for(int i =0; i < baseHits.Count;i++)//其中一个球的碰撞记录集合
        //        {
        //            var baseHit = baseHits[i];
        //            if(baseHit.hitType==HitType.Ball)
        //            {
        //                var _baseHit = baseHit as fastHitBall;
        //                int otherID = _baseHit.staticballObj.ID;
        //                if (ballPairHit.ContainsKey(otherID))
        //                {
        //                    var other_baseHits = ballPairHit[otherID];
        //                    for(int j = 0; j<other_baseHits.Count;j++)
        //                    {
        //                        var otherHit = other_baseHits[j];
        //                        if(otherHit.hitType==HitType.Ball)
        //                        {
        //                            var _otherHit = otherHit as fastHitBall;
        //                            if (_baseHit.runballObj.ID == _otherHit.staticballObj.ID)
        //                            {
        //                                continue;
        //                            }
        //                            else
        //                            {
        //                                if (/*_baseHit.CalHitTime()*/_baseHit.t_percent * _baseHit.staticballObj.deltaTime > _otherHit.CalHitTime())
        //                                {
        //                                    _baseHit.valid = false;
        //                                }
        //                                else
        //                                    _otherHit.valid = false;
        //                            }
        //                        }
        //                        else
        //                        {
        //                            var _otherHit = otherHit as fastEdge;
        //                            if (/*_baseHit.CalHitTime()*/ _baseHit.t_percent * _baseHit.staticballObj.deltaTime > _otherHit.CalHitTime())
        //                            {
        //                                _baseHit.valid = false;
        //                            }
        //                            else
        //                                _otherHit.valid = false;
        //                        }
        //                    }
        //                }
        //                else
        //                    Debug.Log("一般碰撞对都是成对记录出现,不可能走到这!");
        //            }                   
        //        }
        //    }

        //    bool isFlag = false;
        //    foreach (var ballpair in ballPairHit)
        //    {
        //        int ID = ballpair.Key;
        //        var baseHits = ballpair.Value.OrderBy((m)=>m.CalHitTime()).ToList();//按时间来排序

        //        BaseHit baseHit=null;
        //        for(int i = 0; i < baseHits.Count;i++)
        //        {
        //            if (baseHits[i].valid == false) continue;
        //            baseHit = baseHits[i];
        //            break;
        //        }


        //        if (baseHit!=null&&baseHit.valid == true && !baseHit.Isprocess())
        //        {
        //            isFlag = true;
        //            baseHit.TagProcess();
        //            if (baseHit.hitType == HitType.Ball)
        //            {
        //                var _baseHit = baseHit as fastHitBall;
        //                _baseHit.runballObj.CalBallPos(_baseHit.t_percent * _baseHit.runballObj.deltaTime);
        //                _baseHit.staticballObj.CalBallPos(_baseHit.t_percent * _baseHit.staticballObj.deltaTime);
        //                //updateDirAndTimeByBall(_baseHit.t_percent, _baseHit.runballObj, _baseHit.staticballObj);
        //            }
        //            else if (baseHit.hitType == HitType.Edge)
        //            {
        //                var _baseHit = baseHit as fastEdge;
        //                _baseHit.ball.CalBallPos(_baseHit.t_percent * _baseHit.ball.deltaTime);
        //                //updateDirAndTimeByEdge(_baseHit.t_percent, _baseHit.tbe, _baseHit.ball);
        //            }
        //        }
        //    }

        //    if(isFlag==false)
        //    {
        //        Debug.Log("虽然有碰撞记录,但是没有执行碰撞,这里可能会导致死循环的情况,因为一直检测到有碰撞,却不执行");
        //    }
        //    UnLockBalls();
        //    ballPairHit.Clear();
        //}

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
        private List<tableEdge> tableEdges = new List<tableEdge>();
        private  FP tableWidth = 30;
        private   FP tableHeight = 15;
        private FP radius = 0.5;
        private FP ballMaxSpeed = 20;
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


        public List<tableEdge> TableEdges
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
