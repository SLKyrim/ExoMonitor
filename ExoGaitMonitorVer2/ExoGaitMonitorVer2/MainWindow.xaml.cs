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

namespace ExoGaitMonitorVer2
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        #region 声明

        //CMO
        public AmpObj[] ampObj; //声明驱动器
        private ProfileSettingsObj profileSettingsObj; //声明驱动器属性
        private canOpenObj canObj; //声明网络接口
        private LinkageObj Linkage; //连接一组电机，能够按输入序列同时操作

        private DispatcherTimer controlTimer; //控制线程

        const int MOTOR_NUM = 4; //设置电机个数
        const int RATIO = 160; //减速比

        private double[] userUnits = new double[MOTOR_NUM]; // 用户定义单位：编码器每圈计数

        //SAC模式
        private Sensors sensors = new Sensors();
        private string[] SPCount = null; //存储计算机串口名称数组
        private string forceSensors_com = null; //拉压力传感器所用串口
        private int comCount = 0; //用来存储计算机可用串口数目，初始化为0
        private bool scanPorts_flag = false;
        private bool SAC_flag = false;

        #endregion

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            DispatcherTimer dateTimer = new DispatcherTimer();//显示当前时间线程
            dateTimer.Tick += new EventHandler(dateTimer_Tick);
            dateTimer.Interval = new TimeSpan(0, 0, 0, 1, 0);
            dateTimer.Start();

            try
            {
                canObj = new canOpenObj(); //实例化网络接口
                profileSettingsObj = new ProfileSettingsObj(); //实例化驱动器属性

                canObj.BitRate = CML_BIT_RATES.BITRATE_1_Mbit_per_sec; //设置CAN传输速率为1M/s

                canObj.Initialize(); //网络接口初始化

                ampObj = new AmpObj[MOTOR_NUM]; //实例化四个驱动器（盘式电机）
                Linkage = new LinkageObj();//实例化驱动器联动器

                for (int i = 0; i < MOTOR_NUM; i++)//初始化四个驱动器
                {
                    ampObj[i] = new AmpObj();
                    ampObj[i].Initialize(canObj, (short)(i + 1));
                    ampObj[i].HaltMode = CML_HALT_MODE.HALT_DECEL; //选择通过减速来停止电机的方式
                    ampObj[i].CountsPerUnit = 1; //电机转一圈编码器默认计数25600次，设置为4后转一圈计数6400次
                    userUnits[i] = ampObj[i].MotorInfo.ctsPerRev / ampObj[i].CountsPerUnit; //用户定义单位，counts/圈
                }

                Linkage.Initialize(ampObj);
                Linkage.SetMoveLimits(200000, 3000000, 3000000, 200000);
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

        private void showParaTimer_Tick(object sender, EventArgs e)//输出电机参数到相应文本框的委托
        {
            //2017-8-11-用ampObj[2].MotorInfo.ctsPerRev测得EC90盘式电机编码器一圈有25600个计数
            //2017-8-11-验证减速器减速比为160-即电机转160圈，关节转1圈
            double[] ampObjAngleActual = new double[MOTOR_NUM];//电机的转角，单位：°
            double[] ampObjAngleVelActual = new double[MOTOR_NUM];//电机的角速度，单位：rad/s
            double[] ampObjAngleAccActual = new double[MOTOR_NUM];//电机的角加速度，单位：rad/s^2

            try
            {
                for (int i = 0; i < MOTOR_NUM; i++)
                {
                    ampObjAngleActual[i] = (ampObj[i].PositionActual / userUnits[i]) * (360.0 / RATIO);//角度单位从counts转化为°
                    ampObjAngleVelActual[i] = (ampObj[i].VelocityActual / userUnits[i]) * 2.0 * Math.PI;//角速度单位从counts/s转化为rad/s
                    ampObjAngleAccActual[i] = (ampObj[i].TrajectoryAcc / userUnits[i]) * 2.0 * Math.PI;//角加速度单位从counts/s^2转化为rad/s^2
                }
                //电机1(左膝)的文本框输出
                Motor1_Pos_TextBox.Text = ampObjAngleActual[0].ToString("F"); //电机实际位置，单位：°
                Motor1_Cur_TextBox.Text = (ampObj[0].CurrentActual * 0.01).ToString("F"); //电机电流，单位：A
                Motor1_Vel_TextBox.Text = ampObjAngleVelActual[0].ToString("F"); //电机实际速度，单位：rad/s
                Motor1_Acc_TextBox.Text = ampObjAngleAccActual[0].ToString("F"); //由轨迹计算而得的加速度，单位：rad/s^2

                //电机2(左髋)的文本框输出
                Motor2_Pos_TextBox.Text = ampObjAngleActual[0].ToString("F");
                Motor2_Cur_TextBox.Text = (ampObj[0].CurrentActual * 0.01).ToString("F");
                Motor2_Vel_TextBox.Text = ampObjAngleVelActual[0].ToString("F");
                Motor2_Acc_TextBox.Text = ampObjAngleAccActual[0].ToString("F");

                //电机3(右髋)的文本框输出
                Motor3_Pos_TextBox.Text = ampObjAngleActual[0].ToString("F");
                Motor3_Cur_TextBox.Text = (ampObj[0].CurrentActual * 0.01).ToString("F");
                Motor3_Vel_TextBox.Text = ampObjAngleVelActual[0].ToString("F");
                Motor3_Acc_TextBox.Text = ampObjAngleAccActual[0].ToString("F");

                //电机4(右膝)的文本框输出
                Motor4_Pos_TextBox.Text = ampObjAngleActual[0].ToString("F");
                Motor4_Cur_TextBox.Text = (ampObj[0].CurrentActual * 0.01).ToString("F");
                Motor4_Vel_TextBox.Text = ampObjAngleVelActual[0].ToString("F");
                Motor4_Acc_TextBox.Text = ampObjAngleAccActual[0].ToString("F");
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

        #region SAC模式
        private void watchButton_Click(object sender, RoutedEventArgs e)//点击【启动监视】按钮时执行
        {
            controlTimer = new DispatcherTimer();
            controlTimer.Tick += new EventHandler(WriteCMD);
            controlTimer.Interval = TimeSpan.FromMilliseconds(10);
            controlTimer.Start();

            initButton.IsEnabled = true;
            watchButton.IsEnabled = false;

            SAC_flag = true;
        }

        public void WriteCMD(object sender, EventArgs e)//向传感器写命令以及向传感器接收数据的委托
        {

            byte[] command = new byte[8];
            command[0] = 0x01;//#设备地址
            command[1] = 0x03;//#功能代码，读寄存器的值
            command[2] = 0x00;//
            command[3] = 0x00;//
            command[4] = 0x00;//从第AI0号口开始读数据
            command[5] = 0x04;//读四个口
            command[6] = 0x44;//读四个口时的 CRC 校验的低 8 位
            command[7] = 0x09;//读四个口时的 CRC 校验的高 8 位 
            //一路的CRC校验位84 0A; 二路的是C4 0B; 三路的是 05 CB; 四路的是 44 09.

            sensors.forceSensor_SerialPort.Write(command, 0, 8);
            ForceSensor1_TextBox.Text = sensors.presN[0].ToString("F");
            ForceSensor2_TextBox.Text = sensors.presN[1].ToString("F");
            ForceSensor3_TextBox.Text = sensors.presN[2].ToString("F");
            ForceSensor4_TextBox.Text = sensors.presN[3].ToString("F");
        }

        private void initButton_Click(object sender, RoutedEventArgs e)//点击【初始归零】按钮时执行
        {
            sensors.pressInit();
            //SACStartButton.IsEnabled = true;
            initButton.IsEnabled = false;
        }
        #endregion

        private void angleSetButton_Click(object sender, RoutedEventArgs e)
        {

        }

        private void emergencyStopButton_Click(object sender, RoutedEventArgs e)
        {

        }

        private void zeroPointSetButton_Click(object sender, RoutedEventArgs e)
        {

        }

        private void getZeroPointButton_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
