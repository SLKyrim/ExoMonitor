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
using System.Diagnostics;

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

        private DispatcherTimer ShowCurTimeTimer; //显示当前时间的计时器
        private DispatcherTimer ShowTextTimer; //输出电机参数的计时器
        private DispatcherTimer WriteDataTimer; //写入数据委托的计时器
        private void FunctionPage_Loaded(object sender, RoutedEventArgs e)//打开窗口后进行的初始化操作
        {
            //显示当前时间的计时器
            ShowCurTimeTimer = new DispatcherTimer();
            ShowCurTimeTimer.Tick += new EventHandler(ShowCurTimer);
            ShowCurTimeTimer.Interval = new TimeSpan(0, 0, 0, 1, 0);
            ShowCurTimeTimer.Start();

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
                statusBar.Background = new SolidColorBrush(Color.FromArgb(255, 230, 20, 20));
                statusInfoTextBlock.Text = "窗口初始化失败！";
            }

            //输出电机参数文本的计时器
            ShowTextTimer = new DispatcherTimer();
            ShowTextTimer.Tick += new EventHandler(textTimer);
            ShowTextTimer.Interval = TimeSpan.FromMilliseconds(100);
            ShowTextTimer.Start();

        }

        private double[] trajectory = new double[700]; //步态数据轨迹

        #region 按钮

        private void startButton_Click(object sender, RoutedEventArgs e)//点击【开始】按钮时执行
        {
            endButton.IsEnabled = true;
            startButton.IsEnabled = false;

            string[] str = File.ReadAllLines("C:\\Users\\Administrator\\Desktop\\ExoGaitControl\\GaitData.txt", Encoding.Default);
            int lines = str.GetLength(0); //获取str第0维的元素数
            for (int i = 0; i < lines; i++)
            {
                trajectory[i] = (int)double.Parse(str[i]);
            }

            ampObj[0].PositionActual = 0; //此处先用第一个电机做测试

            WriteDataTimer = new DispatcherTimer();
            WriteDataTimer.Tick += new EventHandler(testTimer);
            WriteDataTimer.Interval = TimeSpan.FromMilliseconds(20);// 该时钟频率决定电机运行速度
            WriteDataTimer.Start();
        }

        int timeCountor = 0; //记录录入数据个数的计数器
        private void endButton_Click(object sender, RoutedEventArgs e)//点击【停止】按钮时执行
        {
            endButton.IsEnabled = false;
            startButton.IsEnabled = true;

            ampObj[0].HaltMove();
            timeCountor = 0;

            WriteDataTimer.Stop();
        }

        private DispatcherTimer AngleSetTimer; //电机按设置转角转动的计时器
        private void angleSetButton_Click(object sender, RoutedEventArgs e)
        {
            angleSetButton.IsEnabled = false;
            emergencyStopButton.IsEnabled = true;
            zeroPointSetButton.IsEnabled = true;
            int motorNumber = Convert.ToInt16(motorNumberTextBox.Text);
            int i = motorNumber - 1;

            ampObj[i].PositionActual = 0;

            AngleSetTimer = new DispatcherTimer();
            AngleSetTimer.Tick += new EventHandler(angleSetTimer);
            AngleSetTimer.Interval = TimeSpan.FromMilliseconds(20);// 该时钟频率决定电机运行速度
            AngleSetTimer.Start();
        }

        private void emergencyStopButton_Click(object sender, RoutedEventArgs e)
        {
            emergencyStopButton.IsEnabled = false;
            angleSetButton.IsEnabled = true;
            int motorNumber = Convert.ToInt16(motorNumberTextBox.Text);
            int i = motorNumber - 1;

            ampObj[i].HaltMove();
            AngleSetTimer.Stop();

        }

        private void zeroPointSetButton_Click(object sender, RoutedEventArgs e)
        {
            ampObj[0].PositionActual = 0;
            ampObj[1].PositionActual = 0;
            ampObj[2].PositionActual = 0;
            ampObj[3].PositionActual = 0;
        }

        private DispatcherTimer GetZeroPointTimer; //回归原点的计时器
        private void getZeroPointButton_Click(object sender, RoutedEventArgs e)
        {
            GetZeroPointTimer = new DispatcherTimer();
            GetZeroPointTimer.Tick += new EventHandler(getZeroPointTimer);
            GetZeroPointTimer.Interval = TimeSpan.FromMilliseconds(20);// 该时钟频率决定电机运行速度
            GetZeroPointTimer.Start();
        }
        #endregion

        private Stopwatch st = new Stopwatch();
        double error = 0;
        double e_i = 0; //积分误差
        double error_last = 0;
        double Kp = 0.1;
        double Ki = 0.0001;
        double Kd = 2;

        #region 计时器 

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

        private double[] ampObjAngleActual = new double[4];//电机的转角
        public void textTimer(object sender, EventArgs e)//输出电机参数到相应文本框的委托
        {
            ampObjAngleActual[0] = ampObj[0].PositionActual / 6400;
            ampObjAngleActual[1] = ampObj[1].PositionActual / 6400;
            ampObjAngleActual[2] = ampObj[2].PositionActual / 6400;
            ampObjAngleActual[3] = ampObj[3].PositionActual / 6400;
            //电机1(左膝)的文本框输出
            Motor1_position_textBox.Text = ampObjAngleActual[0].ToString("F"); //电机实际位置; "F"格式，默认保留两位小数
            Motor1_phaseAngle_textBox.Text = ampObj[0].PhaseAngle.ToString(); //电机相位角; short是16位有符号整数类型
            Motor1_velocity_textBox.Text = ampObj[0].VelocityActual.ToString("F"); //电机实际速度
            Motor1_accel_textBox.Text = ampObj[0].TrajectoryAcc.ToString("F"); //由轨迹计算而得的加速度
            Motor1_decel_textBox.Text = ampObj[0].TrajectoryVel.ToString("F"); //由轨迹计算而得的速度

            //电机2(左髋)的文本框输出
            Motor2_position_textBox.Text = ampObjAngleActual[1].ToString("F"); //电机实际位置; "F"格式，默认保留两位小数
            Motor2_phaseAngle_textBox.Text = ampObj[1].PhaseAngle.ToString(); //电机相位角; short是16位有符号整数类型
            Motor2_velocity_textBox.Text = ampObj[1].VelocityActual.ToString("F"); //电机实际速度
            Motor2_accel_textBox.Text = ampObj[1].TrajectoryAcc.ToString("F"); //由轨迹计算而得的加速度
            Motor2_decel_textBox.Text = ampObj[1].TrajectoryVel.ToString("F"); //由轨迹计算而得的速度

            //电机3(右髋)的文本框输出
            Motor3_position_textBox.Text = ampObjAngleActual[2].ToString("F"); //电机实际位置; "F"格式，默认保留两位小数
            Motor3_phaseAngle_textBox.Text = ampObj[2].PhaseAngle.ToString(); //电机相位角; short是16位有符号整数类型
            Motor3_velocity_textBox.Text = ampObj[2].VelocityActual.ToString("F"); //电机实际速度
            Motor3_accel_textBox.Text = ampObj[2].TrajectoryAcc.ToString("F"); //由轨迹计算而得的加速度
            Motor3_decel_textBox.Text = ampObj[2].TrajectoryVel.ToString("F"); //由轨迹计算而得的速度

            //电机4(右膝)的文本框输出
            Motor4_position_textBox.Text = ampObjAngleActual[3].ToString("F"); //电机实际位置; "F"格式，默认保留两位小数
            Motor4_phaseAngle_textBox.Text = ampObj[3].PhaseAngle.ToString(); //电机相位角; short是16位有符号整数类型
            Motor4_velocity_textBox.Text = ampObj[3].VelocityActual.ToString("F"); //电机实际速度
            Motor4_accel_textBox.Text = ampObj[3].TrajectoryAcc.ToString("F"); //由轨迹计算而得的加速度
            Motor4_decel_textBox.Text = ampObj[3].TrajectoryVel.ToString("F"); //由轨迹计算而得的速度
        }

        public void testTimer(object sender, EventArgs e)//测试功能的委托
        {
            StreamWriter toText = new StreamWriter("data.txt", true);//打开记录数据文本,可于

            st.Stop();
            long time_err = st.ElapsedMilliseconds;
            st.Restart();
            if (time_err < 1)
                time_err = 20;

            error = trajectory[timeCountor] - ampObj[0].PositionActual;

            e_i += error;

            profileSettingsObj.ProfileVel = Math.Abs(Kp * error + Ki * e_i + Kd * (error - error_last)) * 1000 / time_err;
            profileSettingsObj.ProfileAccel = Math.Abs((profileSettingsObj.ProfileVel - ampObj[0].VelocityActual) * 1);
            profileSettingsObj.ProfileDecel = profileSettingsObj.ProfileAccel;
            ampObj[0].ProfileSettings = profileSettingsObj;

            error_last = error;

            if (error > 0)
            {
                ampObj[0].MoveRel(1);
            }
            else if (error < 0)
            {
                ampObj[0].MoveRel(-1);
            }

            timeCountor++;

            if (timeCountor > 699)
            {
                ampObj[0].HaltMove();
            }

            if (timeCountor < 699)
            {
                toText.WriteLine(timeCountor.ToString() + '\t' +
                    ampObj[0].VelocityActual.ToString("F") + '\t' +
                    profileSettingsObj.ProfileVel.ToString("F") + '\t' +
                    time_err.ToString("F"));
            }

            toText.Close();

        }

        public void angleSetTimer(object sender, EventArgs e)//电机按设置角度转动的委托
        {
            double angleSet = Convert.ToDouble(angleSetTextBox.Text);
            int motorNumber = Convert.ToInt16(motorNumberTextBox.Text);
            int i = motorNumber - 1;

            ampObjAngleActual[i] = ampObj[i].PositionActual / 6400;


            if (angleSet > 0)
            {
                if (ampObjAngleActual[i] < angleSet)
                {
                    profileSettingsObj.ProfileVel = 50000;
                    profileSettingsObj.ProfileAccel = 50000;
                    profileSettingsObj.ProfileDecel = profileSettingsObj.ProfileAccel;
                    ampObj[i].ProfileSettings = profileSettingsObj;

                    if (ampObjAngleActual[i] > (angleSet - 5))
                    {
                        profileSettingsObj.ProfileVel = 25000;
                        profileSettingsObj.ProfileAccel = 25000;
                        profileSettingsObj.ProfileDecel = profileSettingsObj.ProfileAccel;
                        ampObj[i].ProfileSettings = profileSettingsObj;
                    }
                    ampObj[i].MoveRel(1);
                }
                else
                {
                    ampObj[i].HaltMove();
                    angleSetButton.IsEnabled = true;
                    AngleSetTimer.Stop();
                }
            }

            if (angleSet < 0)
            {
                if (ampObjAngleActual[i] > angleSet)
                {
                    profileSettingsObj.ProfileVel = 50000;
                    profileSettingsObj.ProfileAccel = 50000;
                    profileSettingsObj.ProfileDecel = profileSettingsObj.ProfileAccel;
                    ampObj[i].ProfileSettings = profileSettingsObj;

                    if (ampObjAngleActual[i] < (angleSet + 5))
                    {
                        profileSettingsObj.ProfileVel = 25000;
                        profileSettingsObj.ProfileAccel = 25000;
                        profileSettingsObj.ProfileDecel = profileSettingsObj.ProfileAccel;
                        ampObj[i].ProfileSettings = profileSettingsObj;
                    }
                    ampObj[i].MoveRel(-1);
                }
                else
                {
                    ampObj[i].HaltMove();
                    angleSetButton.IsEnabled = true;
                    AngleSetTimer.Stop();
                }
            }

        }

        public void getZeroPointTimer(object sender, EventArgs e)//回归原点的委托
        {
            ampObjAngleActual[0] = ampObj[0].PositionActual / 6400;
            ampObjAngleActual[1] = ampObj[1].PositionActual / 6400;
            ampObjAngleActual[2] = ampObj[2].PositionActual / 6400;
            ampObjAngleActual[3] = ampObj[3].PositionActual / 6400;

            if (Math.Abs(ampObjAngleActual[0]) > 3)//电机1回归原点
            {
                if (ampObjAngleActual[0] > 0)//此时电机1应往后转
                {
                    if (ampObjAngleActual[0] > 10)
                    {
                        profileSettingsObj.ProfileVel = 50000;
                        profileSettingsObj.ProfileAccel = 50000;
                        profileSettingsObj.ProfileDecel = profileSettingsObj.ProfileAccel;
                        ampObj[0].ProfileSettings = profileSettingsObj;
                    }
                    else
                    {
                        profileSettingsObj.ProfileVel = 25000;
                        profileSettingsObj.ProfileAccel = 25000;
                        profileSettingsObj.ProfileDecel = profileSettingsObj.ProfileAccel;
                        ampObj[0].ProfileSettings = profileSettingsObj;
                    }
                    ampObj[0].MoveRel(-1);     
                }
                else//此时电机1应往前转
                {
                    if (ampObjAngleActual[0] < -10)
                    {
                        profileSettingsObj.ProfileVel = 50000;
                        profileSettingsObj.ProfileAccel = 50000;
                        profileSettingsObj.ProfileDecel = profileSettingsObj.ProfileAccel;
                        ampObj[0].ProfileSettings = profileSettingsObj;
                    }
                    else
                    {
                        profileSettingsObj.ProfileVel = 25000;
                        profileSettingsObj.ProfileAccel = 25000;
                        profileSettingsObj.ProfileDecel = profileSettingsObj.ProfileAccel;
                        ampObj[0].ProfileSettings = profileSettingsObj;
                    }
                    ampObj[0].MoveRel(1);
                }
            }
            else
            {
                ampObj[0].HaltMove();
            }

            if (Math.Abs(ampObjAngleActual[1]) > 3)//电机2回归原点
            {
                if (ampObjAngleActual[1] > 0)//此时电机2应往后转
                {
                    if (ampObjAngleActual[1] > 10)
                    {
                        profileSettingsObj.ProfileVel = 50000;
                        profileSettingsObj.ProfileAccel = 50000;
                        profileSettingsObj.ProfileDecel = profileSettingsObj.ProfileAccel;
                        ampObj[1].ProfileSettings = profileSettingsObj;
                    }
                    else
                    {
                        profileSettingsObj.ProfileVel = 25000;
                        profileSettingsObj.ProfileAccel = 25000;
                        profileSettingsObj.ProfileDecel = profileSettingsObj.ProfileAccel;
                        ampObj[1].ProfileSettings = profileSettingsObj;
                    }
                    ampObj[1].MoveRel(-1);
                }
                else//此时电机2应往前转
                {
                    if (ampObjAngleActual[1] < -10)
                    {
                        profileSettingsObj.ProfileVel = 50000;
                        profileSettingsObj.ProfileAccel = 50000;
                        profileSettingsObj.ProfileDecel = profileSettingsObj.ProfileAccel;
                        ampObj[1].ProfileSettings = profileSettingsObj;
                    }
                    else
                    {
                        profileSettingsObj.ProfileVel = 25000;
                        profileSettingsObj.ProfileAccel = 25000;
                        profileSettingsObj.ProfileDecel = profileSettingsObj.ProfileAccel;
                        ampObj[1].ProfileSettings = profileSettingsObj;
                    }
                    ampObj[1].MoveRel(1);
                }
            }
            else
            {
                ampObj[1].HaltMove();
            }

            if (Math.Abs(ampObjAngleActual[2]) > 3)//电机3回归原点
            {
                if (ampObjAngleActual[2] > 0)//此时电机3应往前转
                {
                    if (ampObjAngleActual[2] > 10)
                    {
                        profileSettingsObj.ProfileVel = 50000;
                        profileSettingsObj.ProfileAccel = 50000;
                        profileSettingsObj.ProfileDecel = profileSettingsObj.ProfileAccel;
                        ampObj[2].ProfileSettings = profileSettingsObj;
                    }
                    else
                    {
                        profileSettingsObj.ProfileVel = 25000;
                        profileSettingsObj.ProfileAccel = 25000;
                        profileSettingsObj.ProfileDecel = profileSettingsObj.ProfileAccel;
                        ampObj[2].ProfileSettings = profileSettingsObj;
                    }
                    ampObj[2].MoveRel(-1);
                }
                else//此时电机3应往后转
                {
                    if (ampObjAngleActual[2] < -10)
                    {
                        profileSettingsObj.ProfileVel = 50000;
                        profileSettingsObj.ProfileAccel = 50000;
                        profileSettingsObj.ProfileDecel = profileSettingsObj.ProfileAccel;
                        ampObj[2].ProfileSettings = profileSettingsObj;
                    }
                    else
                    {
                        profileSettingsObj.ProfileVel = 25000;
                        profileSettingsObj.ProfileAccel = 25000;
                        profileSettingsObj.ProfileDecel = profileSettingsObj.ProfileAccel;
                        ampObj[2].ProfileSettings = profileSettingsObj;
                    }
                    ampObj[2].MoveRel(1);
                }
            }
            else
            {
                ampObj[2].HaltMove();
            }

            if (Math.Abs(ampObjAngleActual[3]) > 3)//电机4回归原点
            {
                if (ampObjAngleActual[3] > 0)//此时电机4应往前转
                {
                    if (ampObjAngleActual[3] > 10)
                    {
                        profileSettingsObj.ProfileVel = 50000;
                        profileSettingsObj.ProfileAccel = 50000;
                        profileSettingsObj.ProfileDecel = profileSettingsObj.ProfileAccel;
                        ampObj[3].ProfileSettings = profileSettingsObj;
                    }
                    else
                    {
                        profileSettingsObj.ProfileVel = 25000;
                        profileSettingsObj.ProfileAccel = 25000;
                        profileSettingsObj.ProfileDecel = profileSettingsObj.ProfileAccel;
                        ampObj[3].ProfileSettings = profileSettingsObj;
                    }
                    ampObj[3].MoveRel(-1);
                }
                else//此时电机4应往后转
                {
                    if (ampObjAngleActual[3] < -10)
                    {
                        profileSettingsObj.ProfileVel = 50000;
                        profileSettingsObj.ProfileAccel = 50000;
                        profileSettingsObj.ProfileDecel = profileSettingsObj.ProfileAccel;
                        ampObj[3].ProfileSettings = profileSettingsObj;
                    }
                    else
                    {
                        profileSettingsObj.ProfileVel = 25000;
                        profileSettingsObj.ProfileAccel = 25000;
                        profileSettingsObj.ProfileDecel = profileSettingsObj.ProfileAccel;
                        ampObj[3].ProfileSettings = profileSettingsObj;
                    }
                    ampObj[3].MoveRel(1);
                }
            }
            else
            {
                ampObj[3].HaltMove();
            }

            if (Math.Abs(ampObjAngleActual[0]) < 3 && Math.Abs(ampObjAngleActual[1]) < 3 && Math.Abs(ampObjAngleActual[2]) < 3 && Math.Abs(ampObjAngleActual[3]) < 3)
            {
                GetZeroPointTimer.Stop();
            }
        }

        #endregion
    }
}
