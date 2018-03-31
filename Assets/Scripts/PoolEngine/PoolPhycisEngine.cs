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
            //balls.Add(ballObj);

            ballObj = new BallObj(4, new TSVector2(2, 1), new TSVector2(-0.5, 0.3).normalized, 2, 0.5);
            //balls.Add(ballObj);

            ballObj = new BallObj(5, new TSVector2(2, 1), new TSVector2(-0.5, 0.6).normalized, 2, 0.5);
            //balls.Add(ballObj);

            ballObj = new BallObj(6, new TSVector2(2, 1), new TSVector2(0.5, 0.3).normalized, 2, 0.5);
            //balls.Add(ballObj);

            ballObj = new BallObj(7, new TSVector2(2, 1), new TSVector2(-0.5, -0.3).normalized, 2, 0.5);
            //balls.Add(ballObj);

            ballObj = new BallObj(8, new TSVector2(2, 1), new TSVector2(-0.5, -0.3).normalized, 2, 0.5);
            //balls.Add(ballObj);

            ballObj = new BallObj(9, new TSVector2(2, 1), new TSVector2(-0.5, -0.3).normalized, 2, 0.5);
            //balls.Add(ballObj);

            ballObj = new BallObj(10, new TSVector2(2, 1), new TSVector2(-0.5, -0.3).normalized, 2, 0.5);
            //balls.Add(ballObj);

            ballObj = new BallObj(11, new TSVector2(2, 1), new TSVector2(-0.5, -0.3).normalized, 2, 0.5);
            //balls.Add(ballObj);
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
            AddTestData(step,balls);
            ClearTestData();

            for (int k = 0; k < balls.Count; k++)
            {
                var ball = balls[k];
                ball.deltaTime = _deltaTime;
            }
            bool checkCollide = false;
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
                checkCollide = false;
                var checkballs = new List<BallObj>(balls);
                for (int k = 0; k < balls.Count; k++)
                {
                    var ball = balls[k];
                    ball.lockcheck = false;
                }

                for (int k = 0; k < checkballs.Count; k++)
                {
                    var ball = checkballs[k];
                    if (ball.lockcheck||ball.deltaTime<=0)
                        break;
                    //var deltaTime = _deltaTime;
                    var deltaTime = ball.deltaTime;//每个球的剩余的时长是不一样的
                    //var _step = true;
                    List<fastHitBall> fastHitBalls = new List<fastHitBall>();
                    #region 球碰撞检测
                    CircleRunData run_crd = new CircleRunData(ball.cur_pos, ball.PredictPos(), ball.radius);
                    for (int j = 0; j < checkballs.Count; j++)
                    {
                        var otherball = checkballs[j];
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
                        //这里有问题 应该是按时间排序
                        fastHitBalls = fastHitBalls.OrderBy((m) =>  m.runballObj.deltaTime*m.t_percent ).ToList();//碰撞集合中，抽取时间最短的碰撞
                        //更新俩球的碰撞位置和方向
                        //updateDirAndTimeByBall(fastHitBalls[0].t_percent, fastHitBalls[0].runballObj, fastHitBalls[0].staticballObj);
                        m_fastHitBall.Add(fastHitBalls[0]);
                        fastHitBalls.Clear();
                        //checkballs.Remove(fastHitBalls[0].runballObj);
                        //checkballs.Remove(fastHitBalls[0].staticballObj);
                        //fastHitBalls[0].runballObj.lockcheck = true;
                        //fastHitBalls[0].staticballObj.lockcheck = true;

                        checkCollide = true;
                        //continue;//发生球碰撞就不需要检测和边的碰撞，直接跳出。
                    }

                    #endregion
                    #region 边检测
                    FP t_percent = 0;

                    TSVector2 predictEndPos = ball.cur_pos + ball.moveDir*100;

                    //bool isflag = false;
                    List<fastEdge> fastedges = new List<fastEdge>();
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
                        fastedges = fastedges.OrderBy((x) => x.t_percent * x.ball.deltaTime).ToList();
                        m_fastHitEdge.Add(fastedges[0]);
                        fastedges.Clear();
                        checkCollide = true;
                    }
                }
                #endregion
                ProcessHitData();
                if (!checkCollide)
                {
                    //没有碰撞了,检查所有球是否有剩余时间，直接走完跳出
                    for(int i =0; i < balls.Count;i++)
                    {
                        var nothitBall = balls[i];
                        if (nothitBall.deltaTime > 0)
                            nothitBall.UpdateBallPos(nothitBall.deltaTime);
                    }
                    break;
                }
            }
        }

        /// <summary>
        /// 处理所有的碰撞记录
        /// </summary>
        void ProcessHitData()
        {
            //将所有的碰撞点都收集起来，再剔除不可能的情况
            if(m_fastHitBall.Count>0)
            {
                for(int i =0; i < m_fastHitBall.Count;i++)
                {
                    var myfastHitBall = m_fastHitBall[i];
                    for(int j = 0; j < m_fastHitBall.Count;j++)
                    {
                        var otherfastHitBall = m_fastHitBall[j];
                        if (myfastHitBall == otherfastHitBall) continue;
                        //检测另一方是否有不同的碰撞情况,如果有,那么可能是和其他球碰撞或者和某一边碰撞，如果花费的时间更少，那么就选择时间少的，剔除时间多的。
                        if (otherfastHitBall.runballObj==myfastHitBall.staticballObj)
                        {
                            //检测双方的碰撞对是否一致,如果不一致那么再比较时间来决定这条碰撞记录是否有效
                            if (otherfastHitBall.staticballObj != myfastHitBall.runballObj)
                            {
                                //该碰撞记录花费时间更短，碰撞有效
                                if(myfastHitBall.runballObj.deltaTime*myfastHitBall.t_percent<otherfastHitBall.runballObj.deltaTime*otherfastHitBall.t_percent)
                                {
                                    m_fastHitBall.Remove(otherfastHitBall);//对应着，另外一条记录无效
                                }
                                //该碰撞记录花费时间更短，碰撞无效
                                else
                                {
                                    m_fastHitBall.Remove(myfastHitBall);//对应着，另外一条记录无效
                                }
                                break;
                            }
                            //一致的话 只需要选择其中一条记录即可
                            else
                            {
                                m_fastHitBall.Remove(otherfastHitBall);//对应着，另外一条记录无效
                            }

                            ////检测是否和边有碰撞并且消耗时间更短
                            //if(m_fastHitEdge.Count > 0)
                            //{
                            //    for (int k= 0; k < m_fastHitEdge.Count; k++)
                            //    {
                            //        var myfastHitEdge = m_fastHitEdge[k];
                            //        if(myfastHitEdge.ball==otherfastHitBall.runballObj&&myfastHitEdge.ball.deltaTime*myfastHitEdge.t_percent<otherfastHitBall.t_percent*otherfastHitBall.runballObj.deltaTime)
                            //        {
                            //            isValid = false;
                            //            goto CheckEnd;
                            //        }
                            //    }
                            //}
                        }


                    }

                    //CheckEnd:
                    //if(isValid)
                    //    updateDirAndTimeByBall(m_fastHitBall[i].t_percent, m_fastHitBall[i].runballObj, m_fastHitBall[i].staticballObj);
                }
            }
            /*            m_fastHitBall.Clear();*/

            //再和边的碰撞集合对比
            if (m_fastHitBall.Count > 0)
            {
                for (int i = 0; i < m_fastHitBall.Count; i++)
                {
                    var myfastHitBall = m_fastHitBall[i];
                    for (int k = 0; k < m_fastHitEdge.Count; k++)
                    {
                        var myfastHitEdge = m_fastHitEdge[k];

                        if (myfastHitEdge.ball == myfastHitBall.runballObj)
                        {
                            //边的碰撞记录花费时间更短
                            if (myfastHitEdge.ball.deltaTime * myfastHitEdge.t_percent < myfastHitBall.t_percent * myfastHitBall.runballObj.deltaTime)
                            {
                                m_fastHitBall.Remove(myfastHitBall);//删除球和球碰撞记录
                            }
                            //边的碰撞记录花费时间更长
                            else
                            {
                                m_fastHitEdge.Remove(myfastHitEdge);//删除球和边的碰撞记录
                            }
                            break;
                        }
                    }
                }
                for (int i = 0; i < m_fastHitBall.Count; i++)
                {
                    updateDirAndTimeByBall(m_fastHitBall[i].t_percent, m_fastHitBall[i].runballObj, m_fastHitBall[i].staticballObj);
                }
            }

            //仅仅剩余边的碰撞集合，无需考虑球和球的碰撞集合

            if (m_fastHitEdge.Count > 0)
            {
                for (int i = 0; i < m_fastHitEdge.Count; i++)
                {
                    updateDirAndTimeByEdge(m_fastHitEdge[i].t_percent, m_fastHitEdge[i].tbe, m_fastHitEdge[i].ball);//更新位置，并且由于撞击而更改速度方向
                }
            }

            m_fastHitBall.Clear();
            m_fastHitEdge.Clear();
        }

        public bool CheckEdgeCollide(TSVector2 E1, TSVector2 E2, TSVector2 cur_pos, TSVector2 next_pos,CircleRunData run_crd,ref FP t_percent)
        {
            //在当前速度下,预测圆最先和哪条边碰撞
            for (int i = 0; i < tableEdges.Length; i++)
            {
                if (Detection.CheckCloseEdge(E1,E2,cur_pos,next_pos))
                {

                    if (Detection.CheckCircle_LineContact(tableEdges[i], run_crd, ref t_percent))
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        private int step = 0;

        private tableEdge[] tableEdges = new tableEdge[4];
        private FP tableWidth = 10;
        private FP tableHeight = 5;
        //private CircleData ball = new CircleData();
        private List<BallObj> balls = new List<BallObj>();

        //private TSVector2 moveDir = TSVector2.zero;
        //private FP moveSpeed = 10;

        //CircleRunData crd = new CircleRunData();
        private List<fastHitBall> m_fastHitBall = new List<fastHitBall>();
        private List<fastEdge> m_fastHitEdge = new List<fastEdge>();
        private List<fastBall> m_fastBall = new List<fastBall>();
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
