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
            CreateBalls();
        }

        void CreateBalls()
        {
            var ballObj = new BallObj(1, new TSVector2(0, 2), new TSVector2(0, -1).normalized,100,0.5);
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


        private bool control = true;
        private FP fixedTime=0;
        private FP stepTime = 0.01;
        // Update is called once per frame
        public void Update(FP deltaTime)
        {
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
        private Dictionary<int, List<BallObj>> record = new Dictionary<int, List<BallObj>>();
        void AddTestData(tableEdge tbg, TSVector2 prepos, TSVector2 hitpos, TSVector2 premoveDir, TSVector2 aftmoveDir)
        {
            td.Add(new testData() { tbg = tbg, prehitPos = prepos, hitpos = hitpos, PremoveDir = premoveDir, AftmoveDir = aftmoveDir });
        }

        public void AddTestData(int step,List<BallObj >objs)
        {
            var temp = new List<BallObj>();
            for(int i =0; i <objs.Count;i++)
            {
                temp.Add(new BallObj(objs[i]));
            }

            record.Add(step, temp);
            //td.Add(new testData() { ball = obj });
        }

        void ClearTestData()
        {
            //td.Clear();
        }


        void updateDirAndTimeByEdge(FP _percent, tableEdge _tbe, BallObj ball)
        {
            ball.UpdateBallPos(ball.deltaTime * _percent);//先更新到撞击点
            ball.deltaTime = ball.deltaTime - ball.deltaTime * _percent;//更新剩余时间
            var curReflcDir = Detection.CheckCircle_LineCollision(_tbe, ball.cur_pos, ball.radius, ball.moveDir);//计算碰撞响应
            AddTestData(_tbe, ball.pre_pos, ball.cur_pos, ball.moveDir, curReflcDir);
            ball.moveDir = curReflcDir;//更新实时方向
                                                  //UpdateBallPos(deltaTime); 在这里更新 如果速度过快 那么会直接跑到球桌
        }
        void updateDirAndTimeByBall(FP _percent,BallObj runball,BallObj staticball)
        {
            runball.UpdateBallPos(runball.deltaTime * _percent);//先更新到撞击点
            staticball.UpdateBallPos(staticball.deltaTime*_percent);
            //runball.deltaTime  = runball.deltaTime - runball.deltaTime * _percent;//更新剩余时间
            //staticball.deltaTime = staticball.deltaTime - staticball.deltaTime * _percent;

            var runcrd = new CircleRunData(runball.cur_pos, runball.PredictPos(), runball.radius);
            var staticcrd = new CircleRunData(staticball.cur_pos, staticball.PredictPos(), staticball.radius);
            var curReflcDir = Detection.CheckCircle_CircleCollision(runball.cur_pos,runball.moveDir,staticball.cur_pos, staticball.moveDir);//计算碰撞响应
            //AddTestData(_tbe, ball.pre_pos, ball.cur_pos, ball.moveDir, curReflcDir);
            runball.moveDir = curReflcDir[0];//更新实时方向
            staticball.moveDir = curReflcDir[1];
                                                  //UpdateBallPos(deltaTime); 在这里更新 如果速度过快 那么会直接跑到球桌
        }

        void checkCollisionBall()
        {

        }

        void checkCollisionEdge()
        {

        }

        void UpdatePhysicStep(FP _deltaTime)
        {
            //bool step = true;
            step++;
            //AddTestData(step,balls);
            //ClearTestData();

            for (int k = 0; k < balls.Count; k++)
            {
                var ball = balls[k];
                ball.deltaTime = _deltaTime;
                ball.lockcheck = false;
            }
            //bool checkCollide = false;
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
                        break;
                    var deltaTime = ball.deltaTime;//每个球的剩余的时长是不一样的
                    List<BaseHit> fastHitBalls = new List<BaseHit>();
                    #region 球碰撞检测
                    CircleRunData run_crd = new CircleRunData(ball.cur_pos, ball.PredictPos(), ball.radius);
                    for (int j = 0; j < balls.Count; j++)
                    {
                        var otherball = balls[j];
                        if (otherball == ball) continue;
                        FP _percent = 0;
                        CircleRunData static_crd = new CircleRunData(otherball.cur_pos, otherball.PredictPos(), otherball.radius);
                        if (Detection.CheckCircle_CircleContact(run_crd, static_crd, ball.deltaTime, ref _percent))
                        {
                            fastHitBalls.Add(new fastHitBall(ball, otherball, _percent));
                        }
                    }

                    if (fastHitBalls.Count > 0)
                    {
                        //fastHitBalls = fastHitBalls.OrderBy((m) =>  m.CalRunBallHitTime() ).ToList();//碰撞集合中，抽取时间最短的碰撞
                        //collectHits.Add(fastHitBalls[0]);
                        CollectBallPairList(fastHitBalls);
                    }

                    #endregion
                    #region 边检测
                    FP t_percent = 0;

                    TSVector2 predictEndPos = ball.cur_pos + ball.moveDir*100;

                    //bool isflag = false;
                    List<BaseHit> fastedges = new List<BaseHit>();
                    //在当前速度下,预测圆最先和哪条边碰撞
                    for (int i = 0; i < tableEdges.Length; i++)
                    {
                        //if (Detection.CheckSegement_Contact(ball.cur_pos, predictEndPos, tableEdges[i].farstart, tableEdges[i].farend))//这个检测是去掉在挨着边但运动方向相反的情况
                        if(Detection.CheckCloseEdge(tableEdges[i].start,tableEdges[i].end,ball.cur_pos,predictEndPos))
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
                        //fastedges = fastedges.OrderBy((x) => x.CalHitTime()).ToList();
                        //if(fastHitBalls.Count<=0|| fastHitBalls[0].CalRunBallHitTime()>fastedges[0].CalHitTime())//边碰撞花费时间更少的话
                        //{
                        //    if(fastHitBalls.Count>0)
                        //        collectHits.Remove(fastHitBalls[0]);
                        //    collectHits.Add(fastedges[0]);
                        //    bool outbound = ball.CalBallPos(fastedges[0].CalHitTime());
                        //    if(outbound)
                        //    {
                        //        Debug.Log("无语");
                        //    }
                        //    //checkCollide = true;
                        //}
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
                //if (!checkCollide)
                //{
                //    //没有碰撞了,检查所有球是否有剩余时间，直接走完跳出
                //    for(int i =0; i < balls.Count;i++)
                //    {
                //        var nothitBall = balls[i];
                //        if (nothitBall.deltaTime > 0)
                //            nothitBall.UpdateBallPos(nothitBall.deltaTime);
                //    }
                //    break;
                //}
            }
        }

        private List<fastHitBall> destroy_m_fastHitBall = new List<fastHitBall>();
        private List<fastEdge> destroy_m_fastEdge = new List<fastEdge>();

        //void CollectBallPair(BaseHit one,BaseHit other)
        //{
        //    var data = new List<BaseHit>() { one, other };
        //    ballPairHit.Add(pairBallKey++, data);
        //}
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
                                        if(/*_baseHit.CalHitTime()*/_baseHit.t_percent*_baseHit.staticballObj.deltaTime>_otherHit.CalHitTime())
                                        {
                                            _baseHit.valid = false;
                                        }
                                    }
                                }
                                else
                                {
                                    var _otherHit = otherHit as fastEdge;
                                    if (/*_baseHit.CalHitTime()*/ _baseHit.t_percent * _baseHit.staticballObj.deltaTime > _otherHit.CalHitTime())
                                    {
                                        _baseHit.valid = false;
                                    }
                                }
                            }
                        }
                        else
                            Debug.Log("一般碰撞对都是成对记录出现,不可能走到这!");
                    }                   
                }
            }

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
                
                //for(int i = 0; i < baseHits.Count;i++)
                //{
                //    var baseHit = baseHits[i];
                //    if (baseHit.valid == false || baseHit.Isprocess()) continue;
                //    baseHit.TagProcess();
                //    if(baseHit.hitType==HitType.Ball)
                //    {
                //        var _baseHit = baseHit as fastHitBall;
                //        _baseHit.runballObj.CalBallPos(_baseHit.t_percent * _baseHit.runballObj.deltaTime);
                //        _baseHit.staticballObj.CalBallPos(_baseHit.t_percent * _baseHit.staticballObj.deltaTime);
                //        updateDirAndTimeByBall(_baseHit.t_percent, _baseHit.runballObj, _baseHit.staticballObj);
                //    }
                //    else if(baseHit.hitType==HitType.Edge)
                //    {
                //        var _baseHit = baseHit as fastEdge;
                //        _baseHit.ball.CalBallPos(_baseHit.t_percent * _baseHit.ball.deltaTime);
                //        updateDirAndTimeByEdge(_baseHit.t_percent, _baseHit.tbe, _baseHit.ball);
                //    }
                //    break;
                //}
            }
            UnLockBalls();
                //pairBallKey = 0;
                //ballPairHit.Clear();
                //collectValidHits.Clear();
                //collectValidHits = new List<BaseHit>(collectHits);
                //// collectHis相当于收集了每个球花费最少时间的碰撞快照
                //for(int i = collectHits.Count-1; i >=0;i--)
                //{
                //    var baseHit = collectHits[i];
                //    if(baseHit.hitType==HitType.Ball)
                //    {
                //        var _baseHit = baseHit as fastHitBall;
                //        for(int j =0;j<collectValidHits.Count;j++)
                //        {
                //            var check_baseHit = collectValidHits[j];

                //            if (baseHit == check_baseHit) continue;

                //            if (check_baseHit.hitType == HitType.Ball)
                //            {
                //                var _check_baseHit = check_baseHit as fastHitBall;
                //                //碰撞对不一致,无效碰撞
                //                if(_baseHit.staticballObj.ID==_check_baseHit.runballObj.ID&&_check_baseHit.staticballObj.ID!=_baseHit.runballObj.ID)
                //                {
                //                    collectHits.Remove(baseHit);
                //                }
                //            }
                //            else
                //            {
                //                var _check_baseHit = check_baseHit as fastEdge;
                //                //碰撞对不一致,无效碰撞
                //                if (_baseHit.staticballObj.ID == _check_baseHit.ball.ID)
                //                {
                //                    collectHits.Remove(baseHit);
                //                }
                //            }
                //        }
                //    }
                //}

                ////这里还要处理相同作用的记录 运动球和静态球的，找静态球的记录是否和运动球的记录对应
                ////collectValidHits = new List<BaseHit>(collectHits);
                //for (int i = collectHits.Count - 1; i >= 0; i--)
                //{
                //    var baseHit = collectHits[i];
                //    if (baseHit.hitType == HitType.Ball)
                //    {
                //        var _baseHit = baseHit as fastHitBall;
                //        for (int j = 0; j < collectHits.Count; j++)
                //        {
                //            var check_baseHit = collectHits[j];

                //            if (baseHit == check_baseHit) continue;

                //            if (check_baseHit.hitType == HitType.Ball)
                //            {
                //                var _check_baseHit = check_baseHit as fastHitBall;
                //                //碰撞对一致,留其一
                //                if (_baseHit.staticballObj.ID == _check_baseHit.runballObj.ID && _check_baseHit.staticballObj.ID == _baseHit.runballObj.ID)
                //                {
                //                    collectHits.Remove(baseHit);
                //                    break;
                //                }
                //            }
                //        }
                //    }
                //}



                //for (int i = 0; i < collectHits.Count; i++)
                //{
                //    var baseHit = collectHits[i];
                //    if(baseHit.hitType==HitType.Edge)
                //    {
                //        var _baseHit = baseHit as fastEdge;
                //        updateDirAndTimeByEdge(_baseHit.t_percent, _baseHit.tbe, _baseHit.ball);//更新位置，并且由于撞击而更改速度方向
                //    }
                //    else
                //    {
                //        var _baseHit = baseHit as fastHitBall;
                //        updateDirAndTimeByBall(_baseHit.t_percent, _baseHit.runballObj, _baseHit.staticballObj);

                //    }

                //}
                ballPairHit.Clear();
            collectHits.Clear();
        }

        //public bool CheckEdgeCollide(TSVector2 E1, TSVector2 E2, TSVector2 cur_pos, TSVector2 next_pos,CircleRunData run_crd,ref FP t_percent)
        //{
        //    //在当前速度下,预测圆最先和哪条边碰撞
        //    for (int i = 0; i < tableEdges.Length; i++)
        //    {
        //        if (Detection.CheckCloseEdge(E1,E2,cur_pos,next_pos))
        //        {

        //            if (Detection.CheckCircle_LineContact(tableEdges[i], run_crd, ref t_percent))
        //            {
        //                return true;
        //            }
        //        }
        //    }
        //    return false;
        //}

        private int step = 0;

        private tableEdge[] tableEdges = new tableEdge[4];
        private FP tableWidth = 10;
        private FP tableHeight = 5;
        //private CircleData ball = new CircleData();
        private List<BallObj> balls = new List<BallObj>();

        //private TSVector2 moveDir = TSVector2.zero;
        //private FP moveSpeed = 10;

        //CircleRunData crd = new CircleRunData();
        //private List<fastHitBall> m_fastHitBall = new List<fastHitBall>();
        //private List<fastEdge> m_fastHitEdge = new List<fastEdge>();
        //private List<fastBall> m_fastBall = new List<fastBall>();

        private List<BaseHit> collectHits = new List<BaseHit>();//可能的碰撞集合
        private List<BaseHit> collectValidHits = new List<BaseHit>();//真实的碰撞集合
        private Dictionary<int, List<BaseHit>> ballPairHit = new Dictionary<int, List<BaseHit>>();

        private int pairBallKey;
        //GameObject ballObj;


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
}
