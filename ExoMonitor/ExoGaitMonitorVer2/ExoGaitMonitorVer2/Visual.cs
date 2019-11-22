using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using lvbo;
using MathWorks.MATLAB.NET.Arrays;
using System.Windows.Controls.Primitives;
using System.Windows.Controls;
using System.Windows.Media;


namespace ExoGaitMonitorVer2
{
    class Visual
    {
        // 根据视觉反馈的数据生成步态

        private double L1 = 0.4;//L1是大腿长度
        private double L2 = 0.4;//L2小腿长度
        private int T = 2;//T为步态周期

        private double x0_sum = 0; // 髋关节累积位移
        private double xa_sum = 0; // 右踝关节累积位移
        private double xc_sum = 0; // 左踝关节累积位移


        public void visualGaitGenerator(int nStep, double normalStepLength, double normalStepHeight,
                                         double lastStepLength, double lastStepHeight, double overStepLength,
                                         double overStepHeight, StatusBar statusBar, TextBlock statusInfoTextBlock) // 参数单位要求：米
        //public void visualGaitGenerator(int nStep, double normalStepLength, double normalStepHeight,
        //                         double lastStepLength, double lastStepHeight, double overStepLength,
        //                         double overStepHeight) // 参数单位要求：米
        {   
            int numPoints; // 步态数组点数

            // 确定步态数组点数（最后都要加100个点用于滤波）
            if (nStep == 0) // 障碍较近只需最后一步接近的情况
            { 
                if (overStepHeight == 0) // 障碍较高无法跨越
                {
                    // plan和finish阶段分别有100个点
                    numPoints = 100 * 2; 
                }
                else // 障碍较低能够跨越
                {
                    // plan和finish阶段分别有100个点, 1 * 200为change段的点数（因为仅有跨越，只有小步转跨越的change）
                    numPoints = 100 * 2 + 1 * 200;
                }
            }
            else // 障碍较远需要跨几步接近障碍
            {
                if (overStepHeight == 0) // 障碍较高无法跨越
                {
                    // plan和finish阶段分别有100个点,  (nStep - 1) * 200为normal阶段的点数， 1 * 200为change段的点数（因为无跨越，只有normal转小步的change）
                    numPoints = 100 * 2 + nStep * 200 + 1 * 200;
                }
                else // 障碍较低能够跨越
                {
                    // plan和finish阶段分别有100个点,  (nStep - 1) * 200为normal阶段的点数， 2 * 200为change段的点数（有normal转小步和小步转跨越的两段change）
                    numPoints = 100 * 2 + nStep * 200 + 2 * 200;
                }
            }

            double[] t = new double[numPoints + 3];
            double[] xa = new double[numPoints + 2];//xa ya 为右脚位置
            double[] ya = new double[numPoints + 2];
            double[] x0 = new double[numPoints + 2];//x0 y0 为髋关节位置
            double[] y0 = new double[numPoints + 2];
            double[] xc = new double[numPoints + 2];//xa ya 为左脚位置
            double[] yc = new double[numPoints + 2];
            double[] xd = new double[numPoints + 2];//xd yd 为左膝关节位置
            double[] yd = new double[numPoints + 2];
            double[] xb = new double[numPoints + 2];//xb yb  位右膝位置
            double[] yb = new double[numPoints + 2];
            // 不滤波
            //double[] a_hl = new double[numPoints + 2];//左髋角度
            //double[] a_hr = new double[numPoints + 2];//右髋角度
            //double[] a_kl = new double[numPoints + 2];//左膝角度
            //double[] a_kr = new double[numPoints + 2];//右膝角度
            // 滤波
            double[] a_hl = new double[numPoints + 12];//左髋角度
            double[] a_hr = new double[numPoints + 12];//右髋角度
            double[] a_kl = new double[numPoints + 12];//左膝角度
            double[] a_kr = new double[numPoints + 12];//右膝角度

            double L;
            double theat1;
            double theat2;
            double x1, x2, x3, x4, y1, y2, y3, y4;

            #region 生成步态数据
            // 生成步态数据
            if (nStep == 0)
            {
                #region 近障碍且无跨越
                if (overStepHeight == 0)
                {
                    // planning
                    // planning和finish段的步长是他衔接段步长的一半
                    double zb = lastStepHeight;
                    double sb = lastStepLength / 2;
                    double vb = sb / (T / 2);

                    for (int i = 1; i <= 101; i++)
                    {
                        //xa ya为右脚
                        xa[i] = 0;
                        ya[i] = 0;
                        //x0 y0髋关节
                        x0[i] = (vb / 2) * t[i];
                        y0[i] = (((L1 + L2) - Math.Sqrt(Math.Pow((L1 + L2), 2) - Math.Pow((sb / 2), 2))) / 2) * Math.Sin((2 * Math.PI / sb) * x0[i] + (Math.PI / 2)) + (L1 + L2 + Math.Sqrt(Math.Pow((L1 + L2), 2) - Math.Pow((sb / 2), 2))) / 2;
                        //xc yc为左脚
                        xc[i] = vb * t[i];
                        yc[i] = (zb / 2) * Math.Sin((2 * Math.PI / (sb)) * xc[i] - (Math.PI / 2)) + (zb / 2);

                        t[i + 1] = t[i] + 0.01;
                    }

                    for (int i = 0; i < t.Length; i++)
                    {
                        // 将时间置零
                        t[i] = 0;
                    }

                    xa_sum = xa[101];
                    x0_sum = x0[101];
                    xc_sum = xc[101];

                    // finish
                    double zf = lastStepHeight;
                    double sf = lastStepLength / 2;
                    double vf = sf / (T / 2);

                    for (int i = 101; i <= 201; i++)
                    {
                        xa[i] = vf * t[i] + xa_sum; // 右脚踝关节累计位移 0
                        ya[i] = (zf / 2) * Math.Sin((2 * Math.PI / (sf)) * (xa[i] - xa_sum) - (Math.PI / 2)) + (zf / 2);

                        x0[i] = (vf / 2) * t[i] + (x0_sum); // 髋关节累计位移 sb/2
                        y0[i] = (((L1 + L2) - Math.Sqrt(Math.Pow((L1 + L2), 2) - Math.Pow((sf / 2), 2))) / 2) * Math.Sin((2 * Math.PI / sf) * (x0[i] - (x0_sum)) - (Math.PI / 2)) + (L1 + L2 + Math.Sqrt(Math.Pow((L1 + L2), 2) - Math.Pow((sf / 2), 2))) / 2;

                        xc[i] = xc_sum; // 左脚踝关节累计位移 sb
                        yc[i] = 0;

                        t[i + 1] = t[i] + 0.01;
                    }

                    // 膝关节角度
                    for (int i = 1; i <= 201; i++)
                    {
                        // 求解左膝关节位置
                        L = Math.Sqrt(Math.Pow((xc[i] - x0[i]), 2) + Math.Pow((yc[i] - y0[i]), 2));
                        theat1 = Math.Acos((Math.Pow(L2, 2) + Math.Pow(L, 2) - Math.Pow(L1, 2)) / (2 * L* L2));
                        theat2 = Math.Atan((y0[i] - yc[i]) / (x0[i] - xc[i]));
                        if (xc[i] > x0[i])
                        {
                            x3 = xc[i] - L2 * Math.Cos(theat2 - theat1);
                            x4 = xc[i] - L2 * Math.Cos(theat2 + theat1);
                            y3 = yc[i] + Math.Abs(L2 * Math.Sin(theat2 - theat1));
                            y4 = yc[i] + Math.Abs(L2 * Math.Sin(theat2 + theat1));
                        }
                        else
                        {
                            x3 = xc[i] + L2 * Math.Cos(theat2 - theat1);
                            x4 = xc[i] + L2 * Math.Cos(theat2 + theat1);
                            y3 = yc[i] + Math.Abs(L2 * Math.Sin(theat2 - theat1));
                            y4 = yc[i] + Math.Abs(L2 * Math.Sin(theat2 + theat1));
                        }
                        xd[i] = Math.Max(x3, x4);
                        yd[i] = Math.Max(y3, y4);

                        // 求解右膝关节位置
                        L = Math.Sqrt(Math.Pow((xa[i] - x0[i]), 2) + Math.Pow((ya[i] - y0[i]), 2));
                        theat1 = Math.Acos((Math.Pow(L2, 2) + Math.Pow(L, 2) - Math.Pow(L1, 2)) / (2 *L * L2));
                        theat2 = Math.Atan((y0[i] - ya[i]) / (x0[i] - xa[i]));
                        if (xa[i] < x0[i])
                        {
                            x1 = xa[i] + L2 * Math.Cos(theat2 - theat1);
                            x2 = xa[i] +L2 * Math.Cos(theat2 + theat1);
                            y1 = ya[i] + Math.Abs(L2 * Math.Sin(theat2 - theat1));
                            y2 = ya[i] + Math.Abs(L2 * Math.Sin(theat2 + theat1));
                        }
                        else
                        {
                            x1 = xa[i] - L2 * Math.Cos(theat2 - theat1);
                            x2 = xa[i] - L2 * Math.Cos(theat2 + theat1);
                            y1 = ya[i] + Math.Abs(L2 * Math.Sin(theat2 - theat1));
                            y2 = ya[i] + Math.Abs(L2 * Math.Sin(theat2 + theat1));
                        }
                        xb[i] = Math.Max(x1, x2);
                        yb[i] = Math.Max(y1, y2);

                        // 防止出现NaN
                        if (double.IsNaN(xd[i]))
                        {
                            xd[i] = xd[i - 1];
                        }
                        if (double.IsNaN(yd[i]))
                        {
                            yd[i] = yd[i - 1];
                        }
                        if (double.IsNaN(xb[i]))
                        {
                            xb[i] = xb[i - 1];
                        }
                        if (double.IsNaN(yb[i]))
                        {
                            yb[i] = yb[i - 1];
                        }

                        // 求解角度
                        a_hl[i] = Math.Atan((xd[i] - x0[i]) / (y0[i] - yd[i]));
                        a_hl[i] = a_hl[i] * 180 / Math.PI;

                        a_hr[i] = Math.Atan((xb[i] - x0[i]) / (y0[i] - yb[i]));
                        a_hr[i] = a_hr[i] * 180 / Math.PI;

                        a_kl[i] = Math.Atan((xc[i] - xd[i]) / (yd[i] - yc[i]));
                        a_kl[i] = a_hl[i] - (a_kl[i] * 180 / Math.PI);
                        if (a_kl[i] < 0)
                        {
                            a_kl[i] = 0; // 膝关节角度不能小于0
                        }

                        a_kr[i] = Math.Atan((xa[i] - xb[i]) / (yb[i] - ya[i]));
                        a_kr[i] = a_hr[i] - (a_kr[i] * 180 / Math.PI);
                        if (a_kr[i] < 0)
                        {
                            a_kr[i] = 0; // 膝关节角度不能小于0
                        }
                    }

                    // 生成辅助滤波用数据
                    for (int i = 202; i <= 211; i++)
                    {
                        a_hl[i] = a_hl[i - 1] - 0.0002;
                        a_hr[i] = a_hr[i - 1] - 0.002;
                        a_kr[i] = a_kr[i - 1] - 0.005;
                        a_kl[i] = a_kl[i - 1] - 0.0002;
                    }

                    // 滤波
                    // 滤波左膝
                    MWArray array_kl = new MWNumericArray(a_kl);
                    lvbo.lvbofilt bo_kl = new lvbofilt();

                    object[] result_kl = bo_kl.lvbo(1, array_kl);
                    double[] ax_kl = new double[211];
                    ax_kl = (double[])((MWNumericArray)result_kl[0]).ToVector(MWArrayComponent.Real);
                    a_kl = ax_kl;

                    // 滤波左髋
                    MWArray array_hl = new MWNumericArray(a_hl);
                    lvbo.lvbofilt bo_hl = new lvbofilt();

                    object[] result_hl = bo_hl.lvbo(1, array_hl);
                    double[] ax_hl = new double[211];
                    ax_hl = (double[])((MWNumericArray)result_hl[0]).ToVector(MWArrayComponent.Real);
                    a_hl = ax_hl;

                    // 滤波右髋
                    MWArray array_hr = new MWNumericArray(a_hr);
                    lvbo.lvbofilt bo_hr = new lvbofilt();

                    object[] result_hr = bo_hr.lvbo(1, array_hr);
                    double[] ax_hr = new double[211];
                    ax_hr = (double[])((MWNumericArray)result_hr[0]).ToVector(MWArrayComponent.Real);
                    a_hr = ax_hr;

                    // 滤波右膝
                    MWArray array_kr = new MWNumericArray(a_kr);
                    lvbo.lvbofilt bo_kr = new lvbofilt();

                    object[] result_kr = bo_kr.lvbo(1, array_kr);
                    double[] ax_kr = new double[211];
                    ax_kr = (double[])((MWNumericArray)result_kr[0]).ToVector(MWArrayComponent.Real);
                    a_kr = ax_kr;

                    // 初始角度设为0防止启动抖动剧烈
                    a_kr[0] = 0;
                    a_kl[0] = 0;
                    a_hl[0] = 0;
                    a_hr[0] = 0;
                }
                #endregion

                #region 近障碍且有跨越
                else
                {
                    // planning
                    double zb = lastStepHeight;
                    double sb = lastStepLength / 2;
                    double vb = sb / (T / 2);

                    for (int i = 1; i <= 101; i++)
                    {
                        //xa ya为右脚
                        xa[i] = 0;
                        ya[i] = 0;
                        //x0 y0髋关节
                        x0[i] = (vb / 2) * t[i];
                        y0[i] = (((L1 + L2) - Math.Sqrt(Math.Pow((L1 + L2), 2) - Math.Pow((sb / 2), 2))) / 2) * Math.Sin((2 * Math.PI / sb) * x0[i] + (Math.PI / 2)) + (L1 + L2 + Math.Sqrt(Math.Pow((L1 + L2), 2) - Math.Pow((sb / 2), 2))) / 2;
                        //xc yc为左脚
                        xc[i] = vb * t[i];
                        yc[i] = (zb / 2) * Math.Sin((2 * Math.PI / (sb)) * xc[i] - (Math.PI / 2)) + (zb / 2);

                        t[i + 1] = t[i] + 0.01;
                    }

                    for (int i = 0; i < t.Length; i++)
                    {
                        // 将时间置零
                        t[i] = 0;
                    }

                    xa_sum = xa[101];
                    x0_sum = x0[101];
                    xc_sum = xc[101];

                    //change
                    double z = lastStepHeight;
                    double s = lastStepLength; // 改变前的步长
                    double zc = overStepHeight;
                    double sc = overStepLength; // 改变后的步长
                    double vc = sc / (T / 2);

                    for (int i = 101; i <= 151; i++)
                    {
                        //xa ya为右脚
                        xa[i] = (-2 * (s)) * Math.Pow(t[i], 3) + (3 * (s)) * Math.Pow(t[i], 2) + (xa_sum); //右踝关节累计位移为sb + nStep * s - 1 / 2 * s
                        //xa[i] = xa[i] - s / 2; // 未知原因xa会在此处突增s/2，这里减去这个偏移
                        ya[i] = (zc / 2) * Math.Sin((2 * Math.PI / (sc / 2 + s / 2)) * (xa[i] - (xa_sum)) - (Math.PI / 2)) + (zc / 2);
                        //x0 y0髋关节
                        x0[i] = ((-2 * s) * Math.Pow(t[i], 3) + (3 * s) * Math.Pow(t[i], 2)) / 2 + (x0_sum); // 髋关节累计位移为 sb + nStep * s - 1 / 4 * s
                        //x0[i] = x0[i] - s / 4; // 未知原因x0会在此处突增s/4，这里减去这个偏移
                        y0[i] = (((L1 + L2) - Math.Sqrt(Math.Pow((L1 + L2), 2) - Math.Pow((s / 4), 2))) / 2) * Math.Sin((2 * Math.PI / (s / 2)) * (x0[i] - (x0_sum)) - (Math.PI / 2)) + (L1 + L2 + Math.Sqrt(Math.Pow((L1 + L2), 2) - Math.Pow((s / 4), 2))) / 2;
                        //xc yc为左脚
                        xc[i] = xc_sum; // 左踝关节累计位移 sb + nStep * s
                        yc[i] = 0;

                        t[i + 1] = t[i] + 0.01;
                    }

                    for (int i = 151; i <= 201; i++)
                    {
                        //xa ya为右脚
                        xa[i] = (-2 * (sc)) * Math.Pow(t[i], 3) + (3 * (sc)) * Math.Pow(t[i], 2) + (xa_sum) - (sc - s) / 2; //踝关节累计位移为sb + nStep * s - 1 / 2 * s
                        //xa[i] = xa[i] - s / 2; // 未知原因xa会在此处突增s/2，这里减去这个偏移
                        ya[i] = (zc / 2) * Math.Sin((2 * Math.PI / (sc / 2 + s / 2)) * (xa[i] - (xa_sum)) - (Math.PI / 2)) + (zc / 2);
                        //x0 y0髋关节
                        x0[i] = ((-2 * sc) * Math.Pow(t[i], 3) + (3 * sc) * Math.Pow(t[i], 2)) / 2 + (x0_sum) - (sc - s) / 4; // 髋关节累计位移为 sb + nStep * s - 1 / 4 * s
                        //x0[i] = x0[i] - s / 4; // 未知原因x0会在此处突增s/4，这里减去这个偏移
                        y0[i] = (((L1 + L2) - Math.Sqrt(Math.Pow((L1 + L2), 2) - Math.Pow((sc / 4), 2))) / 2) * Math.Sin((2 * Math.PI / (sc / 2)) * (x0[i] - (x0_sum)) - (Math.PI / 2)) + (L1 + L2 + Math.Sqrt(Math.Pow((L1 + L2), 2) - Math.Pow((sc / 4), 2))) / 2;
                        //xc yc为左脚
                        xc[i] = xc_sum; // 累计位移 sb + nStep * s
                        yc[i] = 0;

                        t[i + 1] = t[i] + 0.01;
                    }

                    for (int i = 0; i < t.Length; i++)
                    {
                        // 将时间置零
                        t[i] = 0;
                    }

                    xa_sum = xa[201];
                    x0_sum = x0[201];
                    xc_sum = xc[201];

                    for (int i = 201; i <= 301; i++)
                    {
                        //xa ya为右脚
                        xa[i] = xa_sum; //右踝关节累计位移为sb + nStep * s + sc / 2
                        ya[i] = 0;
                        //x0 y0髋关节
                        x0[i] = ((-2 * sc) * Math.Pow(t[i], 3) + (3 * sc) * Math.Pow(t[i], 2)) / 2 + (x0_sum); // 髋关节累计位移为 sb + nStep * s + sc / 4
                        y0[i] = (((L1 + L2) - Math.Sqrt(Math.Pow((L1 + L2), 2) - Math.Pow((sc / 4), 2))) / 2) * Math.Sin((2 * Math.PI / (sc / 2)) * (x0[i] - (x0_sum)) - (Math.PI / 2)) + (L1 + L2 + Math.Sqrt(Math.Pow((L1 + L2), 2) - Math.Pow((sc / 4), 2))) / 2;
                        //xc yc为左脚
                        xc[i] = (-2 * sc) * Math.Pow(t[i], 3) + (3 * sc) * Math.Pow(t[i], 2) + (xc_sum); // 左踝关节累计位移sb + nStep * s
                        yc[i] = (zc / 2) * Math.Sin((2 * Math.PI / sc) * (xc[i] - (xc_sum)) - (Math.PI / 2)) + zc / 2;

                        t[i + 1] = t[i] + 0.01;
                    }

                    for (int i = 0; i < t.Length; i++)
                    {
                        // 将时间置零
                        t[i] = 0;
                    }

                    xa_sum = xa[301];
                    x0_sum = x0[301];
                    xc_sum = xc[301];

                    //finish
                    double zf = overStepHeight;
                    double sf = overStepLength / 2;
                    double vf = sf / (T / 2);

                    for (int i = 301; i <= 401; i++)
                    {
                        xa[i] = vf * t[i] + (xa_sum); //右踝关节累计位移为sb + nStep * s + sc / 2
                        ya[i] = (zf / 2) * Math.Sin((2 * Math.PI / (sf)) * (xa[i] - (xa_sum)) - (Math.PI / 2)) + (zf / 2);

                        x0[i] = (vf / 2) * t[i] + x0_sum; // 髋关节累计位移为 sb + nStep * s + 3 / 4 * sc
                        y0[i] = (((L1 + L2) - Math.Sqrt(Math.Pow((L1 + L2), 2) - Math.Pow((sf / 2), 2))) / 2) * Math.Sin((2 * Math.PI / sf) * (x0[i] - (x0_sum)) - (Math.PI / 2)) + (L1 + L2 + Math.Sqrt(Math.Pow((L1 + L2), 2) - Math.Pow((sf / 2), 2))) / 2;

                        xc[i] = xc_sum; // 左踝关节累计位移sb + nStep * s + sc
                        yc[i] = 0;

                        t[i + 1] = t[i] + 0.01;
                    }

                    // 膝关节角度
                    for (int i = 1; i <= 401; i++)
                    {
                        // 求解左膝关节位置
                        L = Math.Sqrt(Math.Pow((xc[i] - x0[i]), 2) + Math.Pow((yc[i] - y0[i]), 2));
                        theat1 = Math.Acos((Math.Pow(L2, 2) + Math.Pow(L, 2) - Math.Pow(L1, 2)) / (2 * L * L2));
                        theat2 = Math.Atan((y0[i] - yc[i]) / (x0[i] - xc[i]));
                        if (xc[i] > x0[i])
                        {
                            x3 = xc[i] - L2 * Math.Cos(theat2 - theat1);
                            x4 = xc[i] - L2 * Math.Cos(theat2 + theat1);
                            y3 = yc[i] + Math.Abs(L2 * Math.Sin(theat2 - theat1));
                            y4 = yc[i] + Math.Abs(L2 * Math.Sin(theat2 + theat1));
                        }
                        else
                        {
                            x3 = xc[i] + L2 * Math.Cos(theat2 - theat1);
                            x4 = xc[i] + L2 * Math.Cos(theat2 + theat1);
                            y3 = yc[i] + Math.Abs(L2 * Math.Sin(theat2 - theat1));
                            y4 = yc[i] + Math.Abs(L2 * Math.Sin(theat2 + theat1));
                        }
                        xd[i] = Math.Max(x3, x4);
                        yd[i] = Math.Max(y3, y4);

                        // 求解右膝关节位置
                        L = Math.Sqrt(Math.Pow((xa[i] - x0[i]), 2) + Math.Pow((ya[i] - y0[i]), 2));
                        theat1 = Math.Acos((Math.Pow(L2, 2) + Math.Pow(L, 2) - Math.Pow(L1, 2)) / (2 * L * L2));
                        theat2 = Math.Atan((y0[i] - ya[i]) / (x0[i] - xa[i]));
                        if (xa[i] < x0[i])
                        {
                            x1 = xa[i] + L2 * Math.Cos(theat2 - theat1);
                            x2 = xa[i] + L2 * Math.Cos(theat2 + theat1);
                            y1 = ya[i] + Math.Abs(L2 * Math.Sin(theat2 - theat1));
                            y2 = ya[i] + Math.Abs(L2 * Math.Sin(theat2 + theat1));
                        }
                        else
                        {
                            x1 = xa[i] - L2 * Math.Cos(theat2 - theat1);
                            x2 = xa[i] - L2 * Math.Cos(theat2 + theat1);
                            y1 = ya[i] + Math.Abs(L2 * Math.Sin(theat2 - theat1));
                            y2 = ya[i] + Math.Abs(L2 * Math.Sin(theat2 + theat1));
                        }
                        xb[i] = Math.Max(x1, x2);
                        yb[i] = Math.Max(y1, y2);

                        // 防止出现NaN
                        if (double.IsNaN(xd[i]))
                        {
                            xd[i] = xd[i - 1];
                        }
                        if (double.IsNaN(yd[i]))
                        {
                            yd[i] = yd[i - 1];
                        }
                        if (double.IsNaN(xb[i]))
                        {
                            xb[i] = xb[i - 1];
                        }
                        if (double.IsNaN(yb[i]))
                        {
                            yb[i] = yb[i - 1];
                        }

                        // 求解角度
                        a_hl[i] = Math.Atan((xd[i] - x0[i]) / (y0[i] - yd[i]));
                        a_hl[i] = a_hl[i] * 180 / Math.PI;

                        a_hr[i] = Math.Atan((xb[i] - x0[i]) / (y0[i] - yb[i]));
                        a_hr[i] = a_hr[i] * 180 / Math.PI;

                        a_kl[i] = Math.Atan((xc[i] - xd[i]) / (yd[i] - yc[i]));
                        a_kl[i] = a_hl[i] - (a_kl[i] * 180 / Math.PI);
                        if (a_kl[i] < 0)
                        {
                            a_kl[i] = 0; // 膝关节角度不能小于0
                        }

                        a_kr[i] = Math.Atan((xa[i] - xb[i]) / (yb[i] - ya[i]));
                        a_kr[i] = a_hr[i] - (a_kr[i] * 180 / Math.PI);
                        if (a_kr[i] < 0)
                        {
                            a_kr[i] = 0; // 膝关节角度不能小于0
                        }
                    }

                    // 生成辅助滤波用数据
                    for (int i = 402; i <= 411; i++)
                    {
                        a_hl[i] = a_hl[i - 1] - 0.0002;
                        a_hr[i] = a_hr[i - 1] - 0.002;
                        a_kr[i] = a_kr[i - 1] - 0.005;
                        a_kl[i] = a_kl[i - 1] - 0.0002;
                    }

                    // 滤波
                    // 滤波左膝
                    MWArray array_kl = new MWNumericArray(a_kl);
                    lvbo.lvbofilt bo_kl = new lvbofilt();

                    object[] result_kl = bo_kl.lvbo(1, array_kl);
                    double[] ax_kl = new double[411];
                    ax_kl = (double[])((MWNumericArray)result_kl[0]).ToVector(MWArrayComponent.Real);
                    a_kl = ax_kl;

                    // 滤波左髋
                    MWArray array_hl = new MWNumericArray(a_hl);
                    lvbo.lvbofilt bo_hl = new lvbofilt();

                    object[] result_hl = bo_hl.lvbo(1, array_hl);
                    double[] ax_hl = new double[411];
                    ax_hl = (double[])((MWNumericArray)result_hl[0]).ToVector(MWArrayComponent.Real);
                    a_hl = ax_hl;

                    // 滤波右髋
                    MWArray array_hr = new MWNumericArray(a_hr);
                    lvbo.lvbofilt bo_hr = new lvbofilt();

                    object[] result_hr = bo_hr.lvbo(1, array_hr);
                    double[] ax_hr = new double[411];
                    ax_hr = (double[])((MWNumericArray)result_hr[0]).ToVector(MWArrayComponent.Real);
                    a_hr = ax_hr;

                    // 滤波右膝
                    MWArray array_kr = new MWNumericArray(a_kr);
                    lvbo.lvbofilt bo_kr = new lvbofilt();

                    object[] result_kr = bo_kr.lvbo(1, array_kr);
                    double[] ax_kr = new double[411];
                    ax_kr = (double[])((MWNumericArray)result_kr[0]).ToVector(MWArrayComponent.Real);
                    a_kr = ax_kr;

                    // 初始角度设为0防止启动抖动剧烈
                    a_kr[0] = 0;
                    a_kl[0] = 0;
                    a_hl[0] = 0;
                    a_hr[0] = 0;
                }
                #endregion
            }

            else
            {
                #region 远障碍且无跨越
                if (overStepHeight == 0)
                {
                    // planning
                    // planning和finish段的步长是他衔接段步长的一半
                    double zb = normalStepHeight;
                    double sb = normalStepLength / 2;
                    double vb = sb / (T / 2);

                    for (int i = 1; i <= 101; i++)
                    {
                        //xa ya为右脚
                        xa[i] = 0;
                        ya[i] = 0;
                        //x0 y0髋关节
                        x0[i] = (vb / 2) * t[i];
                        y0[i] = (((L1 + L2) - Math.Sqrt(Math.Pow((L1 + L2), 2) - Math.Pow((sb / 2), 2))) / 2) * Math.Sin((2 * Math.PI / sb) * x0[i] + (Math.PI / 2)) + (L1 + L2 + Math.Sqrt(Math.Pow((L1 + L2), 2) - Math.Pow((sb / 2), 2))) / 2;
                        //xc yc为左脚
                        xc[i] = vb * t[i];
                        yc[i] = (zb / 2) * Math.Sin((2 * Math.PI / (sb)) * xc[i] - (Math.PI / 2)) + (zb / 2);

                        t[i + 1] = t[i] + 0.01;
                    }

                    for (int i = 0; i < t.Length; i++)
                    {
                        // 将时间置零
                        t[i] = 0;
                    }

                    xa_sum = xa[101];
                    x0_sum = x0[101];
                    xc_sum = xc[101];

                    // normal
                    double z = normalStepHeight;
                    double s = normalStepLength;
                    double v = s / (T / 2);
                    for (int j = 0; j < nStep; j++)
                    {
                        for (int i = 101 + j*200; i <= 201 + j*200; i++)
                        {
                            //xa ya为右脚
                            xa[i] = (-2 * s) * Math.Pow(t[i], 3) + (3 * s) * Math.Pow(t[i], 2) + xa_sum; // 右脚踝关节累计位移 0
                            ya[i] = (z / 2) * Math.Sin((2 * Math.PI / (s)) * (xa[i] - (xa_sum)) - (Math.PI / 2)) + (z / 2);
                            //x0 y0髋关节
                            x0[i] = ((-2 * s) * Math.Pow(t[i], 3) + 3 * s * Math.Pow(t[i], 2)) / 2 + x0_sum; // 髋关节累计位移为 sb/2
                            y0[i] = (((L1 + L2) - Math.Sqrt(Math.Pow((L1 + L2), 2) - Math.Pow((s / 4), 2))) / 2) * Math.Sin((2 * Math.PI / (s / 2)) * (x0[i] - x0_sum) - (Math.PI / 2)) + (L1 + L2 + Math.Sqrt(Math.Pow((L1 + L2), 2) - Math.Pow((s / 4), 2))) / 2;
                            //xc yc为左脚
                            xc[i] = xc_sum; // 左脚踝关节累计位移 sb
                            yc[i] = 0;

                            t[i + 1] = t[i] + 0.01;
                        }

                        for (int i = 0; i < t.Length; i++)
                        {
                            // 将时间置零
                            t[i] = 0;
                        }

                        xa_sum = xa[201 + j * 200];
                        x0_sum = x0[201 + j * 200];
                        xc_sum = xc[201 + j * 200];

                        for (int i = 201 + j*200; i <= 301+j*200; i++)
                        {
                            //xa ya为右脚
                            xa[i] = xa_sum; // 右脚踝关节累计位移 sb + s / 2
                            ya[i] = 0;
                            //x0 y0髋关节
                            x0[i] = ((-2 * s) * Math.Pow(t[i], 3) + 3 * s * Math.Pow(t[i], 2)) / 2 + (x0_sum); // 髋关节累计位移为 sb + s/4
                            y0[i] = (((L1 + L2) - Math.Sqrt(Math.Pow((L1 + L2), 2) - Math.Pow((s / 4), 2))) / 2) * Math.Sin((2 * Math.PI / (s / 2)) * (x0[i] - (x0_sum)) - (Math.PI / 2)) + (L1 + L2 + Math.Sqrt(Math.Pow((L1 + L2), 2) - Math.Pow((s / 4), 2))) / 2;
                            //xc yc为左脚
                            xc[i] = (-2 * s * Math.Pow(t[i], 3)) + (3 * s) * Math.Pow(t[i], 2) + xc_sum; // 左脚踝关节累计位移 sb
                            yc[i] = (z / 2) * Math.Sin((2 * Math.PI / s) * (xc[i] - xc_sum) - Math.PI / 2) + (z / 2);

                            t[i + 1] = t[i] + 0.01;
                        }

                        for (int i = 0; i < t.Length; i++)
                        {
                            // 将时间置零
                            t[i] = 0;
                        }

                        xa_sum = xa[301 + j * 200];
                        x0_sum = x0[301 + j * 200];
                        xc_sum = xc[301 + j * 200];
                    }

                    // change
                    double zc = lastStepHeight;
                    double sc = lastStepLength; // 改变后的步长
                    double vc = sc / (T / 2);

                    for (int i = (101 + 200 * nStep); i <= (151 + 200 * nStep); i++)
                    {
                        //xa ya为右脚
                        xa[i] = (-2 * (s)) * Math.Pow(t[i], 3) + (3 * (s)) * Math.Pow(t[i], 2) + (xa_sum); //右踝关节累计位移为sb + nStep * s - 1 / 2 * s
                        //xa[i] = xa[i] - s / 2; // 未知原因xa会在此处突增s/2，这里减去这个偏移
                        ya[i] = (zc / 2) * Math.Sin((2 * Math.PI / (sc / 2 + s / 2)) * (xa[i] - (xa_sum)) - (Math.PI / 2)) + (zc / 2);
                        //x0 y0髋关节
                        x0[i] = ((-2 * s) * Math.Pow(t[i], 3) + (3 * s) * Math.Pow(t[i], 2)) / 2 + (x0_sum); // 髋关节累计位移为 sb + nStep * s - 1 / 4 * s
                        //x0[i] = x0[i] - s / 4; // 未知原因x0会在此处突增s/4，这里减去这个偏移
                        y0[i] = (((L1 + L2) - Math.Sqrt(Math.Pow((L1 + L2), 2) - Math.Pow((s / 4), 2))) / 2) * Math.Sin((2 * Math.PI / (s / 2)) * (x0[i] - (x0_sum)) - (Math.PI / 2)) + (L1 + L2 + Math.Sqrt(Math.Pow((L1 + L2), 2) - Math.Pow((s / 4), 2))) / 2;
                        //xc yc为左脚
                        xc[i] = xc_sum; // 左踝关节累计位移 sb + nStep * s
                        yc[i] = 0;

                        t[i + 1] = t[i] + 0.01;
                    }

                    for (int i = (151 + 200 * nStep); i <= (201 + 200 * nStep); i++)
                    {
                        //xa ya为右脚
                        xa[i] = (-2 * (sc)) * Math.Pow(t[i], 3) + (3 * (sc)) * Math.Pow(t[i], 2) + (xa_sum) - (sc - s) / 2; //踝关节累计位移为sb + nStep * s - 1 / 2 * s
                        //xa[i] = xa[i] - s / 2; // 未知原因xa会在此处突增s/2，这里减去这个偏移
                        ya[i] = (zc / 2) * Math.Sin((2 * Math.PI / (sc / 2 + s / 2)) * (xa[i] - (xa_sum)) - (Math.PI / 2)) + (zc / 2);
                        //x0 y0髋关节
                        x0[i] = ((-2 * sc) * Math.Pow(t[i], 3) + (3 * sc) * Math.Pow(t[i], 2)) / 2 + (x0_sum) - (sc - s) / 4; // 髋关节累计位移为 sb + nStep * s - 1 / 4 * s
                        //x0[i] = x0[i] - s / 4; // 未知原因x0会在此处突增s/4，这里减去这个偏移
                        y0[i] = (((L1 + L2) - Math.Sqrt(Math.Pow((L1 + L2), 2) - Math.Pow((sc / 4), 2))) / 2) * Math.Sin((2 * Math.PI / (sc / 2)) * (x0[i] - (x0_sum)) - (Math.PI / 2)) + (L1 + L2 + Math.Sqrt(Math.Pow((L1 + L2), 2) - Math.Pow((sc / 4), 2))) / 2;
                        //xc yc为左脚
                        xc[i] = xc_sum; // 累计位移 sb + nStep * s
                        yc[i] = 0;

                        t[i + 1] = t[i] + 0.01;
                    }

                    xa_sum = xa[201 + 200 * nStep];
                    x0_sum = x0[201 + 200 * nStep];
                    xc_sum = xc[201 + 200 * nStep];

                    for (int i = 0; i < t.Length; i++)
                    {
                        // 将时间置零
                        t[i] = 0;
                    }

                    for (int i = (201 + 200 * nStep); i <= (301 + 200 * nStep); i++)
                    {
                        //xa ya为右脚
                        xa[i] = xa_sum; //右踝关节累计位移为sb + nStep * s + sc / 2
                        ya[i] = 0;
                        //x0 y0髋关节
                        x0[i] = ((-2 * sc) * Math.Pow(t[i], 3) + (3 * sc) * Math.Pow(t[i], 2)) / 2 + (x0_sum); // 髋关节累计位移为 sb + nStep * s + sc / 4
                        y0[i] = (((L1 + L2) - Math.Sqrt(Math.Pow((L1 + L2), 2) - Math.Pow((sc / 4), 2))) / 2) * Math.Sin((2 * Math.PI / (sc / 2)) * (x0[i] - (x0_sum)) - (Math.PI / 2)) + (L1 + L2 + Math.Sqrt(Math.Pow((L1 + L2), 2) - Math.Pow((sc / 4), 2))) / 2;
                        //xc yc为左脚
                        xc[i] = (-2 * sc) * Math.Pow(t[i], 3) + (3 * sc) * Math.Pow(t[i], 2) + (xc_sum); // 左踝关节累计位移sb + nStep * s
                        yc[i] = (zc / 2) * Math.Sin((2 * Math.PI / sc) * (xc[i] - (xc_sum)) - (Math.PI/2)) + zc / 2;

                        t[i + 1] = t[i] + 0.01;
                    }

                    for (int i = 0; i < t.Length; i++)
                    {
                        // 将时间置零
                        t[i] = 0;
                    }

                    xa_sum = xa[301 + 200 * nStep];
                    x0_sum = x0[301 + 200 * nStep];
                    xc_sum = xc[301 + 200 * nStep];
                    
                    // finish
                    double zf = lastStepHeight;
                    double sf = lastStepLength / 2;
                    double vf = sf / (T / 2);

                    for (int i = (301 + nStep * 200); i <= (401 + nStep * 200); i++)
                    {
                        xa[i] = vf * t[i] + (xa_sum); //右踝关节累计位移为sb + nStep * s + sc / 2
                        ya[i] = (zf / 2) * Math.Sin((2 * Math.PI / (sf)) * (xa[i] - (xa_sum)) - (Math.PI / 2)) + (zf / 2);

                        x0[i] = (vf / 2) * t[i] + x0_sum; // 髋关节累计位移为 sb + nStep * s + 3 / 4 * sc
                        y0[i] = (((L1 + L2) - Math.Sqrt(Math.Pow((L1 + L2), 2) - Math.Pow((sf / 2), 2))) / 2) * Math.Sin((2 * Math.PI / sf) * (x0[i] - (x0_sum)) - (Math.PI / 2)) + (L1 + L2 + Math.Sqrt(Math.Pow((L1 + L2), 2) - Math.Pow((sf / 2), 2))) / 2;

                        xc[i] = xc_sum; // 左踝关节累计位移sb + nStep * s + sc
                        yc[i] = 0;

                        t[i + 1] = t[i] + 0.01;
                    }

                    // 膝关节角度
                    for (int i = 1; i <= (401 + nStep * 200); i++)
                    {
                        // 求解左膝关节位置
                        L = Math.Sqrt(Math.Pow((xc[i] - x0[i]), 2) + Math.Pow((yc[i] - y0[i]), 2));
                        theat1 = Math.Acos((Math.Pow(L2, 2) + Math.Pow(L, 2) - Math.Pow(L1, 2)) / (2 * L * L2));
                        theat2 = Math.Atan((y0[i] - yc[i]) / (x0[i] - xc[i]));
                        if (xc[i] > x0[i])
                        {
                            x3 = xc[i] - L2 * Math.Cos(theat2 - theat1);
                            x4 = xc[i] - L2 * Math.Cos(theat2 + theat1);
                            y3 = yc[i] + Math.Abs(L2 * Math.Sin(theat2 - theat1));
                            y4 = yc[i] + Math.Abs(L2 * Math.Sin(theat2 + theat1));
                        }
                        else
                        {
                            x3 = xc[i] + L2 * Math.Cos(theat2 - theat1);
                            x4 = xc[i] + L2 * Math.Cos(theat2 + theat1);
                            y3 = yc[i] + Math.Abs(L2 * Math.Sin(theat2 - theat1));
                            y4 = yc[i] + Math.Abs(L2 * Math.Sin(theat2 + theat1));
                        }
                        xd[i] = Math.Max(x3, x4);
                        yd[i] = Math.Max(y3, y4);

                        // 求解右膝关节位置
                        L = Math.Sqrt(Math.Pow((xa[i] - x0[i]), 2) + Math.Pow((ya[i] - y0[i]), 2));
                        theat1 = Math.Acos((Math.Pow(L2, 2) + Math.Pow(L, 2) - Math.Pow(L1, 2)) / (2 * L * L2));
                        theat2 = Math.Atan((y0[i] - ya[i]) / (x0[i] - xa[i]));
                        if (xa[i] < x0[i])
                        {
                            x1 = xa[i] + L2 * Math.Cos(theat2 - theat1);
                            x2 = xa[i] + L2 * Math.Cos(theat2 + theat1);
                            y1 = ya[i] + Math.Abs(L2 * Math.Sin(theat2 - theat1));
                            y2 = ya[i] + Math.Abs(L2 * Math.Sin(theat2 + theat1));
                        }
                        else
                        {
                            x1 = xa[i] - L2 * Math.Cos(theat2 - theat1);
                            x2 = xa[i] - L2 * Math.Cos(theat2 + theat1);
                            y1 = ya[i] + Math.Abs(L2 * Math.Sin(theat2 - theat1));
                            y2 = ya[i] + Math.Abs(L2 * Math.Sin(theat2 + theat1));
                        }
                        xb[i] = Math.Max(x1, x2);
                        yb[i] = Math.Max(y1, y2);

                        // 防止出现NaN
                        if (double.IsNaN(xd[i]))
                        {
                            xd[i] = xd[i - 1];
                        }
                        if (double.IsNaN(yd[i]))
                        {
                            yd[i] = yd[i - 1];
                        }
                        if (double.IsNaN(xb[i]))
                        {
                            xb[i] = xb[i - 1];
                        }
                        if (double.IsNaN(yb[i]))
                        {
                            yb[i] = yb[i - 1];
                        }

                        // 求解角度
                        a_hl[i] = Math.Atan((xd[i] - x0[i]) / (y0[i] - yd[i]));
                        a_hl[i] = a_hl[i] * 180 / Math.PI;

                        a_hr[i] = Math.Atan((xb[i] - x0[i]) / (y0[i] - yb[i]));
                        a_hr[i] = a_hr[i] * 180 / Math.PI;

                        a_kl[i] = Math.Atan((xc[i] - xd[i]) / (yd[i] - yc[i]));
                        a_kl[i] = a_hl[i] - (a_kl[i] * 180 / Math.PI);
                        if (a_kl[i] < 0)
                        {
                            a_kl[i] = 0; // 膝关节角度不能小于0
                        }

                        a_kr[i] = Math.Atan((xa[i] - xb[i]) / (yb[i] - ya[i]));
                        a_kr[i] = a_hr[i] - (a_kr[i] * 180 / Math.PI);
                        if (a_kr[i] < 0)
                        {
                            a_kr[i] = 0; // 膝关节角度不能小于0
                        }
                    }

                    //生成辅助滤波用数据
                    for (int i = 402 + nStep * 200; i <= 411 + nStep * 200; i++)
                    {
                        a_hl[i] = a_hl[i - 1] - 0.0002;
                        a_hr[i] = a_hr[i - 1] - 0.002;
                        a_kr[i] = a_kr[i - 1] - 0.005;
                        a_kl[i] = a_kl[i - 1] - 0.0002;
                    }

                    // 滤波
                    // 滤波左膝
                    MWArray array_kl = new MWNumericArray(a_kl);
                    lvbo.lvbofilt bo_kl = new lvbofilt();

                    object[] result_kl = bo_kl.lvbo(1, array_kl);
                    double[] ax_kl = new double[411 + nStep * 200];
                    ax_kl = (double[])((MWNumericArray)result_kl[0]).ToVector(MWArrayComponent.Real);
                    a_kl = ax_kl;

                    // 滤波左髋
                    MWArray array_hl = new MWNumericArray(a_hl);
                    lvbo.lvbofilt bo_hl = new lvbofilt();

                    object[] result_hl = bo_hl.lvbo(1, array_hl);
                    double[] ax_hl = new double[411 + nStep * 200];
                    ax_hl = (double[])((MWNumericArray)result_hl[0]).ToVector(MWArrayComponent.Real);
                    a_hl = ax_hl;

                    // 滤波右髋
                    MWArray array_hr = new MWNumericArray(a_hr);
                    lvbo.lvbofilt bo_hr = new lvbofilt();

                    object[] result_hr = bo_hr.lvbo(1, array_hr);
                    double[] ax_hr = new double[411 + nStep * 200];
                    ax_hr = (double[])((MWNumericArray)result_hr[0]).ToVector(MWArrayComponent.Real);
                    a_hr = ax_hr;

                    // 滤波右膝
                    MWArray array_kr = new MWNumericArray(a_kr);
                    lvbo.lvbofilt bo_kr = new lvbofilt();

                    object[] result_kr = bo_kr.lvbo(1, array_kr);
                    double[] ax_kr = new double[411 + nStep * 200];
                    ax_kr = (double[])((MWNumericArray)result_kr[0]).ToVector(MWArrayComponent.Real);
                    a_kr = ax_kr;

                    // 初始角度设为0防止启动抖动剧烈
                    a_kr[0] = 0;
                    a_kl[0] = 0;
                    a_hl[0] = 0;
                    a_hr[0] = 0;
                }
                #endregion

                #region 远障碍且有跨越
                else
                {
                    // planning
                    // planning和finish段的步长是他衔接段步长的一半
                    double zb = normalStepHeight / 2;
                    double sb = normalStepLength / 2;
                    double vb = sb / (T / 2);

                    for (int i = 1; i <= 101; i++)
                    {
                        //xa ya为右脚
                        xa[i] = 0;
                        ya[i] = 0;
                        //x0 y0髋关节
                        x0[i] = (vb / 2) * t[i];
                        y0[i] = (((L1 + L2) - Math.Sqrt(Math.Pow((L1 + L2), 2) - Math.Pow((sb / 2), 2))) / 2) * Math.Sin((2 * Math.PI / sb) * x0[i] + (Math.PI / 2)) + (L1 + L2 + Math.Sqrt(Math.Pow((L1 + L2), 2) - Math.Pow((sb / 2), 2))) / 2;
                        //xc yc为左脚
                        xc[i] = vb * t[i];
                        yc[i] = (zb / 2) * Math.Sin((2 * Math.PI / (sb)) * xc[i] - (Math.PI / 2)) + (zb / 2);

                        t[i + 1] = t[i] + 0.01;
                    }

                    for (int i = 0; i < t.Length; i++)
                    {
                        // 将时间置零
                        t[i] = 0;
                    }

                    xa_sum = xa[101];
                    x0_sum = x0[101];
                    xc_sum = xc[101];

                    // normal
                    double z = normalStepHeight;
                    double s = normalStepLength;
                    double v = s / (T / 2);
                    for (int j = 0; j < nStep; j++)
                    {
                        for (int i = 101 + j * 200; i <= 201 + j * 200; i++)
                        {
                            //xa ya为右脚
                            xa[i] = (-2 * s) * Math.Pow(t[i], 3) + (3 * s) * Math.Pow(t[i], 2) + xa_sum; // 右脚踝关节累计位移 0
                            ya[i] = (z / 2) * Math.Sin((2 * Math.PI / (s)) * (xa[i] - (xa_sum)) - (Math.PI / 2)) + (z / 2);
                            //x0 y0髋关节
                            x0[i] = ((-2 * s) * Math.Pow(t[i], 3) + 3 * s * Math.Pow(t[i], 2)) / 2 + x0_sum; // 髋关节累计位移为 sb/2
                            y0[i] = (((L1 + L2) - Math.Sqrt(Math.Pow((L1 + L2), 2) - Math.Pow((s / 4), 2))) / 2) * Math.Sin((2 * Math.PI / (s / 2)) * (x0[i] - x0_sum) - (Math.PI / 2)) + (L1 + L2 + Math.Sqrt(Math.Pow((L1 + L2), 2) - Math.Pow((s / 4), 2))) / 2;
                            //xc yc为左脚
                            xc[i] = xc_sum; // 左脚踝关节累计位移 sb
                            yc[i] = 0;

                            t[i + 1] = t[i] + 0.01;
                        }

                        for (int i = 0; i < t.Length; i++)
                        {
                            // 将时间置零
                            t[i] = 0;
                        }

                        xa_sum = xa[201 + j * 200];
                        x0_sum = x0[201 + j * 200];
                        xc_sum = xc[201 + j * 200];

                        for (int i = 201 + j * 200; i <= 301 + j * 200; i++)
                        {
                            //xa ya为右脚
                            xa[i] = xa_sum; // 右脚踝关节累计位移 sb + s / 2
                            ya[i] = 0;
                            //x0 y0髋关节
                            x0[i] = ((-2 * s) * Math.Pow(t[i], 3) + 3 * s * Math.Pow(t[i], 2)) / 2 + (x0_sum); // 髋关节累计位移为 sb + s/4
                            y0[i] = (((L1 + L2) - Math.Sqrt(Math.Pow((L1 + L2), 2) - Math.Pow((s / 4), 2))) / 2) * Math.Sin((2 * Math.PI / (s / 2)) * (x0[i] - (x0_sum)) - (Math.PI / 2)) + (L1 + L2 + Math.Sqrt(Math.Pow((L1 + L2), 2) - Math.Pow((s / 4), 2))) / 2;
                            //xc yc为左脚
                            xc[i] = (-2 * s * Math.Pow(t[i], 3)) + (3 * s) * Math.Pow(t[i], 2) + xc_sum; // 左脚踝关节累计位移 sb
                            yc[i] = (z / 2) * Math.Sin((2 * Math.PI / s) * (xc[i] - xc_sum) - Math.PI / 2) + (z / 2);

                            t[i + 1] = t[i] + 0.01;
                        }

                        for (int i = 0; i < t.Length; i++)
                        {
                            // 将时间置零
                            t[i] = 0;
                        }

                        xa_sum = xa[301 + j * 200];
                        x0_sum = x0[301 + j * 200];
                        xc_sum = xc[301 + j * 200];
                    }

                    // change ： normal 变 lastStep
                    double zc = lastStepHeight;
                    double sc = lastStepLength; // 改变后的步长
                    double vc = sc / (T / 2);

                    for (int i = (101 + 200 * nStep); i <= (101 + 200 * nStep + 50); i++)
                    {
                        //xa ya为右脚
                        xa[i] = (-2 * (s)) * Math.Pow(t[i], 3) + (3 * (s)) * Math.Pow(t[i], 2) + (xa_sum); //右踝关节累计位移为sb + nStep * s - 1 / 2 * s
                        //xa[i] = xa[i] - s / 2; // 未知原因xa会在此处突增s/2，这里减去这个偏移
                        ya[i] = (zc / 2) * Math.Sin((2 * Math.PI / (sc / 2 + s / 2)) * (xa[i] - (xa_sum)) - (Math.PI / 2)) + (zc / 2);
                        //x0 y0髋关节
                        x0[i] = ((-2 * s) * Math.Pow(t[i], 3) + (3 * s) * Math.Pow(t[i], 2)) / 2 + (x0_sum); // 髋关节累计位移为 sb + nStep * s - 1 / 4 * s
                        //x0[i] = x0[i] - s / 4; // 未知原因x0会在此处突增s/4，这里减去这个偏移
                        y0[i] = (((L1 + L2) - Math.Sqrt(Math.Pow((L1 + L2), 2) - Math.Pow((s / 4), 2))) / 2) * Math.Sin((2 * Math.PI / (s / 2)) * (x0[i] - (x0_sum)) - (Math.PI / 2)) + (L1 + L2 + Math.Sqrt(Math.Pow((L1 + L2), 2) - Math.Pow((s / 4), 2))) / 2;
                        //xc yc为左脚
                        xc[i] = xc_sum; // 左踝关节累计位移 sb + nStep * s
                        yc[i] = 0;

                        t[i + 1] = t[i] + 0.01;
                    }

                    for (int i = (151 + 200 * nStep); i <= (151 + 200 * nStep + 50); i++)
                    {
                        //xa ya为右脚
                        xa[i] = (-2 * (sc)) * Math.Pow(t[i], 3) + (3 * (sc)) * Math.Pow(t[i], 2) + (xa_sum) - (sc - s) / 2; //踝关节累计位移为sb + nStep * s - 1 / 2 * s
                        //xa[i] = xa[i] - s / 2; // 未知原因xa会在此处突增s/2，这里减去这个偏移
                        ya[i] = (zc / 2) * Math.Sin((2 * Math.PI / (sc / 2 + s / 2)) * (xa[i] - (xa_sum)) - (Math.PI / 2)) + (zc / 2);
                        //x0 y0髋关节
                        x0[i] = ((-2 * sc) * Math.Pow(t[i], 3) + (3 * sc) * Math.Pow(t[i], 2)) / 2 + (x0_sum) - (sc - s) / 4; // 髋关节累计位移为 sb + nStep * s - 1 / 4 * s
                        //x0[i] = x0[i] - s / 4; // 未知原因x0会在此处突增s/4，这里减去这个偏移
                        y0[i] = (((L1 + L2) - Math.Sqrt(Math.Pow((L1 + L2), 2) - Math.Pow((sc / 4), 2))) / 2) * Math.Sin((2 * Math.PI / (sc / 2)) * (x0[i] - (x0_sum)) - (Math.PI / 2)) + (L1 + L2 + Math.Sqrt(Math.Pow((L1 + L2), 2) - Math.Pow((sc / 4), 2))) / 2;
                        //xc yc为左脚
                        xc[i] = xc_sum; // 累计位移 sb + nStep * s
                        yc[i] = 0;

                        t[i + 1] = t[i] + 0.01;
                    }

                    xa_sum = xa[201 + 200 * nStep];
                    x0_sum = x0[201 + 200 * nStep];
                    xc_sum = xc[201 + 200 * nStep];

                    for (int i = 0; i < t.Length; i++)
                    {
                        // 将时间置零
                        t[i] = 0;
                    }

                    for (int i = (201 + 200 * nStep); i <= (201 + 200 * nStep + 100); i++)
                    {
                        //xa ya为右脚
                        xa[i] = xa_sum; //右踝关节累计位移为sb + nStep * s + sc / 2
                        ya[i] = 0;
                        //x0 y0髋关节
                        x0[i] = ((-2 * sc) * Math.Pow(t[i], 3) + (3 * sc) * Math.Pow(t[i], 2)) / 2 + (x0_sum); // 髋关节累计位移为 sb + nStep * s + sc / 4
                        y0[i] = (((L1 + L2) - Math.Sqrt(Math.Pow((L1 + L2), 2) - Math.Pow((sc / 4), 2))) / 2) * Math.Sin((2 * Math.PI / (sc / 2)) * (x0[i] - (x0_sum)) - (Math.PI / 2)) + (L1 + L2 + Math.Sqrt(Math.Pow((L1 + L2), 2) - Math.Pow((sc / 4), 2))) / 2;
                        //xc yc为左脚
                        xc[i] = (-2 * sc) * Math.Pow(t[i], 3) + (3 * sc) * Math.Pow(t[i], 2) + (xc_sum); // 左踝关节累计位移sb + nStep * s
                        yc[i] = (zc / 2) * Math.Sin((2 * Math.PI / sc) * (xc[i] - (xc_sum)) - (Math.PI / 2)) + zc / 2;

                        t[i + 1] = t[i] + 0.01;
                    }

                    for (int i = 0; i < t.Length; i++)
                    {
                        // 将时间置零
                        t[i] = 0;
                    }

                    xa_sum = xa[301 + 200 * nStep];
                    x0_sum = x0[301 + 200 * nStep];
                    xc_sum = xc[301 + 200 * nStep];

                    // change ：lastStep 变 跨越
                    double zo = overStepHeight;
                    double so = overStepLength; // 改变后的步长
                    double vo = so / (T / 2);

                    for (int i = (301 + 200 * nStep); i <= (351 + 200 * nStep); i++)
                    {
                        //xa ya为右脚
                        xa[i] = (-2 * (sc)) * Math.Pow(t[i], 3) + (3 * (sc)) * Math.Pow(t[i], 2) + (xa_sum); //右踝关节累计位移为sb + nStep * s - 1 / 2 * s
                        //xa[i] = xa[i] - s / 2; // 未知原因xa会在此处突增s/2，这里减去这个偏移
                        ya[i] = (zo / 2) * Math.Sin((2 * Math.PI / (so / 2 + sc / 2)) * (xa[i] - (xa_sum)) - (Math.PI / 2)) + (zo / 2);
                        //x0 y0髋关节
                        x0[i] = ((-2 * sc) * Math.Pow(t[i], 3) + (3 * sc) * Math.Pow(t[i], 2)) / 2 + (x0_sum); // 髋关节累计位移为 sb + nStep * s - 1 / 4 * s
                        //x0[i] = x0[i] - s / 4; // 未知原因x0会在此处突增s/4，这里减去这个偏移
                        y0[i] = (((L1 + L2) - Math.Sqrt(Math.Pow((L1 + L2), 2) - Math.Pow((sc / 4), 2))) / 2) * Math.Sin((2 * Math.PI / (sc / 2)) * (x0[i] - (x0_sum)) - (Math.PI / 2)) + (L1 + L2 + Math.Sqrt(Math.Pow((L1 + L2), 2) - Math.Pow((sc / 4), 2))) / 2;
                        //xc yc为左脚
                        xc[i] = xc_sum; // 左踝关节累计位移 sb + nStep * s
                        yc[i] = 0;

                        t[i + 1] = t[i] + 0.01;
                    }

                    for (int i = (351 + 200 * nStep); i <= (401 + 200 * nStep); i++)
                    {
                        //xa ya为右脚
                        xa[i] = (-2 * (so)) * Math.Pow(t[i], 3) + (3 * (so)) * Math.Pow(t[i], 2) + (xa_sum) - (so - sc) / 2; //踝关节累计位移为sb + nStep * s - 1 / 2 * s
                        //xa[i] = xa[i] - s / 2; // 未知原因xa会在此处突增s/2，这里减去这个偏移
                        ya[i] = (zo / 2) * Math.Sin((2 * Math.PI / (so / 2 + sc / 2)) * (xa[i] - (xa_sum)) - (Math.PI / 2)) + (zo / 2);
                        //x0 y0髋关节
                        x0[i] = ((-2 * so) * Math.Pow(t[i], 3) + (3 * so) * Math.Pow(t[i], 2)) / 2 + (x0_sum) - (so - sc) / 4; // 髋关节累计位移为 sb + nStep * s - 1 / 4 * s
                        //x0[i] = x0[i] - s / 4; // 未知原因x0会在此处突增s/4，这里减去这个偏移
                        y0[i] = (((L1 + L2) - Math.Sqrt(Math.Pow((L1 + L2), 2) - Math.Pow((so / 4), 2))) / 2) * Math.Sin((2 * Math.PI / (so / 2)) * (x0[i] - (x0_sum)) - (Math.PI / 2)) + (L1 + L2 + Math.Sqrt(Math.Pow((L1 + L2), 2) - Math.Pow((so / 4), 2))) / 2;
                        //xc yc为左脚
                        xc[i] = xc_sum; // 累计位移 sb + nStep * s
                        yc[i] = 0;

                        t[i + 1] = t[i] + 0.01;
                    }

                    for (int i = 0; i < t.Length; i++)
                    {
                        // 将时间置零
                        t[i] = 0;
                    }

                    xa_sum = xa[401 + 200 * nStep];
                    x0_sum = x0[401 + 200 * nStep];
                    xc_sum = xc[401 + 200 * nStep];

                    for (int i = (401 + 200 * nStep); i <= (501 + 200 * nStep); i++)
                    {
                        //xa ya为右脚
                        xa[i] = xa_sum; //右踝关节累计位移为sb + nStep * s + sc / 2
                        ya[i] = 0;
                        //x0 y0髋关节
                        x0[i] = ((-2 * so) * Math.Pow(t[i], 3) + (3 * so) * Math.Pow(t[i], 2)) / 2 + (x0_sum); // 髋关节累计位移为 sb + nStep * s + sc / 4
                        y0[i] = (((L1 + L2) - Math.Sqrt(Math.Pow((L1 + L2), 2) - Math.Pow((so / 4), 2))) / 2) * Math.Sin((2 * Math.PI / (so / 2)) * (x0[i] - (x0_sum)) - (Math.PI / 2)) + (L1 + L2 + Math.Sqrt(Math.Pow((L1 + L2), 2) - Math.Pow((so / 4), 2))) / 2;
                        //xc yc为左脚
                        xc[i] = (-2 * so) * Math.Pow(t[i], 3) + (3 * so) * Math.Pow(t[i], 2) + (xc_sum); // 左踝关节累计位移sb + nStep * s
                        yc[i] = (zo / 2) * Math.Sin((2 * Math.PI / so) * (xc[i] - (xc_sum)) - (Math.PI / 2)) + zo / 2;

                        t[i + 1] = t[i] + 0.01;
                    }

                    for (int i = 0; i < t.Length; i++)
                    {
                        // 将时间置零
                        t[i] = 0;
                    }

                    xa_sum = xa[501 + 200 * nStep];
                    x0_sum = x0[501 + 200 * nStep];
                    xc_sum = xc[501 + 200 * nStep];

                    // finish
                    double zf = overStepHeight / 2;
                    double sf = overStepLength / 2;
                    double vf = sf / (T / 2);

                    for (int i = (501 + nStep * 200); i <= (601 + nStep * 200); i++)
                    {
                        xa[i] = vf * t[i] + (xa_sum); //右踝关节累计位移为sb + nStep * s + sc / 2
                        ya[i] = (zf / 2) * Math.Sin((2 * Math.PI / (sf)) * (xa[i] - (xa_sum)) - (Math.PI / 2)) + (zf / 2);

                        x0[i] = (vf / 2) * t[i] + x0_sum; // 髋关节累计位移为 sb + nStep * s + 3 / 4 * sc
                        y0[i] = (((L1 + L2) - Math.Sqrt(Math.Pow((L1 + L2), 2) - Math.Pow((sf / 2), 2))) / 2) * Math.Sin((2 * Math.PI / sf) * (x0[i] - (x0_sum)) - (Math.PI / 2)) + (L1 + L2 + Math.Sqrt(Math.Pow((L1 + L2), 2) - Math.Pow((sf / 2), 2))) / 2;

                        xc[i] = xc_sum; // 左踝关节累计位移sb + nStep * s + sc
                        yc[i] = 0;

                        t[i + 1] = t[i] + 0.01;
                    }

                    // 膝关节角度
                    for (int i = 1; i <= (601 + nStep * 200); i++)
                    {
                        // 求解左膝关节位置
                        L = Math.Sqrt(Math.Pow((xc[i] - x0[i]), 2) + Math.Pow((yc[i] - y0[i]), 2));
                        theat1 = Math.Acos((Math.Pow(L2, 2) + Math.Pow(L, 2) - Math.Pow(L1, 2)) / (2 * L * L2));
                        theat2 = Math.Atan((y0[i] - yc[i]) / (x0[i] - xc[i]));
                        if (xc[i] > x0[i])
                        {
                            x3 = xc[i] - L2 * Math.Cos(theat2 - theat1);
                            x4 = xc[i] - L2 * Math.Cos(theat2 + theat1);
                            y3 = yc[i] + Math.Abs(L2 * Math.Sin(theat2 - theat1));
                            y4 = yc[i] + Math.Abs(L2 * Math.Sin(theat2 + theat1));
                        }
                        else
                        {
                            x3 = xc[i] + L2 * Math.Cos(theat2 - theat1);
                            x4 = xc[i] + L2 * Math.Cos(theat2 + theat1);
                            y3 = yc[i] + Math.Abs(L2 * Math.Sin(theat2 - theat1));
                            y4 = yc[i] + Math.Abs(L2 * Math.Sin(theat2 + theat1));
                        }
                        xd[i] = Math.Max(x3, x4);
                        yd[i] = Math.Max(y3, y4);

                        // 求解右膝关节位置
                        L = Math.Sqrt(Math.Pow((xa[i] - x0[i]), 2) + Math.Pow((ya[i] - y0[i]), 2));
                        theat1 = Math.Acos((Math.Pow(L2, 2) + Math.Pow(L, 2) - Math.Pow(L1, 2)) / (2 * L * L2));
                        theat2 = Math.Atan((y0[i] - ya[i]) / (x0[i] - xa[i]));
                        if (xa[i] < x0[i])
                        {
                            x1 = xa[i] + L2 * Math.Cos(theat2 - theat1);
                            x2 = xa[i] + L2 * Math.Cos(theat2 + theat1);
                            y1 = ya[i] + Math.Abs(L2 * Math.Sin(theat2 - theat1));
                            y2 = ya[i] + Math.Abs(L2 * Math.Sin(theat2 + theat1));
                        }
                        else
                        {
                            x1 = xa[i] - L2 * Math.Cos(theat2 - theat1);
                            x2 = xa[i] - L2 * Math.Cos(theat2 + theat1);
                            y1 = ya[i] + Math.Abs(L2 * Math.Sin(theat2 - theat1));
                            y2 = ya[i] + Math.Abs(L2 * Math.Sin(theat2 + theat1));
                        }
                        xb[i] = Math.Max(x1, x2);
                        yb[i] = Math.Max(y1, y2);

                        // 防止出现NaN
                        if (double.IsNaN(xd[i]))
                        {
                            xd[i] = xd[i - 1];
                        }
                        if (double.IsNaN(yd[i]))
                        {
                            yd[i] = yd[i - 1];
                        }
                        if (double.IsNaN(xb[i]))
                        {
                            xb[i] = xb[i - 1];
                        }
                        if (double.IsNaN(yb[i]))
                        {
                            yb[i] = yb[i - 1];
                        }

                        // 求解角度
                        a_hl[i] = Math.Atan((xd[i] - x0[i]) / (y0[i] - yd[i]));
                        a_hl[i] = a_hl[i] * 180 / Math.PI;

                        a_hr[i] = Math.Atan((xb[i] - x0[i]) / (y0[i] - yb[i]));
                        a_hr[i] = a_hr[i] * 180 / Math.PI;

                        a_kl[i] = Math.Atan((xc[i] - xd[i]) / (yd[i] - yc[i]));
                        a_kl[i] = a_hl[i] - (a_kl[i] * 180 / Math.PI);
                        if (a_kl[i] < 0)
                        {
                            a_kl[i] = 0; // 膝关节角度不能小于0
                        }

                        a_kr[i] = Math.Atan((xa[i] - xb[i]) / (yb[i] - ya[i]));
                        a_kr[i] = a_hr[i] - (a_kr[i] * 180 / Math.PI);
                        if (a_kr[i] < 0)
                        {
                            a_kr[i] = 0; // 膝关节角度不能小于0
                        }
                    }

                    // 生成辅助滤波用数据
                    for (int i = 602 + nStep * 200; i <= 611 + nStep * 200; i++)
                    {
                        a_hl[i] = a_hl[i - 1] - 0.0002;
                        a_hr[i] = a_hr[i - 1] - 0.002;
                        a_kr[i] = a_kr[i - 1] - 0.005;
                        a_kl[i] = a_kl[i - 1] - 0.0002;
                    }

                    // 滤波
                    // 滤波左膝
                    MWArray array_kl = new MWNumericArray(a_kl);
                    lvbo.lvbofilt bo_kl = new lvbofilt();

                    object[] result_kl = bo_kl.lvbo(1, array_kl);
                    double[] ax_kl = new double[611 + nStep * 200];
                    ax_kl = (double[])((MWNumericArray)result_kl[0]).ToVector(MWArrayComponent.Real);
                    a_kl = ax_kl;

                    // 滤波左髋
                    MWArray array_hl = new MWNumericArray(a_hl);
                    lvbo.lvbofilt bo_hl = new lvbofilt();

                    object[] result_hl = bo_hl.lvbo(1, array_hl);
                    double[] ax_hl = new double[611 + nStep * 200];
                    ax_hl = (double[])((MWNumericArray)result_hl[0]).ToVector(MWArrayComponent.Real);
                    a_hl = ax_hl;

                    // 滤波右髋
                    MWArray array_hr = new MWNumericArray(a_hr);
                    lvbo.lvbofilt bo_hr = new lvbofilt();

                    object[] result_hr = bo_hr.lvbo(1, array_hr);
                    double[] ax_hr = new double[611 + nStep * 200];
                    ax_hr = (double[])((MWNumericArray)result_hr[0]).ToVector(MWArrayComponent.Real);
                    a_hr = ax_hr;

                    // 滤波右膝
                    MWArray array_kr = new MWNumericArray(a_kr);
                    lvbo.lvbofilt bo_kr = new lvbofilt();

                    object[] result_kr = bo_kr.lvbo(1, array_kr);
                    double[] ax_kr = new double[611 + nStep * 200];
                    ax_kr = (double[])((MWNumericArray)result_kr[0]).ToVector(MWArrayComponent.Real);
                    a_kr = ax_kr;

                    // 初始角度设为0防止启动抖动剧烈
                    a_kr[0] = 0;
                    a_kl[0] = 0;
                    a_hl[0] = 0;
                    a_hr[0] = 0;
                }
                #endregion
            }
            #endregion


            #region 保存数据
            FileStream pw = new FileStream("position.txt", FileMode.Create, FileAccess.ReadWrite);
            StreamWriter psw = new StreamWriter(pw);

            FileStream fs = new FileStream("angle.txt", FileMode.Create, FileAccess.ReadWrite);
            StreamWriter sw = new StreamWriter(fs);

            for (int i = 1; i <= numPoints; i++)
            {
                psw.WriteLine(x0[i].ToString() + "\t" + y0[i].ToString() + "\t" + xc[i].ToString() + "\t" + yc[i] + "\t" + xd[i] + "\t" + yd[i] + "\t" + xa[i] + "\t" + ya[i] + "\t" + xb[i] + "\t" + yb[i]  + "\t" + t[i]);
            }
            psw.Flush();
            psw.Close();

            for(int i=0; i<= numPoints+10; i++)
            {
                a_hl[i] = -a_hl[i];
                a_kr[i] = -a_kr[i];
                sw.WriteLine(a_kl[i].ToString() + "\t" + a_hl[i].ToString() + "\t" + a_hr[i] + "\t" + a_kr[i]);
            }
            sw.Flush();
            sw.Close();
            #endregion

            statusBar.Background = new SolidColorBrush(Color.FromArgb(255, 0, 122, 204));
            statusInfoTextBlock.Text = "步态生成已完毕";
        }
    }
}
