using System;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.IO;
using System.Windows.Threading;
using CMLCOMLib;
using System.Windows.Input;
using Microsoft.Research.DynamicDataDisplay;
using Microsoft.Research.DynamicDataDisplay.DataSources;
using System.Threading;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.Net;
using System.Diagnostics;

namespace ExoGaitMonitorVer2
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {

        #region 绘图

        private ChartPlotter cp;

        public MainWindow()
        {
            EnumerableDataSource<MyPoint> total_M0_Pos;
            EnumerableDataSource<MyPoint> total_M1_Pos;
            EnumerableDataSource<MyPoint> total_M2_Pos;
            EnumerableDataSource<MyPoint> total_M3_Pos;

            InitializeComponent();

            cp = App.Current.Resources["Cp"] as ChartPlotter;

            #region 整体系统绘图

            total_M0_Pos = new EnumerableDataSource<MyPoint>(cp.M0_pointcollection_Pos);
            total_M0_Pos.SetXMapping(x => PosAx_total.ConvertToDouble(x.Date));
            total_M0_Pos.SetYMapping(y => y._point);
            PosPlot_total.AddLineGraph(total_M0_Pos, Colors.Red, 2, "Left Knee");

            total_M1_Pos = new EnumerableDataSource<MyPoint>(cp.M1_pointcollection_Pos);
            total_M1_Pos.SetXMapping(x => PosAx_total.ConvertToDouble(x.Date));
            total_M1_Pos.SetYMapping(y => y._point);
            PosPlot_total.AddLineGraph(total_M1_Pos, Colors.Green, 2, "Left Hip");


            total_M2_Pos = new EnumerableDataSource<MyPoint>(cp.M2_pointcollection_Pos);
            total_M2_Pos.SetXMapping(x => PosAx_total.ConvertToDouble(x.Date));
            total_M2_Pos.SetYMapping(y => y._point);
            PosPlot_total.AddLineGraph(total_M2_Pos, Colors.Brown, 2, "Right Hip");


            total_M3_Pos = new EnumerableDataSource<MyPoint>(cp.M3_pointcollection_Pos);
            total_M3_Pos.SetXMapping(x => PosAx_total.ConvertToDouble(x.Date));
            total_M3_Pos.SetYMapping(y => y._point);
            PosPlot_total.AddLineGraph(total_M3_Pos, Colors.Purple, 2, "Right Knee");

            #endregion
        }



        #endregion

        #region 声明
        //主界面窗口

        //CMO
        public Motors motors = new Motors(); //声明电机类

        //手动操作设置
        private Manumotive manumotive = new Manumotive();

        //PVT模式
        private PVT pvt = new PVT();

        public delegate void showData(string msg);//通信窗口输出
        private TcpClient client;
        private TcpListener server;
        private const int bufferSize = 8000;

        NetworkStream sendStream;

        // 视觉步态
        private double cStepLength = 0; //走完nStep步后最后一小步的步长
        private double cStepHeight = 0; //走完nStep步后最后一小步的步高
        private double nStepLength = 0; //跨越的步长
        private double nStepHeight = 0; //跨越的步高
        private int pattern = 0;
        private double pStepLenth = 0;
        private int go = 0;


        // 视觉
        Visual visual = new Visual();
        private int count = 0;
        private int gait_count = 0; // 记录视觉反馈次数

        private const int PVT_time1 = 24;
        private const int PVT_time2 = 20; //视觉PVT所用时间间隔

        // 秒表
        private bool start_flag = false; // 开始步态的标志
        const int START_TIME = 4000; // 开始走一两步再请求视觉反馈，单位：毫秒
        const int RESET_TIME = 4000; // 更新步态状态用秒表，单位：毫秒
        const int GAIT_COUNT = 7; // 生成步态步数

        #endregion

        #region 界面初始化

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                //motors.motors_Init();     //   
                //cp.plotStart(motors, statusBar, statusInfoTextBlock);
            }
            catch (Exception)
            {
                MessageBox.Show("Drives initialization failed");
                statusBar.Background = new SolidColorBrush(Color.FromArgb(255, 230, 20, 20));
                statusInfoTextBlock.Text = "Window initialization failed！";
            }

            // 显示参数和读表线程
            DispatcherTimer showParaTimer = new DispatcherTimer(); 
            showParaTimer.Tick += new EventHandler(showParaTimer_Tick);
            showParaTimer.Interval = TimeSpan.FromMilliseconds(30);
            showParaTimer.Start();

            Thread thread_gait_action = new Thread(thread_Gait_Action);
            thread_gait_action.Start();

            // 选择调用的步态
            //Thread select_thread = new Thread(Select);
            //select_thread.Start();

            //GaitPlanning gp = new GaitPlanning();
            //var b = gp.StartPattern(0.4, 0.09, 201);
            //var d = gp.TransitionPattern(0.4, 0.153, 0.06, 401);
            //var pos = gp.TerminalPattern(0.4, 1.006, 0.088, 201);
            //var hl = pos.Item1;
            //var hr = pos.Item2;
            //var kr = pos.Item3;
            //var kl = pos.Item4;

            //var a = 0;
            //pvt.PVT_Action(motors, b , 360, 20, 20);
        }

        private void showParaTimer_Tick(object sender, EventArgs e)//输出步态参数到相应文本框的委托 ---主执行程序
        {

            if (start_flag == true && gait_count <= 999)
            {
                gait_count = gait_count + 1;

                //// 向深度相机请求视觉反馈
                string msg = "VFR : " + gait_count.ToString().PadLeft(3, '0');
                byte[] buffer = Encoding.Default.GetBytes(msg);
                sendStream.Write(buffer, 0, buffer.Length);
                ComWinTextBox.AppendText(msg + "\n");
                countTextBox.Text = gait_count.ToString();
                start_flag = false;

            }          
        }


        private void thread_Gait_Action()
        {
            GaitPlanning gp = new GaitPlanning();
            while (true)
            {
                if (pattern==0)
                {
                    var pos = gp.StartPattern(cStepLength, cStepHeight, 201);
                    pvt.PVT_Action(motors, pos, 360, 20, 20);   //是否等待执行完？
                    pattern = 1;
                    pStepLenth = cStepLength;
                    start_flag = true;
                }           
                else if (pattern == 2)
                {
                    var pos = gp.NormalPattern(cStepLength, cStepHeight, 401);
                    pvt.PVT_Action(motors, pos, 360, 20, 20);
                    pStepLenth = cStepLength;
                    start_flag = true;
                }
                else if (pattern == 3)
                {
                    var pos = gp.TransitionPattern(pStepLenth, cStepLength, cStepHeight, 401);
                    pvt.PVT_Action(motors, pos, 360, 20, 20);
                    pStepLenth = cStepLength;
                    start_flag = true;
                }
                else if (pattern == 33)//连续两次转换模式
                {
                    var pos = gp.TransitionPattern(pStepLenth, cStepLength, cStepHeight, 401);
                    pvt.PVT_Action(motors, pos, 360, 20, 20);
                    pStepLenth = cStepLength;

                    pos = gp.TransitionPattern(pStepLenth, nStepLength, nStepHeight, 401);
                    pvt.PVT_Action(motors, pos, 360, 20, 20);
                    pStepLenth = nStepLength;
                    start_flag = true;
                }

            }
        }

    
        private void Window_Closed(object sender, EventArgs e)
        {
            this.Close();
        }
        #endregion

        #region 手动操作设置 Manumotive

        private void angleSetButton_Click(object sender, RoutedEventArgs e)//点击【执行命令】按钮时执行
        {
            angleSetButton.IsEnabled = false;
            emergencyStopButton.IsEnabled = true;
            getZeroPointButton.IsEnabled = false;
            zeroPointSetButton.IsEnabled = false;

            angleSetTextBox.IsReadOnly = true;
            motorNumberTextBox.IsReadOnly = true;


            int motorNumber = Convert.ToInt16(motorNumberTextBox.Text);
            int i = motorNumber - 1;

            motors.ampObj[i].PositionActual = 0;

            manumotive.angleSetStart(motors, Convert.ToDouble(angleSetTextBox.Text), Convert.ToInt16(motorNumberTextBox.Text), statusBar, statusInfoTextBlock,
                                     angleSetButton, emergencyStopButton, getZeroPointButton, zeroPointSetButton, angleSetTextBox, motorNumberTextBox);
        }

        private void emergencyStopButton_Click(object sender, RoutedEventArgs e)//点击【紧急停止】按钮时执行
        {
            emergencyStopButton.IsEnabled = false;
            angleSetButton.IsEnabled = true;
            getZeroPointButton.IsEnabled = true;
            angleSetTextBox.IsReadOnly = false;
            motorNumberTextBox.IsReadOnly = false;
            int motorNumber = Convert.ToInt16(motorNumberTextBox.Text);
            int i = motorNumber - 1;

            motors.ampObj[i].HaltMove();
            manumotive.angleSetStop();
        }

        private void zeroPointSetButton_Click(object sender, RoutedEventArgs e)//点击【设置原点】按钮时执行
        {
            motors.ampObj[0].PositionActual = 0;
            motors.ampObj[1].PositionActual = 0;
            motors.ampObj[2].PositionActual = 0;
            motors.ampObj[3].PositionActual = 0;

            zeroPointSetButton.IsEnabled = false;

            statusBar.Background = new SolidColorBrush(Color.FromArgb(255, 0, 122, 204));
            statusInfoTextBlock.Text = "The origin is set.";
        }

        private void getZeroPointButton_Click(object sender, RoutedEventArgs e)//点击【回归原点】按钮时执行
        {
            angleSetTextBox.IsReadOnly = true;
            motorNumberTextBox.IsReadOnly = true;
            getZeroPointButton.IsEnabled = false;
            angleSetButton.IsEnabled = false;
            emergencyStopButton.IsEnabled = false;
            zeroPointSetButton.IsEnabled = false;

            manumotive.getZeroPointStart(motors, statusBar, statusInfoTextBlock, angleSetButton, emergencyStopButton, getZeroPointButton,
                                         zeroPointSetButton, angleSetTextBox, motorNumberTextBox);
        }

        #endregion

        struct IpAndPort
        {
            public string Ip;
            public string Port;
        }

        private void switch_Click(object sender, RoutedEventArgs e)
        {
            switch_Button.IsEnabled = false;
            stop_Button.IsEnabled = true;

            
            IPAddress ip = IPAddress.Parse(IPAdressTextBox.Text);
            client = new TcpClient();
            ComWinTextBox.AppendText("Start connecting to the server...\n");
            client.Connect(ip, int.Parse(PortTextBox.Text));
            ComWinTextBox.AppendText("The server has been connected\n");

            sendStream = client.GetStream();
            Thread thread = new Thread(ListenerServer);
            thread.Start();
                        
            start_flag = true;

            try
            {
                statusBar.Background = new SolidColorBrush(Color.FromArgb(255, 230, 20, 20));
                statusInfoTextBlock.Text = "Start";


                //pvt.StartPVT(motors, "..\\..\\bin\\Debug\\Once_Step.txt", 360, 20, 20);   

            }
            catch (Exception)
            {
                MessageBox.Show(e.ToString());
            }
        }

        private void ComWinTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            ComWinTextBox.ScrollToEnd();//当通信窗口内容变化时滚动条定位在最下面
        }


        private void ListenerServer(object ipAndPort)
        {
            do
            {
                try
                {
                    int readSize;
                    byte[] buffer = new byte[bufferSize];
                    lock (sendStream)
                    {
                        readSize = sendStream.Read(buffer, 0, bufferSize);
                    }
                    if (readSize == 0)
                    {
                        return;
                    }

                    pattern = buffer[1]; 
                    cStepLength = ((buffer[2] << 8) | buffer[3])*0.001;// 同时将单位转换为米(m)
                    cStepHeight = ((buffer[4] << 8) | buffer[5])*0.001;
                    nStepLength = ((buffer[6] << 8) | buffer[7])*0.001;
                    nStepHeight = ((buffer[8] << 8) | buffer[9])*0.001;
                    
                    ComWinTextBox.Dispatcher.Invoke(new showData(ComWinTextBox.AppendText), "Message from the Server：" + cStepLength.ToString() + "-"+ cStepHeight.ToString() + "-" + nStepLength.ToString() + "-" + nStepHeight.ToString() + "\n");

                    
                    
                
                }
                catch
                {
                    ComWinTextBox.Dispatcher.Invoke(new showData(ComWinTextBox.AppendText), "Error！\n");
                }

            } while (true);
        }

        private void PVT_visual_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                statusBar.Background = new SolidColorBrush(Color.FromArgb(255, 230, 20, 20));
                statusInfoTextBlock.Text = "开始执行步态";

                //pvt.StartPVT(motors, "..\\..\\bin\\Debug\\angle.txt", 360, 24, 64);

                //statusBar.Background = new SolidColorBrush(Color.FromArgb(255, 0, 122, 204));
                //statusInfoTextBlock.Text = "步态执行完毕";
            }
            catch (Exception)
            {
                MessageBox.Show(e.ToString());
            }
        }

        private void gait_generator_Click(object sender, RoutedEventArgs e)
        {
            //statusBar.Background = new SolidColorBrush(Color.FromArgb(255, 230, 20, 20));
            //statusInfoTextBlock.Text = "步态生成进行中";

            //visual.visualGaitGenerator(nStep, normalStepLength, normalStepHeight, lastStepLength,
            //             lastStepHeight, overStepLength, overStepHeight, statusBar, statusInfoTextBlock);
        }

        private void Plot_Button_Click(object sender, RoutedEventArgs e)
        {
            Button bt = sender as Button;

            if (bt.Content.ToString() == "Start Plot")
            {
                switch_Button.IsEnabled = true;
                bt.Content = "Stop Plot";
                cp.plotStart(motors, statusBar, statusInfoTextBlock);
            }
            else
            {
                cp.plotStop();
                bt.Content = "Start Plot";
            }
        }

        private void stop_Button_Click(object sender, RoutedEventArgs e)
        {
            Button bt = sender as Button;

            if (bt.Content.ToString() == "Stop Visual Feedback")
            {
                bt.Content = "Continue Visual Feedback";
            }
            else
            {
                bt.Content = "Stop Visual Feedback";
            }
        }

        private void countTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
           
        }

    }
    
}
