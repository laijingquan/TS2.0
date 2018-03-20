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
            moveSpeed = _moveSpeed;
            moveDir = _moveDir*moveSpeed;
        }
        public BallObj()
        {

        }


        public void Awake()
        {
            //ballrender = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            ballrender = GameObject.Instantiate(Resources.Load("Sphere") as GameObject);
            var text = ballrender.transform.FindChild("Text");
            text.transform.GetComponent<TextMesh>().text = ID.ToString();
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

        public TSVector2 PredictPos(FP _deltaTime)
        {
            return cur_pos + moveDir /** moveSpeed*/ * _deltaTime;
        }
        public TSVector2 PredictPos()
        {
            return cur_pos + moveDir/* * moveSpeed*/ * deltaTime;
        }
        public void UpdateBallPos(FP _deltaTime)
        {
            if (_deltaTime < 0) return;
            pre_pos = cur_pos;
            cur_pos += moveDir /** moveSpeed*/ * _deltaTime;
            deltaTime -= _deltaTime;//更新剩余时间
            if (deltaTime < 0)
                deltaTime = 0;
            //CheckBound();
        }

        public BallObj(BallObj other)
        {
            ID = other.ID;
            cur_pos = other.cur_pos;
            pre_pos = other.pre_pos;
            radius = other.radius;
            moveDir = other.moveDir;
            moveSpeed = other.moveSpeed;
        }

        public int ID=0;
        public TSVector2 cur_pos=TSVector2.zero;
        public TSVector2 pre_pos=TSVector2.zero;
        public FP radius=0.5;
        public TSVector2 moveDir=TSVector2.zero;
        public FP moveSpeed=10;
        public FP deltaTime=0;
        public bool lockcheck = false;
        //public bool isSleep = true;
        public GameObject ballrender;

    }
}
