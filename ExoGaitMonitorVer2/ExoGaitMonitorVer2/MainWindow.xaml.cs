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
using LattePanda.Firmata;
using System.Net;
using System.Net.Sockets;
using System.Threading;

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
            EnumerableDataSource<MyPoint> total_M0_Vel;
            EnumerableDataSource<MyPoint> total_M1_Vel;
            EnumerableDataSource<MyPoint> total_M2_Vel;
            EnumerableDataSource<MyPoint> total_M3_Vel;
            EnumerableDataSource<MyPoint> total_M0_Cur;
            EnumerableDataSource<MyPoint> total_M1_Cur;
            EnumerableDataSource<MyPoint> total_M2_Cur;
            EnumerableDataSource<MyPoint> total_M3_Cur;

            EnumerableDataSource<MyPoint> M0_Pos;
            EnumerableDataSource<MyPoint> M1_Pos;
            EnumerableDataSource<MyPoint> M2_Pos;
            EnumerableDataSource<MyPoint> M3_Pos;
            EnumerableDataSource<MyPoint> M0_Vel;
            EnumerableDataSource<MyPoint> M1_Vel;
            EnumerableDataSource<MyPoint> M2_Vel;
            EnumerableDataSource<MyPoint> M3_Vel;
            EnumerableDataSource<MyPoint> M0_Cur;
            EnumerableDataSource<MyPoint> M1_Cur;
            EnumerableDataSource<MyPoint> M2_Cur;
            EnumerableDataSource<MyPoint> M3_Cur;

            InitializeComponent();

            cp = App.Current.Resources["Cp"] as ChartPlotter;

            #region 整体系统绘图

            total_M0_Pos = new EnumerableDataSource<MyPoint>(cp.M0_pointcollection_Pos);
            total_M0_Pos.SetXMapping(x => PosAx_total.ConvertToDouble(x.Date));
            total_M0_Pos.SetYMapping(y => y._point);
            PosPlot_total.AddLineGraph(total_M0_Pos, Colors.Red, 2, "左膝实际位置");
            total_M0_Vel = new EnumerableDataSource<MyPoint>(cp.M0_pointcollection_Vel);
            total_M0_Vel.SetXMapping(x => VelAx_total.ConvertToDouble(x.Date));
            total_M0_Vel.SetYMapping(y => y._point);
            VelPlot_total.AddLineGraph(total_M0_Vel, Colors.Red, 2, "左膝电机速度");
            total_M0_Cur = new EnumerableDataSource<MyPoint>(cp.M0_pointcollection_Cur);
            total_M0_Cur.SetXMapping(x => CurAx_total.ConvertToDouble(x.Date));
            total_M0_Cur.SetYMapping(y => y._point);
            CurPlot_total.AddLineGraph(total_M0_Cur, Colors.Red, 2, "左膝电机电流");

            total_M1_Pos = new EnumerableDataSource<MyPoint>(cp.M1_pointcollection_Pos);
            total_M1_Pos.SetXMapping(x => PosAx_total.ConvertToDouble(x.Date));
            total_M1_Pos.SetYMapping(y => y._point);
            PosPlot_total.AddLineGraph(total_M1_Pos, Colors.Green, 2, "左髋实际位置");
            total_M1_Vel = new EnumerableDataSource<MyPoint>(cp.M1_pointcollection_Vel);
            total_M1_Vel.SetXMapping(x => VelAx_total.ConvertToDouble(x.Date));
            total_M1_Vel.SetYMapping(y => y._point);
            VelPlot_total.AddLineGraph(total_M1_Vel, Colors.Green, 2, "左髋电机速度");
            total_M1_Cur = new EnumerableDataSource<MyPoint>(cp.M1_pointcollection_Cur);
            total_M1_Cur.SetXMapping(x => CurAx_total.ConvertToDouble(x.Date));
            total_M1_Cur.SetYMapping(y => y._point);
            CurPlot_total.AddLineGraph(total_M1_Cur, Colors.Green, 2, "左髋电机电流");

            total_M2_Pos = new EnumerableDataSource<MyPoint>(cp.M2_pointcollection_Pos);
            total_M2_Pos.SetXMapping(x => PosAx_total.ConvertToDouble(x.Date));
            total_M2_Pos.SetYMapping(y => y._point);
            PosPlot_total.AddLineGraph(total_M2_Pos, Colors.Brown, 2, "右髋实际位置");
            total_M2_Vel = new EnumerableDataSource<MyPoint>(cp.M2_pointcollection_Vel);
            total_M2_Vel.SetXMapping(x => VelAx_total.ConvertToDouble(x.Date));
            total_M2_Vel.SetYMapping(y => y._point);
            VelPlot_total.AddLineGraph(total_M2_Vel, Colors.Brown, 2, "右髋电机速度");
            total_M2_Cur = new EnumerableDataSource<MyPoint>(cp.M2_pointcollection_Cur);
            total_M2_Cur.SetXMapping(x => CurAx_total.ConvertToDouble(x.Date));
            total_M2_Cur.SetYMapping(y => y._point);
            CurPlot_total.AddLineGraph(total_M2_Cur, Colors.Brown, 2, "右髋电机电流");

            total_M3_Pos = new EnumerableDataSource<MyPoint>(cp.M3_pointcollection_Pos);
            total_M3_Pos.SetXMapping(x => PosAx_total.ConvertToDouble(x.Date));
            total_M3_Pos.SetYMapping(y => y._point);
            PosPlot_total.AddLineGraph(total_M3_Pos, Colors.Purple, 2, "右膝实际位置");
            total_M3_Vel = new EnumerableDataSource<MyPoint>(cp.M3_pointcollection_Vel);
            total_M3_Vel.SetXMapping(x => VelAx_total.ConvertToDouble(x.Date));
            total_M3_Vel.SetYMapping(y => y._point);
            VelPlot_total.AddLineGraph(total_M3_Vel, Colors.Purple, 2, "右膝电机速度");
            total_M3_Cur = new EnumerableDataSource<MyPoint>(cp.M3_pointcollection_Cur);
            total_M3_Cur.SetXMapping(x => CurAx_total.ConvertToDouble(x.Date));
            total_M3_Cur.SetYMapping(y => y._point);
            CurPlot_total.AddLineGraph(total_M3_Cur, Colors.Purple, 2, "右膝电机电流");

            #endregion

            M0_Pos = new EnumerableDataSource<MyPoint>(cp.M0_pointcollection_Pos);
            M0_Pos.SetXMapping(x => PosAx_M0.ConvertToDouble(x.Date));
            M0_Pos.SetYMapping(y => y._point);
            PosPlot_M0.AddLineGraph(M0_Pos, Colors.Red, 2, "左膝实际位置");
            M0_Vel = new EnumerableDataSource<MyPoint>(cp.M0_pointcollection_Vel);
            M0_Vel.SetXMapping(x => VelAx_M0.ConvertToDouble(x.Date));
            M0_Vel.SetYMapping(y => y._point);
            VelPlot_M0.AddLineGraph(M0_Vel, Colors.Red, 2, "左膝电机速度");
            M0_Cur = new EnumerableDataSource<MyPoint>(cp.M0_pointcollection_Cur);
            M0_Cur.SetXMapping(x => CurAx_M0.ConvertToDouble(x.Date));
            M0_Cur.SetYMapping(y => y._point);
            CurPlot_M0.AddLineGraph(M0_Cur, Colors.Red, 2, "左膝电机电流");

            M1_Pos = new EnumerableDataSource<MyPoint>(cp.M1_pointcollection_Pos);
            M1_Pos.SetXMapping(x => PosAx_M1.ConvertToDouble(x.Date));
            M1_Pos.SetYMapping(y => y._point);
            PosPlot_M1.AddLineGraph(M1_Pos, Colors.Green, 2, "左髋实际位置");
            M1_Vel = new EnumerableDataSource<MyPoint>(cp.M1_pointcollection_Vel);
            M1_Vel.SetXMapping(x => VelAx_M1.ConvertToDouble(x.Date));
            M1_Vel.SetYMapping(y => y._point);
            VelPlot_M1.AddLineGraph(M1_Vel, Colors.Green, 2, "左髋电机速度");
            M1_Cur = new EnumerableDataSource<MyPoint>(cp.M1_pointcollection_Cur);
            M1_Cur.SetXMapping(x => CurAx_M1.ConvertToDouble(x.Date));
            M1_Cur.SetYMapping(y => y._point);
            CurPlot_M1.AddLineGraph(M1_Cur, Colors.Green, 2, "左髋电机电流");

            M2_Pos = new EnumerableDataSource<MyPoint>(cp.M2_pointcollection_Pos);
            M2_Pos.SetXMapping(x => PosAx_M2.ConvertToDouble(x.Date));
            M2_Pos.SetYMapping(y => y._point);
            PosPlot_M2.AddLineGraph(M2_Pos, Colors.Brown, 2, "右髋实际位置");
            M2_Vel = new EnumerableDataSource<MyPoint>(cp.M2_pointcollection_Vel);
            M2_Vel.SetXMapping(x => VelAx_M2.ConvertToDouble(x.Date));
            M2_Vel.SetYMapping(y => y._point);
            VelPlot_M2.AddLineGraph(M2_Vel, Colors.Brown, 2, "右髋电机速度");
            M2_Cur = new EnumerableDataSource<MyPoint>(cp.M2_pointcollection_Cur);
            M2_Cur.SetXMapping(x => CurAx_M2.ConvertToDouble(x.Date));
            M2_Cur.SetYMapping(y => y._point);
            CurPlot_M2.AddLineGraph(M2_Cur, Colors.Brown, 2, "右髋电机电流");

            M3_Pos = new EnumerableDataSource<MyPoint>(cp.M3_pointcollection_Pos);
            M3_Pos.SetXMapping(x => PosAx_M3.ConvertToDouble(x.Date));
            M3_Pos.SetYMapping(y => y._point);
            PosPlot_M3.AddLineGraph(M3_Pos, Colors.Purple, 2, "右膝实际位置");
            M3_Vel = new EnumerableDataSource<MyPoint>(cp.M3_pointcollection_Vel);
            M3_Vel.SetXMapping(x => VelAx_M3.ConvertToDouble(x.Date));
            M3_Vel.SetYMapping(y => y._point);
            VelPlot_M3.AddLineGraph(M3_Vel, Colors.Purple, 2, "右膝电机速度");
            M3_Cur = new EnumerableDataSource<MyPoint>(cp.M3_pointcollection_Cur);
            M3_Cur.SetXMapping(x => CurAx_M3.ConvertToDouble(x.Date));
            M3_Cur.SetYMapping(y => y._point);
            CurPlot_M3.AddLineGraph(M3_Cur, Colors.Purple, 2, "右膝电机电流");
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

        #endregion

        #region 声明
        //主界面窗口

        //CMO
        public Motors motors = new Motors(); //声明电机类

        //手动操作设置
        private Manumotive manumotive = new Manumotive();

        //传感器
        private Sensors sensors = new Sensors();
        private string[] SPCount = null; //存储计算机串口名称数组
        private string forceSensors_com = null; //拉压力传感器所用串口
        private int comCount = 0; //用来存储计算机可用串口数目，初始化为0
        private bool scanPorts_flag = false;

        private int value1;
        private int value2;
        private int value3;
        private int value4;
        private int _pattern;
        //PVT模式
        private PVT pvt = new PVT();

        //Sit Down 模式
        private Sitdown sit = new Sitdown();
        //Stand up
        private Standup stand = new Standup();
        //SAC模式
        private bool SAC_flag = false;
        private SAC sac = new SAC();
        
        //写数据
        private WriteExcel writeExcel = new WriteExcel();

        //力学模式
        private Force force = new Force();
        //平地行走模式
        private Flat flat = new Flat();

        Arduino arduino = new Arduino();

        public delegate void showData(string msg);//通信窗口输出
        private TcpClient client;
        private TcpListener server;
        private const int bufferSize = 8000;

        #endregion

        #region 界面初始化

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            DispatcherTimer dateTimer = new DispatcherTimer();//显示当前时间线程
            dateTimer.Tick += new EventHandler(dateTimer_Tick);
            dateTimer.Interval = new TimeSpan(0, 0, 0, 1, 0);
            dateTimer.Start();

            try
            {
                motors.motors_Init();
                //cp.plotStart(motors, statusBar, statusInfoTextBlock);
            }
            catch
            {
                statusBar.Background = new SolidColorBrush(Color.FromArgb(255, 230, 20, 20));
                statusInfoTextBlock.Text = "窗口初始化失败！";
            }
            
            DispatcherTimer showParaTimer = new DispatcherTimer(); //显示参数线程
            showParaTimer.Tick += new EventHandler(showParaTimer_Tick);
            showParaTimer.Interval = TimeSpan.FromMilliseconds(100);
            showParaTimer.Start();

            DispatcherTimer keystate = new DispatcherTimer(); //显示拐杖状态
            keystate.Tick += new EventHandler(Keystate_Tick);
            keystate.Interval = TimeSpan.FromMilliseconds(50);
            keystate.Start();

            DispatcherTimer Executekey = new DispatcherTimer();
            Executekey.Tick += new EventHandler(Execute_Tick);
            Executekey.Interval = TimeSpan.FromMilliseconds(100);
            Executekey.Start();
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            sensors.SerialPortClose();
        }

        private void dateTimer_Tick(object sender, EventArgs e)//取当前时间的委托
        {
            string timeDateString = "";
            DateTime now = DateTime.Now;
            timeDateString = string.Format("{0}年{1}月{2}日 {3}:{4}:{5}",
                now.Year,
                now.Month.ToString("00"),
                now.Day.ToString("00"),
                now.Hour.ToString("00"),
                now.Minute.ToString("00"),
                now.Second.ToString("00"));

            timeDateTextBlock.Text = timeDateString;

            ScanPorts();
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

                    _pattern = 3;  //"Stand";  DoStandUp_Click(sender, e);

                }
                else
                {
                    if (value1 == 0 & value2 == 0 & value3 == 0 & value4 == 0)
                    {

                        _pattern = 16; //"Sit";   DoSitDown_Click(sender, e);

                    }
                    else
                    {
                        if (value1 == 0 & value2 == 1 & value3 == 0 & value4 == 0)
                        {

                            _pattern = 7;

                        }
                        else
                        {
                            if (value1 == 0 & value2 == 0 & value3 == 0 & value4 == 1)
                            {
                                _pattern = 11;

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
            switch (_pattern)
            {
                case 3:
                   stand.start_Standup(motors);
                    break;
                case 15:
                    sit.StartSitdown(motors);
                    break;
                case 7:
                case 11:
                    pvt.StartPVT(motors);
                    break;
                default:
                    break;

            }
        }
        private void showParaTimer_Tick(object sender, EventArgs e)//输出电机参数到相应文本框的委托
        {
            try
            {
                //电机1(左膝)的文本框输出
                Motor1_Pos_TextBox.Text = motors.ampObjAngleActual[0].ToString("F"); //电机实际位置，单位：°
                Motor1_Cur_TextBox.Text = (motors.ampObj[0].CurrentActual * 0.01).ToString("F"); //电机电流，单位：A
                Motor1_Vel_TextBox.Text = motors.ampObjAngleVelActual[0].ToString("F"); //电机实际速度，单位：rad/s
                Motor1_Acc_TextBox.Text = motors.ampObjAngleAccActual[0].ToString("F"); //由轨迹计算而得的加速度，单位：rad/s^2

                //电机2(左髋)的文本框输出
                Motor2_Pos_TextBox.Text = motors.ampObjAngleActual[1].ToString("F");
                Motor2_Cur_TextBox.Text = (motors.ampObj[1].CurrentActual * 0.01).ToString("F");
                Motor2_Vel_TextBox.Text = motors.ampObjAngleVelActual[1].ToString("F");
                Motor2_Acc_TextBox.Text = motors.ampObjAngleAccActual[1].ToString("F");

                //电机3(右髋)的文本框输出
                Motor3_Pos_TextBox.Text = motors.ampObjAngleActual[2].ToString("F");
                Motor3_Cur_TextBox.Text = (motors.ampObj[2].CurrentActual * 0.01).ToString("F");
                Motor3_Vel_TextBox.Text = motors.ampObjAngleVelActual[2].ToString("F");
                Motor3_Acc_TextBox.Text = motors.ampObjAngleAccActual[2].ToString("F");

                //电机4(右膝)的文本框输出
                Motor4_Pos_TextBox.Text = motors.ampObjAngleActual[3].ToString("F");
                Motor4_Cur_TextBox.Text = (motors.ampObj[3].CurrentActual * 0.01).ToString("F");
                Motor4_Vel_TextBox.Text = motors.ampObjAngleVelActual[3].ToString("F");
                Motor4_Acc_TextBox.Text = motors.ampObjAngleAccActual[3].ToString("F");

                if(SAC_flag)//拉压力传感器的文本框输出
                {
                    ForceSensor1_TextBox.Text = sensors.presN[0].ToString("F");
                    ForceSensor2_TextBox.Text = sensors.presN[1].ToString("F");
                    ForceSensor3_TextBox.Text = sensors.presN[2].ToString("F");
                    ForceSensor4_TextBox.Text = sensors.presN[3].ToString("F");
                }
            }
            catch
            {
                statusBar.Background = new SolidColorBrush(Color.FromArgb(255, 230, 20, 20));
                statusInfoTextBlock.Text = "未连接外骨骼！";
            }
        }

        private void ScanPorts()//扫描可用串口
        {
            SPCount = sensors.CheckSerialPortCount();      //获得计算机可用串口名称数组

            ComboBoxItem tempComboBoxItem = new ComboBoxItem();

            if (comCount != SPCount.Length)            //SPCount.length其实就是可用串口的个数
            {
                //当可用串口计数器与实际可用串口个数不相符时
                //初始化下拉窗口并将flag初始化为false

                ForceSensorPort_ComboBox.Items.Clear();


                tempComboBoxItem = new ComboBoxItem();
                tempComboBoxItem.Content = "请选择串口";
                ForceSensorPort_ComboBox.Items.Add(tempComboBoxItem);
                ForceSensorPort_ComboBox.SelectedIndex = 0;

                forceSensors_com = null;
                scanPorts_flag = false;

                if (comCount != 0)
                {
                    //在操作过程中增加或减少串口时发生
                    statusBar.Background = new SolidColorBrush(Color.FromArgb(255, 230, 20, 20));
                    statusInfoTextBlock.Text = "串口数目已改变，请重新选择串口！";
                }
                comCount = SPCount.Length;     //将可用串口计数器与现在可用串口个数匹配
            }

            if (!scanPorts_flag)
            {
                if (SPCount.Length > 0)
                {
                    //有可用串口时执行
                    comCount = SPCount.Length;

                    statusBar.Background = new SolidColorBrush(Color.FromArgb(255, 0, 122, 204));
                    statusInfoTextBlock.Text = "检测到" + SPCount.Length + "个串口!";

                    for (int i = 0; i < SPCount.Length; i++)
                    {
                        //分别将可用串口添加到各个下拉窗口中
                        string tempstr = "串口" + SPCount[i];

                        tempComboBoxItem = new ComboBoxItem();
                        tempComboBoxItem.Content = tempstr;
                        ForceSensorPort_ComboBox.Items.Add(tempComboBoxItem);

                    }
                    scanPorts_flag = true;
                }
                else
                {
                    comCount = 0;
                    statusBar.Background = new SolidColorBrush(Color.FromArgb(255, 230, 20, 20));
                    statusInfoTextBlock.Text = "未检测到串口!";
                }
            }
        }

        #endregion

        #region 传感器 Sensors

        private void ForceSensorPort_ComboBox_DropDownClosed(object sender, EventArgs e)//选择拉压力传感器串口
        {
            ComboBoxItem item = ForceSensorPort_ComboBox.SelectedItem as ComboBoxItem;
            string tempstr = item.Content.ToString();

            for (int i = 0; i < SPCount.Length; i++)
            {
                if (tempstr == "串口" + SPCount[i])
                {
                    try
                    {
                        forceSensors_com = SPCount[i];
                        sensors.forceSensor_SerialPort_Init(SPCount[i]);

                        watchButton.IsEnabled = true;
                    }
                    catch
                    {
                        statusBar.Background = new SolidColorBrush(Color.FromArgb(255, 230, 20, 20));
                        statusInfoTextBlock.Text = "未正确选择串口!";
                    }
                }
            }
        }

        private void watchButton_Click(object sender, RoutedEventArgs e)//点击【启动监视】按钮时执行
        {
            initButton.IsEnabled = true;
            watchButton.IsEnabled = false;

            SAC_flag = true;
            sensors.writeCommandStart();
        }

        private void initButton_Click(object sender, RoutedEventArgs e)//点击【初始归零】按钮时执行
        {
            sensors.pressInit();
            SAC_Button.IsEnabled = true;
            initButton.IsEnabled = false;
            Force_Button.IsEnabled = true;
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

            if(bt.Content.ToString() == "PVT Mode")
            {
                SAC_Button.IsEnabled = false;
                angleSetButton.IsEnabled = false;
                getZeroPointButton.IsEnabled = false;
                if (SAC_flag)//避免和力学模式及SAC模式冲突
                {                    
                    sensors.writeCommandStop();
                    SAC_flag = false;
                }

                statusBar.Background = new SolidColorBrush(Color.FromArgb(255, 230, 20, 20));
                statusInfoTextBlock.Text = "PVT模式";
                bt.Content = "Stop";

                pvt.StartPVT(motors);
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

        private void SAC_Button_Click(object sender, RoutedEventArgs e)//进入SAC模式
        {
            Button bt = sender as Button;

            if (bt.Content.ToString() == "SAC Mode")
            {
                PVT_Button.IsEnabled = false;
                angleSetButton.IsEnabled = false;
                getZeroPointButton.IsEnabled = false;

                sac.StartSAC(motors, sensors, statusBar, statusInfoTextBlock);

                statusBar.Background = new SolidColorBrush(Color.FromArgb(255, 230, 20, 20));
                statusInfoTextBlock.Text = "SAC模式";
                bt.Content = "Stop";
            }

            else
            {
                PVT_Button.IsEnabled = true;
                angleSetButton.IsEnabled = true;
                getZeroPointButton.IsEnabled = true;

                sac.StopSAC(motors);

                statusBar.Background = new SolidColorBrush(Color.FromArgb(255, 0, 122, 204));
                statusInfoTextBlock.Text = "SAC控制模式已停止";
                bt.Content = "SAC Mode";
            }
        }

        private void WriteExcel_Button_Click(object sender, RoutedEventArgs e)//写数据进Excel
        {
            Button bt = sender as Button;
            if (bt.Content.ToString() == "写入数据")
            {
                if (!writeExcel.writeStart(statusBar, statusInfoTextBlock, motors))
                {
                    statusBar.Background = new SolidColorBrush(Color.FromArgb(255, 230, 20, 20));
                    statusInfoTextBlock.Text = "写入数据失败！";
                    return;
                }
                bt.Content = "停止写入";
            }
            else
            {
                writeExcel.writeStop();
                bt.Content = "写入数据";
            }
        }

        private void Force_Button_Click(object sender, RoutedEventArgs e)
        {
            Button bt = sender as Button;

            if (bt.Content.ToString() == "Force Mode")
            {
                PVT_Button.IsEnabled = false;
                angleSetButton.IsEnabled = false;
                getZeroPointButton.IsEnabled = false;

                force.StartForce(motors, sensors, statusBar, statusInfoTextBlock);

                statusBar.Background = new SolidColorBrush(Color.FromArgb(255, 230, 20, 20));
                statusInfoTextBlock.Text = "力学模式";
                bt.Content = "Stop";
            }

            else
            {
                PVT_Button.IsEnabled = true;
                angleSetButton.IsEnabled = true;
                getZeroPointButton.IsEnabled = true;

                force.StopForce(motors);

                statusBar.Background = new SolidColorBrush(Color.FromArgb(255, 0, 122, 204));
                statusInfoTextBlock.Text = "力学模式已停止";
                bt.Content = "Force Mode";
            }
        }

        private void Sit_button_Click(object sender, RoutedEventArgs e)
        {
            Button bt = sender as Button;
            if(bt.Content.ToString()=="Sit Down")
            {
                PVT_Button.IsEnabled = false;
                SAC_Button.IsEnabled = false;
                angleSetButton.IsEnabled = false;
                getZeroPointButton.IsEnabled = false;
                if(SAC_flag)//
                {
                    sensors.writeCommandStop();
                    SAC_flag = false;
                }

                statusBar.Background = new SolidColorBrush(Color.FromArgb(255, 230, 20, 20));
                statusInfoTextBlock.Text = "坐下模式";
                bt.Content = "Stop";

                sit.StartSitdown(motors);
            }
            else
            {
                angleSetButton.IsEnabled = true;
                getZeroPointButton.IsEnabled = true;

                motors.Linkage.HaltMove();

                statusBar.Background = new SolidColorBrush(Color.FromArgb(255, 0, 122, 204));
                statusInfoTextBlock.Text = "坐下模式已停止";
                bt.Content = "Sit Down";
            }
        }

        private void Stand_up_Button_Click(object sender, RoutedEventArgs e)
        {
            Button bt = sender as Button;
            if(bt.Content.ToString()=="Stand Up")
            {
                PVT_Button.IsEnabled = false;
                angleSetButton.IsEnabled = false;
                SAC_Button.IsEnabled = false;
                Sit_button.IsEnabled = false;

                statusBar.Background = new SolidColorBrush(Color.FromArgb(255, 230, 20, 20));
                statusInfoTextBlock.Text = "起立模式";
                bt.Content = "Stop";

                stand.start_Standup(motors);
            }
            else
            {
                PVT_Button.IsEnabled = true;
                angleSetButton.IsEnabled = true;
                SAC_Button.IsEnabled = true;
                Sit_button.IsEnabled = true;

                motors.Linkage.HaltMove();

                statusBar.Background = new SolidColorBrush(Color.FromArgb(255, 0, 122, 204));
                statusInfoTextBlock.Text = "起立模式已结束";
                bt.Content = "Stand Up";
            }
        }
                    
        
        private void flat_button_Click(object sender, RoutedEventArgs e)
        {

        }

        private void yunsuan_Click_1(object sender, RoutedEventArgs e)
        {
            Button bt = sender as Button;
            if (bt.Content.ToString() == "Flat")
            {
                flat.StartFlat(Convert.ToSingle(textBox1.Text), Convert.ToSingle(textBox.Text), textBox1, textBox);
                statusBar.Background = new SolidColorBrush(Color.FromArgb(255, 230, 20, 20));
                statusInfoTextBlock.Text = "正在运算";
                bt.Content = "Stop";

            }
            else
            {
                //flat.stopflat();
                statusBar.Background = new SolidColorBrush(Color.FromArgb(255, 0, 122, 204));
                statusInfoTextBlock.Text = "运算结束 可以开始行走";
                bt.Content = "Flat";
            }
        }
        struct IpAndPort
        {
            public string Ip;
            public string Port;
        }
        private void switch_Click(object sender, RoutedEventArgs e)
        {
            if(IPAdressTextBox.Text.Trim()==string.Empty)
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
            IpAndPort ipHePort = new IpAndPort();
            ipHePort.Ip = IPAdressTextBox.Text;
            ipHePort.Port = PortTextBox.Text;

            thread.Start((object)ipHePort);
        }

        private void btnSend_Click(object sender, RoutedEventArgs e)
        {
            if(stxtSendMsg.Text.Trim()!=string.Empty)
            {
                NetworkStream sendStream = client.GetStream();//获得用于数据传输的流
                byte[] buffer = Encoding.Default.GetBytes(stxtSendMsg.Text.Trim());//将数据存在缓冲中
                sendStream.Write(buffer, 0, buffer.Length);//最终写入流中
                string showmsg = Encoding.Default.GetString(buffer, 0, buffer.Length);
                ComWinTextBox1.AppendText("发送给服务端数据：" + showmsg + "\n");
                stxtSendMsg.Text = string.Empty;
            }
        }
        private void ComWinTextBox_TextChanged(object sender,TextChangedEventArgs e)
        {
            ComWinTextBox.ScrollToEnd();//当通信窗口内容变化时滚动条定位在最下面
        }
        private void ComWinTextBox1_TextChanged(object sender, TextChangedEventArgs e)
        {
            ComWinTextBox1.ScrollToEnd();//当通信窗口内容变化时滚动条定位在最下面
        }
        private void reciveAndListener(object ipAndPort)
        {
            IpAndPort ipHePort = (IpAndPort)ipAndPort;

            IPAddress ip = IPAddress.Parse(ipHePort.Ip);
            server = new TcpListener(ip, int.Parse(ipHePort.Port));
            server.Start();//启动监听

            ComWinTextBox.Dispatcher.Invoke(new showData(ComWinTextBox.AppendText), "服务端开启侦听....\n");

            //获取连接的客户d端的对象
            client = server.AcceptTcpClient();
            ComWinTextBox.Dispatcher.Invoke(new showData(ComWinTextBox.AppendText), "有客户端请求连接，连接已建立！");//AcceptTcpClient 是同步方法，会阻塞进程，得到连接对象后才会执行这一步  

            //获得流
            NetworkStream reciveStream = client.GetStream();
            #region 循环监听客户端发来的信息
            do
            {
                byte[] buffer = new byte[bufferSize];
                int msgSize;
                try
                {
                    lock (reciveStream)
                    {
                        msgSize = reciveStream.Read(buffer, 0, bufferSize);
                    }

                    if (msgSize == 0)
                        return;
                    string msg = Encoding.Default.GetString(buffer, 0, bufferSize);
                    ComWinTextBox.Dispatcher.Invoke(new showData(ComWinTextBox.AppendText), "\n客户端曰：" +
                        Encoding.Default.GetString(buffer, 0, msgSize));
                }
                catch
                {
                    ComWinTextBox.Dispatcher.Invoke(new showData(ComWinTextBox.AppendText), "\n 出现异常：连接被迫关闭");
                    break;
                }
            } while (true);
            #endregion
        }
    }
}
