using UnityEngine;
using System.Collections;
using TrueSync;

namespace PoolEngine
{
    public class BallObj
    {
        public BallObj(int _ID,TSVector2 _cur_pos,TSVector2 _moveDir,FP _moveSpeed,FP _radius)
        {
            ID = _ID;
            cur_pos = _cur_pos;
            pre_pos = TSVector2.zero;
            radius = _radius;
            moveDir = _moveDir;
            moveSpeed = _moveSpeed;
        }
        public BallObj()
        {

        }


        public void Awake()
        {
            ballrender = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            UpdateBallDraw();
        }
        // Use this for initialization
        public void Start()
        {

        }

        // Update is called once per frame
        public void Update()
        {
            UpdateBallDraw();
        }

        public void UpdateBallDraw()
        {
            ballrender.transform.position = new Vector3(cur_pos.x.AsFloat(), 0, cur_pos.y.AsFloat());
        }

        public TSVector2 PredictPos(FP deltaTime)
        {
            return cur_pos + moveDir * moveSpeed * deltaTime;
        }

        public void UpdateBallPos(FP deltaTime)
        {
            pre_pos = cur_pos;
            cur_pos += moveDir * moveSpeed * deltaTime;
            //CheckBound();
        }

        public int ID=0;
        public TSVector2 cur_pos=TSVector2.zero;
        public TSVector2 pre_pos=TSVector2.zero;
        public FP radius=0.5;
        public TSVector2 moveDir=TSVector2.zero;
        public FP moveSpeed=10;

        public GameObject ballrender;
    }
}
