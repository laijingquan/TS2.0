using UnityEngine;
using System.Collections;
using TrueSync;

public class PoolPhycisEngine : MonoBehaviour {

	// Use this for initialization
	void Start ()
    {
	
	}
	
	// Update is called once per frame
	void Update ()
    {
	
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
    public TSVector2 dir;
    public FP radius;
    public FP timeDelta;
}

/// <summary>
/// 静态圆表示
/// </summary>
public class CircleData
{
    public TSVector2 cur_pos;
    public FP rudius;
}


public class Detection
{
    /// <summary>
    /// 圆和边的动态相交检测
    /// </summary>
    /// <param name="tedge"></param>
    /// <param name="crd"></param>
    /// <returns>是否在该段时间内相交</returns>
    public bool CheckCircle_LineCollision(tableEdge tedge,CircleRunData crd,ref FP t_percent)
    {
        //Sc
        TSVector2 Sc = PointToLineDir(tedge.start, tedge.end, crd.cur_pos);
        //Se
        TSVector2 Se = PointToLineDir(tedge.start, tedge.end, crd.cur_pos);

        TSVector2 Scnormal = TSVector2.Normalize(Sc);
        TSVector2 Senormal = TSVector2.Normalize(Se);
        //只有两种结果 同向和 反向
        FP result = TSVector2.Dot(Sc, Senormal);
        FP Scnorm = TSVector2.Dot(Scnormal, Scnormal);//Sc模平方
        FP Senorm = TSVector2.Dot(Senormal, Senormal);//Se模平方
        FP radius_square = crd.radius * crd.radius;

        if (result>0&& Scnorm > radius_square && Senorm > radius_square)//Sc,Se同向，俩圆圆半径大于到直线距离,不相交
        {
            return false;
        }
        else//相交 求t
        {
            TSVector2 sce = Sc - Se;
            FP S = TSMath.Sqrt( TSVector2.Dot(sce, sce));
            t_percent = (Scnorm-crd.radius) / S;//圆心到达撞击点的距离/圆心经过的总距离 来求出时间占比
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
    public TSVector2 PointToLineDir(TSVector2 start,TSVector2 end,TSVector2 circlePos)
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
    /// 圆和圆的动态相交检测
    /// </summary>
    /// <param name="cd"></param>
    /// <param name="crd"></param>
    /// <returns></returns>
    public PoolCollision CheckCircle_CircleCollision(CircleData cd,CircleRunData crd)
    {
        return null;
    }

}
