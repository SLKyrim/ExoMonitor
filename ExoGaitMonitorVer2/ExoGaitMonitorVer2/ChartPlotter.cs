using System;
using Microsoft.Research.DynamicDataDisplay.Common;
using System.ComponentModel;
using System.Windows.Threading;

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
        private const int TOTAL_POINTS = 200;
        public PointCollection() : base(TOTAL_POINTS) { }
    }

    class ChartPlotter
    {
        #region 声明

        //声明绘制对象
        public PointCollection M1_pointcollection_PositionActual;
        public PointCollection M1_pointcollection_VelocityActual;
        public PointCollection M1_pointcollection_CurrentActual;

        private DispatcherTimer timer;
        private Motors motors = new Motors();
        private static ChartPlotter cp;
        private int count;
        #endregion



        public ChartPlotter()
        {
            //为一堆点分配空间
            M1_pointcollection_PositionActual = new PointCollection();
            M1_pointcollection_VelocityActual = new PointCollection();
            M1_pointcollection_CurrentActual = new PointCollection();
        }

        public void plotStart(Motors motorsIn)
        {
            motors = motorsIn;
            count = 0;

            timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromMilliseconds(10);
            timer.Tick += new EventHandler(plotTimer_Tick);
            timer.Start();
        }

        private void plotTimer_Tick(object sender, EventArgs e)
        {
            cp = App.Current.Resources["Cp"] as ChartPlotter;
            count++;

            cp.M1_pointcollection_PositionActual.Add(new MyPoint(motors.ampObjAngleActual[0], count));
            cp.M1_pointcollection_VelocityActual.Add(new MyPoint(motors.ampObjAngleVelActual[0], count));
            cp.M1_pointcollection_CurrentActual.Add(new MyPoint((motors.ampObj[0].CurrentActual * 0.01), count));
        }
    }
}
