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
        private int nStep = 0;  //接下来应该按正常步长走nStep步
        private double normalStepLength = 400; // 正常行走步长
        private double normalStepHeight = 100; // 正常行走步高
        private double lastStepLength = 0; //走完nStep步后最后一小步的步长
        private double lastStepHeight = 0; //走完nStep步后最后一小步的步高
        private double overStepLength = 0; //跨越的步长
        private double overStepHeight = 0; //跨越的步高
        private double demoStepLength = 0; //Demo用步长
        private double demoStepHeight = 0; //Demo用步高

        // 视觉
        Visual visual = new Visual();
        private int count = 0;
        private int visual_count = 0; // 记录视觉反馈次数

        private string gaittype = "Static";
        private const int PVT_time1 = 24;
        private const int PVT_time2 = 20; //视觉PVT所用时间间隔

        // 秒表
        private Stopwatch stopwatch = new Stopwatch(); // Demo视觉反馈后更新步态显示用秒表
        private Stopwatch stopwatch_start = new Stopwatch(); // 开始请求视觉反馈用秒表
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
                motors.motors_Init();
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
            showParaTimer.Interval = TimeSpan.FromMilliseconds(40);
            showParaTimer.Start();

            // 选择调用的步态
            //Thread select_thread = new Thread(Select);
            //select_thread.Start();
        }

        private void showParaTimer_Tick(object sender, EventArgs e)//输出步态参数到相应文本框的委托
        {
            //nStepTextBox.Text = nStep.ToString();
            //normalStepLengthTextBox.Text = normalStepLength.ToString();
            //normalStepHeightTextBox.Text = normalStepHeight.ToString();
            //lastStepLengthTextBox.Text = lastStepLength.ToString();
            //lastStepHeightTextBox.Text = lastStepHeight.ToString();
            //overStepLengthTextBox.Text = overStepLength.ToString();
            //overStepHeightTextBox.Text = overStepHeight.ToString();

            // Demo用显示步长步高
            overStepLengthTextBox.Text = demoStepLength.ToString();
            overStepHeightTextBox.Text = demoStepHeight.ToString();

            gaitTypeTextBox.Text = gaittype;

            //stopWatchStartTextBlock.Text = stopwatch_start.ElapsedMilliseconds.ToString(); // 秒表文本显示【以毫秒为单位】
            //stopWatchTextBlock.Text = stopwatch.ElapsedMilliseconds.ToString(); // 秒表文本显示【以毫秒为单位】

            if (start_flag)
            {
                start_flag = false;
                gaittype = "Initial Pattern";
                demoStepLength = 0.2;
                demoStepHeight = 0.09;
                stopwatch_start.Start();
            }
            if (stopwatch_start.ElapsedMilliseconds > START_TIME)
            {
                stopwatch_start.Reset();
                stopwatch_start.Stop();

                IPAddress ip = IPAddress.Parse(IPAdressTextBox.Text);
                client = new TcpClient();
                ComWinTextBox.AppendText("Start connecting to the server...\n");
                client.Connect(ip, int.Parse(PortTextBox.Text));
                ComWinTextBox.AppendText("The server has been connected\n");
                statusInfoTextBlock.Text = "A connection has been established to the server";

                statusBar.Background = new SolidColorBrush(Color.FromArgb(255, 230, 20, 20));
                //statusInfoTextBlock.Text = "可以开始生成步态";

                sendStream = client.GetStream();
                Thread thread = new Thread(ListenerServer);
                thread.Start();

                // 秒表开始
                stopwatch.Start();
            }

            if (visual_count <= GAIT_COUNT)
            {
                if (stopwatch.ElapsedMilliseconds > RESET_TIME)
                {
                    stopwatch.Reset();
                    stopwatch.Start();

                    //if (count == 1)
                    //{
                    //    visual_count = 0;
                    //}
                    //else
                    //{
                    //    visual_count = count - 1;
                    //}
                    //stopWatchTextBlock.Text = stopwatch.ElapsedMilliseconds.ToString(); // 秒表文本显示【以毫秒为单位】

                    //// 向深度相机请求视觉反馈
                    //string msg = "The exoskeleton requests visual feedback from the server";

                    //byte[] buffer = Encoding.Default.GetBytes(msg);
                    ////lock (sendStream)
                    ////{
                    //sendStream.Write(buffer, 0, buffer.Length);
                    ////}
                    //ComWinTextBox.AppendText(msg + "\n");

                    visual_count = count;

                    count = count + 1;
                }
            }

            countTextBox.Text = visual_count.ToString();
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

            start_flag = true;

            try
            {
                statusBar.Background = new SolidColorBrush(Color.FromArgb(255, 230, 20, 20));
                statusInfoTextBlock.Text = "Start";

                pvt.StartPVT(motors, "..\\..\\bin\\Debug\\Once_Step.txt", 360, 20, 20);


                //statusBar.Background = new SolidColorBrush(Color.FromArgb(255, 0, 122, 204));
                //statusInfoTextBlock.Text = "步态执行完毕";
            }
            catch (Exception)
            {
                MessageBox.Show(e.ToString());
            }

            //if (IPAdressTextBox.Text.Trim() == string.Empty)
            //{
            //    ComWinTextBox.Dispatcher.Invoke(new showData(ComWinTextBox.AppendText), "Please enter the server IP address\n");
            //    return;
            //}
            //if (PortTextBox.Text.Trim() == string.Empty)
            //{
            //    ComWinTextBox.Dispatcher.Invoke(new showData(ComWinTextBox.AppendText), "Please enter the server port\n");
            //    return;
            //}
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
                    //ComWinTextBox.Dispatcher.Invoke(new showData(ComWinTextBox.AppendText), "从服务端发来信息：" + Encoding.Default.GetString(buffer, 0, readSize) + "\n");

                    nStep = Convert.ToInt16(Math.Ceiling(Convert.ToDouble(buffer[1]) / 2)); // 任豪步态normal两步算一步，故除以2向上取整
                    lastStepLength = (buffer[2] << 8) | buffer[3];
                    lastStepHeight = (buffer[4] << 8) | buffer[5];
                    overStepLength = (buffer[6] << 8) | buffer[7];
                    overStepHeight = (buffer[8] << 8) | buffer[9];
                    normalStepHeight = 100;
                    normalStepLength = 400;

                    ComWinTextBox.Dispatcher.Invoke(new showData(ComWinTextBox.AppendText), "Message from the Server：" + lastStepLength.ToString() + lastStepHeight.ToString() + overStepLength.ToString() + overStepHeight.ToString() + "\n");

                    // 将单位转换为米(m)
                    normalStepLength = normalStepLength / 1000;
                    normalStepHeight = normalStepHeight / 1000;
                    lastStepLength = lastStepLength / 1000;
                    lastStepHeight = lastStepHeight / 1000;
                    overStepLength = overStepLength / 1000;
                    overStepHeight = overStepHeight / 1000;

                    count = count + 1;
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

                pvt.StartPVT(motors, "..\\..\\bin\\Debug\\angle.txt", 360, 24, 64);

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

            visual.visualGaitGenerator(nStep, normalStepLength, normalStepHeight, lastStepLength,
                         lastStepHeight, overStepLength, overStepHeight, statusBar, statusInfoTextBlock);
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
                stopwatch.Reset();
                stopwatch.Stop();
            }
            else
            {
                bt.Content = "Stop Visual Feedback";
                stopwatch.Start();
            }
        }

        private void countTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            // 该文本改变说明视觉已反馈

            switch (visual_count)
            {
                case 1:
                    demoStepLength = 0.2;
                    demoStepHeight = 0.09;
                    gaittype = "Normal Pattern";
                    break;
                case 2:
                    demoStepLength = 0.076;
                    demoStepHeight = 0.06;
                    gaittype = "Transition Pattern";
                    break;
                case 3:
                    demoStepLength = 0.076;
                    demoStepHeight = 0.06;
                    gaittype = "Transition Pattern";
                    break;
                case 4:
                    demoStepLength = 0.503;
                    demoStepHeight = 0.177;
                    gaittype = "Transition Pattern";
                    break;
                case 5:
                    demoStepLength = 0.503;
                    demoStepHeight = 0.177;
                    gaittype = "Transition Pattern";
                    break;
                case 6:
                    demoStepLength = 0.503;
                    demoStepHeight = 0.088;
                    gaittype = "Terminal Pattern";
                    break;
                default:
                    demoStepLength = 0;
                    demoStepHeight = 0;
                    stopwatch.Reset();
                    stopwatch.Stop();


                    angleSetTextBox.IsReadOnly = true;
                    motorNumberTextBox.IsReadOnly = true;
                    getZeroPointButton.IsEnabled = false;
                    angleSetButton.IsEnabled = false;
                    emergencyStopButton.IsEnabled = false;
                    zeroPointSetButton.IsEnabled = false;

                    manumotive.getZeroPointStart(motors, statusBar, statusInfoTextBlock, angleSetButton, emergencyStopButton, getZeroPointButton,
                                                 zeroPointSetButton, angleSetTextBox, motorNumberTextBox);
                    break;
            }


            //#region 两步Demo
            //// 使用时注释掉一步Demo

            //#region 两步跨越Demo
            ////// 使用时注释掉两步无法跨越Demo

            ////// 跨越Demo
            ////switch (visual_count)
            ////{
            ////    case 1:
            ////        demoStepLength = 0.2;
            ////        demoStepHeight = 0.09;
            ////        gaittype = "Initial Pattern";
            ////        pvt.StartPVT(motors, "..\\..\\bin\\Debug\\A_start.txt", 360, PVT_time1, PVT_time2);
            ////        break;
            ////    case 2:
            ////        demoStepLength = 0.4;
            ////        demoStepHeight = 0.09;
            ////        gaittype = "Normal Pattern";
            ////        pvt.StartPVT(motors, "..\\..\\bin\\Debug\\A_normal.txt", 360, PVT_time1, PVT_time2);
            ////        break;
            ////    case 3:
            ////        demoStepLength = 0.4;
            ////        demoStepHeight = 0.09;
            ////        gaittype = "Normal Pattern";
            ////        pvt.StartPVT(motors, "..\\..\\bin\\Debug\\A_normal.txt", 360, PVT_time1, PVT_time2);
            ////        break;
            ////    case 4:
            ////        demoStepLength = 0.153;
            ////        demoStepHeight = 0.06;
            ////        gaittype = "Transition Pattern";
            ////        pvt.StartPVT(motors, "..\\..\\bin\\Debug\\A_change_small_y.txt", 360, PVT_time1, PVT_time2);
            ////        break;
            ////    case 5:
            ////        demoStepLength = 0.8;
            ////        demoStepHeight = 0.2;
            ////        gaittype = "Transition Pattern";
            ////        pvt.StartPVT(motors, "..\\..\\bin\\Debug\\A_change_big.txt", 360, PVT_time1, PVT_time2);
            ////        break;
            ////    case 6:
            ////        demoStepLength = 0.4;
            ////        demoStepHeight = 0.09;
            ////        gaittype = "Terminal Pattern";
            ////        pvt.StartPVT(motors, "..\\..\\bin\\Debug\\A_finish_y.txt", 360, PVT_time1, PVT_time2);
            ////        break;
            ////    default:
            ////        demoStepLength = 0;
            ////        demoStepHeight = 0;
            ////        stopwatch.Reset();
            ////        stopwatch.Stop();
            ////        break;
            ////}
            //#endregion

            //#region 两步无法跨越Demo
            ////// 使用时注释掉跨越Demo

            ////// 无法跨越Demo
            ////switch (visual_count)
            ////{
            ////    case 1:
            ////        demoStepLength = 0.2;
            ////        demoStepHeight = 0.09;
            ////        gaittype = "Initial Pattern";
            ////        pvt.StartPVT(motors, "..\\..\\bin\\Debug\\A_start.txt", 360, PVT_time1, PVT_time2);
            ////        break;
            ////    case 2:
            ////        demoStepLength = 0.4;
            ////        demoStepHeight = 0.09;
            ////        gaittype = "Normal Pattern";
            ////        pvt.StartPVT(motors, "..\\..\\bin\\Debug\\A_normal.txt", 360, PVT_time1, PVT_time2);
            ////        break;
            ////    case 3:
            ////        demoStepLength = 0.4;
            ////        demoStepHeight = 0.09;
            ////        gaittype = "Normal Pattern";
            ////        pvt.StartPVT(motors, "..\\..\\bin\\Debug\\A_normal.txt", 360, PVT_time1, PVT_time2);
            ////        break;
            ////    case 4:
            ////        demoStepLength = 0.4;
            ////        demoStepHeight = 0.09;
            ////        gaittype = "Normal Pattern";
            ////        pvt.StartPVT(motors, "..\\..\\bin\\Debug\\A_normal.txt", 360, PVT_time1, PVT_time2);
            ////        break;
            ////    case 5:
            ////        demoStepLength = 0.266;
            ////        demoStepHeight = 0.07;
            ////        gaittype = "Transition Pattern";
            ////        pvt.StartPVT(motors, "..\\..\\bin\\Debug\\A_change_small_n.txt", 360, PVT_time1, PVT_time2);
            ////        break;
            ////    case 6:
            ////        demoStepLength = 0.133;
            ////        demoStepHeight = 0.09;
            ////        gaittype = "Terminal Pattern";
            ////        pvt.StartPVT(motors, "..\\..\\bin\\Debug\\A_finish_n.txt", 360, PVT_time1, PVT_time2);
            ////        break;
            ////    default:
            ////        demoStepLength = 0;
            ////        demoStepHeight = 0;
            ////        stopwatch.Reset();
            ////        stopwatch.Stop();
            ////        break;
            ////}
            //#endregion

            //#endregion

            //#region 一步Demo
            //// 使用时注释掉两步Demo

            //#region 一步无跨越Demo
            ////使用时注释掉一步跨越Demo

            //switch (visual_count)
            //{
            //    case 1:
            //        demoStepLength = 0.2;
            //        demoStepHeight = 0.09;
            //        gaittype = "Initial Pattern";
            //        pvt.StartPVT(motors, "..\\..\\bin\\Debug\\NoStep_1_start.txt", 360, PVT_time1, PVT_time2);
            //        break;
            //    case 2:
            //        demoStepLength = 0.2;
            //        demoStepHeight = 0.09;
            //        gaittype = "Normal Pattern";
            //        pvt.StartPVT(motors, "..\\..\\bin\\Debug\\NoStep_2_normal.txt", 360, PVT_time1, PVT_time2);
            //        break;
            //    case 3:
            //        demoStepLength = 0.2;
            //        demoStepHeight = 0.09;
            //        gaittype = "Normal Pattern";
            //        pvt.StartPVT(motors, "..\\..\\bin\\Debug\\NoStep_3_normal.txt", 360, PVT_time1, PVT_time2);
            //        break;
            //    case 4:
            //        demoStepLength = 0.2;
            //        demoStepHeight = 0.09;
            //        gaittype = "Normal Pattern";
            //        pvt.StartPVT(motors, "..\\..\\bin\\Debug\\NoStep_2_normal.txt", 360, PVT_time1, PVT_time2);
            //        break;
            //    case 5:
            //        demoStepLength = 0.2;
            //        demoStepHeight = 0.09;
            //        gaittype = "Normal Pattern";
            //        pvt.StartPVT(motors, "..\\..\\bin\\Debug\\NoStep_3_normal.txt", 360, PVT_time1, PVT_time2);
            //        break;
            //    case 6:
            //        demoStepLength = 0.2;
            //        demoStepHeight = 0.09;
            //        gaittype = "Normal Pattern";
            //        pvt.StartPVT(motors, "..\\..\\bin\\Debug\\NoStep_2_normal.txt", 360, PVT_time1, PVT_time2);
            //        break;
            //    case 7:
            //        demoStepLength = 0.2;
            //        demoStepHeight = 0.09;
            //        gaittype = "Normal Pattern";
            //        pvt.StartPVT(motors, "..\\..\\bin\\Debug\\NoStep_3_normal.txt", 360, PVT_time1, PVT_time2);
            //        break;
            //    case 8:
            //        demoStepLength = 0.133;
            //        demoStepHeight = 0.06;
            //        gaittype = "Transition Pattern";
            //        pvt.StartPVT(motors, "..\\..\\bin\\Debug\\NoStep_4_change_small.txt", 360, PVT_time1, PVT_time2);
            //        break;
            //    case 9:
            //        demoStepLength = 0.133;
            //        demoStepHeight = 0.06;
            //        gaittype = "Transition Pattern";
            //        pvt.StartPVT(motors, "..\\..\\bin\\Debug\\NoStep_5_change_small.txt", 360, PVT_time1, PVT_time2);
            //        break;
            //    case 10:
            //        demoStepLength = 0.133;
            //        demoStepHeight = 0.09;
            //        gaittype = "Terminal Pattern";
            //        pvt.StartPVT(motors, "..\\..\\bin\\Debug\\NoStep_6_finish.txt", 360, PVT_time1, PVT_time2);
            //        break;
            //    default:
            //        demoStepLength = 0;
            //        demoStepHeight = 0;
            //        stopwatch.Reset();
            //        stopwatch.Stop();


            //        angleSetTextBox.IsReadOnly = true;
            //        motorNumberTextBox.IsReadOnly = true;
            //        getZeroPointButton.IsEnabled = false;
            //        angleSetButton.IsEnabled = false;
            //        emergencyStopButton.IsEnabled = false;
            //        zeroPointSetButton.IsEnabled = false;

            //        manumotive.getZeroPointStart(motors, statusBar, statusInfoTextBlock, angleSetButton, emergencyStopButton, getZeroPointButton,
            //                                     zeroPointSetButton, angleSetTextBox, motorNumberTextBox);
            //        break;
            //}
            //#endregion

            //#region 一步跨越Demo
            ////// 使用时注释掉一步无跨越Demo

            ////// 2018-12-4 障碍物长宽高尺寸：170-93-65
            ////switch (visual_count)
            ////{
            ////    case 1:
            ////        demoStepLength = 0.2;
            ////        demoStepHeight = 0.09;
            ////        gaittype = "Initial Pattern";
            ////        pvt.StartPVT(motors, "..\\..\\bin\\Debug\\Step_1_start.txt", 360, PVT_time1, PVT_time2);
            ////        break;
            ////    case 2:
            ////        demoStepLength = 0.2;
            ////        demoStepHeight = 0.09;
            ////        gaittype = "Normal Pattern";
            ////        pvt.StartPVT(motors, "..\\..\\bin\\Debug\\Step_2_normal.txt", 360, PVT_time1, PVT_time2);
            ////        break;
            ////    case 3:
            ////        demoStepLength = 0.2;
            ////        demoStepHeight = 0.09;
            ////        gaittype = "Normal Pattern";
            ////        pvt.StartPVT(motors, "..\\..\\bin\\Debug\\Step_3_normal.txt", 360, PVT_time1, PVT_time2);
            ////        break;
            ////    case 4:
            ////        demoStepLength = 0.2;
            ////        demoStepHeight = 0.09;
            ////        gaittype = "Normal Pattern";
            ////        pvt.StartPVT(motors, "..\\..\\bin\\Debug\\Step_2_normal.txt", 360, PVT_time1, PVT_time2);
            ////        break;
            ////    case 5:
            ////        demoStepLength = 0.2;
            ////        demoStepHeight = 0.09;
            ////        gaittype = "Normal Pattern";
            ////        pvt.StartPVT(motors, "..\\..\\bin\\Debug\\Step_3_normal.txt", 360, PVT_time1, PVT_time2);
            ////        break;
            ////    case 6:
            ////        demoStepLength = 0.076;
            ////        demoStepHeight = 0.06;
            ////        gaittype = "Transition Pattern";
            ////        pvt.StartPVT(motors, "..\\..\\bin\\Debug\\Step_4_change_small.txt", 360, PVT_time1, PVT_time2);
            ////        break;
            ////    case 7:
            ////        demoStepLength = 0.076;
            ////        demoStepHeight = 0.06;
            ////        gaittype = "Transition Pattern";
            ////        pvt.StartPVT(motors, "..\\..\\bin\\Debug\\Step_5_change_small.txt", 360, PVT_time1, PVT_time2);
            ////        break;
            ////    case 8:
            ////        demoStepLength = 0.503;
            ////        demoStepHeight = 0.177;
            ////        gaittype = "Transition Pattern";
            ////        pvt.StartPVT(motors, "..\\..\\bin\\Debug\\Step_6_change_big.txt", 360, PVT_time1, PVT_time2);
            ////        break;
            ////    case 9:
            ////        demoStepLength = 0.503;
            ////        demoStepHeight = 0.177;
            ////        gaittype = "Transition Pattern";
            ////        pvt.StartPVT(motors, "..\\..\\bin\\Debug\\Step_7_change_big.txt", 360, PVT_time1, PVT_time2);
            ////        break;
            ////    case 10:
            ////        demoStepLength = 0.503;
            ////        demoStepHeight = 0.088;
            ////        gaittype = "Terminal Pattern";
            ////        pvt.StartPVT(motors, "..\\..\\bin\\Debug\\Step_8_finish.txt", 360, PVT_time1, PVT_time2);
            ////        break;
            ////    default:
            ////        demoStepLength = 0;
            ////        demoStepHeight = 0;
            ////        stopwatch.Reset();
            ////        stopwatch.Stop();

            ////        angleSetTextBox.IsReadOnly = true;
            ////        motorNumberTextBox.IsReadOnly = true;
            ////        getZeroPointButton.IsEnabled = false;
            ////        angleSetButton.IsEnabled = false;
            ////        emergencyStopButton.IsEnabled = false;
            ////        zeroPointSetButton.IsEnabled = false;

            ////        manumotive.getZeroPointStart(motors, statusBar, statusInfoTextBlock, angleSetButton, emergencyStopButton, getZeroPointButton,
            ////                                     zeroPointSetButton, angleSetTextBox, motorNumberTextBox);
            ////        break;
            ////}
            //#endregion

            //#endregion
        }
    }
}
