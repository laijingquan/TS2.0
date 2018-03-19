using System;
using TrueSync;

namespace PoolEngine
{
    public class Detection
    {
        /// <summary>
        /// 计算球和球的互相作用方向
        /// </summary>
        /// <returns></returns>
        public static TSVector2[] CheckCircle_CircleCollision(CircleRunData runCircle, CircleRunData staticCircle)
        {
            TSVector2 V = runCircle.next_pos - runCircle.cur_pos;
            TSVector2 U = staticCircle.next_pos - staticCircle.cur_pos;
            TSVector2 AB = staticCircle.cur_pos - runCircle.cur_pos;
            TSVector2 BA = runCircle.cur_pos - staticCircle.cur_pos;
            TSVector2 Vx = TSVector2.Dot(V, AB.normalized)*AB.normalized;
            TSVector2 Vy = V - Vx;
            TSVector2 Ux = TSVector2.Dot(U, BA.normalized) * BA.normalized;
            TSVector2 Uy = U - Ux;
            V = Ux + Vy;//反弹后的方向
            U = Vx + Uy;//反弹后的方向
            return new TSVector2[2] { V,U };
        }
        public static TSVector2[] CheckCircle_CircleCollision(TSVector2 V, TSVector2 U)
        {
            TSVector2 Vx = new TSVector2(V.x, 0);
            TSVector2 Vy = new TSVector2(0, V.y);
            TSVector2 Ux = new TSVector2(U.x, 0);
            TSVector2 Uy = new TSVector2(0, U.y);
            V = Ux + Vy;//反弹后的方向
            U = Vx + Uy;//反弹后的方向
            return new TSVector2[2] { V, U };
        }
        /// <summary>
        /// 圆和圆的动态相交检测(根据相对运动,抽象为一方是运动,另一方是静止)
        /// </summary>
        /// <param name="cd"></param>
        /// <param name="crd"></param>
        /// <returns></returns>
        public static bool CheckCircle_CircleContact(CircleRunData runCircle, CircleRunData staticCircle,FP deltaTime,ref FP _percent)
        {
            TSVector2 VA = runCircle.next_pos - runCircle.cur_pos;
            TSVector2 VB = staticCircle.next_pos - staticCircle.cur_pos;
            //两个运动方向描述为一方运动另一方静止 so
            TSVector2 VAB = VA - VB;//runCircle相对于staticCircle的运动方向pc
            TSVector2 Idir = staticCircle.cur_pos - runCircle.cur_pos;//射线起点到静态圆的方向
            FP Idir_length_square = TSVector2.Dot(Idir, Idir);
            FP static_radius_square = (staticCircle.radius * 2)* (staticCircle.radius * 2);

            Func<FP> calHitInfo = () =>
            //可以在返回true的时候再计算，后优化
            {
                TSVector2 e_dir = staticCircle.cur_pos - runCircle.cur_pos;
                //TSVector2 d_dir = Idir/*runCircle.next_pos - runCircle.cur_pos*/;
                //TSVector2 d_dirnormal = VAB.normalized;
                FP a_projvalue = TSMath.Abs(TSVector2.Dot(e_dir, VAB.normalized));
                FP b_squar = TSVector2.Dot(e_dir, e_dir) - a_projvalue * a_projvalue;
                FP f = TSMath.Sqrt(static_radius_square - b_squar);
                FP t = a_projvalue - f;//碰撞到静态圆所走的路程，总路程是runCircle.cur_pos+VAB*delataTime;
                return t /*/ (VAB * deltaTime).magnitude*/;//求出占比
            };

            if (Idir_length_square < static_radius_square)//射线起点在圆心内部,相交
            {
                _percent =  calHitInfo();
                //_percent = 1;//一开始就相交的
                return false;
            }
            else//射线起点在圆心外部的情况
            {
                FP s = TSVector2.Dot(Idir, VAB.normalized);
                if(s<0)//球体位于射线原点的后面 不相交
                {
                    return false;
                }
                else
                {
                    FP m_square = Idir_length_square - s * s;//球心到投影点距离的平方
                    if (m_square > static_radius_square) //预测不相交
                    {
                        return false;
                    }
                    else
                    {
                        var t = calHitInfo();
                        if(t<=VAB.magnitude)
                        {
                            _percent = t / VAB.magnitude;
                            return true;
                        }
                        return false;
                    }
                }
            }
        }

        /// <summary>
        /// 圆和边的动态相交检测
        /// </summary>
        /// <param name="tedge"></param>
        /// <param name="crd"></param>
        /// <returns>是否在该段时间内相交</returns>
        public static bool CheckCircle_LineContact(tableEdge tedge, CircleRunData crd, ref FP t_percent)
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

            FP Scnorm = TSMath.Sqrt(TSMath.Abs(TSVector2.Dot(Sc, Sc)));//Sc模
            FP Senorm = TSMath.Sqrt(TSMath.Abs(TSVector2.Dot(Se, Se)));//Se模
                                                                       //FP radius_square = crd.radius * crd.radius;

            if (result > 0 && Scnorm > crd.radius && Senorm > crd.radius)//Sc,Se同向，俩圆圆半径大于到直线距离,不相交
            {
                return false;
            }
            else//相交 求t
            {
                FP S = 0;
                if (result > 0)
                {
                    S = Scnorm - Senorm;
                }
                else
                {
                    S = Scnorm + Senorm;
                }
                //TSVector2 sce = Sc - Se;
                //FP S = TSMath.Sqrt( TSVector2.Dot(sce, sce));
                t_percent = (Scnorm - crd.radius) / S;//圆心到达撞击点的距离/圆心经过的总距离 来求出时间占比
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
        public static TSVector2 PointToLineDir(TSVector2 start, TSVector2 end, TSVector2 circlePos)
        {
            TSVector2 C = circlePos;
            TSVector2 A = start;
            TSVector2 B = end;
            TSVector2 CA = A - C;
            TSVector2 AC = C - A;
            TSVector2 AB = B - A;
            TSVector2 ABnormal = TSVector2.Normalize(AB);
            TSVector2 AO = TSVector2.Dot(AC, ABnormal) * ABnormal;
            TSVector2 CO = CA + AO;
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
            TSVector2 A = tedge.farstart;
            TSVector2 B = tedge.farend;
            TSVector2 V = moveDir;
            TSVector2 AC = C - A;
            TSVector2 AB = B - A;
            TSVector2 ABnormal = TSVector2.Normalize(AB);
            TSVector2 HC = AC - ABnormal * TSMath.Abs(TSVector2.Dot(AC, ABnormal));
            TSVector2 HCnormal = TSVector2.Normalize(HC);
            FP VP = TSMath.Abs(TSVector2.Dot(V, HCnormal));
            TSVector2 VF = V + HCnormal * VP * 2;//反射方向

            return VF;
        }

        #region 线段相交算法1
        /// <summary>
        /// 检测俩线段是否有交点,如果是预测圆的走向是否和线段有交点,那么线段的俩端点需要延长圆半径长度
        /// </summary>
        /// <param name="start1">圆心</param>
        /// <param name="end1">圆心目标点</param>
        /// <param name="start2">线段起点</param>
        /// <param name="end2">线段终点</param>
        /// <returns></returns>
        public static bool CheckSegement_Contact(TSVector2 start1, TSVector2 end1, TSVector2 start2, TSVector2 end2)
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

            var r1 = vector_product(AC, AD);
            var r2 = vector_product(BC, BD);
            var r3 = vector_product(CA, CB);
            var r4 = vector_product(DA, DB);
            //这里直接相乘可以会溢出 所以暂时这样判断
            if ((r1 > 0 && r2 < 0 || r1 < 0 && r2 > 0) && (r3 > 0 && r4 < 0 || r3 < 0 && r4 > 0))
            {
                return true;
            }
            else
            {
                return false;
            }
            //return r1*r2 <= 0 && r3*r4 <= 0
        }

        public static FP vector_product(TSVector2 va, TSVector2 vb)
        {
            return va.x * vb.y - vb.x * va.y;
        }
        #endregion
        #region 线段相交算法2
        public static FP cross(TSVector2 A, TSVector2 B, TSVector2 C)
        {
            FP cross1 = (C.x - A.x) * (B.y - A.y);
            FP cross2 = (C.y - A.y) * (B.x - A.x);
            return (cross1 - cross2);
        }

        public static bool rectsIntersect(TSVector2 S1, TSVector2 E1, TSVector2 S2, TSVector2 E2)
        {
            if (TSMath.Min(S1.y, E1.y) <= TSMath.Max(S2.y, E2.y) &&
                    TSMath.Max(S1.y, E1.y) >= TSMath.Min(S2.y, E2.y) &&
                    TSMath.Min(S1.x, E1.x) <= TSMath.Max(S2.x, E2.x) &&
                    TSMath.Max(S1.x, E1.x) >= TSMath.Min(S2.x, E2.x))
            {
                return true;
            }
            return false;
        }

        public static bool segmentsIntersect(TSVector2 A1, TSVector2 A2, TSVector2 B1, TSVector2 B2)
        {
            FP T1 = cross(A1, A2, B1);
            FP T2 = cross(A1, A2, B2);
            FP T3 = cross(B1, B2, A1);
            FP T4 = cross(B1, B2, A2);
            if (((T1 * T2) > 0) || ((T3 * T4) > 0))
            {    // 一条线段的两个端点在另一条线段的同侧，不相交。（可能需要额外处理以防止乘法溢出，视具体情况而定。）
                return false;
            }
            else if (T1 == 0 && T2 == 0)
            {             // 两条线段共线，利用快速排斥实验进一步判断。此时必有 T3 == 0 && T4 == 0。
                return rectsIntersect(A1, A2, B1, B2);
            }
            else
            {                                    // 其它情况，两条线段相交。
                return true;
            }
        }
        #endregion
    }
}
