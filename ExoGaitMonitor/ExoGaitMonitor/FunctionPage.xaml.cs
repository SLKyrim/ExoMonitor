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
using System.IO.Ports;
using System.IO;
using System.Windows.Threading;
using System.Xml.Serialization;
using CMLCOMLib;

namespace ExoGaitMonitor
{
    /// <summary>
    /// FunctionPage.xaml 的交互逻辑
    /// </summary>
    public partial class FunctionPage : Page
    {
        public FunctionPage()
        {
            InitializeComponent();
        }

        private AmpObj[] ampObj; //声明驱动器
        private ProfileSettingsObj profileSettingsObj; //声明驱动器属性
        private canOpenObj canObj; //声明网络接口

        private DispatcherTimer ShowTimer;
        private void FunctionPage_Loaded(object sender, RoutedEventArgs e)//打开窗口后进行的初始化操作
        {
            //显示当前时间的计时器
            ShowTimer = new System.Windows.Threading.DispatcherTimer();
            ShowTimer.Tick += new EventHandler(ShowCurTimer); 
            ShowTimer.Interval = new TimeSpan(0, 0, 0, 1, 0); 
            ShowTimer.Start();

            try
            {
                canObj = new canOpenObj(); //实例化网络接口
                profileSettingsObj = new ProfileSettingsObj(); //实例化驱动器属性

                canObj.BitRate = CML_BIT_RATES.BITRATE_1_Mbit_per_sec; //设置CAN传输速率为1M/s
                canObj.Initialize(); //网络接口初始化

                ampObj = new AmpObj[4]; //实例化四个驱动器（盘式电机）

                for (int i = 0; i < 4; i++)//初始化四个驱动器
                {
                    ampObj[i] = new AmpObj();
                    ampObj[i].Initialize(canObj, (short)(i + 1));
                    ampObj[i].HaltMode = CML_HALT_MODE.HALT_DECEL; //选择通过减速来停止电机的方式
                }

                profileSettingsObj.ProfileType = CML_PROFILE_TYPE.PROFILE_VELOCITY; // 选择速度模式控制电机

                for (int i = 0; i < 4; i++)
                {
                    profileSettingsObj.ProfileAccel = ampObj[i].VelocityLoopSettings.VelLoopMaxAcc;
                    profileSettingsObj.ProfileDecel = ampObj[i].VelocityLoopSettings.VelLoopMaxDec;
                    profileSettingsObj.ProfileVel = ampObj[i].VelocityLoopSettings.VelLoopMaxVel;
                    ampObj[i].ProfileSettings = profileSettingsObj;
                }

                if (ampObj[0] == null || ampObj[1] == null || ampObj[2] == null || ampObj[3] == null)
                {
                    statusBar.Background = new SolidColorBrush(Color.FromArgb(255, 230, 20, 20));
                    statusInfoTextBlock.Text = "电机初始化失败！";
                }
            }
            catch
            {
                MessageBox.Show("fuck");
                statusBar.Background = new SolidColorBrush(Color.FromArgb(255, 230, 20, 20));
                statusInfoTextBlock.Text = "窗口初始化失败！";
            }

            //输出电机参数文本的计时器
            ShowTimer = new System.Windows.Threading.DispatcherTimer();
            ShowTimer.Tick += new EventHandler(textTimer);
            ShowTimer.Interval = new TimeSpan(0, 0, 0, 0, 100);
            ShowTimer.Start();
        }
        public void ShowCurTimer(object sender, EventArgs e)//取当前时间的委托
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

        public void textTimer(object sender, EventArgs e)//输出电机参数到相应文本框的委托
        {
            //电机1(左膝)的文本框输出
            Motor1_position_textBox.Text = ampObj[0].PositionActual.ToString("F"); //电机实际位置; "F"格式，默认保留两位小数
            Motor1_phaseAngle_textBox.Text = ampObj[0].PhaseAngle.ToString(); //电机相位角; short是16位有符号整数类型
            Motor1_velocity_textBox.Text = ampObj[0].VelocityActual.ToString("F"); //电机实际速度
            Motor1_accel_textBox.Text = ampObj[0].TrajectoryAcc.ToString("F"); //由轨迹计算而得的加速度
            Motor1_decel_textBox.Text = ampObj[0].TrajectoryVel.ToString("F"); //由轨迹计算而得的速度

            //电机2(左髋)的文本框输出
            Motor2_position_textBox.Text = ampObj[1].PositionActual.ToString("F"); //电机实际位置; "F"格式，默认保留两位小数
            Motor2_phaseAngle_textBox.Text = ampObj[1].PhaseAngle.ToString(); //电机相位角; short是16位有符号整数类型
            Motor2_velocity_textBox.Text = ampObj[1].VelocityActual.ToString("F"); //电机实际速度
            Motor2_accel_textBox.Text = ampObj[1].TrajectoryAcc.ToString("F"); //由轨迹计算而得的加速度
            Motor2_decel_textBox.Text = ampObj[1].TrajectoryVel.ToString("F"); //由轨迹计算而得的速度

            //电机3(右髋)的文本框输出
            Motor3_position_textBox.Text = ampObj[2].PositionActual.ToString("F"); //电机实际位置; "F"格式，默认保留两位小数
            Motor3_phaseAngle_textBox.Text = ampObj[2].PhaseAngle.ToString(); //电机相位角; short是16位有符号整数类型
            Motor3_velocity_textBox.Text = ampObj[2].VelocityActual.ToString("F"); //电机实际速度
            Motor3_accel_textBox.Text = ampObj[2].TrajectoryAcc.ToString("F"); //由轨迹计算而得的加速度
            Motor3_decel_textBox.Text = ampObj[2].TrajectoryVel.ToString("F"); //由轨迹计算而得的速度

            //电机4(右膝)的文本框输出
            Motor4_position_textBox.Text = ampObj[3].PositionActual.ToString("F"); //电机实际位置; "F"格式，默认保留两位小数
            Motor4_phaseAngle_textBox.Text = ampObj[3].PhaseAngle.ToString(); //电机相位角; short是16位有符号整数类型
            Motor4_velocity_textBox.Text = ampObj[3].VelocityActual.ToString("F"); //电机实际速度
            Motor4_accel_textBox.Text = ampObj[3].TrajectoryAcc.ToString("F"); //由轨迹计算而得的加速度
            Motor4_decel_textBox.Text = ampObj[3].TrajectoryVel.ToString("F"); //由轨迹计算而得的速度
        }

    }
}
