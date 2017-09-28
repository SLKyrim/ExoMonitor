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

        const int MOTOR_NUM = 4; //设置电机个数
        const int RATIO = 160; //减速比

        private double[] userUnits = new double[MOTOR_NUM]; // 用户定义单位：编码器每圈计数

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

        public void dateTimer_Tick(object sender, EventArgs e)//取当前时间的委托
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
        }

        public void showParaTimer_Tick(object sender, EventArgs e)//输出电机参数到相应文本框的委托
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
                Motor2_Pos_TextBox.Text = ampObjAngleActual[0].ToString("F"); //电机实际位置，单位：°
                Motor2_Cur_TextBox.Text = (ampObj[0].CurrentActual * 0.01).ToString("F"); //电机电流，单位：A
                Motor2_Vel_TextBox.Text = ampObjAngleVelActual[0].ToString("F"); //电机实际速度，单位：rad/s
                Motor2_Acc_TextBox.Text = ampObjAngleAccActual[0].ToString("F"); //由轨迹计算而得的加速度，单位：rad/s^2

                //电机3(右髋)的文本框输出
                Motor3_Pos_TextBox.Text = ampObjAngleActual[0].ToString("F"); //电机实际位置，单位：°
                Motor3_Cur_TextBox.Text = (ampObj[0].CurrentActual * 0.01).ToString("F"); //电机电流，单位：A
                Motor3_Vel_TextBox.Text = ampObjAngleVelActual[0].ToString("F"); //电机实际速度，单位：rad/s
                Motor3_Acc_TextBox.Text = ampObjAngleAccActual[0].ToString("F"); //由轨迹计算而得的加速度，单位：rad/s^2

                //电机4(右膝)的文本框输出
                Motor4_Pos_TextBox.Text = ampObjAngleActual[0].ToString("F"); //电机实际位置，单位：°
                Motor4_Cur_TextBox.Text = (ampObj[0].CurrentActual * 0.01).ToString("F"); //电机电流，单位：A
                Motor4_Vel_TextBox.Text = ampObjAngleVelActual[0].ToString("F"); //电机实际速度，单位：rad/s
                Motor4_Acc_TextBox.Text = ampObjAngleAccActual[0].ToString("F"); //由轨迹计算而得的加速度，单位：rad/s^2
            }
            catch
            {
                statusBar.Background = new SolidColorBrush(Color.FromArgb(255, 230, 20, 20));
                statusInfoTextBlock.Text = "未连接外骨骼！";
            }
        }
    }
}
