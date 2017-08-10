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
using Visifire.Charts;

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
                    //profileSettingsObj.ProfileAccel = ampObj[i].VelocityLoopSettings.VelLoopMaxAcc;
                    //profileSettingsObj.ProfileDecel = ampObj[i].VelocityLoopSettings.VelLoopMaxDec;
                    //profileSettingsObj.ProfileVel = ampObj[i].VelocityLoopSettings.VelLoopMaxVel;
                    profileSettingsObj.ProfileAccel = 0.0;
                    profileSettingsObj.ProfileDecel = 0.0;
                    profileSettingsObj.ProfileVel = 0.0;
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

        private double[] trajectory = new double[1100]; //步态采集数据轨迹
        private double[,] trajectories = new double[1001,4]; //输入四个关节的步态数据
        private double[,] tempPositionActual = new double[1001, 4]; //记录步态进行时电机的实际位置
        private int[] countor = new int[1001];
        private double[,] originalTrajectories = new double[1001, 4]; //保存初始输入数据

        #region 按钮

        private void getDataButton_Click(object sender, RoutedEventArgs e)//点击【采集数据】按钮时执行
        {
            endGetDataButton.IsEnabled = true;
            getDataButton.IsEnabled = false;

            string[] str = File.ReadAllLines("C:\\Users\\Administrator\\Desktop\\ExoGaitControl\\GaitData.txt", Encoding.Default);
            int lines = str.GetLength(0); //获取str第0维的元素数
            for (int i = 0; i < lines; i++)
            {
                trajectory[i] = (int) (double.Parse(str[i]) * 6400);
            }

            ampObj[0].PositionActual = 0; //此处先用第一个电机做测试
            ampObj[1].PositionActual = 0;
            ampObj[2].PositionActual = 0;
            ampObj[3].PositionActual = 0;

            WriteDataTimer = new DispatcherTimer();
            WriteDataTimer.Tick += new EventHandler(testTimer);
            WriteDataTimer.Interval = TimeSpan.FromMilliseconds(20);// 该时钟频率决定电机运行速度
            WriteDataTimer.Start();
        }

        int timeCountor = 0; //记录录入数据个数的计数器
        private void endGetDataButton_Click(object sender, RoutedEventArgs e)//点击【采集停止】按钮时执行
        {
            endGetDataButton.IsEnabled = false;
            getDataButton.IsEnabled = true;

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

        private DispatcherTimer GaitStartTimer; //开始步态的计时器
        private void startButton_Click(object sender, RoutedEventArgs e)//点击【步态开始】按钮时执行
        {
            startButton.IsEnabled = false;
            endButton.IsEnabled = true;

            string[] ral = File.ReadAllLines("C:\\Users\\Administrator\\Desktop\\ExoGaitControl\\GaitSequence.txt", Encoding.Default);
            int lines = ral.GetLength(0);
            for (int i = 0; i < lines; i++)
            {
                string[] str = (ral[i] ?? string.Empty).Split(new char[] { '\t' }, StringSplitOptions.RemoveEmptyEntries);
                for (int j = 0; j < 4; j++)
                {
                    trajectories[i, j] = (int)(double.Parse(str[j]) * 6400);
                    originalTrajectories[i, j] = (int)(double.Parse(str[j]) * 6400);
                }
            }

            timeCountor = 0;

            ampObj[0].PositionActual = 0;
            ampObj[1].PositionActual = 0;
            ampObj[2].PositionActual = 0;
            ampObj[3].PositionActual = 0;

            GaitStartTimer = new DispatcherTimer();
            GaitStartTimer.Tick += new EventHandler(gaitStartTimer);
            GaitStartTimer.Interval = TimeSpan.FromMilliseconds(20);// 该时钟频率决定电机运行速度
            GaitStartTimer.Start();
        }
  
        
        private void endButton_Click(object sender, RoutedEventArgs e)//点击【步态结束】按钮时执行
        {
            startButton.IsEnabled = true;
            endButton.IsEnabled = false;

            statusBar.Background = new SolidColorBrush(Color.FromArgb(255, 0, 122, 204));
            statusInfoTextBlock.Text = "执行完毕";

            ampObj[0].HaltMove();
            ampObj[1].HaltMove();
            ampObj[2].HaltMove();
            ampObj[3].HaltMove();

            GaitStartTimer.Stop();

            for (int i = 0; i < 1001; i++)
            {
                countor[i] = i;

                for (int j = 0; j < 4; j++)
                {
                    trajectories[i, j] = trajectories[i, j] / 6400;
                    tempPositionActual[i, j] = tempPositionActual[i, j] / 6400;
                    originalTrajectories[i, j] = originalTrajectories[i, j] / 6400;
                    
                }
            }

            Motor1.Children.Clear();
            Motor2.Children.Clear();
            Motor3.Children.Clear();
            Motor4.Children.Clear();

            plotChart(originalTrajectories, tempPositionActual, countor);

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
            Motor1_phaseAngle_textBox.Text = (ampObj[0].CurrentActual * 0.01).ToString("F"); //电机电流
            Motor1_velocity_textBox.Text = ampObj[0].VelocityActual.ToString("F"); //电机实际速度
            Motor1_accel_textBox.Text = ampObj[0].TrajectoryAcc.ToString("F"); //由轨迹计算而得的加速度
            Motor1_decel_textBox.Text = ampObj[0].TrajectoryVel.ToString("F"); //由轨迹计算而得的速度

            //电机2(左髋)的文本框输出
            Motor2_position_textBox.Text = ampObjAngleActual[1].ToString("F"); //电机实际位置; "F"格式，默认保留两位小数
            Motor2_phaseAngle_textBox.Text = (ampObj[1].CurrentActual * 0.01).ToString("F"); 
            Motor2_velocity_textBox.Text = ampObj[1].VelocityActual.ToString("F"); //电机实际速度
            Motor2_accel_textBox.Text = ampObj[1].TrajectoryAcc.ToString("F"); //由轨迹计算而得的加速度
            Motor2_decel_textBox.Text = ampObj[1].TrajectoryVel.ToString("F"); //由轨迹计算而得的速度

            //电机3(右髋)的文本框输出
            Motor3_position_textBox.Text = ampObjAngleActual[2].ToString("F"); //电机实际位置; "F"格式，默认保留两位小数
            Motor3_phaseAngle_textBox.Text = (ampObj[2].CurrentActual * 0.01).ToString("F"); 
            Motor3_velocity_textBox.Text = ampObj[2].VelocityActual.ToString("F"); //电机实际速度
            Motor3_accel_textBox.Text = ampObj[2].TrajectoryAcc.ToString("F"); //由轨迹计算而得的加速度
            Motor3_decel_textBox.Text = ampObj[2].TrajectoryVel.ToString("F"); //由轨迹计算而得的速度

            //电机4(右膝)的文本框输出
            Motor4_position_textBox.Text = ampObjAngleActual[3].ToString("F"); //电机实际位置; "F"格式，默认保留两位小数
            Motor4_phaseAngle_textBox.Text = (ampObj[3].CurrentActual * 0.01).ToString("F");
            Motor4_velocity_textBox.Text = ampObj[3].VelocityActual.ToString("F"); //电机实际速度
            Motor4_accel_textBox.Text = ampObj[3].TrajectoryAcc.ToString("F"); //由轨迹计算而得的加速度
            Motor4_decel_textBox.Text = ampObj[3].TrajectoryVel.ToString("F"); //由轨迹计算而得的速度
        }

        public void testTimer(object sender, EventArgs e)//测试功能的委托
        {

            statusBar.Background = new SolidColorBrush(Color.FromArgb(255, 230, 20, 20));
            statusInfoTextBlock.Text = "正在执行";

            StreamWriter toText = new StreamWriter("data.txt", true);//打开记录数据文本,可于

            st.Stop();
            long time_err = st.ElapsedMilliseconds;
            st.Restart();
            if (time_err < 1)
                time_err = 20;

            error = trajectory[timeCountor] - ampObj[3].PositionActual * -1;

            e_i += error;

            profileSettingsObj.ProfileVel = Math.Abs(0.2 * Kp * error + Ki * e_i + Kd * (error - error_last)) * 500 / time_err;
            profileSettingsObj.ProfileAccel = Math.Abs((profileSettingsObj.ProfileVel - ampObj[3].VelocityActual) * 1);
            profileSettingsObj.ProfileDecel = profileSettingsObj.ProfileAccel;
            ampObj[3].ProfileSettings = profileSettingsObj;

            error_last = error;

            if (error > 0)
            {
                ampObj[3].MoveRel(-1);
            }
            else if (error < 0)
            {
                ampObj[3].MoveRel(1);
            }

            timeCountor++;

            if (timeCountor > 411)
            {
                ampObj[3].HaltMove();
                statusBar.Background = new SolidColorBrush(Color.FromArgb(255, 0, 122, 204));
                statusInfoTextBlock.Text = "执行完毕";
            }

            if (timeCountor < 411)
            {
                toText.WriteLine(timeCountor.ToString() + '\t' +
                    ampObj[3].PositionActual.ToString());
            }

            toText.Close();

        }

        public void angleSetTimer(object sender, EventArgs e)//电机按设置角度转动的委托
        {
            statusBar.Background = new SolidColorBrush(Color.FromArgb(255, 230, 20, 20));
            statusInfoTextBlock.Text = "正在执行";

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
                    statusBar.Background = new SolidColorBrush(Color.FromArgb(255, 0, 122, 204));
                    statusInfoTextBlock.Text = "执行完毕";
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
                    statusBar.Background = new SolidColorBrush(Color.FromArgb(255, 0, 122, 204));
                    statusInfoTextBlock.Text = "执行完毕";
                }
            }

        }

        public void getZeroPointTimer(object sender, EventArgs e)//回归原点的委托
        {
            statusBar.Background = new SolidColorBrush(Color.FromArgb(255, 230, 20, 20));
            statusInfoTextBlock.Text = "正在回归原点";

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
                statusBar.Background = new SolidColorBrush(Color.FromArgb(255, 0, 122, 204));
                statusInfoTextBlock.Text = "回归原点完毕";
                GetZeroPointTimer.Stop();
            }
        }


        double error1 = 0;
        double error2 = 0;
        double error3 = 0;
        double error4 = 0;
        double error_last1 = 0;
        double error_last2 = 0;
        double error_last3 = 0;
        double error_last4 = 0;
        double e_i1 = 0;
        double e_i2 = 0;
        double e_i3 = 0;
        double e_i4 = 0;
        public void gaitStartTimer(object sender, EventArgs e)//开始步态的委托
        {
            statusBar.Background = new SolidColorBrush(Color.FromArgb(255, 230, 20, 20));
            statusInfoTextBlock.Text = "正在执行";

            StreamWriter toText = new StreamWriter("data.txt", true);//打开记录数据文本,可于



            st.Stop();
            long time_err = st.ElapsedMilliseconds;
            st.Restart();
            if (time_err < 1) time_err = 20;

            trajectories[timeCountor, 3] = trajectories[timeCountor, 3] * 0.6;//左膝
            trajectories[timeCountor, 1] = trajectories[timeCountor, 1] * 0.8;//右膝

            #region 电机1左膝
            //Read error between Target and Actual values. 
            error1 = trajectories[timeCountor, 3] - ampObj[0].PositionActual * -1;
            //Intergral error
            e_i1 += error1;

            profileSettingsObj.ProfileVel = Math.Abs(3.5 * Kp * error1 + Ki / 10 * e_i1 + Kd * (error1 - error_last1)) * 1000 / time_err;
            profileSettingsObj.ProfileAccel = Math.Abs((profileSettingsObj.ProfileVel - ampObj[0].VelocityActual) * 1);
            profileSettingsObj.ProfileDecel = profileSettingsObj.ProfileAccel;
            ampObj[0].ProfileSettings = profileSettingsObj;

            error_last1 = error1;

            if (error1 > 0)
            {
                ampObj[0].MoveRel(-1);
            }
            else if (error1 < 0)
            {
                ampObj[0].MoveRel(1);
            }
            #endregion

            #region 电机2左髋
            //Read error between Target and Actual values.
            error2 = trajectories[timeCountor, 2] - ampObj[1].PositionActual * -1;
            //Intergral error
            e_i2 += error2;

            profileSettingsObj.ProfileVel = Math.Abs(Kp * error2 + Ki * e_i2 + Kd * (error2 - error_last2)) * 1000 / time_err;
            profileSettingsObj.ProfileAccel = Math.Abs((profileSettingsObj.ProfileVel - ampObj[1].VelocityActual) * 1);
            profileSettingsObj.ProfileDecel = profileSettingsObj.ProfileAccel;
            ampObj[1].ProfileSettings = profileSettingsObj;

            error_last2 = error2;

            if (error2 > 0)
            {
                ampObj[1].MoveRel(-1);
            }
            else if (error2 < 0)
            {
                ampObj[1].MoveRel(1);
            }
            #endregion

            #region 电机3右髋
            //Read error between Target and Actual values.
            error3 = trajectories[timeCountor, 0] - ampObj[2].PositionActual * -1;
            //Intergral error
            e_i3 += error3;

            profileSettingsObj.ProfileVel = Math.Abs(Kp * error3 + Ki * e_i3 + Kd * (error3 - error_last3)) * 1000 / time_err;
            profileSettingsObj.ProfileAccel = Math.Abs((profileSettingsObj.ProfileVel - ampObj[2].VelocityActual) * 1);
            profileSettingsObj.ProfileDecel = profileSettingsObj.ProfileAccel;
            ampObj[2].ProfileSettings = profileSettingsObj;

            error_last3 = error3;

            if (error3 > 0)
            {
                ampObj[2].MoveRel(-1);
            }
            else if (error3 < 0)
            {
                ampObj[2].MoveRel(1);
            }
            #endregion

            #region 电机4右膝
            //Read error between Target and Actual values.
            error4 = trajectories[timeCountor, 1] - ampObj[3].PositionActual * -1;
            //Intergral error
            e_i4 += error4;

            profileSettingsObj.ProfileVel = Math.Abs(3.5 * Kp * error4 + Ki / 10 * e_i4 + Kd * (error4 - error_last4)) * 600 / time_err;
            profileSettingsObj.ProfileAccel = Math.Abs((profileSettingsObj.ProfileVel - ampObj[3].VelocityActual) * 0.5);
            profileSettingsObj.ProfileDecel = profileSettingsObj.ProfileAccel;
            ampObj[3].ProfileSettings = profileSettingsObj;

            error_last4 = error4;

            if (error4 > 0)
            {
                ampObj[3].MoveRel(-1);
            }
            else if (error4 < 0)
            {
                ampObj[3].MoveRel(1);
            }
            //chart1.Series[0].Points.AddXY(CurrentTime, trajectory[CurrentTime, 0]);
            //chart1.Series[1].Points.AddXY(CurrentTime, ampObj[0].PositionActual);
            //chart1.Series[2].Points.AddXY(CurrentTime, ampObj[0].VelocityActual);
            //chart2.Series[1].Points.AddXY(CurrentTime, ampObj[3].CurrentActual);
            #endregion

            timeCountor++;

            for(int i = 0; i < 4; i++)
            {
                tempPositionActual[timeCountor, i] = ampObj[i].PositionActual;
            }
            

            if (timeCountor > 499)
            {
                statusBar.Background = new SolidColorBrush(Color.FromArgb(255, 0, 122, 204));
                statusInfoTextBlock.Text = "执行完毕";

                ampObj[0].HaltMove();
                ampObj[1].HaltMove();
                ampObj[2].HaltMove();
                ampObj[3].HaltMove();
                GaitStartTimer.Stop();
            }

            if (timeCountor < 500)
            {
                toText.WriteLine(timeCountor.ToString() + '\t' +
                    ampObj[0].PositionActual.ToString() + '\t' +
                    ampObj[1].PositionActual.ToString() + '\t' +
                    ampObj[2].PositionActual.ToString());
            }

            toText.Close();
        }

        #endregion

        public void plotChart(double[,] trajectories, double[,] positionActual, int[] countor)//绘制轨迹的方法
        {
            #region 电机1左膝
            //创建一个图标
            Chart chart_motor1 = new Chart();

            //设置图标的宽度和高度
            chart_motor1.Width = 500;
            chart_motor1.Height = 180;
            chart_motor1.Margin = new Thickness(5, 5, 5, 5);
            //是否启用打印和保持图片
            chart_motor1.ToolBarEnabled = false;

            //设置图标的属性
            chart_motor1.ScrollingEnabled = false;//是否启用或禁用滚动
            chart_motor1.View3D = false;//3D效果显示

            Axis yAxis_motor1 = new Axis();
            //设置图标中Y轴的最小值永远为-90           
            yAxis_motor1.AxisMinimum = -15;
            //设置图标中Y轴的最大值永远为90           
            yAxis_motor1.AxisMaximum = 80;
            //设置图表中Y轴的后缀          
            yAxis_motor1.Suffix = "°";
            chart_motor1.AxesY.Add(yAxis_motor1);

            // 创建一个新的数据线。               
            DataSeries dataSeries_input_motor1 = new DataSeries();
            // 设置数据线的格式。               
            dataSeries_input_motor1.LegendText = "输入步态";

            dataSeries_input_motor1.RenderAs = RenderAs.Spline;//折线图

            dataSeries_input_motor1.XValueType = ChartValueTypes.Numeric;
            // 设置输入轨迹数据点              
            DataPoint dataPoint_input_motor1;
            for (int i = 0; i < 500; i++)
            {
                // 创建一个数据点的实例。                   
                dataPoint_input_motor1 = new DataPoint();
                // 设置X轴点                    
                dataPoint_input_motor1.XValue = countor[i];
                //设置Y轴点                   
                dataPoint_input_motor1.YValue = trajectories[i, 3];
                dataPoint_input_motor1.MarkerSize = 1;
                //dataPoint.Tag = tableName.Split('(')[0];
                //设置数据点颜色                  
                // dataPoint.Color = new SolidColorBrush(Colors.LightGray);                   
                dataPoint_input_motor1.MouseLeftButtonDown += new MouseButtonEventHandler(dataPoint_MouseLeftButtonDown);
                //添加数据点                   
                dataSeries_input_motor1.DataPoints.Add(dataPoint_input_motor1);
            }

            // 添加数据线到数据序列。                
            chart_motor1.Series.Add(dataSeries_input_motor1);


            // 创建一个新的数据线。               
            DataSeries dataSeries_actual_motor1 = new DataSeries();
            // 设置数据线的格式。         

            dataSeries_actual_motor1.LegendText = "实际步态";

            dataSeries_actual_motor1.RenderAs = RenderAs.Spline;//折线图

            dataSeries_actual_motor1.XValueType = ChartValueTypes.Numeric;
            // 设置数据点              

            DataPoint dataPoint_actual_motor1;
            for (int i = 0; i < 500; i++)
            {
                // 创建一个数据点的实例。                   
                dataPoint_actual_motor1 = new DataPoint();
                // 设置X轴点                    
                dataPoint_actual_motor1.XValue = countor[i];
                //设置Y轴点                   
                dataPoint_actual_motor1.YValue = positionActual[i, 0] * -1;
                dataPoint_actual_motor1.MarkerSize = 1;
                //dataPoint2.Tag = tableName.Split('(')[0];
                //设置数据点颜色                  
                // dataPoint.Color = new SolidColorBrush(Colors.LightGray);                   
                dataPoint_actual_motor1.MouseLeftButtonDown += new MouseButtonEventHandler(dataPoint_MouseLeftButtonDown);
                //添加数据点                   
                dataSeries_actual_motor1.DataPoints.Add(dataPoint_actual_motor1);
            }
            // 添加数据线到数据序列。                
            chart_motor1.Series.Add(dataSeries_actual_motor1);

            //将生产的图表增加到Grid，然后通过Grid添加到上层Grid.           
            Grid gr1 = new Grid();
            gr1.Children.Add(chart_motor1);
            Motor1.Children.Add(gr1);
            #endregion

            #region 电机2左髋
            //创建一个图标
            Chart chart_motor2 = new Chart();

            //设置图标的宽度和高度
            chart_motor2.Width = 500;
            chart_motor2.Height = 180;
            chart_motor2.Margin = new Thickness(5, 5, 5, 5);
            //是否启用打印和保持图片
            chart_motor2.ToolBarEnabled = false;

            //设置图标的属性
            chart_motor2.ScrollingEnabled = false;//是否启用或禁用滚动
            chart_motor2.View3D = false;//3D效果显示

            Axis yAxis_motor2 = new Axis();
            //设置图标中Y轴的最小值永远为-90           
            yAxis_motor2.AxisMinimum = -15;
            //设置图标中Y轴的最大值永远为90           
            yAxis_motor2.AxisMaximum = 80;
            //设置图表中Y轴的后缀          
            yAxis_motor2.Suffix = "°";
            chart_motor2.AxesY.Add(yAxis_motor2);

            // 创建一个新的数据线。               
            DataSeries dataSeries_input_motor2 = new DataSeries();
            // 设置数据线的格式。               
            dataSeries_input_motor2.LegendText = "输入步态";

            dataSeries_input_motor2.RenderAs = RenderAs.Spline;//折线图

            dataSeries_input_motor2.XValueType = ChartValueTypes.Numeric;
            // 设置输入轨迹数据点              
            DataPoint dataPoint_input_motor2;
            for (int i = 0; i < 500; i++)
            {
                // 创建一个数据点的实例。                   
                dataPoint_input_motor2 = new DataPoint();
                // 设置X轴点                    
                dataPoint_input_motor2.XValue = countor[i];
                //设置Y轴点                   
                dataPoint_input_motor2.YValue = trajectories[i, 2] * -1;
                dataPoint_input_motor2.MarkerSize = 1;
                //dataPoint.Tag = tableName.Split('(')[0];
                //设置数据点颜色                  
                // dataPoint.Color = new SolidColorBrush(Colors.LightGray);                   
                dataPoint_input_motor2.MouseLeftButtonDown += new MouseButtonEventHandler(dataPoint_MouseLeftButtonDown);
                //添加数据点                   
                dataSeries_input_motor2.DataPoints.Add(dataPoint_input_motor2);
            }

            // 添加数据线到数据序列。                
            chart_motor2.Series.Add(dataSeries_input_motor2);


            // 创建一个新的数据线。               
            DataSeries dataSeries_actual_motor2 = new DataSeries();
            // 设置数据线的格式。         

            dataSeries_actual_motor2.LegendText = "实际步态";

            dataSeries_actual_motor2.RenderAs = RenderAs.Spline;//折线图

            dataSeries_actual_motor2.XValueType = ChartValueTypes.Numeric;
            // 设置数据点              

            DataPoint dataPoint_actual_motor2;
            for (int i = 0; i < 500; i++)
            {
                // 创建一个数据点的实例。                   
                dataPoint_actual_motor2 = new DataPoint();
                // 设置X轴点                    
                dataPoint_actual_motor2.XValue = countor[i];
                //设置Y轴点                   
                dataPoint_actual_motor2.YValue = positionActual[i, 1];
                dataPoint_actual_motor2.MarkerSize = 1;
                //dataPoint2.Tag = tableName.Split('(')[0];
                //设置数据点颜色                  
                // dataPoint.Color = new SolidColorBrush(Colors.LightGray);                   
                dataPoint_actual_motor2.MouseLeftButtonDown += new MouseButtonEventHandler(dataPoint_MouseLeftButtonDown);
                //添加数据点                   
                dataSeries_actual_motor2.DataPoints.Add(dataPoint_actual_motor2);
            }
            // 添加数据线到数据序列。                
            chart_motor2.Series.Add(dataSeries_actual_motor2);

            //将生产的图表增加到Grid，然后通过Grid添加到上层Grid.           
            Grid gr2 = new Grid();
            gr2.Children.Add(chart_motor2);
            Motor2.Children.Add(gr2);
            #endregion

            #region 电机3右髋
            //创建一个图标
            Chart chart_motor3 = new Chart();

            //设置图标的宽度和高度
            chart_motor3.Width = 500;
            chart_motor3.Height = 180;
            chart_motor3.Margin = new Thickness(5, 5, 5, 5);
            //是否启用打印和保持图片
            chart_motor3.ToolBarEnabled = false;

            //设置图标的属性
            chart_motor3.ScrollingEnabled = false;//是否启用或禁用滚动
            chart_motor3.View3D = false;//3D效果显示

            Axis yAxis_motor3 = new Axis();
            //设置图标中Y轴的最小值永远为-90           
            yAxis_motor3.AxisMinimum = -15;
            //设置图标中Y轴的最大值永远为90           
            yAxis_motor3.AxisMaximum = 80;
            //设置图表中Y轴的后缀          
            yAxis_motor3.Suffix = "°";
            chart_motor3.AxesY.Add(yAxis_motor3);

            // 创建一个新的数据线。               
            DataSeries dataSeries_input_motor3 = new DataSeries();
            // 设置数据线的格式。               
            dataSeries_input_motor3.LegendText = "输入步态";

            dataSeries_input_motor3.RenderAs = RenderAs.Spline;//折线图

            dataSeries_input_motor3.XValueType = ChartValueTypes.Numeric;
            // 设置输入轨迹数据点              
            DataPoint dataPoint_input_motor3;
            for (int i = 0; i < 500; i++)
            {
                // 创建一个数据点的实例。                   
                dataPoint_input_motor3 = new DataPoint();
                // 设置X轴点                    
                dataPoint_input_motor3.XValue = countor[i];
                //设置Y轴点                   
                dataPoint_input_motor3.YValue = trajectories[i, 0];
                dataPoint_input_motor3.MarkerSize = 1;
                //dataPoint.Tag = tableName.Split('(')[0];
                //设置数据点颜色                  
                // dataPoint.Color = new SolidColorBrush(Colors.LightGray);                   
                dataPoint_input_motor3.MouseLeftButtonDown += new MouseButtonEventHandler(dataPoint_MouseLeftButtonDown);
                //添加数据点                   
                dataSeries_input_motor3.DataPoints.Add(dataPoint_input_motor3);
            }

            // 添加数据线到数据序列。                
            chart_motor3.Series.Add(dataSeries_input_motor3);


            // 创建一个新的数据线。               
            DataSeries dataSeries_actual_motor3 = new DataSeries();
            // 设置数据线的格式。         

            dataSeries_actual_motor3.LegendText = "实际步态";

            dataSeries_actual_motor3.RenderAs = RenderAs.Spline;//折线图

            dataSeries_actual_motor3.XValueType = ChartValueTypes.Numeric;
            // 设置数据点              

            DataPoint dataPoint_actual_motor3;
            for (int i = 0; i < 500; i++)
            {
                // 创建一个数据点的实例。                   
                dataPoint_actual_motor3 = new DataPoint();
                // 设置X轴点                    
                dataPoint_actual_motor3.XValue = countor[i];
                //设置Y轴点                   
                dataPoint_actual_motor3.YValue = positionActual[i, 2] * -1;
                dataPoint_actual_motor3.MarkerSize = 1;
                //dataPoint2.Tag = tableName.Split('(')[0];
                //设置数据点颜色                  
                // dataPoint.Color = new SolidColorBrush(Colors.LightGray);                   
                dataPoint_actual_motor3.MouseLeftButtonDown += new MouseButtonEventHandler(dataPoint_MouseLeftButtonDown);
                //添加数据点                   
                dataSeries_actual_motor3.DataPoints.Add(dataPoint_actual_motor3);
            }
            // 添加数据线到数据序列。                
            chart_motor3.Series.Add(dataSeries_actual_motor3);

            //将生产的图表增加到Grid，然后通过Grid添加到上层Grid.           
            Grid gr3 = new Grid();
            gr3.Children.Add(chart_motor3);
            Motor3.Children.Add(gr3);
            #endregion

            #region 电机4右膝
            //创建一个图标
            Chart chart_motor4 = new Chart();

            //设置图标的宽度和高度
            chart_motor4.Width = 500;
            chart_motor4.Height = 180;
            chart_motor4.Margin = new Thickness(5, 5, 5, 5);
            //是否启用打印和保持图片
            chart_motor4.ToolBarEnabled = false;

            //设置图标的属性
            chart_motor4.ScrollingEnabled = false;//是否启用或禁用滚动
            chart_motor4.View3D = false;//3D效果显示

            Axis yAxis_motor4 = new Axis();
            //设置图标中Y轴的最小值永远为-90           
            yAxis_motor4.AxisMinimum = -15;
            //设置图标中Y轴的最大值永远为90           
            yAxis_motor4.AxisMaximum = 80;
            //设置图表中Y轴的后缀          
            yAxis_motor4.Suffix = "°";
            chart_motor4.AxesY.Add(yAxis_motor4);

            // 创建一个新的数据线。               
            DataSeries dataSeries_input_motor4 = new DataSeries();
            // 设置数据线的格式。               
            dataSeries_input_motor4.LegendText = "输入步态";

            dataSeries_input_motor4.RenderAs = RenderAs.Spline;//折线图

            dataSeries_input_motor4.XValueType = ChartValueTypes.Numeric;
            // 设置输入轨迹数据点              
            DataPoint dataPoint_input_motor4;
            for (int i = 0; i < 500; i++)
            {
                // 创建一个数据点的实例。                   
                dataPoint_input_motor4 = new DataPoint();
                // 设置X轴点                    
                dataPoint_input_motor4.XValue = countor[i];
                //设置Y轴点                   
                dataPoint_input_motor4.YValue = trajectories[i, 1] * -1;
                dataPoint_input_motor4.MarkerSize = 1;
                //dataPoint.Tag = tableName.Split('(')[0];
                //设置数据点颜色                  
                // dataPoint.Color = new SolidColorBrush(Colors.LightGray);                   
                dataPoint_input_motor4.MouseLeftButtonDown += new MouseButtonEventHandler(dataPoint_MouseLeftButtonDown);
                //添加数据点                   
                dataSeries_input_motor4.DataPoints.Add(dataPoint_input_motor4);
            }

            // 添加数据线到数据序列。                
            chart_motor4.Series.Add(dataSeries_input_motor4);


            // 创建一个新的数据线。               
            DataSeries dataSeries_actual_motor4 = new DataSeries();
            // 设置数据线的格式。         

            dataSeries_actual_motor4.LegendText = "实际步态";

            dataSeries_actual_motor4.RenderAs = RenderAs.Spline;//折线图

            dataSeries_actual_motor4.XValueType = ChartValueTypes.Numeric;
            // 设置数据点              

            DataPoint dataPoint_actual_motor4;
            for (int i = 0; i < 500; i++)
            {
                // 创建一个数据点的实例。                   
                dataPoint_actual_motor4 = new DataPoint();
                // 设置X轴点                    
                dataPoint_actual_motor4.XValue = countor[i];
                //设置Y轴点                   
                dataPoint_actual_motor4.YValue = positionActual[i, 3];
                dataPoint_actual_motor4.MarkerSize = 1;
                //dataPoint2.Tag = tableName.Split('(')[0];
                //设置数据点颜色                  
                // dataPoint.Color = new SolidColorBrush(Colors.LightGray);                   
                dataPoint_actual_motor4.MouseLeftButtonDown += new MouseButtonEventHandler(dataPoint_MouseLeftButtonDown);
                //添加数据点                   
                dataSeries_actual_motor4.DataPoints.Add(dataPoint_actual_motor4);
            }
            // 添加数据线到数据序列。                
            chart_motor4.Series.Add(dataSeries_actual_motor4);

            //将生产的图表增加到Grid，然后通过Grid添加到上层Grid.           
            Grid gr4 = new Grid();
            gr4.Children.Add(chart_motor4);
            Motor4.Children.Add(gr4);
            #endregion
        }

        void dataPoint_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)// 显示数据点值的委托
        {
            DataPoint dp = sender as DataPoint;
            MessageBox.Show(dp.YValue.ToString());
        }
    }
}
