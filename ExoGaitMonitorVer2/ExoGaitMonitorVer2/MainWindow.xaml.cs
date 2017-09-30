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

        //PVT模式
        private PVT pvt = new PVT();

        //SAC模式
        private bool SAC_flag = false;

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
            Brush brush = bt.Background;

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

                pvt.StartPVT(motors);

                statusBar.Background = new SolidColorBrush(Color.FromArgb(255, 230, 20, 20));
                statusInfoTextBlock.Text = "PVT模式";
                bt.Content = "Stop";
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

        private void SAC_Button_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
