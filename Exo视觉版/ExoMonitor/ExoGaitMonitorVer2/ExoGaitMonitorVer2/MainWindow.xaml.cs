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
            PosPlot_total.AddLineGraph(total_M0_Pos, Colors.Red, 2, "左膝实际位置");

            total_M1_Pos = new EnumerableDataSource<MyPoint>(cp.M1_pointcollection_Pos);
            total_M1_Pos.SetXMapping(x => PosAx_total.ConvertToDouble(x.Date));
            total_M1_Pos.SetYMapping(y => y._point);
            PosPlot_total.AddLineGraph(total_M1_Pos, Colors.Green, 2, "左髋实际位置");


            total_M2_Pos = new EnumerableDataSource<MyPoint>(cp.M2_pointcollection_Pos);
            total_M2_Pos.SetXMapping(x => PosAx_total.ConvertToDouble(x.Date));
            total_M2_Pos.SetYMapping(y => y._point);
            PosPlot_total.AddLineGraph(total_M2_Pos, Colors.Brown, 2, "右髋实际位置");


            total_M3_Pos = new EnumerableDataSource<MyPoint>(cp.M3_pointcollection_Pos);
            total_M3_Pos.SetXMapping(x => PosAx_total.ConvertToDouble(x.Date));
            total_M3_Pos.SetYMapping(y => y._point);
            PosPlot_total.AddLineGraph(total_M3_Pos, Colors.Purple, 2, "右膝实际位置");

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

        DispatcherTimer Detection = new DispatcherTimer();
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

        // 视觉
        Visual visual = new Visual();

        // 秒表
        private Stopwatch stopwatch = new Stopwatch();
        const int RESET_TIME = 10000; // 秒表置零阈值，单位：毫秒
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
                MessageBox.Show("驱动器初始化失败");
                statusBar.Background = new SolidColorBrush(Color.FromArgb(255, 230, 20, 20));
                statusInfoTextBlock.Text = "窗口初始化失败！";
            }

            DispatcherTimer showParaTimer = new DispatcherTimer(); //显示参数线程
            showParaTimer.Tick += new EventHandler(showParaTimer_Tick);
            showParaTimer.Interval = TimeSpan.FromMilliseconds(40);
            showParaTimer.Start();

            //stopWatchTimer.Tick += new EventHandler(stopWatchTimer_Tick);
            //stopWatchTimer.Interval = new TimeSpan(0, 0, 0, 0, 40);
        }

        private void showParaTimer_Tick(object sender, EventArgs e)//输出步态参数到相应文本框的委托
        {
            nStepTextBox.Text = nStep.ToString();
            normalStepLengthTextBox.Text = normalStepLength.ToString();
            normalStepHeightTextBox.Text = normalStepHeight.ToString();
            lastStepLengthTextBox.Text = lastStepLength.ToString();
            lastStepHeightTextBox.Text = lastStepHeight.ToString();
            overStepLengthTextBox.Text = overStepLength.ToString();
            overStepHeightTextBox.Text = overStepHeight.ToString();

            if (Convert.ToInt16(stopWatchTextBlock.Text) > RESET_TIME)
            {
                stopwatch.Reset();
                stopwatch.Start();
                stopWatchTextBlock.Text = stopwatch.ElapsedMilliseconds.ToString(); // 秒表文本显示【以毫秒为单位】

                // 向深度相机请求视觉反馈
                string msg = "外骨骼向服务端请求视觉反馈";

                byte[] buffer = Encoding.Default.GetBytes(msg);
                //lock (sendStream)
                //{
                sendStream.Write(buffer, 0, buffer.Length);
                //}
                ComWinTextBox.AppendText(msg + "\n");
            }
            else
            {
                stopWatchTextBlock.Text = stopwatch.ElapsedMilliseconds.ToString(); // 秒表文本显示【以毫秒为单位】
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
            statusInfoTextBlock.Text = "原点设置完毕";
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

            if (IPAdressTextBox.Text.Trim() == string.Empty)
            {
                ComWinTextBox.Dispatcher.Invoke(new showData(ComWinTextBox.AppendText), "请填入服务器IP地址\n");
                return;
            }
            if (PortTextBox.Text.Trim() == string.Empty)
            {
                ComWinTextBox.Dispatcher.Invoke(new showData(ComWinTextBox.AppendText), "请填入服务器端口号\n");
                return;
            }

            IPAddress ip = IPAddress.Parse(IPAdressTextBox.Text);
            client = new TcpClient();
            ComWinTextBox.AppendText("开始连接服务端....\n");
            client.Connect(ip, int.Parse(PortTextBox.Text));
            ComWinTextBox.AppendText("已经连接服务端\n");
            statusInfoTextBlock.Text = "已与服务器建立连接";

            statusBar.Background = new SolidColorBrush(Color.FromArgb(255, 230, 20, 20));
            statusInfoTextBlock.Text = "可以开始生成步态";

            sendStream = client.GetStream();
            Thread thread = new Thread(ListenerServer);
            thread.Start();

            // 秒表开始
            stopwatch.Start();
        }

        private void btnSend_Click(object sender, RoutedEventArgs e)
        {
            if (client != null)
            {
                //要发送的信息
                if (stxtSendMsg.Text.Trim() == string.Empty)
                    return;
                string msg = stxtSendMsg.Text.Trim();

                byte[] buffer = Encoding.Default.GetBytes(msg);
                //lock (sendStream)
                //{
                sendStream.Write(buffer, 0, buffer.Length);
                //}
                ComWinTextBox.AppendText("发送给服务端的数据：" + msg + "\n");
                stxtSendMsg.Text = string.Empty;
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
                    //ComWinTextBox.Dispatcher.Invoke(new showData(ComWinTextBox.AppendText), "从服务端发来信息：" + Encoding.Default.GetString(buffer, 0, readSize) + "\n");

                    nStep = Convert.ToInt16(Math.Ceiling(Convert.ToDouble(buffer[1]) / 2)); // 任豪步态normal两步算一步，故除以2向上取整
                    lastStepLength = (buffer[2] << 8) | buffer[3];
                    lastStepHeight = (buffer[4] << 8) | buffer[5];
                    overStepLength = (buffer[6] << 8) | buffer[7];
                    overStepHeight = (buffer[8] << 8) | buffer[9];
                    normalStepHeight = 100;
                    normalStepLength = 400;

                    ComWinTextBox.Dispatcher.Invoke(new showData(ComWinTextBox.AppendText), "从服务端发来信息：" + lastStepLength.ToString() + lastStepHeight.ToString() + overStepLength.ToString() + overStepHeight.ToString() + "\n");

                    // 将单位转换为米(m)
                    normalStepLength = normalStepLength / 1000;
                    normalStepHeight = normalStepHeight / 1000;
                    lastStepLength = lastStepLength / 1000;
                    lastStepHeight = lastStepHeight / 1000;
                    overStepLength = overStepLength / 1000;
                    overStepHeight = overStepHeight / 1000;
                }
                catch
                {
                    ComWinTextBox.Dispatcher.Invoke(new showData(ComWinTextBox.AppendText), "报错\n");
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

            if (bt.Content.ToString() == "开始绘图")
            {
                bt.Content = "停止绘图";
                cp.plotStart(motors, statusBar, statusInfoTextBlock);
            }
            else
            {
                cp.plotStop();
                bt.Content = "开始绘图";
            }
        }

        private void stop_Button_Click(object sender, RoutedEventArgs e)
        {
            Button bt = sender as Button;

            if (bt.Content.ToString() == "停止视觉反馈")
            {
                bt.Content = "继续视觉反馈";
                stopwatch.Reset();
                stopwatch.Stop();
            }
            else
            {
                stopwatch.Start();
            }
        }
    }
}
