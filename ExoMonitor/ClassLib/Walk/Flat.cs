using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Windows.Threading;
using System.Windows.Controls.Primitives;
using System.Windows.Controls;
using System.IO;
using gaitplan;
using lvbo2;
using MathWorks.MATLAB.NET.Arrays;

namespace ClassLib.Walk
{
    
    public class Flat
    {
        private float buchang;
        private float bugao;
        TextBox Textbugao;
        TextBox textBuchang;
        //static DispatcherTimer timer;
        private double l1 = 0.4;//l1是大腿长度
        private double l2 = 0.4;//l2小腿长度
        private int T = 2;//T为步态周期
        double[] t = new double[703];
        double[] xa = new double[702];//xa ya 为右脚位置
        double[] ya = new double[702];
        double[] x0 = new double[702];//xa ya 为髋关节位置
        double[] y0 = new double[702];
        double[] xc = new double[702];//xa ya 为左脚位置
        double[] yc = new double[702];
        double[] xd = new double[702];//xd yd 为左膝关节位置
        double[] yd = new double[702];
        double[] xb = new double[702];//xb yb  位右膝位置
        double[] yb = new double[702];
        double[] a_hl = new double[702];//左髋角度
        double[] a_hr = new double[702];//右髋角度
        double[] a_kl = new double[702];//左膝角度
        double[] a_kr = new double[702];//右膝角度

        public void StartFlat(float bugaoIn, float buchangIn,  TextBox textBox1In, TextBox textBoxIn)
        {
            bugao = bugaoIn;
            buchang = buchangIn;

            Textbugao = textBox1In;
            textBuchang = textBoxIn;
            float zb = bugao;//zb为步高
            float sb = buchang;//sb为步长
            Double vb = sb / (T / 2);
            t[1] = 0;
            for (int i = 1; i <= 101; i++)
            {
                xa[i] = 0;
                ya[i] = 0;
                //x0 y0髋关节
                x0[i] = (vb / 2) * t[i];
                y0[i] = (((l1 + l2) - Math.Sqrt(Math.Pow((l1 + l2), 2) - Math.Pow((sb / 2), 2))) / 2) * Math.Sin((2 * Math.PI / sb) * x0[i] + (Math.PI / 2)) + (l1 + l2 + Math.Sqrt(Math.Pow((l1 + l2), 2) - Math.Pow((sb / 2), 2))) / 2;
                //y0[i] = (((l1 + l2) - Math.Sqrt(Math.Pow((l1 + l2) , 2) -Math.Pow( (sb / 2) , 2))) / 2) *Math.Sin((2 * Math.PI / sb) * x0[i] + (Math.PI / 2)) + (l1 + l2 + Math.Sqrt(Math.Pow((l1 + l2) , 2) -Math.Pow( (sb / 2) , 2))) / 2;
                //xc yc 为左脚
                xc[i] = vb * t[i];
                yc[i] = (zb / 2) * Math.Sin((2 * Math.PI / (sb)) * xc[i] - (Math.PI / 2)) + (zb / 2);
                t[i + 1] = t[i] + 0.01;
            }

            for (int i = 101; i <= 201; i++)
            {
                float s = sb * 2;
                float v = s / (T / 2);
                float z = zb;
                xa[i] = v * (t[i] - 1); // xa,ya为右脚位置
                ya[i] = (z / 2) * Math.Sin((2 * Math.PI / (s)) * xa[i] - (Math.PI / 2)) + (z / 2);

                x0[i] = (v / 2) * (t[i] - 1) + sb / 2;//x0,y0为髋关节位置
                y0[i] = (((l1 + l2) - Math.Sqrt(Math.Pow((l1 + l2), 2) - Math.Pow((s / 4), 2))) / 2) * Math.Sin((2 * Math.PI / (s / 2)) * (x0[i] - sb / 2) - (Math.PI / 2)) + (l1 + l2 + Math.Sqrt(Math.Pow((l1 + l2), 2) - Math.Pow((s / 4), 2))) / 2;

                xc[i] = sb; // xc,yc为左脚位置
                yc[i] = 0;
                t[i + 1] = t[i] + 0.01;
            }

            for (int i = 201; i <= 301; i++)
            {
                float s = sb * 2;
                float v = s / (T / 2);
                xa[i] = 2 * sb; // xa,ya为右脚位置
                ya[i] = 0;
                x0[i] = (v / 2) * (t[i] - 2) + 1.5 * sb;// x0,y0为髋关节位置
                y0[i] = (((l1 + l2) - Math.Sqrt(Math.Pow((l1 + l2), 2) - Math.Pow((s / 4), 2))) / 2) * Math.Sin((2 * Math.PI / (s / 2)) * (x0[i] - 1.5 * sb) - (Math.PI / 2)) + (l1 + l2 + Math.Sqrt(Math.Pow((l1 + l2), 2) - Math.Pow((s / 4), 2))) / 2;
                xc[i] = sb + (t[i] - 2) * v; // xc,yc为左脚位置
                yc[i] = (zb / 2) * Math.Sin((2 * Math.PI / (s)) * (xc[i] - sb) - (Math.PI / 2)) + (zb / 2);
                t[i + 1] = t[i] + 0.01;

            }

            for (int i = 301; i <= 401; i++)
            {
                float s = sb * 2;
                float v = s / (T / 2);
                xa[i] = v * (t[i] - 3) + 2 * sb; // xa,ya为右脚位置
                ya[i] = (zb / 2) * Math.Sin((2 * Math.PI / (s)) * (xa[i] - 2 * sb) - (Math.PI / 2)) + (zb / 2);

                x0[i] = (v / 2) * (t[i] - 3) + 2.5 * sb;//x0,y0为髋关节位置
                y0[i] = (((l1 + l2) - Math.Sqrt(Math.Pow((l1 + l2), 2) - Math.Pow((s / 4), 2))) / 2) * Math.Sin((2 * Math.PI / (s / 2)) * (x0[i] - 2.5 * sb) - (Math.PI / 2)) + (l1 + l2 + Math.Sqrt(Math.Pow((l1 + l2), 2) - Math.Pow((s / 4), 2))) / 2;

                xc[i] = 3 * sb; // xc,yc为左脚位置
                yc[i] = 0;
                t[i + 1] = (t[i]) + 0.01;
            }

            for (int i = 401; i <= 501; i++)
            {
                float s = sb * 2;
                float v = s / (T / 2);
                xa[i] = 4 * sb; // xa,ya为右脚位置
                ya[i] = 0;
                x0[i] = (v / 2) * (t[i] - 4) + 3.5 * sb;// x0,y0为髋关节位置
                y0[i] = (((l1 + l2) - Math.Sqrt(Math.Pow((l1 + l2), 2) - Math.Pow((s / 4), 2))) / 2) * Math.Sin((2 * Math.PI / (s / 2)) * (x0[i] - 3.5 * sb) - (Math.PI / 2)) + (l1 + l2 + Math.Sqrt(Math.Pow((l1 + l2), 2) - Math.Pow((s / 4), 2))) / 2;
                xc[i] = 3 * sb + (t[i] - 4) * v; // xc,yc为左脚位置
                yc[i] = (zb / 2) * Math.Sin((2 * Math.PI / (s)) * (xc[i] - 3 * sb) - (Math.PI / 2)) + (zb / 2);
                t[i + 1] = t[i] + 0.01;

            }
            for (int i = 501; i <= 601; i++)
            {
                float vf = sb / (T / 2);
                xa[i] = 4 * sb + vf * (t[i] - 5); // xa,ya为右脚位置
                ya[i] = (zb / 2) * Math.Sin((2 * Math.PI / (sb)) * (xa[i] - 4 * sb) - (Math.PI / 2)) + (zb / 2);

                x0[i] = (vf / 2) * (t[i] - 5) + 4.5 * sb;//x0,y0为髋关节位置
                y0[i] = (((l1 + l2) - Math.Sqrt(Math.Pow((l1 + l2), 2) - Math.Pow((sb / 2), 2))) / 2) * Math.Sin((2 * Math.PI / sb) * (x0[i] - 4.5 * sb) - (Math.PI / 2)) + (l1 + l2 + Math.Sqrt(Math.Pow((l1 + l2), 2) - Math.Pow((sb / 2), 2))) / 2;
                xc[i] = 5 * sb;// xc,yc为左脚位置
                yc[i] = 0;
                t[i + 1] = t[i] + 0.01;
            }

            //timer.Start();
            gaitplane crossp = new gaitplane();
            for (int i = 1; i <= 601; i++) //求解右膝关节位置
            {
                double a1 = x0[i]; double b1 = xa[i]; double c1 = y0[i]; double a2 = ya[i];

                object[] resultObj = crossp.gaitplan(4, (MWArray)a1, (MWArray)b1, (MWArray)c1, (MWArray)a2, (MWArray)l1);

                double cx1 = ((MWNumericArray)resultObj[0]).ToScalarDouble();
                double cx2 = ((MWNumericArray)resultObj[1]).ToScalarDouble();
                double cy1 = ((MWNumericArray)resultObj[2]).ToScalarDouble();
                double cy2 = ((MWNumericArray)resultObj[3]).ToScalarDouble();

                xb[i] = (Math.Max(cx1, cx2));
                yb[i] = (Math.Max(cy1, cy2));

                if (i == 1)
                {
                    xb[i] = 0;
                }
                else if (xa[i] == x0[i] && yb[i] == 0.5 * y0[i])
                {
                    xb[i] = (x0[i] + xa[i]) / 2;

                }
                else
                {
                    if (xa[i] == x0[i] && yb[i] != 0.5 * y0[i])
                    {
                        xb[i] = xb[i - 1];
                    }
                    else
                    {
                        xb[i] = xb[i];
                        yb[i] = yb[i];
                    }
                }
            }
            for (int i = 1; i <= 601; i++) //求解左膝关节位置
            {
                double a1 = x0[i]; double b1 = xc[i]; double c1 = y0[i]; double a2 = yc[i];

                object[] resultObj = crossp.gaitplan(4, (MWArray)a1, (MWArray)b1, (MWArray)c1, (MWArray)a2, (MWArray)l1);

                double cx1 = ((MWNumericArray)resultObj[0]).ToScalarDouble();
                double cx2 = ((MWNumericArray)resultObj[1]).ToScalarDouble();
                double cy1 = ((MWNumericArray)resultObj[2]).ToScalarDouble();
                double cy2 = ((MWNumericArray)resultObj[3]).ToScalarDouble();

                xd[i] = (Math.Max(cx1, cx2));
                yd[i] = (Math.Max(cy1, cy2));

                if (i == 1)
                {
                    xd[i] = 0;
                }
                else if (xc[i] == x0[i] && yd[i] == 0.5 * y0[i])
                {
                    xd[i] = (x0[i] + xc[i]) / 2;

                }
                else
                {
                    if (xc[i] == x0[i] && yd[i] != 0.5 * y0[i])
                    {
                        xd[i] = xd[i - 1];
                    }
                    else
                    {
                        xd[i] = xd[i];
                        yd[i] = yd[i];
                    }

                }
            }

            //求解角度

            for (int i = 1; i <= 601; i++)
            {
                a_hl[i] = Math.Atan((xd[i] - x0[i]) / (y0[i] - yd[i]));
                a_hl[i] = a_hl[i] * 180 / Math.PI;

                a_kl[i] = Math.Atan((xc[i] - xd[i]) / (yd[i] - yc[i]));
                a_kl[i] = a_hl[i] - (a_kl[i] * 180 / Math.PI);

                a_hr[i] = Math.Atan((xb[i] - x0[i]) / (y0[i] - yb[i]));
                a_hr[i] = a_hr[i] * 180 / Math.PI;

                a_kr[i] = Math.Atan((xa[i] - xb[i]) / (yb[i] - ya[i]));
                a_kr[i] = a_hr[i] - (a_kr[i] * 180 / Math.PI);

            }
            for (int i = 602; i <= 701; i++)
            {
                a_hl[i] = a_hl[i - 1] - 0.005;
                a_hr[i] = a_hr[i - 1] - 0.005;
                a_kr[i] = a_kr[i - 1] - 0.003;
                a_kl[i] = a_kl[i - 1] - 0.003;

            }

            #region
            for (int i = 701; i <= 701; i++)//滤波左膝
            {
                MWArray array = new MWNumericArray(a_kl);
                lvbo2.lvboxiugai bo = new lvboxiugai();

                object[] result = bo.lvbo(1, array);
                double[] ax = new double[702];
                ax = (double[])((MWNumericArray)result[0]).ToVector(MWArrayComponent.Real);
                a_kl = ax;
            }
            for (int i = 701; i <= 701; i++) //滤波左髋
            {
                MWArray array = new MWNumericArray(a_hl);
                lvbo2.lvboxiugai bo = new lvboxiugai();

                object[] result = bo.lvbo(1, array);
                double[] ax = new double[702];
                ax = (double[])((MWNumericArray)result[0]).ToVector(MWArrayComponent.Real);
                a_hl = ax;
            }
            for (int i = 701; i <= 701; i++)
            {
                MWArray array = new MWNumericArray(a_hr);
                lvbo2.lvboxiugai bo = new lvboxiugai();

                object[] result = bo.lvbo(1, array);
                double[] ax = new double[702];
                ax = (double[])((MWNumericArray)result[0]).ToVector(MWArrayComponent.Real);
                a_hr = ax;
            }

            for (int i = 701; i <= 701; i++)
            {
                MWArray array = new MWNumericArray(a_kr);
                lvbo2.lvboxiugai bo = new lvboxiugai();

                object[] result = bo.lvbo(1, array);
                double[] ax = new double[702];
                ax = (double[])((MWNumericArray)result[0]).ToVector(MWArrayComponent.Real);
                a_kr = ax;

            }

            #endregion


            FileStream pw = new FileStream("positiont.txt", FileMode.Create, FileAccess.ReadWrite);
            StreamWriter psw = new StreamWriter(pw);

            FileStream fs = new FileStream("gaittest.txt", FileMode.Create, FileAccess.ReadWrite);
            StreamWriter sw = new StreamWriter(fs);

            for (int i = 1; i <= 701; i++)//输出位置
            {
                psw.WriteLine(yd[i].ToString() + "\t" + yb[i].ToString() + "\t" + y0[i].ToString() + "\t" + xd[i] + "\t" + xa[i] + "\t" + x0[i] + "\t" + xc[i] + "\t" + ya[i] + "\t" + yc[i] + "\t" + t[i] + "\t" + xb[i]);
            }
            psw.Flush();
            psw.Close();

            for (int i = 1; i <= 701; i++)//输出角度
            {
                a_hl[i] = -a_hl[i];
                a_kr[i] = -a_kr[i];
                sw.WriteLine(a_kl[i].ToString() + "\t" + a_hl[i].ToString() + "\t" + a_hr[i] + "\t" + a_kr[i]);
            }
            sw.Flush();
            sw.Close();
        }


    }



    

}

