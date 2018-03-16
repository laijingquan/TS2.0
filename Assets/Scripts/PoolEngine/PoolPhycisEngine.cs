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
            //DrawTable();
            //DrawBall();
        }

        void CreateBalls()
        {
            var ballObj = new BallObj();
            ballObj.moveDir = new TSVector2(1, 0.3).normalized;
            ballObj.moveSpeed = 10;
            balls.Add(ballObj);
            ballObj = new BallObj();
            ballObj.moveDir = new TSVector2(-1, 0.3).normalized;
            ballObj.moveSpeed = 20;
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
        // Update is called once per frame
        public void Update(FP deltaTime)
        {
            if (control)
                UpdatePhysicStep(deltaTime);//逻辑层
            //else
            //    UpdateBallPos(Time.deltaTime);
            //UpdateBallDraw();//渲染层
        }

        //TSVector2 PredictPos(BallObj ball,FP deltaTime)
        //{
        //    //ball.predict_pos = ball.cur_pos + moveDir * moveSpeed * deltaTime;
        //    return ball.cur_pos + ball.moveDir * moveSpeed * deltaTime;
        //}

        //void UpdateBallPos(BallObj ball,FP deltaTime)
        //{
        //    ball.pre_pos = ball.cur_pos;
        //    ball.cur_pos += ball.moveDir * moveSpeed * deltaTime;
        //    //CheckBound();
        //}

        private static int testnumber = 0;
        private List<testData> td = new List<testData>();
        void AddTestData(tableEdge tbg, TSVector2 prepos, TSVector2 hitpos, TSVector2 premoveDir, TSVector2 aftmoveDir)
        {
            td.Add(new testData() { tbg = tbg, prehitPos = prepos, hitpos = hitpos, PremoveDir = premoveDir, AftmoveDir = aftmoveDir });
        }
        void ClearTestData()
        {
            //td.Clear();
        }

        //public void CheckBound()
        //{
        //    var x = TSMath.Abs(ball.cur_pos.x);
        //    var y = TSMath.Abs(ball.cur_pos.y);
        //    if (x > 4.5 || y > 2)
        //    {
        //        if (ball.cur_pos.x > 4.5)
        //        {
        //            ball.cur_pos.x = 4.5;
        //        }

        //        if (ball.cur_pos.x < -4.5)
        //        {
        //            ball.cur_pos.x = -4.5;
        //        }
        //        if (y > 2)
        //        {
        //            ball.cur_pos.y = 2;
        //        }

        //        if (y < -2)
        //        {
        //            ball.cur_pos.y = -2;
        //        }
        //        Debug.Log("球出界了");
        //    }
        //}


        void updateDirAndTime(ref FP deltaTime, FP _percent, tableEdge _tbe, BallObj ball)
        {
            ball.UpdateBallPos(deltaTime * _percent);//先更新到撞击点
            deltaTime = deltaTime - deltaTime * _percent;//更新剩余时间
            var curReflcDir = Detection.CheckCircle_LineCollision(_tbe, ball.cur_pos, ball.radius, ball.moveDir);//计算碰撞响应
            AddTestData(_tbe, ball.pre_pos, ball.cur_pos, ball.moveDir, curReflcDir);
            ball.moveDir = curReflcDir.normalized;//更新实时方向
                                                  //UpdateBallPos(deltaTime); 在这里更新 如果速度过快 那么会直接跑到球桌
        }


        void UpdatePhysicStep(FP _deltaTime)
        {
            bool step = true;

            //Func<FP, FP, FP, bool> checkBound = (pos, start, end) =>
            //{
            //    if (pos > start && pos < end)
            //        return true;

            //    else
            //        return false;
            //};

            //Action<FP, tableEdge> updateDirAndTime = (_percent, _tbe) =>
            //{
            //    UpdateBallPos(deltaTime * _percent);//先更新到撞击点
            //    deltaTime = deltaTime - deltaTime * _percent;//更新剩余时间
            //    var curReflcDir = Detection.CheckCircle_LineCollision(_tbe, ball.cur_pos, ball.radius, moveDir);//计算碰撞响应
            //    AddTestData(_tbe, ball.pre_pos, ball.cur_pos, moveDir, curReflcDir);
            //    moveDir = curReflcDir.normalized;//更新实时方向
            //                                     //UpdateBallPos(deltaTime); 在这里更新 如果速度过快 那么会直接跑到球桌
            //};
            ClearTestData();

            for(int k = 0; k < balls.Count;k++)
            {
                var ball = balls[k];
                var deltaTime = _deltaTime;
                step = true;
                while (step)
                {
                    testnumber++;
                    if (testnumber > 3)
                    {
                        Debug.Log("大于3次检测");
                    }
                    //step = false;
                    var next_pos = ball.PredictPos(deltaTime);//预测经过deltaTime后的位置
                    crd.cur_pos = ball.cur_pos;
                    crd.next_pos = next_pos;
                    crd.radius = ball.radius;

                    FP t_percent = 0;

                    TSVector2 predictEndPos = ball.cur_pos + ball.moveDir * 100;

                    bool isflag = false;
                    List<fastEdge> fastedges = new List<fastEdge>();
                    //在当前速度下,预测圆最先和哪条边碰撞
                    for (int i = 0; i < tableEdges.Length; i++)
                    {
                        if (Detection.CheckSegement_Contact(ball.cur_pos, predictEndPos, tableEdges[i].farstart, tableEdges[i].farend))
                        {
                            //var cur_proj = Detection.PointToLineDir(tableEdges[i].start, tableEdges[i].end, crd.cur_pos);
                            //var next_proj = Detection.PointToLineDir(tableEdges[i].start, tableEdges[i].end,crd.next_pos);
                            //FP cur_projValue = TSMath.Abs(TSVector2.Dot(cur_proj, cur_proj));//当前圆心位置到线段的有向距离
                            //FP next_projValue = TSMath.Abs(TSVector2.Dot(next_proj, next_proj));//预测下一圆心位置到线段的有向距离
                            //if (cur_projValue <= next_projValue) continue;//证明球正在远离该线段,不用检测

                            if (Detection.CheckCircle_LineContact(tableEdges[i], crd, ref t_percent))
                            {
                                fastedges.Add(new fastEdge(tableEdges[i], t_percent));
                                //updateDirAndTime(t_percent, tableEdges[i]);
                                //isflag = true;//还要继续step
                                //break;//最先碰撞到的边会先break
                            }
                            //else
                            //{
                            //    testnumber = 0;
                            //    step = false;
                            //    UpdateBallPos(deltaTime);//这次更新后 无任何碰撞
                            //}
                            //isflag = true;
                            //break;//每次只能有一边能碰撞(如果射线交于两条线段的公共点，那么直接break就会有问题，可能最先碰撞的线段被忽略了)
                        }
                    }

                    if (fastedges.Count > 0)
                    {
                        fastedges = fastedges.OrderBy((x) => x.t_percent).ToList();
                        isflag = true;//还要继续step
                        updateDirAndTime(ref deltaTime, fastedges[0].t_percent, fastedges[0].tbe,ball);//更新位置，并且由于撞击而更改速度方向
                    }

                    if (!isflag)
                    {
                        step = false;
                        testnumber = 0;
                        ball.UpdateBallPos(deltaTime);//无任何碰撞直接更新位置
                    }
                }
            }
        }
//         Vector3 ToXZ(TSVector2 target)
//         {
//             return new Vector3(target.x.AsFloat(), 0, target.y.AsFloat());
//         }
//         public void OnDrawGizmos()
//         {
//             if (Application.isPlaying)
//             {
//                 if (tableEdges != null)
//                 {
//                     for (int i = 0; i < tableEdges.Length; i++)
//                     {
//                         var tbg = tableEdges[i];
//                         if (tbg != null)
//                             Gizmos.DrawLine(ToXZ(tbg.start), ToXZ(tbg.end));
//                     }
//                 }
//                 if (ball != null)
//                     Gizmos.DrawRay(ToXZ(ball.cur_pos), ToXZ(moveDir * 1000));
//             }
//         }

        private tableEdge[] tableEdges = new tableEdge[4];
        private FP tableWidth = 10;
        private FP tableHeight = 5;
        //private CircleData ball = new CircleData();
        private List<BallObj> balls = new List<BallObj>();

        //private TSVector2 moveDir = TSVector2.zero;
        //private FP moveSpeed = 10;

        CircleRunData crd = new CircleRunData();

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
