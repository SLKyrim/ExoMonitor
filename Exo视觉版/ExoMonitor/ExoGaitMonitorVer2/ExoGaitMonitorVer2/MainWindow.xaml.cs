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
using LattePanda.Firmata;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.Net;

namespace ExoGaitMonitorVer2
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {

        #region 声明
        //主界面窗口

        //CMO
        public Motors motors = new Motors(); //声明电机类

        //手动操作设置
        private Manumotive manumotive = new Manumotive();

        //传感器

        private int value1;
        private int value2;
        private int value3;
        private int value4;
        private int _pattern;
        private int KeyTime = 0;

        private int PositionState = 0;
        private int Dete = 2;
        private int ss = 0;

        //PVT模式
        private PVT pvt = new PVT();

        //Stand up
        private Standup2 stand2 = new Standup2();

        Arduino arduino = new Arduino();

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

        Visual visual = new Visual();
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


            //DispatcherTimer keystate = new DispatcherTimer();
            //keystate.Tick += new EventHandler(Keystate_Tick);
            //keystate.Interval = TimeSpan.FromMilliseconds(200);
            //keystate.Start();

            DispatcherTimer Executekey = new DispatcherTimer();
            Executekey.Tick += new EventHandler(Execute_Tick);
            Executekey.Interval = TimeSpan.FromMilliseconds(200);
            Executekey.Start();


            Detection.Tick += Detection_Tick;
            Detection.Interval = TimeSpan.FromMilliseconds(200);
            Detection.Start();

            Thread shoutdown = new Thread(Select);
            shoutdown.Start();
        }



        private void Detection_Tick(object sender, EventArgs e)
        {
            nStepTextBox.Text = nStep.ToString();
            normalStepLengthTextBox.Text = normalStepLength.ToString();
            normalStepHeightTextBox.Text = normalStepHeight.ToString();
            lastStepLengthTextBox.Text = lastStepLength.ToString();
            lastStepHeightTextBox.Text = lastStepHeight.ToString();
            overStepLengthTextBox.Text = overStepLength.ToString();
            overStepHeightTextBox.Text = overStepHeight.ToString();


            if (PositionState == 0 && Dete == -1 && ss == 0)
            {
                PositionState = 1;
                Dete = 2;
                ss = 1;
            }
            if (PositionState == 0 && Dete == 1 && ss == 0)
            {
                PositionState = 2;
                Dete = 2;
                ss = 2;
            }
            if (PositionState == 1 && Dete == -1 && ss == 0)
            {
                PositionState = 3;
                Dete = 2;
                ss = 3;
            }
            if (PositionState == 1 && Dete == 1 && ss == 0)
            {
                PositionState = 4;
                Dete = 2;
                ss = 4;
            }
            if (PositionState == 2 && Dete == -1 && ss == 0)
            {
                PositionState = 3;
                Dete = 2;
                ss = 5;
            }
            if (PositionState == 2 && Dete == 1 && ss == 0)
            {
                PositionState = 4;
                Dete = 2;
                ss = 6;
            }
            if (PositionState == 3 && Dete == -1 && ss == 0)
            {
                PositionState = 1;
                Dete = 2;
                ss = 7;
            }
            if (PositionState == 3 && Dete == 1 && ss == 0)
            {
                PositionState = 2;
                Dete = 2;
                ss = 8;
            }
            if (PositionState == 4 && Dete == -1 && ss == 0)
            {
                PositionState = 1;
                Dete = 2;
                ss = 9;
            }
            if (PositionState == 4 && Dete == 1 && ss == 0)
            {
                PositionState = 2;
                Dete = 2;
                ss = 10;
            }
            //if(PositionState==3&&Dete==0&&ss==0)
            //{
            //    PositionState = 0;
            //    Dete = 2;
            //    ss = 11;
            //}
            //if(PositionState==4&&Dete==1&&ss==0)
            //{
            //    PositionState = 0;
            //    Dete = 2;
            //    ss = 12;
            //}
        }

        private void Select()
        {
            while (true)
            {
                if (ss != 0)
                {
                    Detection.Stop();
                    switch (ss)
                    {
                        case 1:
                            MessageBox.Show("1");
                            try
                            {
                                pvt.StartPVT(motors, "..\\..\\InputData\\左脚起始步低.txt", 2 * Math.PI, 115, 95);
                            }
                            catch (Exception e)
                            {
                                MessageBox.Show(e.ToString());
                            }
                            break;
                        case 2:
                            MessageBox.Show("2");
                            try
                            {
                                pvt.StartPVT(motors, "..\\..\\InputData\\左脚起始步高.txt", 2 * Math.PI, 115, 95);
                            }
                            catch (Exception e)
                            {
                                MessageBox.Show(e.ToString());
                            }
                            break;
                        case 3:
                            MessageBox.Show("3");
                            try
                            {
                                pvt.StartPVT(motors, "..\\..\\InputData\\左脚在前低到右脚前伸低.txt", 2 * Math.PI, 115, 95);
                            }
                            catch (Exception e)
                            {
                                MessageBox.Show(e.ToString());
                            }
                            break;
                        case 4:
                            MessageBox.Show("4");
                            try
                            {
                                pvt.StartPVT(motors, "..\\..\\InputData\\左脚在前低到右脚前伸高.txt", 2 * Math.PI, 115, 95);
                            }
                            catch (Exception e)
                            {
                                MessageBox.Show(e.ToString());
                            }
                            break;
                        case 5:
                            MessageBox.Show("5");
                            try
                            {
                                pvt.StartPVT(motors, "..\\..\\InputData\\左脚在前高到右脚前伸低.txt", 2 * Math.PI, 115, 95);
                            }
                            catch (Exception e)
                            {
                                MessageBox.Show(e.ToString());
                            }
                            break;
                        case 6:

                            MessageBox.Show("6");
                            try
                            {
                                pvt.StartPVT(motors, "..\\..\\InputData\\左脚在前高到右脚前伸高.txt", 2 * Math.PI, 115, 95);
                            }
                            catch (Exception e)
                            {
                                MessageBox.Show(e.ToString());
                            }
                            break;
                        case 7:
                            MessageBox.Show("7");
                            try
                            {
                                pvt.StartPVT(motors, "..\\..\\InputData\\右脚在前低到左脚前伸低.txt", 2 * Math.PI, 115, 95);
                            }
                            catch (Exception e)
                            {
                                MessageBox.Show(e.ToString());
                            }
                            break;
                        case 8:

                            MessageBox.Show("8");
                            try
                            {
                                pvt.StartPVT(motors, "..\\..\\InputData\\右脚在前低到左脚前伸高.txt", 2 * Math.PI, 115, 105);
                            }
                            catch (Exception e)
                            {
                                MessageBox.Show(e.ToString());
                            }
                            break;
                        case 9:
                            MessageBox.Show("9");
                            pvt.StartPVT(motors, "..\\..\\InputData\\右脚在前高到左脚前伸低.txt", 2 * Math.PI, 115, 95);
                            break;
                        case 10:
                            MessageBox.Show("10");
                            try
                            {
                                pvt.StartPVT(motors, "..\\..\\InputData\\右脚在前高到左脚前伸高.txt", 2 * Math.PI, 115, 95);
                            }
                            catch (Exception e)
                            {
                                MessageBox.Show(e.ToString());
                            }
                            break;
                        case 11:
                            MessageBox.Show("11");
                            try
                            {
                                pvt.StartPVT(motors, "..\\..\\InputData\\左脚向前低收步.txt", 2 * Math.PI, 115, 95);
                            }
                            catch (Exception e)
                            {
                                MessageBox.Show(e.ToString());
                            }
                            break;
                        case 12:
                            MessageBox.Show("12");
                            try
                            {
                                pvt.StartPVT(motors, "..\\..\\InputData\\左脚向前高收步.txt", 2 * Math.PI, 115, 95);
                            }
                            catch (Exception e)
                            {
                                MessageBox.Show(e.ToString());
                            }
                            break;
                        default:
                            break;
                    }
                    ss = 0;
                    Detection.Start();
                }
                Thread.Sleep(100);
            }



        }





        private void Keystate_Tick(object sender, EventArgs e)
        {
            try
            {
                arduino.pinMode(9, Arduino.OUTPUT);
                arduino.digitalWrite(9, Arduino.HIGH);
                arduino.pinMode(9, Arduino.INPUT);
                value1 = arduino.digitalRead(9);
                arduino.pinMode(10, Arduino.OUTPUT);
                arduino.digitalWrite(10, Arduino.HIGH);
                arduino.pinMode(10, Arduino.INPUT);
                value2 = arduino.digitalRead(10);
                arduino.pinMode(11, Arduino.OUTPUT);
                arduino.digitalWrite(11, Arduino.HIGH);
                arduino.pinMode(11, Arduino.INPUT);
                value3 = arduino.digitalRead(11);
                value4 = arduino.analogRead(0);
                double valuepositon = motors.ampObj[3].PositionActual;
                if (value4 < 800)
                {
                    value4 = 0;
                }
                else
                {
                    value4 = 1;
                }
                if (value1 == 0 & value2 == 1 & value3 == 0 & value4 == 1)
                {
                    if (valuepositon > 932977)
                    {
                        if (KeyTime == 0)
                        {
                            KeyTime = 1;
                        }
                        else if (KeyTime == 1)
                        {
                            _pattern = 3; ; //"Stand";  DoStandUp_Click(sender, e);
                        }

                    }


                }
                else
                {
                    if (value1 == 0 & value2 == 0 & value3 == 0 & value4 == 0)
                    {
                        if (valuepositon < 440000)
                        {
                            if (KeyTime == 0)
                            {
                                KeyTime = 1;
                            }
                            else if (KeyTime == 1)
                            {
                                _pattern = 15; //"Sit";   DoSitDown_Click(sender, e);
                            }

                        }


                    }
                    else
                    {
                        if (value1 == 0 & value2 == 1 & value3 == 0 & value4 == 0)
                        {

                            if (valuepositon < 100000)
                            {
                                if (KeyTime == 0)
                                {
                                    KeyTime = 1;
                                }
                                else if (KeyTime == 1)
                                {
                                    _pattern = 7;
                                }

                            }
                        }
                        else
                        {
                            if (value1 == 0 & value2 == 0 & value3 == 0 & value4 == 1)
                            {

                                if (valuepositon < 100000)
                                {
                                    if (KeyTime == 0)
                                    {
                                        KeyTime = 1;
                                    }
                                    else if (KeyTime == 1)
                                    {
                                        _pattern = 11;
                                    }

                                }

                            }
                            else
                            {
                                _pattern = 0;
                            }
                        }
                    }
                }
            }
            catch (Exception)
            {
                MessageBox.Show("拐杖状态有错");
            }
        }

        private void Execute_Tick(object sender, EventArgs e)
        {
            DispatcherTimer Executekey = new DispatcherTimer();
            switch (_pattern)
            {
                case 3:
                    _pattern = 0;
                    try
                    {
                        stand2.start_Standup2(motors);
                    }
                    catch (Exception)
                    {
                        MessageBox.Show(e.ToString());
                    }
                    Thread.Sleep(6000);
                    Executekey.IsEnabled = true;
                    break;
                case 15:
                    _pattern = 0;
                    try
                    {
                        pvt.start_Sitdown2(motors);
                    }
                    catch (Exception)
                    {
                        MessageBox.Show(e.ToString());
                    }
                    Thread.Sleep(5000);
                    Executekey.IsEnabled = true;
                    break;
                case 7:
                case 11:
                    _pattern = 0;
                    try
                    {
                        pvt.StartPVT(motors, "..\\..\\InputData\\6步新.txt", 360, 45, 15);
                    }
                    catch (Exception)
                    {
                        MessageBox.Show(e.ToString());
                    }
                    Thread.Sleep(10000);
                    Executekey.IsEnabled = true;
                    break;
                default:
                    break;

            }
        }

        private void Window_Closed(object sender, EventArgs e)
        {

        }

        #endregion



        #region 手动操作设置 Manumotive

        private void angleSetButton_Click(object sender, RoutedEventArgs e)//点击【执行命令】按钮时执行
        {
            angleSetButton.IsEnabled = false;
            emergencyStopButton.IsEnabled = true;
            getZeroPointButton.IsEnabled = false;
            zeroPointSetButton.IsEnabled = false;
            PVT_Button.IsEnabled = false;

            angleSetTextBox.IsReadOnly = true;
            motorNumberTextBox.IsReadOnly = true;


            int motorNumber = Convert.ToInt16(motorNumberTextBox.Text);
            int i = motorNumber - 1;

            motors.ampObj[i].PositionActual = 0;

            manumotive.angleSetStart(motors, Convert.ToDouble(angleSetTextBox.Text), Convert.ToInt16(motorNumberTextBox.Text), statusBar, statusInfoTextBlock,
                                     angleSetButton, emergencyStopButton, getZeroPointButton, zeroPointSetButton, PVT_Button, angleSetTextBox, motorNumberTextBox);
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
            PVT_Button.IsEnabled = false;
            getZeroPointButton.IsEnabled = false;
            angleSetButton.IsEnabled = false;
            emergencyStopButton.IsEnabled = false;
            zeroPointSetButton.IsEnabled = false;

            manumotive.getZeroPointStart(motors, statusBar, statusInfoTextBlock, angleSetButton, emergencyStopButton, getZeroPointButton,
                                          zeroPointSetButton, PVT_Button, angleSetTextBox, motorNumberTextBox);
        }

        #endregion

        private void PVT_Button_Click(object sender, RoutedEventArgs e)//进入PVT模式
        {
            Button bt = sender as Button;
            double positon = motors.ampObj[3].PositionActual;
            if (bt.Content.ToString() == "PVT Mode")
            {

                angleSetButton.IsEnabled = false;
                getZeroPointButton.IsEnabled = false;


                statusBar.Background = new SolidColorBrush(Color.FromArgb(255, 230, 20, 20));
                statusInfoTextBlock.Text = "PVT模式";
                bt.Content = "Stop";
                if (positon < 100000)
                {
                    try
                    {
                        pvt.StartPVT(motors, "..\\..\\InputData\\6步新.txt", 360, 45, 15);
                    }
                    catch (Exception)
                    {
                        MessageBox.Show(e.ToString());
                    }
                }

            }

            else
            {
                angleSetButton.IsEnabled = true;
                getZeroPointButton.IsEnabled = true;

                motors.Linkage.HaltMove();

                statusBar.Background = new SolidColorBrush(Color.FromArgb(255, 0, 122, 204));
                statusInfoTextBlock.Text = "PVT控制模式已停止";
                bt.Content = "PVT Mode";
            }
        }



        private void Sit_button_Click(object sender, RoutedEventArgs e)
        {
            Button bt = sender as Button;
            double positon = motors.ampObj[3].PositionActual;
            if (bt.Content.ToString() == "Sit Down")
            {
                PVT_Button.IsEnabled = false;
                Stand_up_Button.IsEnabled = false;
                angleSetButton.IsEnabled = false;
                getZeroPointButton.IsEnabled = false;

                statusBar.Background = new SolidColorBrush(Color.FromArgb(255, 230, 20, 20));
                statusInfoTextBlock.Text = "坐下模式";
                bt.Content = "Stop";
                if (positon < 430000)
                {
                    try
                    {
                        pvt.start_Sitdown2(motors);
                    }
                    catch (Exception)
                    { MessageBox.Show(e.ToString()); }
                }

            }
            else
            {
                angleSetButton.IsEnabled = true;
                getZeroPointButton.IsEnabled = true;
                PVT_Button.IsEnabled = true;
                Stand_up_Button.IsEnabled = true;

                //motors.Linkage.HaltMove();

                statusBar.Background = new SolidColorBrush(Color.FromArgb(255, 0, 122, 204));
                statusInfoTextBlock.Text = "坐下模式已停止";
                bt.Content = "Sit Down";
            }
        }

        private void Stand_up_Button_Click(object sender, RoutedEventArgs e)
        {
            Button bt = sender as Button;
            double positon = motors.ampObj[3].PositionActual;
            if (bt.Content.ToString() == "Stand Up")
            {
                PVT_Button.IsEnabled = false;
                angleSetButton.IsEnabled = false;
                Sit_button.IsEnabled = false;

                statusBar.Background = new SolidColorBrush(Color.FromArgb(255, 230, 20, 20));
                statusInfoTextBlock.Text = "起立模式";
                bt.Content = "Stop";
                if (positon > 1000000)
                {
                    try
                    {
                        stand2.start_Standup2(motors);
                    }
                    catch (Exception)
                    {
                        MessageBox.Show(e.ToString());
                    }
                }

            }
            else
            {
                PVT_Button.IsEnabled = true;
                angleSetButton.IsEnabled = true;

                Sit_button.IsEnabled = true;

                //motors.Linkage.HaltMove();

                statusBar.Background = new SolidColorBrush(Color.FromArgb(255, 0, 122, 204));
                statusInfoTextBlock.Text = "起立模式已结束";
                bt.Content = "Stand Up";
            }
        }

        private void Motor4_Pos_TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {

        }

        private void button1_Click(object sender, RoutedEventArgs e)
        {
            Dete = 1;
        }

        private void button_1_Click(object sender, RoutedEventArgs e)
        {
            Dete = 0;
        }
        struct IpAndPort
        {
            public string Ip;
            public string Port;
        }

        private void switch_Click(object sender, RoutedEventArgs e)
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
                    ComWinTextBox.Dispatcher.Invoke(new showData(ComWinTextBox.AppendText), "从服务端发来信息：" + Encoding.Default.GetString(buffer, 0, readSize) + "\n");

                    nStep = Convert.ToInt16(Math.Ceiling(Convert.ToDouble(buffer[1]) / 2)); // 任豪步态normal两步算一步，故除以2向上取整
                    lastStepLength = (buffer[2] << 8) | buffer[3];
                    lastStepHeight = (buffer[4] << 8) | buffer[5];
                    overStepLength = (buffer[6] << 8) | buffer[7];
                    overStepHeight = (buffer[8] << 8) | buffer[9];
                    normalStepHeight = 100;
                    normalStepLength = 400;

                    // 将单位转换为米(m)
                    normalStepLength = normalStepLength / 1000;
                    normalStepHeight = normalStepHeight / 1000;
                    lastStepLength = lastStepLength / 1000;
                    lastStepHeight = lastStepHeight / 1000;
                    overStepLength = overStepLength / 1000;
                    overStepHeight = overStepHeight / 1000;

                    //visual.visualGaitGenerator(nStep, normalStepLength, normalStepHeight, lastStepLength,
                    //       lastStepHeight, overStepLength, overStepHeight);
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
                pvt.StartPVT(motors, "..\\..\\bin\\Debug\\angle.txt", 360, 24, 64);
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
    }
}
