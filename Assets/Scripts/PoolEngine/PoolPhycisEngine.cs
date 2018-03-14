using UnityEngine;
using System.Collections;
using TrueSync;
using System;

public class PoolPhycisEngine : MonoBehaviour {

    private tableEdge[] tableEdges = new tableEdge[4] { new tableEdge(), new tableEdge(), new tableEdge(), new tableEdge() };
    private FP tableWidth=10;
    private FP tableHeight=5;
    private CircleData ball = new CircleData();

    private TSVector2 moveDir = TSVector2.zero;
    private FP moveSpeed = 10;

    CircleRunData crd = new CircleRunData();

    GameObject ballObj;
    private void Awake()
    {
        //球
        ball.cur_pos = TSVector2.zero;
        ball.radius = 0.5;

        //上
        tableEdges[0].start = new TSVector2(-tableWidth/2, tableHeight/2);
        tableEdges[0].end = new TSVector2(tableWidth/2, tableHeight/2);

        //下
        tableEdges[1].start = new TSVector2(-tableWidth / 2, -tableHeight / 2);
        tableEdges[1].end = new TSVector2(tableWidth / 2, -tableHeight / 2);

        //左
        tableEdges[2].start = tableEdges[1].start;
        tableEdges[2].end = tableEdges[0].start;

        //右
        tableEdges[3].start = tableEdges[1].end;
        tableEdges[3].end = tableEdges[0].end;

        CreateTable();
        CreateBall();
    }

    GameObject createEdgePos()
    {
        return GameObject.CreatePrimitive(PrimitiveType.Cube);
    }
    void CreateTable()
    {
        var up1 = createEdgePos();
        up1.transform.position = new Vector3(tableEdges[0].start.x.AsFloat(),0,tableEdges[0].start.y.AsFloat());

        var up2 = createEdgePos();
        up2.transform.position = new Vector3(tableEdges[0].end.x.AsFloat(), 0, tableEdges[0].end.y.AsFloat());

        var down1 = createEdgePos();
        down1.transform.position = new Vector3(tableEdges[1].start.x.AsFloat(), 0, tableEdges[1].start.y.AsFloat());

        var down2 = createEdgePos();
        down2.transform.position = new Vector3(tableEdges[1].end.x.AsFloat(), 0, tableEdges[1].end.y.AsFloat());
    }

    void CreateBall()
    {
        ballObj = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        UpdateBallDraw();
    }

    void UpdateBallDraw()
    {
        ballObj.transform.position = new Vector3(ball.cur_pos.x.AsFloat(),0,ball.cur_pos.y.AsFloat());
    }
    // Use this for initialization
    void Start ()
    {
        moveDir = new TSVector2(1, 0.3).normalized;
	}

    private bool control = true;
	// Update is called once per frame
	void Update ()
    {
        if (control)
            UpdatePhysicStep(Time.deltaTime);//逻辑层
        else
            UpdateBallPos(Time.deltaTime);
        UpdateBallDraw();//渲染层
	}

    TSVector2 PredictPos(FP deltaTime)
    {
        return ball.cur_pos + moveDir * moveSpeed * deltaTime;
    }

    void UpdateBallPos(FP deltaTime)
    {
        ball.cur_pos+= moveDir * moveSpeed * deltaTime;
    }

    private static int testnumber = 0;
    void UpdatePhysicStep(FP deltaTime)
    {
        bool step = true;

        Func<FP, FP, FP, bool> checkBound = (pos, start, end) =>
        {
            if (pos > start && pos < end)
                return true;

            else
                return false;
        };

        Action<FP,TSVector2,tableEdge> updateDirAndTime = (_percent,_hitPos,_tbe) =>{
            UpdateBallPos(deltaTime * _percent);//先更新到撞击点
            deltaTime = deltaTime - deltaTime * _percent;//更新剩余时间
            var curReflcDir = Detection.CheckCircle_LineCollision(_tbe, _hitPos, ball.radius, moveDir);//计算碰撞响应
            moveDir = curReflcDir.normalized;//更新实时方向
            //UpdateBallPos(deltaTime); 在这里更新 如果速度过快 那么会直接跑到球桌
        };
        while (step)
        {
            testnumber++;
            if(testnumber>3)
            {
                Debug.Log("大于3次检测");
            }
            //step = false;
            TSVector2 nextPos = PredictPos(deltaTime);
            crd.cur_pos = ball.cur_pos;
            crd.next_pos = nextPos;
            crd.radius = ball.radius;
            //TSVector2 curMoveDir = nextPos - ball.cur_pos;
            FP t_percent = 0;
            TSVector2 hitPos = TSVector2.zero;

            TSVector2 predictEndPos = ball.cur_pos + moveDir * 100;

            //在当前速度下,预测圆最先和哪条边碰撞
            for (int i =0;i<tableEdges.Length;i++)
            {
                if (Detection.CheckSegement_Contact(ball.cur_pos, predictEndPos, tableEdges[i].start, tableEdges[i].end))
                {
                    if (Detection.CheckCircle_LineContact(tableEdges[i], crd, ref t_percent))
                    {
                        updateDirAndTime(t_percent, hitPos, tableEdges[i]);
                    }
                    else
                    {
                        testnumber = 0;
                        step = false;
                        UpdateBallPos(deltaTime);//这次更新后 无任何碰撞
                    }
                    break;//每次只能有一边能碰撞
                }
            }
            //if (Detection.CheckCircle_LineContact(tableEdges[0], crd, ref t_percent) && checkBound((hitPos = PredictPos(deltaTime * t_percent)).x, tableEdges[0].start.x, tableEdges[0].end.x))
            //{
            //    updateDirAndTime(t_percent,hitPos,tableEdges[0]);
            //    //control = false;
            //}
            //else if (Detection.CheckCircle_LineContact(tableEdges[1], crd, ref t_percent) && checkBound((hitPos = PredictPos(deltaTime * t_percent)).x, tableEdges[1].start.x, tableEdges[1].end.x))
            //{
            //    updateDirAndTime(t_percent, hitPos, tableEdges[1]);
            //}
            //else if (Detection.CheckCircle_LineContact(tableEdges[2], crd, ref t_percent) && checkBound((hitPos = PredictPos(deltaTime * t_percent)).y, tableEdges[2].start.y, tableEdges[2].end.y))
            //{
            //    updateDirAndTime(t_percent, hitPos, tableEdges[2]);
            //}
            //else if (Detection.CheckCircle_LineContact(tableEdges[3], crd, ref t_percent) && checkBound((hitPos = PredictPos(deltaTime * t_percent)).y, tableEdges[3].start.y, tableEdges[3].end.y))
            //{
            //    updateDirAndTime(t_percent, hitPos, tableEdges[3]);
            //}
            //else
            //{
            //    step = false;
            //    UpdateBallPos(deltaTime);//这次更新后 无任何碰撞
            //}
        }
    }

}

public class tableEdge
{
    public TSVector2 start;
    public TSVector2 end;
}

public class PoolCollision
{
    public TSVector2 point;
    public TSVector2 normal;
}

/// <summary>
/// 动态数据，表示圆正在运动的数据。当前速度和位置，下一帧的位置
/// </summary>
public class CircleRunData
{
    public TSVector2 cur_pos;
    public TSVector2 next_pos;
    public FP radius;
}

/// <summary>
/// 静态圆表示
/// </summary>
public class CircleData
{
    public TSVector2 cur_pos;
    public FP radius;
}


public class Detection
{
    /// <summary>
    /// 圆和边的动态相交检测
    /// </summary>
    /// <param name="tedge"></param>
    /// <param name="crd"></param>
    /// <returns>是否在该段时间内相交</returns>
    public static bool CheckCircle_LineContact(tableEdge tedge,CircleRunData crd,ref FP t_percent)
    {
        //Sc
        TSVector2 Sc = PointToLineDir(tedge.start, tedge.end, crd.cur_pos);
        //Se
        TSVector2 Se = PointToLineDir(tedge.start, tedge.end, crd.next_pos);

        TSVector2 Scnormal = TSVector2.Normalize(Sc);
        TSVector2 Senormal = TSVector2.Normalize(Se);
        //TSVector2 Scnormal = Sc.normalized;
        //TSVector2 Senormal = Se.normalized;
        //只有两种结果 同向和 反向
        FP result = TSVector2.Dot(Scnormal, Senormal);//1同向,0垂直,-1反向
        FP Scnorm = TSMath.Sqrt(TSVector2.Dot(Sc, Sc));//Sc模
        FP Senorm = TSMath.Sqrt(TSVector2.Dot(Se, Se));//Se模
        //FP radius_square = crd.radius * crd.radius;

        if (result>0&& Scnorm > crd.radius && Senorm > crd.radius)//Sc,Se同向，俩圆圆半径大于到直线距离,不相交
        {
            return false;
        }
        else//相交 求t
        {
            FP S = 0;
            if(result>0)
            {
                S = Scnorm - Senorm;
            }
            else
            {
                S = Scnorm + Senorm;
            }
            //TSVector2 sce = Sc - Se;
            //FP S = TSMath.Sqrt( TSVector2.Dot(sce, sce));
            t_percent = (Scnorm- crd.radius) / S;//圆心到达撞击点的距离/圆心经过的总距离 来求出时间占比
            return true;
        }
    }

    /// <summary>
    /// 求圆心到线段的有向距离
    /// </summary>
    /// <param name="start">线段起点</param>
    /// <param name="end">线段终点</param>
    /// <param name="circlePos">圆心位置</param>
    /// <returns></returns>
    public static TSVector2 PointToLineDir(TSVector2 start,TSVector2 end,TSVector2 circlePos)
    {
        TSVector2 C = circlePos;
        TSVector2 A = start;
        TSVector2 B = end;
        TSVector2 CA = A - C;
        TSVector2 AC = C - A;
        TSVector2 AB = B - A;
        TSVector2 ABnormal = TSVector2.Normalize(AB);
        TSVector2 AO = TSMath.Abs(TSVector2.Dot(AC, ABnormal)) * ABnormal;
        TSVector2 CO= CA + AO;
        return CO;
    }

    /// <summary>
    /// 圆碰撞的反弹方向
    /// </summary>
    /// <param name="tedge">线段</param>
    /// <param name="circlePos">圆心</param>
    /// <param name="radius">半径</param>
    /// <param name="moveDir">圆的速度方向</param>
    /// <returns></returns>
    public static TSVector2 CheckCircle_LineCollision(tableEdge tedge, TSVector2 circlePos, FP radius, TSVector2 moveDir)
    {
        TSVector2 C = circlePos;
        TSVector2 A = tedge.start;
        TSVector2 B = tedge.end;
        TSVector2 V = moveDir;
        TSVector2 AC = C - A;
        TSVector2 AB = B - A;
        TSVector2 ABnormal = TSVector2.Normalize(AB);
        TSVector2 HC = AC - ABnormal * TSMath.Abs( TSVector2.Dot(AC,ABnormal) );
        TSVector2 HCnormal = TSVector2.Normalize(HC);
        FP VP = TSMath.Abs( TSVector2.Dot(V, HCnormal) );
        TSVector2 VF = V + HCnormal * VP * 2;//反射方向

        return VF;
    }

    /// <summary>
    /// 检测俩线段是否有交点,如果是预测圆的走向是否和线段有交点,那么线段的俩端点需要延长圆半径长度
    /// </summary>
    /// <param name="start1">圆心</param>
    /// <param name="end1">圆心目标点</param>
    /// <param name="start2">线段起点</param>
    /// <param name="end2">线段终点</param>
    /// <returns></returns>
    public static bool CheckSegement_Contact(TSVector2 start1,TSVector2 end1,TSVector2 start2,TSVector2 end2)
    {
        TSVector2 A = start1;
        TSVector2 B = end1;
        TSVector2 C = start2;
        TSVector2 D = end2;
        TSVector2 AC = C - A;
        TSVector2 AD = D - A;
        TSVector2 BC = C - B;
        TSVector2 BD = D - B;
        TSVector2 CA = A - C;
        TSVector2 CB = B - C;
        TSVector2 DA = A - D;
        TSVector2 DB = B - D;

        return vector_product(AC, AD) * vector_product(BC, BD) <= 0 && vector_product(CA, CB) * vector_product(DA, DB) <= 0;
    }

    public static FP vector_product(TSVector2 va,TSVector2 vb)
    {
        return va.x * vb.y - vb.x * va.y;
    }

    /// <summary>
    /// 圆和圆的动态相交检测
    /// </summary>
    /// <param name="cd"></param>
    /// <param name="crd"></param>
    /// <returns></returns>
    //public PoolCollision CheckCircle_CircleCollision(CircleData cd,CircleRunData crd)
    //{
    //    return null;
    //}
}
