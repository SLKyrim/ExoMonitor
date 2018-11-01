using System;
using Microsoft.Research.DynamicDataDisplay.Common;
using System.Windows.Threading;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using System.Windows.Controls;

namespace ExoGaitMonitorVer2
{
    public class MyPoint //定义一个点
    {
        public int Date { get; set; }
        public double _point { get; set; }
        public MyPoint(double point, int date)
        {
            this.Date = date; //横坐标
            this._point = point; //纵坐标
        }
    }

    public class PointCollection : RingArray<MyPoint> //一幅图中显示TOTAL_POINTS个点
    {
        private const int TOTAL_POINTS = 1000; //图上最多保存的点数
        public PointCollection() : base(TOTAL_POINTS) { }
    }

    class ChartPlotter
    {
        #region 声明

        //声明绘制对象
        public PointCollection Total_pointcollection_Cur;

        //public PointCollection M0_pointcollection_Pos;
        //public PointCollection M0_pointcollection_Vel;
        //public PointCollection M0_pointcollection_Cur;
        //public PointCollection M1_pointcollection_Pos;
        //public PointCollection M1_pointcollection_Vel;
        //public PointCollection M1_pointcollection_Cur;
        //public PointCollection M2_pointcollection_Pos;
        //public PointCollection M2_pointcollection_Vel;
        //public PointCollection M2_pointcollection_Cur;
        //public PointCollection M3_pointcollection_Pos;
        //public PointCollection M3_pointcollection_Vel;
        //public PointCollection M3_pointcollection_Cur;



        private DispatcherTimer timer;
        private Motors motors = new Motors();
        private static ChartPlotter cp;
        private int count;
        private int lastCount = 0; //记录上次绘图所在位置
        private const int MOTOR_NUM = 4;
        private StatusBar statusBar;
        private TextBlock statusInfoTextBlock;
        #endregion

        public ChartPlotter()
        {
            //为一堆点分配空间
            Total_pointcollection_Cur = new PointCollection();

            //M0_pointcollection_Pos = new PointCollection();
            //M0_pointcollection_Vel = new PointCollection();
            //M0_pointcollection_Cur = new PointCollection();

            //M1_pointcollection_Pos = new PointCollection();
            //M1_pointcollection_Vel = new PointCollection();
            //M1_pointcollection_Cur = new PointCollection();

            //M2_pointcollection_Pos = new PointCollection();
            //M2_pointcollection_Vel = new PointCollection();
            //M2_pointcollection_Cur = new PointCollection();

            //M3_pointcollection_Pos = new PointCollection();
            //M3_pointcollection_Vel = new PointCollection();
            //M3_pointcollection_Cur = new PointCollection();
        }

        public void plotStart(Motors motorsIn, StatusBar statusBarIn, TextBlock statusInfoTextBlockIn)
        {
            motors = motorsIn;
            count = lastCount;
            statusBar = statusBarIn;
            statusInfoTextBlock = statusInfoTextBlockIn;

            timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromMilliseconds(10);
            timer.Tick += new EventHandler(plotTimer_Tick);
            timer.Start();
        }

        public void plotStop()
        {
            timer.Stop();
            timer.Tick -= new EventHandler(plotTimer_Tick);

            lastCount = count;
        }

        private void plotTimer_Tick(object sender, EventArgs e)
        {
            cp = App.Current.Resources["Cp"] as ChartPlotter;

            try
            {
                cp.Total_pointcollection_Cur.Add(new MyPoint((0.01*(Math.Abs(motors.ampObj[0].CurrentActual)+Math.Abs(motors.ampObj[1].CurrentActual)+Math.Abs(motors.ampObj[2].CurrentActual)+ Math.Abs(motors.ampObj[3].CurrentActual))), count));

                //cp.M0_pointcollection_Pos.Add(new MyPoint(motors.ampObjAngleActual[0], count));
                //cp.M0_pointcollection_Vel.Add(new MyPoint(motors.ampObjAngleVelActual[0], count));
                //cp.M0_pointcollection_Cur.Add(new MyPoint((motors.ampObj[0].CurrentActual * 0.01), count));

                //cp.M1_pointcollection_Pos.Add(new MyPoint(motors.ampObjAngleActual[1], count));
                //cp.M1_pointcollection_Vel.Add(new MyPoint(motors.ampObjAngleVelActual[1], count));
                //cp.M1_pointcollection_Cur.Add(new MyPoint((motors.ampObj[1].CurrentActual * 0.01), count));

                //cp.M2_pointcollection_Pos.Add(new MyPoint(motors.ampObjAngleActual[2], count));
                //cp.M2_pointcollection_Vel.Add(new MyPoint(motors.ampObjAngleVelActual[2], count));
                //cp.M2_pointcollection_Cur.Add(new MyPoint((motors.ampObj[2].CurrentActual * 0.01), count));

                //cp.M3_pointcollection_Pos.Add(new MyPoint(motors.ampObjAngleActual[3], count));
                //cp.M3_pointcollection_Vel.Add(new MyPoint(motors.ampObjAngleVelActual[3], count));
                //cp.M3_pointcollection_Cur.Add(new MyPoint((motors.ampObj[3].CurrentActual * 0.01), count));
            }
            catch
            {
                statusBar.Background = new SolidColorBrush(Color.FromArgb(255, 230, 20, 20));
                statusInfoTextBlock.Text = "绘图失败！";
            }

            count++;
        }
    }
}
