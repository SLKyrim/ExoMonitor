using System;
using Microsoft.Research.DynamicDataDisplay.Common;
using System.ComponentModel;

namespace ExoGaitMonitor
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
        private const int TOTAL_POINTS = 1000; 
        public PointCollection() : base(TOTAL_POINTS) { }
    }

    class ChartPlotter
    {
        //声明绘制对象
        public PointCollection pointcollection_PositionActual;
        public PointCollection pointcollection_PositionCommand;

        public ChartPlotter()
        {
            //为一堆点分配空间
            pointcollection_PositionActual = new PointCollection();
            pointcollection_PositionCommand = new PointCollection();
        }
    }
}
