﻿using UnityEngine;
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
        }

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


        void updateDirAndTimeByEdge(ref FP deltaTime, FP _percent, tableEdge _tbe, BallObj ball)
        {
            ball.UpdateBallPos(deltaTime * _percent);//先更新到撞击点
            deltaTime = deltaTime - deltaTime * _percent;//更新剩余时间
            var curReflcDir = Detection.CheckCircle_LineCollision(_tbe, ball.cur_pos, ball.radius, ball.moveDir);//计算碰撞响应
            AddTestData(_tbe, ball.pre_pos, ball.cur_pos, ball.moveDir, curReflcDir);
            ball.moveDir = curReflcDir.normalized;//更新实时方向
                                                  //UpdateBallPos(deltaTime); 在这里更新 如果速度过快 那么会直接跑到球桌
        }
        void updateDirAndTimeByBall(ref FP deltaTime, FP _percent,BallObj runball,BallObj staticball)
        {
            runball.UpdateBallPos(deltaTime * _percent);//先更新到撞击点
            staticball.UpdateBallPos(deltaTime*_percent);
            deltaTime = deltaTime - deltaTime * _percent;//更新剩余时间

            var runcrd = new CircleRunData(runball.cur_pos, runball.PredictPos(deltaTime), runball.radius);
            var staticcrd = new CircleRunData(staticball.cur_pos, staticball.PredictPos(deltaTime), staticball.radius);
            var curReflcDir = Detection.CheckCircle_CircleCollision(runcrd, staticcrd);//计算碰撞响应
            //AddTestData(_tbe, ball.pre_pos, ball.cur_pos, ball.moveDir, curReflcDir);
            runball.moveDir = curReflcDir[0].normalized;//更新实时方向
            staticball.moveDir = curReflcDir[1].normalized;
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
            bool step = true;

            ClearTestData();

            //for (int k = 0; k < balls.Count; k++)
            //{
            //    var ball = balls[k];
            //    ball.deltaTime = _deltaTime;
            //}


            for (int k = 0; k < balls.Count;k++)
            {
                var ball = balls[k];
                var deltaTime = _deltaTime;
                //var _step = true;
                //List<fastHitBall> fastHitBalls = new List<fastHitBall>();
                //while (_step)
                //{
                //    CircleRunData run_crd = new CircleRunData(ball.cur_pos, ball.PredictPos(), ball.radius);
                //    for (int j = 0; j < balls.Count; j++)
                //    {
                //        var otherball = balls[j];
                //        FP _percent = 0;
                //        CircleRunData static_crd = new CircleRunData(otherball.cur_pos, otherball.PredictPos(), otherball.radius);
                //        if (Detection.CheckCircle_CircleContact(run_crd, static_crd, ball.deltaTime, ref _percent))
                //        {
                //            fastHitBalls.Add(new fastHitBall(ball, otherball, _percent));
                //        }
                //    }

                //    if (fastHitBalls.Count > 0)
                //    {
                //        fastHitBalls = fastHitBalls.OrderBy((m) => m.t_percent).ToList();//碰撞集合中，抽取时间最短的碰撞
                //        //更新俩球的碰撞位置和方向
                //        updateDirAndTimeByBall(ref deltaTime, fastHitBalls[0].t_percent, fastHitBalls[0].runballObj, fastHitBalls[0].staticballObj);
                //    }
                //    else
                //    {
                //        _step = false;
                //        //ball.UpdateBallPos(deltaTime);
                //    }
                //}

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

                            if (Detection.CheckCircle_LineContact(tableEdges[i], crd, ref t_percent))
                            {
                                fastedges.Add(new fastEdge(tableEdges[i], t_percent));
                            }
                        }
                    }
                    //有碰撞集合,找到最先的碰撞点
                    if (fastedges.Count > 0)
                    {
                        fastedges = fastedges.OrderBy((x) => x.t_percent).ToList();
                        //isflag = true;//还要继续step
                        updateDirAndTimeByEdge(ref deltaTime, fastedges[0].t_percent, fastedges[0].tbe,ball);//更新位置，并且由于撞击而更改速度方向
                    }
                    //无碰撞,直接更新
                    else
                    {
                        step = false;
                        testnumber = 0;
                        ball.UpdateBallPos(deltaTime);//无任何碰撞直接更新位置
                    }
                }
            }
        }

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
