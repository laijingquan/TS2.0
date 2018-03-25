using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using TrueSync;

namespace PoolEngine
{
    public class PoolMain : MonoBehaviour
    {

        void Awake()
        {
            PoolPhycisEngine.instance.Awake();
            tableEdges = PoolPhycisEngine.instance.TableEdges;
            balls = PoolPhycisEngine.instance.Ball;
            DispatchBallsAwake();
        }
        // Use this for initialization
        void Start()
        {
            DispatchBallsStart();
            DrawTable();           
        }
        // Update is called once per frame
        void Update()
        {
            PoolPhycisEngine.instance.Update(Time.deltaTime);
            DispatchBallsUpdate();
        }
        public void DispatchBallsAwake()
        {
            for (int i = 0; i < balls.Count; i++)
            {
                balls[i].Awake();
            }
        }

        public void DispatchBallsStart()
        {
            for (int i = 0; i < balls.Count; i++)
            {
                balls[i].Start();
            }
        }
        public void DispatchBallsUpdate()
        {
            for (int i = 0; i < balls.Count; i++)
            {
                balls[i].Update();
            }
        }

        GameObject createEdgePos()
        {
            return GameObject.CreatePrimitive(PrimitiveType.Cube);
        }
        void DrawTable()
        {
            var up1 = createEdgePos();
            up1.transform.position = new Vector3(tableEdges[0].start.x.AsFloat(), 0, tableEdges[0].start.y.AsFloat());

            var up2 = createEdgePos();
            up2.transform.position = new Vector3(tableEdges[0].end.x.AsFloat(), 0, tableEdges[0].end.y.AsFloat());

            var down1 = createEdgePos();
            down1.transform.position = new Vector3(tableEdges[1].start.x.AsFloat(), 0, tableEdges[1].start.y.AsFloat());

            var down2 = createEdgePos();
            down2.transform.position = new Vector3(tableEdges[1].end.x.AsFloat(), 0, tableEdges[1].end.y.AsFloat());
        }

        Vector3 ToXZ(TSVector2 target)
        {
            return new Vector3(target.x.AsFloat(), 0, target.y.AsFloat());
        }
        public void OnDrawGizmos()
        {
            if (Application.isPlaying)
            {
                if (tableEdges != null)
                {
                    for (int i = 0; i < tableEdges.Length; i++)
                    {
                        var tbg = tableEdges[i];
                        if (tbg != null)
                            Gizmos.DrawLine(ToXZ(tbg.start), ToXZ(tbg.end));
                    }
                }
                if (balls == null) return;
                for(int i=0; i < balls.Count;i++)
                {
                    var ball = balls[i];
                    if (ball != null)
                        Gizmos.DrawRay(ToXZ(ball.cur_pos), ToXZ(ball.moveDir * 1000));
                }
            }
        }
        //注意 变量一定要初始化
        private PoolPhycisEngine poolPhysicEngine=null;
        private tableEdge[] tableEdges=null;
        private List<BallObj> balls = null;
    }
}
