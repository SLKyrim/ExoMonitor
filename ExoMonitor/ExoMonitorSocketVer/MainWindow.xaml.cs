using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using ClassLib.Motor;
using ClassLib.Walk;
using System.Windows.Threading;
using System.Net.Sockets;
using System.Threading;
using System.Net;
using System.Runtime.InteropServices;

namespace ExoMonitorSocketVer
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        //CMO
        public Motors motors = new Motors(); //声明电机类

        //手动操作设置
        private Manumotive manumotive = new Manumotive();

        //平地行走模式
        private Flat flat = new Flat();

        //PVT模式
        private PVT pvt = new PVT();

        public MainWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                motors.motors_Init();
            }
            catch
            {
                statusBar.Background = new SolidColorBrush(Color.FromArgb(255, 230, 20, 20));
                statusInfoTextBlock.Text = "窗口初始化失败！";
            }
        }

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


        #region 服务端

        public delegate void showData(string msg); //通信窗口输出相关
        private TcpListener server;
        private TcpClient client;
        private const int bufferSiize = 8000;
        struct IpAndPort
        {
            public string Ip;
            public string Port;
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            Environment.Exit(0);
        }

        private void Switch_Button_Click(object sender, RoutedEventArgs e) //启动服务器
        {
            Button bt = sender as Button;

            IpAndPort ipAndport = new IpAndPort();
            ipAndport.Ip = IPAdressTextBox.Text;
            ipAndport.Port = PortTextBox.Text;

            if (bt.Content.ToString() == "启动服务器")
            {
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

                Thread thread = new Thread(reciveAndListener);
                thread.Start((object)ipAndport);

                ComWinTextBox.Dispatcher.Invoke(new showData(ComWinTextBox.AppendText), "服务器 " + ipAndport.Ip + " : " + ipAndport.Port + " 已开启监听\n");
                statusInfoTextBlock.Text = "服务器已启动";
                bt.Content = "关闭服务器";
            }

            else
            {
                server.Stop();
                ComWinTextBox.Dispatcher.Invoke(new showData(ComWinTextBox.AppendText), "服务器 " + ipAndport.Ip + " : " + ipAndport.Port + " 已关闭\n");
                statusInfoTextBlock.Text = "服务器已关闭";
                bt.Content = "启动服务器";
            }

        }

        private void reciveAndListener(object ipAndPort)
        {
            IpAndPort ipAndport = (IpAndPort)ipAndPort;

            IPAddress ip = IPAddress.Parse(ipAndport.Ip);
            server = new TcpListener(ip, int.Parse(ipAndport.Port));
            server.Start();//启动监听

            client = server.AcceptTcpClient();
            ComWinTextBox.Dispatcher.Invoke(new showData(ComWinTextBox.AppendText), "有客户端请求连接，连接已建立\n");
            //AcceptTcpClient 是同步方法，会阻塞进程，得到连接对象后才会执行这一步 

            //获得流
            NetworkStream reciveStream = client.GetStream();
            #region
            do
            {
                byte[] buffer = new byte[bufferSiize];
                int msgSize;
                try
                {
                    lock (reciveStream)
                    {
                        msgSize = reciveStream.Read(buffer, 0, bufferSiize);
                    }
                    if (msgSize == 0)
                        return;
                    string msg = Encoding.Default.GetString(buffer, 0, bufferSiize);
                    ComWinTextBox.Dispatcher.Invoke(new showData(ComWinTextBox.AppendText), "从客户端发来信息：" + Encoding.Default.GetString(buffer, 0, msgSize) + "\n");
                }
                catch
                {
                    ComWinTextBox.Dispatcher.Invoke(new showData(ComWinTextBox.AppendText), "出现异常：连接被迫关闭\n");
                    break;
                }
            } while (true);
            #endregion
        }


        private void Send_Button_Click(object sender, RoutedEventArgs e)
        {
            if (MessageTextBox.Text.Trim() != string.Empty)
            {
                NetworkStream sendStream = client.GetStream();
                byte[] buffer = Encoding.Default.GetBytes(MessageTextBox.Text.Trim());
                sendStream.Write(buffer, 0, buffer.Length);
                MessageTextBox.Text = string.Empty;
            }
        }

        private void ComWinTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            ComWinTextBox.ScrollToEnd(); //当通信窗口内容有变化时保持滚动条在最下面
        }



        #endregion

        private void GenGait_Button_Click(object sender, RoutedEventArgs e)
        {
            flat.StartFlat(Convert.ToSingle(textBox1.Text), Convert.ToSingle(textBox.Text), textBox1, textBox);
            statusBar.Background = new SolidColorBrush(Color.FromArgb(255, 230, 20, 20));
            statusInfoTextBlock.Text = "运算结束 可以开始行走";
            Walk_Button.IsEnabled = true;
        }

        private void Walk_Button_Click(object sender, RoutedEventArgs e)
        {
            Button bt = sender as Button;

            if (bt.Content.ToString() == "开始行走")
            {
                angleSetButton.IsEnabled = false;
                getZeroPointButton.IsEnabled = false;

                statusBar.Background = new SolidColorBrush(Color.FromArgb(255, 230, 20, 20));
                statusInfoTextBlock.Text = "PVT模式";
                bt.Content = "停止行走";

                pvt.StartPVT(motors);
            }

            else
            {
                angleSetButton.IsEnabled = true;
                getZeroPointButton.IsEnabled = true;

                motors.Linkage.HaltMove();

                statusBar.Background = new SolidColorBrush(Color.FromArgb(255, 0, 122, 204));
                statusInfoTextBlock.Text = "PVT控制模式已停止";
                bt.Content = "开始行走";
            }
        }
    }
}
