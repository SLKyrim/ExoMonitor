using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExoGaitMonitorVer2
{
    public class GaitPlanning
    {

        public Tuple<double[], double[], double[], double[]> StartPattern(double steplen, double stephgt, int len)
        {
            double[] pos_hl = new double[len];
            double[] pos_hr = new double[len];
            double[] pos_kl = new double[len];
            double[] pos_kr = new double[len];

            int[] index = new int[len];
            double[] t = new double[len];
            double H = stephgt;
            double S = steplen / 2;
            double l1 = 0.4;
            double l2 = 0.4;
            double T = 2;
            double vb = 2 * S / T;

            double[] xa = new double[len];
            double[] ya = new double[len];
            double[] x0 = new double[len];
            double[] y0 = new double[len];
            double[] xc = new double[len];
            double[] yc = new double[len];

            double x3 = 0;
            double x4 = 0;
            double y3 = 0;
            double y4 = 0;
            double[] xd = new double[len];
            double[] yd = new double[len];
            double[] xb = new double[len];
            double[] yb = new double[len];

            for (int i = 0; i < len; i++)
            {
                index[i] = i;
                t[i] = i * 0.005;
            }  

            for (int i = 0; i < len; i++)   
            {
                xa[i] = 0;
                ya[i] = 0;
                x0[i] = (vb / 2) * t[i];
                y0[i] = (((l1 + l2) - Math.Sqrt((l1 + l2)* (l1 + l2) - (S / 2)* (S / 2))) / 2) * Math.Sin((2 * Math.PI / S) * x0[i] + (Math.PI / 2)) + (l1 + l2 + Math.Sqrt((l1 + l2)* (l1 + l2) - (S / 2)* (S / 2))) / 2;
                xc[i] = vb * t[i];
                yc[i] = (H / 2) * Math.Sin((2 * Math.PI / S) * xc[i] - Math.PI / 2) + H / 2;
            }
                
            for (int i = 0; i < len; i++)   //求解左膝关节位置
            {
                var l= Math.Sqrt((xc[i] - x0[i])* (xc[i] - x0[i]) + (yc[i] - y0[i])* (yc[i] - y0[i]));
                var theat1 = Math.Acos((l2 * l2 + l * l - l1 * l1) / (2 * l * l2));
                var theat2 = Math.Atan((y0[i] - yc[i]) / (x0[i] - xc[i]));

                if (xc[i] > x0[i])
                {
                    x3 = xc[i] - l2 * Math.Cos(theat2 - theat1);
                    x4 = xc[i] - l2 * Math.Cos(theat2 + theat1);
                    y3 = yc[i] + Math.Abs(l2 * Math.Sin(theat2 - theat1));
                    y4 = yc[i] + Math.Abs(l2 * Math.Sin(theat2 + theat1));
                }
                else
                {
                    x3 = xc[i] + l2 * Math.Cos(theat2 - theat1);
                    x4 = xc[i] + l2 * Math.Cos(theat2 + theat1);
                    y3 = yc[i] + Math.Abs(l2 * Math.Sin(theat2 - theat1));
                    y4 = yc[i] + Math.Abs(l2 * Math.Sin(theat2 + theat1));
                }
                xd[i] = Math.Max(x3, x4);
                yd[i] = Math.Max(y3, y4);
                if(double.IsNaN(xd[i]))
                {
                    if (i == len - 1)
                    {
                        xd[i] = xd[i - 1];
                    }
                    else
                    {
                        xd[i] = (xd[i - 1]+ xd[i + 1])/2;
                    }
                }
                if (double.IsNaN(yd[i]))
                {
                    if (i == len - 1)
                    {
                        yd[i] = yd[i - 1];
                    }
                    else
                    {
                        yd[i] = (yd[i - 1] + yd[i + 1]) / 2;
                    }
                }
            }

            for (int i = 0; i < len; i++)   //求解右膝关节位置
            {
                var l = Math.Sqrt((xa[i] - x0[i]) * (xa[i] - x0[i]) + (ya[i] - y0[i]) * (ya[i] - y0[i]));
                var theat1 = Math.Acos((l2 * l2 + l * l - l1 * l1) / (2 * l * l2));
                var theat2 = Math.Atan((y0[i] - ya[i]) / (x0[i] - xa[i]));

                if (xa[i] < x0[i])
                {
                    x3 = xa[i] + l2 * Math.Cos(theat2 - theat1);
                    x4 = xa[i] + l2 * Math.Cos(theat2 + theat1);
                    y3 = ya[i] + Math.Abs(l2 * Math.Sin(theat2 - theat1));
                    y4 = ya[i] + Math.Abs(l2 * Math.Sin(theat2 + theat1));
                }
                else
                {
                    x3 = xa[i] - l2 * Math.Cos(theat2 - theat1);
                    x4 = xa[i] - l2 * Math.Cos(theat2 + theat1);
                    y3 = ya[i] + Math.Abs(l2 * Math.Sin(theat2 - theat1));
                    y4 = ya[i] + Math.Abs(l2 * Math.Sin(theat2 + theat1));
                }
                xb[i] = Math.Max(x3, x4);
                yb[i] = Math.Max(y3, y4);
                if (double.IsNaN(xb[i]))
                {
                    if (i == len - 1)
                    {
                        xb[i] = xb[i - 1];
                    }
                    else
                    {
                        xb[i] = (xb[i - 1] + xb[i + 1]) / 2;
                    }
                }
                if (double.IsNaN(yb[i]))
                {
                    if (i == len - 1)
                    {
                        yb[i] = yb[i - 1];
                    }
                    else
                    {
                        yb[i] = (yb[i - 1] + yb[i + 1]) / 2;
                    }
                }
            }

            for (int i = 0; i < len; i++)   //HL
            {
                pos_hl[i] = -1 * (Math.Atan((xd[i] - x0[i]) / (y0[i] - yd[i]))) * 180 / Math.PI;              
            }
            for (int i = 0; i < len; i++)   //HR
            {
                pos_hr[i] = (Math.Atan((xb[i] - x0[i]) / (y0[i] - yb[i]))) * 180 / Math.PI;
            }
            for (int i = 0; i < len; i++)   //KR
            {
                pos_kr[i] = -1 * (pos_hr[i] - (Math.Atan((xa[i] - xb[i]) / (yb[i] - ya[i]))) * 180 / Math.PI);
                if (pos_kr[i] < 0)
                {
                    pos_kr[i] = 0;
                }
            }
            for (int i = 0; i < len; i++)   //KL
            {
                pos_kl[i] = pos_hl[i] - (Math.Atan((xc[i] - xd[i]) / (yd[i] - yc[i]))) * 180 / Math.PI;
                if (pos_kl[i] < 0)
                {
                    pos_kl[i] = 0;
                }
            }
            Tuple<double[], double[], double[], double[]> pos = new Tuple<double[], double[], double[], double[]>(pos_hl, pos_hr, pos_kr, pos_kl); 

            return pos;
        }


        public Tuple<double[], double[], double[], double[]> NormalPattern(double steplen, double stephgt, int len)
        {
            double[] pos_hl = new double[len];
            double[] pos_hr = new double[len];
            double[] pos_kl = new double[len];
            double[] pos_kr = new double[len];

            int[] index = new int[len];
            double[] t = new double[len];
            double z = stephgt;
            double s = steplen;
            double l1 = 0.4;
            double l2 = 0.4;
            double T = 2;
            double vb = 2 * s / T;
            double sb = s / 2;
            int len2 = len / 2 + 1;

            double[] xa = new double[len];
            double[] ya = new double[len];
            double[] x0 = new double[len];
            double[] y0 = new double[len];
            double[] xc = new double[len];
            double[] yc = new double[len];

            double x3 = 0;
            double x4 = 0;
            double y3 = 0;
            double y4 = 0;
            double[] xd = new double[len];
            double[] yd = new double[len];
            double[] xb = new double[len];
            double[] yb = new double[len];

            for (int i = 0; i < len; i++)
            {
                index[i] = i;
                t[i] = i * 0.005;
            }

            for (int i = 0; i < len2; i++)
            {
                xa[i] = -2 * s * t[i] * t[i] * t[i] + 3 * s * t[i] * t[i];
                ya[i] = (z / 2) * Math.Sin((2 * Math.PI / s) * (xa[i]) - (Math.PI / 2)) + (z / 2);
                x0[i] = 0.5 * ((-2 * s) * t[i] * t[i] * t[i] + 3 * s * t[i] * t[i]) + sb / 2;    // x0,y0为髋关节位置
                y0[i] = (((l1 + l2) - Math.Sqrt((l1 + l2) * (l1 + l2) - (s / 4) * (s / 4))) / 2) * Math.Sin((2 * Math.PI / (s / 2)) * (x0[i] - sb / 2) - (Math.PI / 2)) + (l1 + l2 + Math.Sqrt((l1 + l2) * (l1 + l2) - (s / 4) * (s / 4))) / 2; 
                xc[i] = sb;
                yc[i] = 0;
            }


            for (int i = len2; i < len; i++)
            {
                xa[i] = s;
                ya[i] = 0;
                x0[i] = 0.5 * ((-2 * s) * (t[i] - 1) * (t[i] - 1) * (t[i] - 1) + 3 * s * (t[i] - 1)*(t[i] - 1)) + sb + s / 4;    // x0,y0为髋关节位置
                y0[i] = (((l1 + l2) - Math.Sqrt((l1 + l2)* (l1 + l2) - (s / 4)* (s / 4))) / 2) * Math.Sin((2 * Math.PI / (s / 2)) * (x0[i] - sb - s / 4) - (Math.PI / 2)) + (l1 + l2 + Math.Sqrt((l1 + l2)* (l1 + l2) - (s / 4)* (s / 4))) / 2;
                xc[i] = sb + (-2 * s) * (t[i] - 1) * (t[i] - 1) * (t[i] - 1) + (3 * s) * (t[i] - 1) * (t[i] - 1);
                yc[i] = (z / 2) * Math.Sin((2 * Math.PI / (s)) * (xc[i] - sb) - (Math.PI / 2)) + (z / 2);
            }


            for (int i = 0; i < len; i++)   //求解右膝关节位置
            {
                var l = Math.Sqrt((xa[i] - x0[i]) * (xa[i] - x0[i]) + (ya[i] - y0[i]) * (ya[i] - y0[i]));
                var theat1 = Math.Acos((l2 * l2 + l * l - l1 * l1) / (2 * l * l2));
                var theat2 = Math.Atan((y0[i] - ya[i]) / (x0[i] - xa[i]));

                if (xa[i] < x0[i])
                {
                    x3 = xa[i] + l2 * Math.Cos(theat2 - theat1);
                    x4 = xa[i] + l2 * Math.Cos(theat2 + theat1);
                    y3 = ya[i] + Math.Abs(l2 * Math.Sin(theat2 - theat1));
                    y4 = ya[i] + Math.Abs(l2 * Math.Sin(theat2 + theat1));
                }
                else
                {
                    x3 = xa[i] - l2 * Math.Cos(theat2 - theat1);
                    x4 = xa[i] - l2 * Math.Cos(theat2 + theat1);
                    y3 = ya[i] + Math.Abs(l2 * Math.Sin(theat2 - theat1));
                    y4 = ya[i] + Math.Abs(l2 * Math.Sin(theat2 + theat1));
                }
                xb[i] = Math.Max(x3, x4);
                yb[i] = Math.Max(y3, y4);               
            }
            xb[0] = xb[1];
            yb[0] = yb[1];
            xb[len2-1] = (xb[len2 - 2] + xb[len2]) / 2;
            yb[len2-1] = (yb[len2 - 2] + yb[len2]) / 2;
            xb[len-1] = xb[len - 2];
            yb[len-1] = yb[len - 2];


            for (int i = 0; i < len; i++)   //求解左膝关节位置
            {
                var l = Math.Sqrt((xc[i] - x0[i]) * (xc[i] - x0[i]) + (yc[i] - y0[i]) * (yc[i] - y0[i]));
                var theat1 = Math.Acos((l2 * l2 + l * l - l1 * l1) / (2 * l * l2));
                var theat2 = Math.Atan((y0[i] - yc[i]) / (x0[i] - xc[i]));

                if (xc[i] > x0[i])
                {
                    x3 = xc[i] - l2 * Math.Cos(theat2 - theat1);
                    x4 = xc[i] - l2 * Math.Cos(theat2 + theat1);
                    y3 = yc[i] + Math.Abs(l2 * Math.Sin(theat2 - theat1));
                    y4 = yc[i] + Math.Abs(l2 * Math.Sin(theat2 + theat1));
                }
                else
                {
                    x3 = xc[i] + l2 * Math.Cos(theat2 - theat1);
                    x4 = xc[i] + l2 * Math.Cos(theat2 + theat1);
                    y3 = yc[i] + Math.Abs(l2 * Math.Sin(theat2 - theat1));
                    y4 = yc[i] + Math.Abs(l2 * Math.Sin(theat2 + theat1));
                }
                xd[i] = Math.Max(x3, x4);
                yd[i] = Math.Max(y3, y4);
            }
            xd[0] = xd[1];
            yd[0] = yd[1];
            xd[len2-1] = (xd[len2 - 2] + xd[len2]) / 2;
            yd[len2-1] = (yd[len2 - 2] + yd[len2]) / 2;
            xd[len-1] = xd[len - 2];
            yd[len-1] = yd[len - 2];

            for (int i = 0; i < len; i++)   //HL
            {
                pos_hl[i] = -1 * (Math.Atan((xd[i] - x0[i]) / (y0[i] - yd[i]))) * 180 / Math.PI;
            }
            for (int i = 0; i < len; i++)   //HR
            {
                pos_hr[i] = (Math.Atan((xb[i] - x0[i]) / (y0[i] - yb[i]))) * 180 / Math.PI;
            }
            for (int i = 0; i < len; i++)   //KR
            {
                pos_kr[i] = -1 * (pos_hr[i] - (Math.Atan((xa[i] - xb[i]) / (yb[i] - ya[i]))) * 180 / Math.PI);
                if (pos_kr[i] < 0)
                {
                    pos_kr[i] = 0;
                }
            }
            for (int i = 0; i < len; i++)   //KL
            {
                pos_kl[i] = pos_hl[i] - (Math.Atan((xc[i] - xd[i]) / (yd[i] - yc[i]))) * 180 / Math.PI;
                if (pos_kl[i] < 0)
                {
                    pos_kl[i] = 0;
                }
            }
            Tuple<double[], double[], double[], double[]> pos = new Tuple<double[], double[], double[], double[]>(pos_hl, pos_hr, pos_kr, pos_kl);

            return pos;
        }



        public Tuple<double[], double[], double[], double[]> TransitionPattern(double laststep, double steplen, double stephgt, int len)
        {
            double[] pos_hl = new double[len];
            double[] pos_hr = new double[len];
            double[] pos_kl = new double[len];
            double[] pos_kr = new double[len];

            int[] index = new int[len];
            double[] t = new double[len];
            double zc = stephgt;
            double sc = steplen;
            double s = laststep;
            double l1 = 0.4;
            double l2 = 0.4;
            double T = 2;
            double vc = 2 * sc / T;           
            int len2 = len / 2 + 1;
            int len4 = len / 4 + 1;

            double[] xa = new double[len];
            double[] ya = new double[len];
            double[] x0 = new double[len];
            double[] y0 = new double[len];
            double[] xc = new double[len];
            double[] yc = new double[len];

            double x3 = 0;
            double x4 = 0;
            double y3 = 0;
            double y4 = 0;
            double[] xd = new double[len];
            double[] yd = new double[len];
            double[] xb = new double[len];
            double[] yb = new double[len];

            for (int i = 0; i < len; i++)
            {
                index[i] = i;
                t[i] = i * 0.005;
            }


            for (int i = 0; i < len4; i++)
            {
                xa[i] = (-2 * (sc / 2 + s / 2)) * t[i] * t[i] * t[i] + (3 * (sc / 2 + s / 2)) * t[i] * t[i]; // xa,ya为右脚位置
                ya[i] = (zc / 2) * Math.Sin((2 * Math.PI / (sc / 2 + s / 2)) * (xa[i]) - (Math.PI / 2)) + (zc / 2);
                x0[i] = ((-2 * (s)) * t[i] * t[i] * t[i] + (3 * s) * t[i] * t[i]) / 2 + s / 4;   //x0,y0为髋关节位置
                y0[i] = (((l1 + l2) - Math.Sqrt((l1 + l2)* (l1 + l2) - (s*s / 16))) / 2) * Math.Sin((2 * Math.PI / (s / 2)) * (x0[i] - s / 4) - Math.PI / 2) + (l1 + l2 + Math.Sqrt((l1 + l2)* (l1 + l2) - (s*s / 16))) / 2;
                xc[i] = s / 2;    //xc,yc为左脚位置
                yc[i] = 0;

            }


            for (int i = len4; i < len2; i++)
            {
                xa[i] = (-2 * (sc / 2 + s / 2)) * t[i] * t[i] * t[i] + (3 * (sc / 2 + s / 2)) * t[i] * t[i]; // xa,ya为右脚位置
                ya[i] = (zc / 2) * Math.Sin((2 * Math.PI / (sc / 2 + s / 2)) * (xa[i]) - (Math.PI / 2)) + (zc / 2);
                x0[i] = ((-2 * (sc)) * t[i] * t[i] * t[i] + (3 * sc) * t[i] * t[i]) / 2 + s / 4 - (sc - s) / 4;  // x0,y0为髋关节位置
                y0[i] = (((l1 + l2) - Math.Sqrt((l1 + l2)* (l1 + l2) - (sc / 4)* (sc / 4))) / 2) * Math.Sin((2 * Math.PI / (sc / 2)) * (x0[i] - s / 2) + Math.PI / 2) + (l1 + l2 + Math.Sqrt((l1 + l2)* (l1 + l2) - (sc*sc / 16))) / 2;
                xc[i] = s / 2;    // xc,yc为左脚位置
                yc[i] = 0;
            }


            
            for (int i = len2; i < len; i++)
            {
                xa[i] = sc / 2 + s / 2; // xa,ya为右脚位置
                ya[i] = 0;
                x0[i] = ((-2 * sc) * (t[i] - 1) * (t[i] - 1) * (t[i] - 1) + (3 * sc) * (t[i] - 1) * (t[i] - 1)) / 2 + s / 4 + sc / 4 + s / 4;
                y0[i] = (((l1 + l2) - Math.Sqrt((l1 + l2)* (l1 + l2) - (sc / 4)* (sc / 4))) / 2) * Math.Sin((2 * Math.PI / (sc / 2)) * (x0[i] - s / 4 - sc / 4 - s / 4) - (Math.PI / 2)) + (l1 + l2 + Math.Sqrt((l1 + l2)* (l1 + l2) - (sc / 4)* (sc / 4))) / 2;
                xc[i] = s / 2 + (-2 * sc) * (t[i] - 1) * (t[i] - 1) * (t[i] - 1) + (3 * sc) * (t[i] - 1) * (t[i] - 1);
                yc[i] = (zc / 2) * Math.Sin((2 * Math.PI / (sc)) * (xc[i] - s / 2) - (Math.PI / 2)) + (zc / 2);
            }


            for (int i = 0; i < len; i++)   //求解右膝关节位置
            {
                var l = Math.Sqrt((xa[i] - x0[i]) * (xa[i] - x0[i]) + (ya[i] - y0[i]) * (ya[i] - y0[i]));
                var theat1 = Math.Acos((l2 * l2 + l * l - l1 * l1) / (2 * l * l2));
                var theat2 = Math.Atan((y0[i] - ya[i]) / (x0[i] - xa[i]));

                if (xa[i] < x0[i])
                {
                    x3 = xa[i] + l2 * Math.Cos(theat2 - theat1);
                    x4 = xa[i] + l2 * Math.Cos(theat2 + theat1);
                    y3 = ya[i] + Math.Abs(l2 * Math.Sin(theat2 - theat1));
                    y4 = ya[i] + Math.Abs(l2 * Math.Sin(theat2 + theat1));
                }
                else
                {
                    x3 = xa[i] - l2 * Math.Cos(theat2 - theat1);
                    x4 = xa[i] - l2 * Math.Cos(theat2 + theat1);
                    y3 = ya[i] + Math.Abs(l2 * Math.Sin(theat2 - theat1));
                    y4 = ya[i] + Math.Abs(l2 * Math.Sin(theat2 + theat1));
                }
                xb[i] = Math.Max(x3, x4);
                yb[i] = Math.Max(y3, y4);
            }
            xb[0] = xb[1];
            yb[0] = yb[1];
            xb[len2 - 1] = (xb[len2 - 2] + xb[len2]) / 2;
            yb[len2 - 1] = (yb[len2 - 2] + yb[len2]) / 2;
            xb[len - 1] = xb[len - 2];
            yb[len - 1] = yb[len - 2];


            for (int i = 0; i < len; i++)   //求解左膝关节位置
            {
                var l = Math.Sqrt((xc[i] - x0[i]) * (xc[i] - x0[i]) + (yc[i] - y0[i]) * (yc[i] - y0[i]));
                var theat1 = Math.Acos((l2 * l2 + l * l - l1 * l1) / (2 * l * l2));
                var theat2 = Math.Atan((y0[i] - yc[i]) / (x0[i] - xc[i]));

                if (xc[i] > x0[i])
                {
                    x3 = xc[i] - l2 * Math.Cos(theat2 - theat1);
                    x4 = xc[i] - l2 * Math.Cos(theat2 + theat1);
                    y3 = yc[i] + Math.Abs(l2 * Math.Sin(theat2 - theat1));
                    y4 = yc[i] + Math.Abs(l2 * Math.Sin(theat2 + theat1));
                }
                else
                {
                    x3 = xc[i] + l2 * Math.Cos(theat2 - theat1);
                    x4 = xc[i] + l2 * Math.Cos(theat2 + theat1);
                    y3 = yc[i] + Math.Abs(l2 * Math.Sin(theat2 - theat1));
                    y4 = yc[i] + Math.Abs(l2 * Math.Sin(theat2 + theat1));
                }
                xd[i] = Math.Max(x3, x4);
                yd[i] = Math.Max(y3, y4);
            }
            xd[0] = xd[1];
            yd[0] = yd[1];
            xd[len2 - 1] = (xd[len2 - 2] + xd[len2]) / 2;
            yd[len2 - 1] = (yd[len2 - 2] + yd[len2]) / 2;
            xd[len - 1] = xd[len - 2];
            yd[len - 1] = yd[len - 2];

            for (int i = 0; i < len; i++)   //HL
            {
                pos_hl[i] = -1 * (Math.Atan((xd[i] - x0[i]) / (y0[i] - yd[i]))) * 180 / Math.PI;
            }
            for (int i = 0; i < len; i++)   //HR
            {
                pos_hr[i] = (Math.Atan((xb[i] - x0[i]) / (y0[i] - yb[i]))) * 180 / Math.PI;
            }
            for (int i = 0; i < len; i++)   //KR
            {
                pos_kr[i] = -1 * (pos_hr[i] - (Math.Atan((xa[i] - xb[i]) / (yb[i] - ya[i]))) * 180 / Math.PI);
                if (pos_kr[i] < 0)
                {
                    pos_kr[i] = 0;
                }
            }
            for (int i = 0; i < len; i++)   //KL
            {
                pos_kl[i] = pos_hl[i] - (Math.Atan((xc[i] - xd[i]) / (yd[i] - yc[i]))) * 180 / Math.PI;
                if (pos_kl[i] < 0)
                {
                    pos_kl[i] = 0;
                }
            }
            Tuple<double[], double[], double[], double[]> pos = new Tuple<double[], double[], double[], double[]>(pos_hl, pos_hr, pos_kr, pos_kl);

            return pos;
        }


        public Tuple<double[], double[], double[], double[]> TerminalPattern(double laststep, double steplen, double stephgt, int len)
        {
            double[] pos_hl = new double[len];
            double[] pos_hr = new double[len];
            double[] pos_kl = new double[len];
            double[] pos_kr = new double[len];

            int[] index = new int[len];
            double[] t = new double[len];
            double zf = stephgt;
            double sf = steplen / 2;
            double s = laststep;
            double l1 = 0.4;
            double l2 = 0.4;
            double T = 2;
            double vf = 2 * sf / T;
            double sb = laststep / 2;
            double sc = steplen;
            int len2 = len / 2 + 1;
            int len4 = len / 4 + 1;

            double[] xa = new double[len];
            double[] ya = new double[len];
            double[] x0 = new double[len];
            double[] y0 = new double[len];
            double[] xc = new double[len];
            double[] yc = new double[len];

            double x3 = 0;
            double x4 = 0;
            double y3 = 0;
            double y4 = 0;
            double[] xd = new double[len];
            double[] yd = new double[len];
            double[] xb = new double[len];
            double[] yb = new double[len];

            for (int i = 0; i < len; i++)
            {
                index[i] = i;
                t[i] = i * 0.005;
            }


            for (int i = 0; i < len; i++)
            {
                xa[i] = vf * t[i]; // xa,ya为右脚位置
                ya[i] = (zf / 2) * Math.Sin((2 * Math.PI / sf) * xa[i] - (Math.PI / 2)) + (zf / 2);
                x0[i] = (vf / 2) * t[i] + sf / 2;
                y0[i] = (((l1 + l2) - Math.Sqrt((l1 + l2)* (l1 + l2) - (sf / 2)* (sf / 2))) / 2) * Math.Sin((2 * Math.PI / sf) * (x0[i] - sf / 2) - (Math.PI / 2)) + (l1 + l2 + Math.Sqrt((l1 + l2)* (l1 + l2) - (sf / 2)* (sf / 2))) / 2;
                xc[i] = sf;    //xc,yc为左脚位置
                yc[i] = 0;
            }


            for (int i = 0; i < len; i++)   //求解左膝关节位置
            {
                var l = Math.Sqrt((xc[i] - x0[i]) * (xc[i] - x0[i]) + (yc[i] - y0[i]) * (yc[i] - y0[i]));
                var theat1 = Math.Acos((l2 * l2 + l * l - l1 * l1) / (2 * l * l2));
                var theat2 = Math.Atan((y0[i] - yc[i]) / (x0[i] - xc[i]));

                if (xc[i] > x0[i])
                {
                    x3 = xc[i] - l2 * Math.Cos(theat2 - theat1);
                    x4 = xc[i] - l2 * Math.Cos(theat2 + theat1);
                    y3 = yc[i] + Math.Abs(l2 * Math.Sin(theat2 - theat1));
                    y4 = yc[i] + Math.Abs(l2 * Math.Sin(theat2 + theat1));
                }
                else
                {
                    x3 = xc[i] + l2 * Math.Cos(theat2 - theat1);
                    x4 = xc[i] + l2 * Math.Cos(theat2 + theat1);
                    y3 = yc[i] + Math.Abs(l2 * Math.Sin(theat2 - theat1));
                    y4 = yc[i] + Math.Abs(l2 * Math.Sin(theat2 + theat1));
                }
                xd[i] = Math.Max(x3, x4);
                yd[i] = Math.Max(y3, y4);
            }
            xd[0] = xd[1];
            yd[0] = yd[1];
            xd[len2 - 1] = (xd[len2 - 2] + xd[len2]) / 2;
            yd[len2 - 1] = (yd[len2 - 2] + yd[len2]) / 2;
            xd[len - 1] = xd[len - 2];
            yd[len - 1] = yd[len - 2];


            for (int i = 0; i < len; i++)   //求解右膝关节位置
            {
                var l = Math.Sqrt((xa[i] - x0[i]) * (xa[i] - x0[i]) + (ya[i] - y0[i]) * (ya[i] - y0[i]));
                var theat1 = Math.Acos((l2 * l2 + l * l - l1 * l1) / (2 * l * l2));
                var theat2 = Math.Atan((y0[i] - ya[i]) / (x0[i] - xa[i]));

                if (xa[i] < x0[i])
                {
                    x3 = xa[i] + l2 * Math.Cos(theat2 - theat1);
                    x4 = xa[i] + l2 * Math.Cos(theat2 + theat1);
                    y3 = ya[i] + Math.Abs(l2 * Math.Sin(theat2 - theat1));
                    y4 = ya[i] + Math.Abs(l2 * Math.Sin(theat2 + theat1));
                }
                else
                {
                    x3 = xa[i] - l2 * Math.Cos(theat2 - theat1);
                    x4 = xa[i] - l2 * Math.Cos(theat2 + theat1);
                    y3 = ya[i] + Math.Abs(l2 * Math.Sin(theat2 - theat1));
                    y4 = ya[i] + Math.Abs(l2 * Math.Sin(theat2 + theat1));
                }
                xb[i] = Math.Max(x3, x4);
                yb[i] = Math.Max(y3, y4);
            }
            xb[0] = xb[1];
            yb[0] = yb[1];
            xb[len2 - 1] = (xb[len2 - 2] + xb[len2]) / 2;
            yb[len2 - 1] = (yb[len2 - 2] + yb[len2]) / 2;
            xb[len - 1] = xb[len - 2];
            yb[len - 1] = yb[len - 2];


            

            for (int i = 0; i < len; i++)   //HL
            {
                pos_hl[i] = -1 * (Math.Atan((xd[i] - x0[i]) / (y0[i] - yd[i]))) * 180 / Math.PI;
            }
            for (int i = 0; i < len; i++)   //HR
            {
                pos_hr[i] = (Math.Atan((xb[i] - x0[i]) / (y0[i] - yb[i]))) * 180 / Math.PI;
            }
            for (int i = 0; i < len; i++)   //KR
            {
                pos_kr[i] = -1 * (pos_hr[i] - (Math.Atan((xa[i] - xb[i]) / (yb[i] - ya[i]))) * 180 / Math.PI);
                if (pos_kr[i] < 0)
                {
                    pos_kr[i] = 0;
                }
            }
            for (int i = 0; i < len; i++)   //KL
            {
                pos_kl[i] = pos_hl[i] - (Math.Atan((xc[i] - xd[i]) / (yd[i] - yc[i]))) * 180 / Math.PI;
                if (pos_kl[i] < 0)
                {
                    pos_kl[i] = 0;
                }
            }
            Tuple<double[], double[], double[], double[]> pos = new Tuple<double[], double[], double[], double[]>(pos_hl, pos_hr, pos_kr, pos_kl);

            return pos;
        }

    }
}
