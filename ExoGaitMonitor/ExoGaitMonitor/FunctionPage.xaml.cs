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

        #region 参数定义

        private AmpObj[] ampObj; //声明驱动器
        private ProfileSettingsObj profileSettingsObj; //声明驱动器属性
        private canOpenObj canObj; //声明网络接口

        private DispatcherTimer ShowCurTimeTimer; //显示当前时间的计时器
        private DispatcherTimer ShowTextTimer; //输出电机参数的计时器
        private DispatcherTimer AngleSetTimer; //电机按设置转角转动的计时器
        private DispatcherTimer GetZeroPointTimer; //回归原点的计时器
        private DispatcherTimer TempTimer; //写电机实际位置数据的计时器
        private DispatcherTimer SensorTimer; //传感器读写计时器
        private DispatcherTimer ForceTimer; //力学控制模式的计时器
        private DispatcherTimer SACTimer; //SAC模式的计时器

        private double[] trajectory = new double[1100]; //步态采集数据轨迹
        private double[,] trajectories = new double[1001, 4]; //输入四个关节的步态数据
        private double[,] tempPositionActual = new double[1001, 4]; //记录步态进行时电机的实际位置
        private int[] countor = new int[1001];
        private double[,] originalTrajectories = new double[1001, 4]; //保存初始输入数据

        //输出文本委托的参数
        private double[] ampObjAngleActual = new double[NUM_MOTOR];//电机的转角，单位：°
        private double[] ampObjAngleVelActual = new double[NUM_MOTOR];//电机的角速度，单位：rad/s
        private double[] ampObjAngleAccActual = new double[NUM_MOTOR];//电机的角加速度，单位：rad/s^2

        //PVT
        const int ARRAY_COL = 4; //轨迹数据数组列数的大小

        public LinkageObj Linkage; //连接一组电机，能够按输入序列同时操作
        public eventObj PVT_EventObj = new eventObj(); //PVT事件对象
        private AmpObj[] ampObjs = new AmpObj[ARRAY_COL]; //一组电机
        private double[] userUnits = new double[ARRAY_COL]; // 用户定义单位：编码器每圈计数
        private int timeCountor = 0; //计数器，写数据的计时器用到
        Methods methods = new Methods();

        const int RATIO = 160; //减速比
        const double SAFE = -1; //PVT原始数据缩减比例
        private int MAXRICH = 0; //记录最终扩充数据的大小，用来标记写实时位置计时器的结束

        //力学模式
        public string[] SPCount = null;           //用来存储计算机串口名称数组
        public int comcount = 0;                  //用来存储计算机可用串口数目，初始化为0
        public bool flag = false;
        public string sensor1_com = null;         //传感器1所用串口
        bool forceflag = false;
        const double FORCE_THRES = 0.4; //力學模式拉壓力傳感器閾值 
        const double FORCE_VEL = 100000; //设置力学模式下速度

        //SAC
        const double THIGH_LENGTH = 0.384; //外骨骼大腿长，单位：m
        const double THIGH_WEIGHT = 0.381; //外骨骼大腿重，单位：kg
        const double SHANK_LENGTH = 0.450; //外骨骼小腿长，单位：m
        const double SHANK_WEIGHT = 1.323; //外骨骼小腿重，单位：kg
        const double THIGH_MOMENT = 0.26; //外骨骼大腿力矩長，單位：m
        const double SHANK_MOMENT = 0.24; //外骨骼小腿力矩長，單位：m
        const double MOTOR_WEIGHT = 0.6; //电机质量，单位：kg
        const double MOTOR_RADIUS = 0.045; //电机半径，单位：m 
        const double MOTOR_INERTIA = 1.0 / 2.0 * MOTOR_WEIGHT * MOTOR_RADIUS * MOTOR_RADIUS; //电机绕中心轴转动惯量，单位：kg·m^2
        const double ALPHA = 10; //灵敏度放大因子
        const int NUM_MOTOR = 4; //电机个数
        const double G = 9.8; //重力加速度
        const double BATVOL = 26.9; //电池电压
        const double ETA = 0.8; //减速器使用系数
        const double INTERVAL = 20; //SAC执行频率，单位：ms
        const double SHANK_VEL_FAC = 0.1; //小腿速度可執行係數
        const double SHANK_ACC_FAC = 0.1; //小腿加速度可執行係數
        const double THIGH_VEL_FAC = 0.1; //大腿速度可執行係數
        const double THIGH_ACC_FAC = 0.1; //大腿加速度可執行係數
        const double SAC_THRES = 0.8; //SAC模式拉壓力傳感器閾值

        double[] radian = new double[NUM_MOTOR]; //角度矩阵，单位：rad
        double[] ang_vel = new double[NUM_MOTOR]; //角速度矩阵，单位：rad/s
        double[] ang_acc = new double[NUM_MOTOR]; //角加速度矩阵，单位：rad/s^2
        double[] inertia = new double[NUM_MOTOR]; //惯性矩阵
        double[] coriolis = new double[NUM_MOTOR]; //科里奥利矩阵
        double[] gravity = new double[NUM_MOTOR]; //重力矩阵
        double[] torque = new double[NUM_MOTOR]; //减速器扭矩
        double[] tempVel = new double[NUM_MOTOR]; //記錄上次的減速器角速度，单位：rad/s
        #endregion

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

                ampObj = new AmpObj[ARRAY_COL]; //实例化四个驱动器（盘式电机）
                Linkage = new LinkageObj();//实例化驱动器联动器

                for (int i = 0; i < ARRAY_COL; i++)//初始化四个驱动器
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

            //输出电机参数文本的计时器
            ShowTextTimer = new DispatcherTimer();
            ShowTextTimer.Tick += new EventHandler(textTimer);
            ShowTextTimer.Interval = TimeSpan.FromMilliseconds(100);
            ShowTextTimer.Start();

        }

        #region PVT模式
        private void startButton_Click(object sender, RoutedEventArgs e)//点击【PVT模式】按钮时执行
        {
            statusBar.Background = new SolidColorBrush(Color.FromArgb(255, 230, 20, 20));
            statusInfoTextBlock.Text = "正在执行";

            startButton.IsEnabled = false;
            endButton.IsEnabled = true;
            forceStartButton.IsEnabled = false;
            watchButton.IsEnabled = true;
            SACStartButton.IsEnabled = false;

            if (forceflag)//避免和力学模式及SAC模式冲突
            {
                SensorTimer.Stop();
                forceflag = false;
            }

            #region 计算轨迹位置，速度和时间间隔序列
            //原始数据
            string[] ral = File.ReadAllLines(@"C:\Users\Administrator\Desktop\龙兴国\ExoGaitMonitor\GaitData.txt", Encoding.Default);
            int lineCounter = ral.Length; //获取步态数据行数
            string[] col = (ral[0] ?? string.Empty).Split(new char[] { '\t' }, StringSplitOptions.RemoveEmptyEntries);
            int colCounter = col.Length; //获取步态数据列数
            double[,] pos0 = new double[lineCounter, colCounter]; //原始位置数据
            for (int i = 0; i < lineCounter; i++)
            {
                string[] str = (ral[i] ?? string.Empty).Split(new char[] { '\t' }, StringSplitOptions.RemoveEmptyEntries);
                for (int j = 0; j < colCounter; j++)
                {
                    pos0[i, j] = double.Parse(str[j]) / (360.0 / RATIO) * userUnits[j] * SAFE;
                }
            }

            //一次扩充
            int richLine = lineCounter * 2 - 1;
            double[,] pos1 = new double[richLine, colCounter]; //一次扩充位置数据
            for (int i = 0; i < richLine; i++)
            {
                for (int j = 0; j < colCounter; j++)
                {
                    if (i % 2 == 0)//偶数位存放原始数据
                    {
                        pos1[i, j] = pos0[i / 2, j];
                    }
                    else//奇数位存放扩充数据
                    {
                        pos1[i, j] = (pos0[i / 2 + 1, j] + pos0[i / 2, j]) / 2.0;
                    }
                }
            }

            //二次扩充
            int rich2Line = richLine * 2 - 1;
            double[,] pos2 = new double[rich2Line, colCounter]; //二次扩充位置数据
            for (int i = 0; i < rich2Line; i++)
            {
                for (int j = 0; j < colCounter; j++)
                {
                    if (i % 2 == 0)//偶数位存放原始数据
                    {
                        pos2[i, j] = pos1[i / 2, j];
                    }
                    else//奇数位存放扩充数据
                    {
                        pos2[i, j] = (pos1[i / 2 + 1, j] + pos1[i / 2, j]) / 2.0;
                    }
                }
            }

            //三次扩充
            int rich3Line = rich2Line * 2 - 1;
            double[,] pos3 = new double[rich3Line, colCounter]; //三次扩充位置数据
            int[] times = new int[rich3Line]; //时间间隔
            double[,] vel = new double[rich3Line, colCounter]; //速度
            for (int i = 0; i < rich3Line; i++)
            {
                times[i] = 5; //【设置】时间间隔
                for (int j = 0; j < colCounter; j++)
                {
                    if (i % 2 == 0)//偶数位存放原始数据
                    {
                        pos3[i, j] = pos2[i / 2, j];
                    }
                    else//奇数位存放扩充数据
                    {
                        pos3[i, j] = (pos2[i / 2 + 1, j] + pos2[i / 2, j]) / 2.0;
                    }
                }
            }
            for (int i = 0; i < rich3Line - 1; i++)
            {
                for (int j = 0; j < colCounter; j++)
                {
                    vel[i, j] = (pos3[i + 1, j] - pos3[i, j]) * 1000.0 / times[i];
                }
            }
            vel[rich3Line - 1, 0] = 0;
            vel[rich3Line - 1, 1] = 0;
            vel[rich3Line - 1, 2] = 0;
            vel[rich3Line - 1, 3] = 0;
            #endregion

            for (int i = 0; i < ARRAY_COL; i++)//开始步态前各电机回到轨迹初始位置
            {
                ProfileSettingsObj ProfileSettings;

                ProfileSettings = ampObj[i].ProfileSettings;
                ProfileSettings.ProfileAccel = (ampObj[i].VelocityLoopSettings.VelLoopMaxAcc) / 10;
                ProfileSettings.ProfileDecel = (ampObj[i].VelocityLoopSettings.VelLoopMaxDec) / 10;
                ProfileSettings.ProfileVel = (ampObj[i].VelocityLoopSettings.VelLoopMaxVel) / 10;
                ProfileSettings.ProfileType = CML_PROFILE_TYPE.PROFILE_TRAP; //PVT模式下的控制模式类型
                ampObj[i].ProfileSettings = ProfileSettings;
                ampObj[i].MoveAbs(pos0[0, i]); //PVT模式开始后先移动到各关节初始位置
                ampObj[i].WaitMoveDone(10000); //等待各关节回到初始位置的最大时间
            }

            Linkage.TrajectoryInitialize(pos3, vel, times, 100); //开始步态

            File.WriteAllText(@"C:\Users\Administrator\Desktop\龙兴国\ExoGaitMonitor\ExoGaitMonitor\ExoGaitMonitor\bin\Debug\PVT_ExoGaitData.txt", string.Empty);
            timeCountor = 0;
            MAXRICH = rich3Line;

            TempTimer = new DispatcherTimer();
            TempTimer.Tick += new EventHandler(tempTimer); //写电机在运动过程的一些实际参数
            TempTimer.Interval = TimeSpan.FromMilliseconds(times[0]);
            TempTimer.Start();
        }

        public void tempTimer(object sender, EventArgs e)//写电机实际位置的委托
        {
            statusBar.Background = new SolidColorBrush(Color.FromArgb(255, 230, 20, 20));
            statusInfoTextBlock.Text = "正在执行";

            if (timeCountor == MAXRICH)
            {
                statusBar.Background = new SolidColorBrush(Color.FromArgb(255, 0, 122, 204));
                statusInfoTextBlock.Text = "执行完毕";
                TempTimer.Stop();
            }

            StreamWriter toText = new StreamWriter("PVT_ExoGaitData.txt", true);//打开记录数据文本,可于
            toText.WriteLine(timeCountor.ToString() + '\t' +
            ampObj[0].PositionActual.ToString() + '\t' +
            ampObj[1].PositionActual.ToString() + '\t' +
            ampObj[2].PositionActual.ToString() + '\t' +
            ampObj[3].PositionActual.ToString());
            timeCountor++;
            toText.Close();
        }

        private void endButton_Click(object sender, RoutedEventArgs e)//点击【PVT停止】按钮时执行
        {
            endButton.IsEnabled = false;
            startButton.IsEnabled = true;

            statusBar.Background = new SolidColorBrush(Color.FromArgb(255, 0, 122, 204));
            statusInfoTextBlock.Text = "PVT控制模式已停止";

            Linkage.HaltMove();
            TempTimer.Stop();
        }

        #endregion

        #region 力学模式

        private void watchButton_Click(object sender, RoutedEventArgs e)//点击【启动监视】按钮时执行
        {
            SensorTimer = new DispatcherTimer();
            SensorTimer.Tick += new EventHandler(WriteCMD);
            SensorTimer.Interval = TimeSpan.FromMilliseconds(10);
            SensorTimer.Start();

            initButton.IsEnabled = true;
            watchButton.IsEnabled = false;

            forceflag = true;
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

            //string returnStr = "";
            //for (int i = 0; i < command.Length; i++)
            //{
            //    returnStr += command[i].ToString("X2");
            //}
            //WritetextBox.Text = returnStr;//输出写入命令

            methods.sensor1_SerialPort.Write(command, 0, 8);
            Sensor1_textBox.Text = methods.presN[0].ToString("F");
            Sensor2_textBox.Text = methods.presN[1].ToString("F");
            Sensor3_textBox.Text = methods.presN[2].ToString("F");
            Sensor4_textBox.Text = methods.presN[3].ToString("F");
        }

        private void initButton_Click(object sender, RoutedEventArgs e)//点击【初始归零】按钮时执行
        {
            methods.pressInit();
            forceStartButton.IsEnabled = true;
            SACStartButton.IsEnabled = true;
            initButton.IsEnabled = false;
        }

        private void forceStartButton_Click(object sender, RoutedEventArgs e)//点击【力学模式】按钮时执行
        {
            timeCountor = 0;
            File.WriteAllText(@"C:\Users\Administrator\Desktop\龙兴国\ExoGaitMonitor\ExoGaitMonitor\ExoGaitMonitor\bin\Debug\force.txt", string.Empty);//写拉压力传感器读数

            forceStartButton.IsEnabled = false;
            SACStartButton.IsEnabled = false;
            forceEndButton.IsEnabled = true;
            startButton.IsEnabled = false;
            endButton.IsEnabled = false;

            ForceTimer = new DispatcherTimer();
            ForceTimer.Tick += new EventHandler(forceTimer);
            ForceTimer.Interval = TimeSpan.FromMilliseconds(10);
            ForceTimer.Start();
        }

        public void forceTimer(object sender, EventArgs e)//力学模式控制的委托
        {
            statusBar.Background = new SolidColorBrush(Color.FromArgb(255, 230, 20, 20));
            statusInfoTextBlock.Text = "进入力学控制模式!";

            profileSettingsObj.ProfileType = CML_PROFILE_TYPE.PROFILE_VELOCITY;

            #region 左膝
            if (Math.Abs(methods.presN[0]) < FORCE_THRES)
            {
                ampObj[0].HaltMove();
            }
            if (methods.presN[0] < -FORCE_THRES)
            {
                profileSettingsObj.ProfileVel = FORCE_VEL;
                profileSettingsObj.ProfileAccel = FORCE_VEL;
                profileSettingsObj.ProfileDecel = profileSettingsObj.ProfileAccel;
                ampObj[0].ProfileSettings = profileSettingsObj;

                try
                {
                    ampObj[0].MoveRel(1);
                }
                catch
                {
                    statusBar.Background = new SolidColorBrush(Color.FromArgb(255, 255, 0, 20));
                    statusInfoTextBlock.Text = "左膝限位！";
                }

            }
            if (methods.presN[0] > FORCE_THRES)
            {
                profileSettingsObj.ProfileVel = FORCE_VEL;
                profileSettingsObj.ProfileAccel = FORCE_VEL;
                profileSettingsObj.ProfileDecel = profileSettingsObj.ProfileAccel;
                ampObj[0].ProfileSettings = profileSettingsObj;
                try
                {
                    ampObj[0].MoveRel(-1);
                }
                catch
                {
                    statusBar.Background = new SolidColorBrush(Color.FromArgb(255, 255, 0, 20));
                    statusInfoTextBlock.Text = "左膝限位！";
                }
             
            }
            #endregion

            #region 左髋
            if (Math.Abs(methods.presN[1]) < FORCE_THRES)
            {
                ampObj[1].HaltMove();
            }

            if (methods.presN[1] < -FORCE_THRES)
            {
                profileSettingsObj.ProfileVel = FORCE_VEL;
                profileSettingsObj.ProfileAccel = FORCE_VEL;
                profileSettingsObj.ProfileDecel = profileSettingsObj.ProfileAccel;
                ampObj[1].ProfileSettings = profileSettingsObj;

                try
                {
                    ampObj[1].MoveRel(1);
                }
                catch
                {
                    statusBar.Background = new SolidColorBrush(Color.FromArgb(255, 255, 0, 20));
                    statusInfoTextBlock.Text = "左髋限位！";
                }

            }

            if (methods.presN[1] > FORCE_THRES)
            {
                profileSettingsObj.ProfileVel = FORCE_VEL;
                profileSettingsObj.ProfileAccel = FORCE_VEL;
                profileSettingsObj.ProfileDecel = profileSettingsObj.ProfileAccel;
                ampObj[1].ProfileSettings = profileSettingsObj;

                try
                {
                    ampObj[1].MoveRel(-1);
                }
                catch
                {
                    statusBar.Background = new SolidColorBrush(Color.FromArgb(255, 255, 0, 20));
                    statusInfoTextBlock.Text = "左髋限位！";
                }

            }
            #endregion

            #region 右髋
            if (Math.Abs(methods.presN[2]) < FORCE_THRES)
            {
                ampObj[2].HaltMove();
            }
            if (methods.presN[2] < -FORCE_THRES)
            {
                profileSettingsObj.ProfileVel = FORCE_VEL;
                profileSettingsObj.ProfileAccel = FORCE_VEL;
                profileSettingsObj.ProfileDecel = profileSettingsObj.ProfileAccel;
                ampObj[2].ProfileSettings = profileSettingsObj;

                try
                {
                    ampObj[2].MoveRel(-1);
                }
                catch
                {
                    statusBar.Background = new SolidColorBrush(Color.FromArgb(255, 255, 0, 20));
                    statusInfoTextBlock.Text = "右髋限位！";
                }

            }
            if (methods.presN[2] > FORCE_THRES)
            {
                profileSettingsObj.ProfileVel = FORCE_VEL;
                profileSettingsObj.ProfileAccel = FORCE_VEL;
                profileSettingsObj.ProfileDecel = profileSettingsObj.ProfileAccel;
                ampObj[2].ProfileSettings = profileSettingsObj;

                try
                {
                    ampObj[2].MoveRel(1);
                }
                catch
                {
                    statusBar.Background = new SolidColorBrush(Color.FromArgb(255, 255, 0, 20));
                    statusInfoTextBlock.Text = "右髋限位！";
                }

            }
            #endregion

            #region 右膝
            if (Math.Abs(methods.presN[3]) < FORCE_THRES)
            {
                ampObj[3].HaltMove();
            }
            if (methods.presN[3] < -FORCE_THRES)
            {
                profileSettingsObj.ProfileVel = FORCE_VEL;
                profileSettingsObj.ProfileAccel = FORCE_VEL;
                profileSettingsObj.ProfileDecel = profileSettingsObj.ProfileAccel;
                ampObj[3].ProfileSettings = profileSettingsObj;

                try
                {
                    ampObj[3].MoveRel(-1);
                }
                catch
                {
                    statusBar.Background = new SolidColorBrush(Color.FromArgb(255, 255, 0, 20));
                    statusInfoTextBlock.Text = "右膝限位！";
                }
          
            }
            if (methods.presN[3] > FORCE_THRES)
            {
                profileSettingsObj.ProfileVel = FORCE_VEL;
                profileSettingsObj.ProfileAccel = FORCE_VEL;
                profileSettingsObj.ProfileDecel = profileSettingsObj.ProfileAccel;
                ampObj[3].ProfileSettings = profileSettingsObj;

                try
                {
                    ampObj[3].MoveRel(1);
                }
                catch
                {
                    statusBar.Background = new SolidColorBrush(Color.FromArgb(255, 255, 0, 20));
                    statusInfoTextBlock.Text = "右膝限位！";
                }
            
            }
            #endregion

            StreamWriter toText = new StreamWriter("force.txt", true);//打开记录数据文本,可于
            toText.WriteLine(timeCountor.ToString() + '\t' +
            methods.presN[0].ToString());
            timeCountor++;
            toText.Close();
        }

        private void forceEndButton_Click(object sender, RoutedEventArgs e)//点击【停止】按钮时执行
        {
            for (int i = 0; i < 4; i++)
            {
                ampObj[i].HaltMove();
            }
            ForceTimer.Stop();
            SensorTimer.Stop();
            profileSettingsObj.ProfileType = CML_PROFILE_TYPE.PROFILE_TRAP;

            statusBar.Background = new SolidColorBrush(Color.FromArgb(255, 0, 122, 204));
            statusInfoTextBlock.Text = "力学控制模式已停止";

            forceEndButton.IsEnabled = false;
            watchButton.IsEnabled = true;

            startButton.IsEnabled = true;
        }

        #endregion

        #region SAC模式

        private void SACStartButton_Click(object sender, RoutedEventArgs e)//点击【SAC模式】按钮时执行
        {
            SACEndButton.IsEnabled = true;
            SACStartButton.IsEnabled = false;
            forceStartButton.IsEnabled = false;
            startButton.IsEnabled = false;
            endButton.IsEnabled = false;

            timeCountor = 0;
            File.WriteAllText(@"C:\Users\Administrator\Desktop\龙兴国\ExoGaitMonitor\ExoGaitMonitor\ExoGaitMonitor\bin\Debug\SAC.txt", string.Empty);

            SACTimer = new DispatcherTimer();
            SACTimer.Tick += new EventHandler(sacTimer);
            SACTimer.Interval = TimeSpan.FromMilliseconds(INTERVAL);
            SACTimer.Start();
        }

        public void sacTimer(object sender, EventArgs e)//SAC模式控制的委托
        {
            statusBar.Background = new SolidColorBrush(Color.FromArgb(255, 230, 20, 20));
            statusInfoTextBlock.Text = "进入SAC控制模式!";

            profileSettingsObj.ProfileType = CML_PROFILE_TYPE.PROFILE_VELOCITY;

            for (int i = 0; i < NUM_MOTOR; i++)
            {
                //电机的速度默认单位为0.1counts/s；加速度默认单位为10counts/s
                ampObjAngleActual[i] = (ampObj[i].PositionActual / userUnits[i]) * (360.0 / RATIO);//减速器转的角度
                radian[i] = Math.Abs(Math.PI / 180.0 * ampObjAngleActual[i]);//减速器角度转换为弧度，取绝对值
                ampObjAngleVelActual[i] = (ampObj[i].VelocityActual * 0.1 / userUnits[i]) * 2.0 * Math.PI / RATIO;//减速器角速度单位从0.1counts/s转化为rad/s
                ampObjAngleAccActual[i] = (ampObjAngleVelActual[i] - tempVel[i]) / (INTERVAL / 1000.0);//减速器角加速度单位为rad/s^2
                tempVel[i] = ampObjAngleVelActual[i];
            }
            
            //左膝
            inertia[0] = (1.0 / 3.0 * SHANK_WEIGHT * SHANK_LENGTH * SHANK_LENGTH +
                          1.0 / 2.0 * SHANK_WEIGHT * SHANK_LENGTH * THIGH_LENGTH * Math.Cos(radian[0])) * ampObjAngleAccActual[1] +
                          1.0 / 3.0 * SHANK_WEIGHT * SHANK_LENGTH * SHANK_LENGTH * ampObjAngleAccActual[0];
            coriolis[0] = 1.0 / 2.0 * SHANK_WEIGHT * THIGH_LENGTH * SHANK_LENGTH * Math.Sin(radian[0]) * ampObjAngleVelActual[1] * ampObjAngleVelActual[1];
            gravity[0] = 1.0 / 2.0 * SHANK_WEIGHT * G * SHANK_LENGTH * Math.Sin(radian[0] + radian[1]);
            //左髋
            inertia[1] = (1.0 / 3.0 * THIGH_WEIGHT * THIGH_LENGTH * THIGH_LENGTH +
                         SHANK_WEIGHT * THIGH_LENGTH * THIGH_LENGTH +
                         1.0 / 3.0 * SHANK_WEIGHT * SHANK_LENGTH * SHANK_LENGTH +
                         SHANK_WEIGHT * THIGH_LENGTH * SHANK_LENGTH * Math.Cos(radian[0])) * ampObjAngleAccActual[1] +
                         (1.0 / 3.0 * SHANK_WEIGHT * SHANK_LENGTH * SHANK_LENGTH +
                         1.0 / 2.0 * SHANK_WEIGHT * SHANK_LENGTH * THIGH_LENGTH * Math.Cos(radian[0])) * ampObjAngleAccActual[0];
            coriolis[1] = -1.0 * SHANK_WEIGHT * SHANK_LENGTH * THIGH_LENGTH * Math.Sin(radian[0]) * ampObjAngleVelActual[0] * ampObjAngleVelActual[1] -
                          1.0 / 2.0 * SHANK_WEIGHT * SHANK_LENGTH * THIGH_LENGTH * Math.Sin(radian[0]) * ampObjAngleVelActual[0] * ampObjAngleVelActual[0];
            gravity[1] = (1.0 / 2.0 * THIGH_WEIGHT + SHANK_WEIGHT) * G * THIGH_LENGTH * Math.Sin(radian[1]) +
                          1.0 / 2.0 * SHANK_WEIGHT * G * SHANK_LENGTH * Math.Sin(radian[0] + radian[1]);
            //右髋
            inertia[2] = (1.0 / 3.0 * THIGH_WEIGHT * THIGH_LENGTH * THIGH_LENGTH +
                       SHANK_WEIGHT * THIGH_LENGTH * THIGH_LENGTH +
                       1.0 / 3.0 * SHANK_WEIGHT * SHANK_LENGTH * SHANK_LENGTH +
                       SHANK_WEIGHT * THIGH_LENGTH * SHANK_LENGTH * Math.Cos(radian[3])) * ampObjAngleAccActual[2] +
                       (1.0 / 3.0 * SHANK_WEIGHT * SHANK_LENGTH * SHANK_LENGTH +
                       1.0 / 2.0 * SHANK_WEIGHT * SHANK_LENGTH * THIGH_LENGTH * Math.Cos(radian[3])) * ampObjAngleAccActual[3];
            coriolis[2] = -1.0 * SHANK_WEIGHT * SHANK_LENGTH * THIGH_LENGTH * Math.Sin(radian[3]) * ampObjAngleVelActual[3] * ampObjAngleVelActual[2] -
                          1.0 / 2.0 * SHANK_WEIGHT * SHANK_LENGTH * THIGH_LENGTH * Math.Sin(radian[3]) * ampObjAngleVelActual[3] * ampObjAngleVelActual[3];
            gravity[2] = (1.0 / 2.0 * THIGH_WEIGHT + SHANK_WEIGHT) * G * THIGH_LENGTH * Math.Sin(radian[2]) +
                          1.0 / 2.0 * SHANK_WEIGHT * G * SHANK_LENGTH * Math.Sin(radian[3] + radian[2]);
            //右膝
            inertia[3] = (1.0 / 3.0 * SHANK_WEIGHT * SHANK_LENGTH * SHANK_LENGTH +
                       1.0 / 2.0 * SHANK_WEIGHT * SHANK_LENGTH * THIGH_LENGTH * Math.Cos(radian[3])) * ampObjAngleAccActual[2] +
                       1.0 / 3.0 * SHANK_WEIGHT * SHANK_LENGTH * SHANK_LENGTH * ampObjAngleAccActual[3];
            coriolis[3] = 1.0 / 2.0 * SHANK_WEIGHT * THIGH_LENGTH * SHANK_LENGTH * Math.Sin(radian[3]) * ampObjAngleVelActual[2] * ampObjAngleVelActual[2];
            gravity[3] = 1.0 / 2.0 * SHANK_WEIGHT * G * SHANK_LENGTH * Math.Sin(radian[3] + radian[2]);

            for (int i = 0; i < NUM_MOTOR; i++)
            {
                torque[i] = gravity[i] + (1 - 1.0 / ALPHA) * (inertia[i] + coriolis[i]); //基于SAC计算减速器扭矩
            }

            //左边向后弯曲角为负，向后速度VelocityActual和加速度TrajectoryAcc同样为负
            //右边向后弯曲角为正，向后速度VelocityActual和加速度TrajectoryAcc同样为正
            //这里说的符号和MoveRel()里的符号相同
            //拉压力传感器受压力presN为正，受拉力presN为负
            //即左边受压力时，应配合向后弯曲，

            #region 左膝
            if (Math.Abs(methods.presN[0]) <= SAC_THRES)
            {
                ampObj[0].HaltMove();
            }
            if (methods.presN[0] < -SAC_THRES)
            {
                if (torque[0] > 0)
                {
                    //为使presN尽可能小，torque必定与presN同号
                    torque[0] *= -1.0;
                }

                ang_vel[0] = Math.Abs(((9550.0 * (Math.Abs(ampObj[0].CurrentActual * 0.01) * BATVOL / 1000.0) * ETA) / ((methods.presN[0] * SHANK_MOMENT + torque[0]) / RATIO)) * (userUnits[0] / 60.0)) * 10 * SHANK_VEL_FAC; //电机转速，单位：0.1counts/s；SHANK_VEL_FAC为定义可执行调整系数
                ang_acc[0] = Math.Abs(((torque[0] + methods.presN[0] * SHANK_MOMENT) / RATIO) / MOTOR_INERTIA) * (2.0 * Math.PI) * userUnits[0] * 0.1 *SHANK_ACC_FAC; //电机角加速度，单位：10counts/s^2；SHANK_ACC_FAC为定义可执行调整系数

                profileSettingsObj.ProfileVel = ang_vel[0];
                profileSettingsObj.ProfileAccel = ang_acc[0];
                profileSettingsObj.ProfileDecel = profileSettingsObj.ProfileAccel;
                ampObj[0].ProfileSettings = profileSettingsObj;

                try
                {
                    ampObj[0].MoveRel(1);
                }
                catch
                {
                    statusBar.Background = new SolidColorBrush(Color.FromArgb(255, 255, 0, 20));
                    statusInfoTextBlock.Text = "左膝限位！";
                }
            }
            if (methods.presN[0] > SAC_THRES)
            {
                if (torque[0] < 0)
                {
                    //为使presN尽可能小，torque必定与presN同号
                    torque[0] *= -1.0;
                }

                ang_vel[0] = Math.Abs(((9550.0 * (Math.Abs(ampObj[0].CurrentActual * 0.01) * BATVOL / 1000.0) * ETA) / ((methods.presN[0] * SHANK_MOMENT + torque[0]) / RATIO)) * (userUnits[0] / 60.0)) * 10 * SHANK_VEL_FAC; //电机转速，单位：0.1counts/s；SHANK_VEL_FAC为定义可执行调整系数
                ang_acc[0] = Math.Abs(((torque[0] + methods.presN[0] * SHANK_MOMENT) / RATIO) / MOTOR_INERTIA) * (2.0 * Math.PI) * userUnits[0] * 0.1 * SHANK_ACC_FAC; //电机角加速度，单位：10counts/s^2；SHANK_ACC_FAC为定义可执行调整系数

                profileSettingsObj.ProfileVel = ang_vel[0];
                profileSettingsObj.ProfileAccel = ang_acc[0];
                profileSettingsObj.ProfileDecel = profileSettingsObj.ProfileAccel;
                ampObj[0].ProfileSettings = profileSettingsObj;

                try
                {
                    ampObj[0].MoveRel(-1);
                }
                catch
                {
                    statusBar.Background = new SolidColorBrush(Color.FromArgb(255, 255, 0, 20));
                    statusInfoTextBlock.Text = "左膝限位！";
                }
            }

            #endregion

            #region 左髋
            if (Math.Abs(methods.presN[1]) <= SAC_THRES)
            {
                ampObj[1].HaltMove();
            }

            if (methods.presN[1] < -SAC_THRES)
            {
                if (torque[1] > 0)
                {
                    //为使presN尽可能小，torque必定与presN同号
                    torque[1] *= -1.0;
                }

                ang_vel[1] = Math.Abs(((9550.0 * (Math.Abs(ampObj[1].CurrentActual * 0.01) * BATVOL / 1000.0) * ETA) / ((methods.presN[1] * SHANK_MOMENT + torque[1]) / RATIO)) * (userUnits[1] / 60.0)) * 10 * SHANK_VEL_FAC; //电机转速，单位：0.1counts/s；SHANK_VEL_FAC为定义可执行调整系数
                ang_acc[1] = Math.Abs(((torque[1] + methods.presN[1] * SHANK_MOMENT) / RATIO) / MOTOR_INERTIA) * (2.0 * Math.PI) * userUnits[1] * 0.1 * SHANK_ACC_FAC; //电机角加速度，单位：10counts/s^2；SHANK_ACC_FAC为定义可执行调整系数

                profileSettingsObj.ProfileVel = ang_vel[1];
                profileSettingsObj.ProfileAccel = ang_acc[1];
                profileSettingsObj.ProfileDecel = profileSettingsObj.ProfileAccel;
                ampObj[1].ProfileSettings = profileSettingsObj;

                try
                {
                    ampObj[1].MoveRel(1);
                }
                catch
                {
                    statusBar.Background = new SolidColorBrush(Color.FromArgb(255, 255, 0, 20));
                    statusInfoTextBlock.Text = "左髋限位！";
                }

            }

            if (methods.presN[1] > SAC_THRES)
            {
                if (torque[1] < 0)
                {
                    //为使presN尽可能小，torque必定与presN同号
                    torque[1] *= -1.0;
                }

                ang_vel[1] = Math.Abs(((9550.0 * (Math.Abs(ampObj[1].CurrentActual * 0.01) * BATVOL / 1000.0) * ETA) / ((methods.presN[1] * SHANK_MOMENT + torque[1]) / RATIO)) * (userUnits[1] / 60.0)) * 10 * SHANK_VEL_FAC; //电机转速，单位：0.1counts/s；SHANK_VEL_FAC为定义可执行调整系数
                ang_acc[1] = Math.Abs(((torque[1] + methods.presN[1] * SHANK_MOMENT) / RATIO) / MOTOR_INERTIA) * (2.0 * Math.PI) * userUnits[1] * 0.1 * SHANK_ACC_FAC; //电机角加速度，单位：10counts/s^2；SHANK_ACC_FAC为定义可执行调整系数

                profileSettingsObj.ProfileVel = ang_vel[1];
                profileSettingsObj.ProfileAccel = ang_acc[1];
                profileSettingsObj.ProfileDecel = profileSettingsObj.ProfileAccel;
                ampObj[1].ProfileSettings = profileSettingsObj;

                try
                {
                    ampObj[1].MoveRel(-1);
                }
                catch
                {
                    statusBar.Background = new SolidColorBrush(Color.FromArgb(255, 255, 0, 20));
                    statusInfoTextBlock.Text = "左髋限位！";
                }

            }
            #endregion

            #region 右髋
            if (Math.Abs(methods.presN[2]) <= SAC_THRES)
            {
                ampObj[2].HaltMove();
            }
            if (methods.presN[2] < -SAC_THRES)
            {
                if (torque[2] > 0)
                {
                    //为使presN尽可能小，torque必定与presN同号
                    torque[2] *= -1.0;
                }

                ang_vel[2] = Math.Abs(((9550.0 * (Math.Abs(ampObj[2].CurrentActual * 0.01) * BATVOL / 1000.0) * ETA) / ((methods.presN[2] * SHANK_MOMENT + torque[2]) / RATIO)) * (userUnits[2] / 60.0)) * 10 * SHANK_VEL_FAC; //电机转速，单位：0.1counts/s；SHANK_VEL_FAC为定义可执行调整系数
                ang_acc[2] = Math.Abs(((torque[2] + methods.presN[2] * SHANK_MOMENT) / RATIO) / MOTOR_INERTIA) * (2.0 * Math.PI) * userUnits[2] * 0.1 * SHANK_ACC_FAC; //电机角加速度，单位：10counts/s^2；SHANK_ACC_FAC为定义可执行调整系数

                profileSettingsObj.ProfileVel = ang_vel[2];
                profileSettingsObj.ProfileAccel = ang_acc[2];
                profileSettingsObj.ProfileDecel = profileSettingsObj.ProfileAccel;
                ampObj[2].ProfileSettings = profileSettingsObj;

                try
                {
                    ampObj[2].MoveRel(-1);
                }
                catch
                {
                    statusBar.Background = new SolidColorBrush(Color.FromArgb(255, 255, 0, 20));
                    statusInfoTextBlock.Text = "右髋限位！";
                }

            }
            if (methods.presN[2] > SAC_THRES)
            {
                if (torque[2] < 0)
                {
                    //为使presN尽可能小，torque必定与presN同号
                    torque[2] *= -1.0;
                }

                ang_vel[2] = Math.Abs(((9550.0 * (Math.Abs(ampObj[2].CurrentActual * 0.01) * BATVOL / 1000.0) * ETA) / ((methods.presN[2] * SHANK_MOMENT + torque[2]) / RATIO)) * (userUnits[2] / 60.0)) * 10 * SHANK_VEL_FAC; //电机转速，单位：0.1counts/s；SHANK_VEL_FAC为定义可执行调整系数
                ang_acc[2] = Math.Abs(((torque[2] + methods.presN[2] * SHANK_MOMENT) / RATIO) / MOTOR_INERTIA) * (2.0 * Math.PI) * userUnits[2] * 0.1 * SHANK_ACC_FAC; //电机角加速度，单位：10counts/s^2；SHANK_ACC_FAC为定义可执行调整系数

                profileSettingsObj.ProfileVel = ang_vel[2];
                profileSettingsObj.ProfileAccel = ang_acc[2];
                profileSettingsObj.ProfileDecel = profileSettingsObj.ProfileAccel;
                ampObj[2].ProfileSettings = profileSettingsObj;

                try
                {
                    ampObj[2].MoveRel(1);
                }
                catch
                {
                    statusBar.Background = new SolidColorBrush(Color.FromArgb(255, 255, 0, 20));
                    statusInfoTextBlock.Text = "右髋限位！";
                }
            }
            #endregion

            #region 右膝
            if (Math.Abs(methods.presN[3]) <= SAC_THRES)
            {
                ampObj[3].HaltMove();
            }
            if (methods.presN[3] < -SAC_THRES)
            {
                if (torque[3] > 0)
                {
                    //为使presN尽可能小，torque必定与presN同号
                    torque[3] *= -1.0;
                }

                ang_vel[3] = Math.Abs(((9550.0 * (Math.Abs(ampObj[3].CurrentActual * 0.01) * BATVOL / 1000.0) * ETA) / ((methods.presN[3] * SHANK_MOMENT + torque[3]) / RATIO)) * (userUnits[3] / 60.0)) * 10 * SHANK_VEL_FAC; //电机转速，单位：0.1counts/s；SHANK_VEL_FAC为定义可执行调整系数
                ang_acc[3] = Math.Abs(((torque[3] + methods.presN[3] * SHANK_MOMENT) / RATIO) / MOTOR_INERTIA) * (2.0 * Math.PI) * userUnits[3] * 0.1 * SHANK_ACC_FAC; //电机角加速度，单位：10counts/s^2；SHANK_ACC_FAC为定义可执行调整系数

                profileSettingsObj.ProfileVel = ang_vel[3];
                profileSettingsObj.ProfileAccel = ang_acc[3];
                profileSettingsObj.ProfileDecel = profileSettingsObj.ProfileAccel;
                ampObj[3].ProfileSettings = profileSettingsObj;

                try
                {
                    ampObj[3].MoveRel(-1);
                }
                catch
                {
                    statusBar.Background = new SolidColorBrush(Color.FromArgb(255, 255, 0, 20));
                    statusInfoTextBlock.Text = "右膝限位！";
                }

            }
            if (methods.presN[3] > SAC_THRES)
            {
                if (torque[3] < 0)
                {
                    //为使presN尽可能小，torque必定与presN同号
                    torque[3] *= -1.0;
                }

                ang_vel[3] = Math.Abs(((9550.0 * (Math.Abs(ampObj[3].CurrentActual * 0.01) * BATVOL / 1000.0) * ETA) / ((methods.presN[3] * SHANK_MOMENT + torque[3]) / RATIO)) * (userUnits[3] / 60.0)) * 10 * SHANK_VEL_FAC; //电机转速，单位：0.1counts/s；SHANK_VEL_FAC为定义可执行调整系数
                ang_acc[3] = Math.Abs(((torque[3] + methods.presN[3] * SHANK_MOMENT) / RATIO) / MOTOR_INERTIA) * (2.0 * Math.PI) * userUnits[3] * 0.1 * SHANK_ACC_FAC; //电机角加速度，单位：10counts/s^2；SHANK_ACC_FAC为定义可执行调整系数

                profileSettingsObj.ProfileVel = ang_vel[3];
                profileSettingsObj.ProfileAccel = ang_acc[3];
                profileSettingsObj.ProfileDecel = profileSettingsObj.ProfileAccel;
                ampObj[3].ProfileSettings = profileSettingsObj;

                try
                {
                    ampObj[3].MoveRel(1);
                }
                catch
                {
                    statusBar.Background = new SolidColorBrush(Color.FromArgb(255, 255, 0, 20));
                    statusInfoTextBlock.Text = "右膝限位！";
                }
            }
            #endregion

            StreamWriter sacText = new StreamWriter("SAC.txt", true);

            sacText.WriteLine(timeCountor.ToString() + '\t' +
                methods.presN[0].ToString() + '\t' +
                ampObjAngleActual[0].ToString() + '\t' +
                radian[0].ToString() + '\t' +
                ampObjAngleVelActual[0].ToString() + '\t' +
                ampObj[0].TrajectoryAcc.ToString() + '\t' +
                ampObjAngleAccActual[0].ToString() + '\t' +
                inertia[0].ToString() + '\t' +
                coriolis[0].ToString() + '\t' +
                gravity[0].ToString() + '\t' +
                torque[0].ToString() + '\t' +
                ang_vel[0].ToString() + '\t' +
                ang_acc[0].ToString() + '\t' +
                (ampObj[0].CurrentActual * 0.01).ToString() + '\t' + '\t' +
                timeCountor.ToString() + '\t' +
                methods.presN[1].ToString() + '\t' +
                ampObjAngleActual[1].ToString() + '\t' +
                radian[1].ToString() + '\t' +
                ampObjAngleVelActual[1].ToString() + '\t' +
                ampObj[1].TrajectoryAcc.ToString() + '\t' +
                ampObjAngleAccActual[1].ToString() + '\t' +
                inertia[1].ToString() + '\t' +
                coriolis[1].ToString() + '\t' +
                gravity[1].ToString() + '\t' +
                torque[1].ToString() + '\t' +
                ang_vel[1].ToString() + '\t' +
                ang_acc[1].ToString() + '\t' +
                (ampObj[1].CurrentActual * 0.01).ToString() + '\t' + '\t' +
                timeCountor.ToString() + '\t' +
                methods.presN[2].ToString() + '\t' +
                ampObjAngleActual[2].ToString() + '\t' +
                radian[2].ToString() + '\t' +
                ampObjAngleVelActual[2].ToString() + '\t' +
                ampObj[2].TrajectoryAcc.ToString() + '\t' +
                ampObjAngleAccActual[2].ToString() + '\t' +
                inertia[2].ToString() + '\t' +
                coriolis[2].ToString() + '\t' +
                gravity[2].ToString() + '\t' +
                torque[2].ToString() + '\t' +
                ang_vel[2].ToString() + '\t' +
                ang_acc[2].ToString() + '\t' +
                (ampObj[2].CurrentActual * 0.01).ToString() + '\t' + '\t' +
                timeCountor.ToString() + '\t' +
                methods.presN[3].ToString() + '\t' +
                ampObjAngleActual[3].ToString() + '\t' +
                radian[3].ToString() + '\t' +
                ampObjAngleVelActual[3].ToString() + '\t' +
                ampObj[3].TrajectoryAcc.ToString() + '\t' +
                ampObjAngleAccActual[3].ToString() + '\t' +
                inertia[3].ToString() + '\t' +
                coriolis[3].ToString() + '\t' +
                gravity[3].ToString() + '\t' +
                torque[3].ToString() + '\t' +
                ang_vel[3].ToString() + '\t' +
                ang_acc[3].ToString() + '\t' +
                (ampObj[3].CurrentActual * 0.01).ToString());
            sacText.Close();
            timeCountor++;
        }

        private void SACEndButton_Click(object sender, RoutedEventArgs e)//点击【SAC停止】按钮时执行
        {
            for (int i = 0; i < 4; i++)
            {
                ampObj[i].HaltMove();
            }
            SACTimer.Stop();
            SensorTimer.Stop();
            profileSettingsObj.ProfileType = CML_PROFILE_TYPE.PROFILE_TRAP;

            statusBar.Background = new SolidColorBrush(Color.FromArgb(255, 0, 122, 204));
            statusInfoTextBlock.Text = "SAC控制模式已停止";

            SACEndButton.IsEnabled = false;
            watchButton.IsEnabled = true;

            startButton.IsEnabled = true;
        }

        #endregion

        #region ComboBox

        private void Sensor1_comboBox_DropDownClosed(object sender, EventArgs e)//传感器1串口下拉菜单收回时发生
        {
            ComboBoxItem item = Sensor1_comboBox.SelectedItem as ComboBoxItem; //下拉窗口当前选中的项赋给item
            string tempstr = item.Content.ToString();                        //将选中的项目转为字串存储在tempstr中

            for (int i = 0; i < SPCount.Length; i++)
            {
                if (tempstr == "串口" + SPCount[i])
                {
                    try
                    {
                        sensor1_com = SPCount[i];
                        methods.sensor1_SerialPort_Init(SPCount[i]);

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

        #endregion

        #region 按钮

        private void angleSetButton_Click(object sender, RoutedEventArgs e)//点击【执行】按钮时执行
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

        private void emergencyStopButton_Click(object sender, RoutedEventArgs e)//点击【急停】按钮时执行
        {
            emergencyStopButton.IsEnabled = false;
            angleSetButton.IsEnabled = true;
            int motorNumber = Convert.ToInt16(motorNumberTextBox.Text);
            int i = motorNumber - 1;

            ampObj[i].HaltMove();
            AngleSetTimer.Stop();

        }

        private void zeroPointSetButton_Click(object sender, RoutedEventArgs e)//点击【设置原点】按钮时执行
        {
            ampObj[0].PositionActual = 0;
            ampObj[1].PositionActual = 0;
            ampObj[2].PositionActual = 0;
            ampObj[3].PositionActual = 0;

            zeroPointSetButton.IsEnabled = false;
            statusBar.Background = new SolidColorBrush(Color.FromArgb(255, 0, 122, 204));
            statusInfoTextBlock.Text = "原点设置完毕";
        }

        private void getZeroPointButton_Click(object sender, RoutedEventArgs e)//点击【回归原点】按钮时执行
        {
            GetZeroPointTimer = new DispatcherTimer();
            GetZeroPointTimer.Tick += new EventHandler(getZeroPointTimer);
            GetZeroPointTimer.Interval = TimeSpan.FromMilliseconds(100);// 该时钟频率决定电机运行速度
            GetZeroPointTimer.Start();
        }

        #endregion

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

            ScanPorts();//扫描可用串口
        }

        public void ScanPorts()//扫描可用串口
        {
            SPCount = methods.CheckSerialPortCount();      //获得计算机可用串口名称数组

            ComboBoxItem tempComboBoxItem = new ComboBoxItem();

            if (comcount != SPCount.Length)            //SPCount.length其实就是可用串口的个数
            {
                //当可用串口计数器与实际可用串口个数不相符时
                //初始化下拉窗口并将flag初始化为false

                Sensor1_comboBox.Items.Clear();


                tempComboBoxItem = new ComboBoxItem();
                tempComboBoxItem.Content = "请选择串口";
                Sensor1_comboBox.Items.Add(tempComboBoxItem);
                Sensor1_comboBox.SelectedIndex = 0;

                sensor1_com = null;
                flag = false;

                if (comcount != 0)
                {
                    //在操作过程中增加或减少串口时发生
                    statusBar.Background = new SolidColorBrush(Color.FromArgb(255, 230, 20, 20));
                    statusInfoTextBlock.Text = "串口数目已改变，请重新选择串口！";
                }

                comcount = SPCount.Length;     //将可用串口计数器与现在可用串口个数匹配
            }

            if (!flag)
            {
                if (SPCount.Length > 0)
                {
                    //有可用串口时执行
                    comcount = SPCount.Length;

                    statusBar.Background = new SolidColorBrush(Color.FromArgb(255, 0, 122, 204));
                    statusInfoTextBlock.Text = "检测到" + SPCount.Length + "个串口!";

                    for (int i = 0; i < SPCount.Length; i++)
                    {
                        //分别将可用串口添加到各个下拉窗口中
                        string tempstr = "串口" + SPCount[i];

                        tempComboBoxItem = new ComboBoxItem();
                        tempComboBoxItem.Content = tempstr;
                        Sensor1_comboBox.Items.Add(tempComboBoxItem);

                    }

                    flag = true;

                }
                else
                {
                    comcount = 0;
                    statusBar.Background = new SolidColorBrush(Color.FromArgb(255, 230, 20, 20));
                    statusInfoTextBlock.Text = "未检测到串口!";
                }
            }
        }

        public void textTimer(object sender, EventArgs e)//输出电机参数到相应文本框的委托
        {
            //2017-8-11-用ampObj[2].MotorInfo.ctsPerRev测得EC90盘式电机编码器一圈有25600个计数
            //2017-8-11-验证减速器减速比为160-即电机转160圈，关节转1圈
            try
            {
                for(int i = 0; i < NUM_MOTOR; i++)
                {
                    ampObjAngleActual[i] = (ampObj[i].PositionActual / userUnits[i]) * (360.0 / RATIO);//角度单位从counts转化为°
                    ampObjAngleVelActual[i] = (ampObj[i].VelocityActual / userUnits[i]) * 2.0 * Math.PI;//角速度单位从counts/s转化为rad/s
                    ampObjAngleAccActual[i] = (ampObj[i].TrajectoryAcc / userUnits[i]) * 2.0 * Math.PI;//角加速度单位从counts/s^2转化为rad/s^2
                }
                //电机1(左膝)的文本框输出
                Motor1_position_textBox.Text = ampObjAngleActual[0].ToString("F"); //电机实际位置，单位：°
                Motor1_phaseAngle_textBox.Text = (ampObj[0].CurrentActual * 0.01).ToString("F"); //电机电流，单位：A
                Motor1_velocity_textBox.Text = ampObjAngleVelActual[0].ToString("F"); //电机实际速度，单位：rad/s
                Motor1_accel_textBox.Text = ampObjAngleAccActual[0].ToString("F"); //由轨迹计算而得的加速度，单位：rad/s^2
                //Motor1_decel_textBox.Text = userUnits[0].ToString(); //编码器用户自定义单位，单位：counts/圈

                //电机2(左髋)的文本框输出
                Motor2_position_textBox.Text = ampObjAngleActual[1].ToString("F");
                Motor2_phaseAngle_textBox.Text = (ampObj[1].CurrentActual * 0.01).ToString("F");
                Motor2_velocity_textBox.Text = ampObjAngleVelActual[1].ToString("F");
                Motor2_accel_textBox.Text = ampObjAngleAccActual[1].ToString("F");
                //Motor2_decel_textBox.Text = userUnits[1].ToString();

                //电机3(右髋)的文本框输出
                Motor3_position_textBox.Text = ampObjAngleActual[2].ToString("F");
                Motor3_phaseAngle_textBox.Text = (ampObj[2].CurrentActual * 0.01).ToString("F");
                Motor3_velocity_textBox.Text = ampObjAngleVelActual[2].ToString("F");
                Motor3_accel_textBox.Text = ampObjAngleAccActual[2].ToString("F");
                //Motor3_decel_textBox.Text = userUnits[2].ToString();

                //电机4(右膝)的文本框输出
                Motor4_position_textBox.Text = ampObjAngleActual[3].ToString("F");
                Motor4_phaseAngle_textBox.Text = (ampObj[3].CurrentActual * 0.01).ToString("F");
                Motor4_velocity_textBox.Text = ampObjAngleVelActual[3].ToString("F");
                Motor4_accel_textBox.Text = ampObjAngleAccActual[3].ToString("F");
                //Motor4_decel_textBox.Text = userUnits[3].ToString();
            }
            catch
            {
                statusBar.Background = new SolidColorBrush(Color.FromArgb(255, 230, 20, 20));
                statusInfoTextBlock.Text = "未连接外骨骼！";
            }
        }

        public void angleSetTimer(object sender, EventArgs e)//电机按设置角度转动的委托
        {
            statusBar.Background = new SolidColorBrush(Color.FromArgb(255, 230, 20, 20));
            statusInfoTextBlock.Text = "正在执行";

            const double FAST_VEL = 60000; //设置未到目标角度时较快的速度
            const double SLOW_VEL = 25000; //设置快到目标角度时较慢的速度

            double angleSet = Convert.ToDouble(angleSetTextBox.Text);
            int motorNumber = Convert.ToInt16(motorNumberTextBox.Text);
            int i = motorNumber - 1;

            ampObjAngleActual[i] = (ampObj[i].PositionActual / userUnits[i]) * (360.0 / RATIO);

            profileSettingsObj.ProfileType = CML_PROFILE_TYPE.PROFILE_VELOCITY; // 选择速度模式控制电机

            if (angleSet > 0)
            {
                if (ampObjAngleActual[i] < angleSet)
                {
                    profileSettingsObj.ProfileVel = FAST_VEL;
                    profileSettingsObj.ProfileAccel = FAST_VEL;
                    profileSettingsObj.ProfileDecel = profileSettingsObj.ProfileAccel;
                    ampObj[i].ProfileSettings = profileSettingsObj;

                    if (ampObjAngleActual[i] > (angleSet - 5))
                    {
                        profileSettingsObj.ProfileVel = SLOW_VEL;
                        profileSettingsObj.ProfileAccel = SLOW_VEL;
                        profileSettingsObj.ProfileDecel = profileSettingsObj.ProfileAccel;
                        ampObj[i].ProfileSettings = profileSettingsObj;
                    }
                    try
                    {
                        ampObj[i].MoveRel(1);
                    }
                    catch
                    {
                        statusBar.Background = new SolidColorBrush(Color.FromArgb(255, 230, 20, 20));
                        statusInfoTextBlock.Text = "该电机已限位！";
                    }
    
                }
                else
                {
                    ampObj[i].HaltMove();
                    profileSettingsObj.ProfileType = CML_PROFILE_TYPE.PROFILE_TRAP;
                    for (int j = 0; j < ARRAY_COL; j++)
                    {
                        profileSettingsObj.ProfileVel = 0;
                        profileSettingsObj.ProfileAccel = 0;
                        profileSettingsObj.ProfileDecel = profileSettingsObj.ProfileAccel;
                        ampObj[j].ProfileSettings = profileSettingsObj;
                    }
                    angleSetButton.IsEnabled = true;
                    emergencyStopButton.IsEnabled = false;
                    AngleSetTimer.Stop();
                    statusBar.Background = new SolidColorBrush(Color.FromArgb(255, 0, 122, 204));
                    statusInfoTextBlock.Text = "执行完毕";
                }
            }

            if (angleSet < 0)
            {
                if (ampObjAngleActual[i] > angleSet)
                {
                    profileSettingsObj.ProfileVel = FAST_VEL;
                    profileSettingsObj.ProfileAccel = FAST_VEL;
                    profileSettingsObj.ProfileDecel = profileSettingsObj.ProfileAccel;
                    ampObj[i].ProfileSettings = profileSettingsObj;

                    if (ampObjAngleActual[i] < (angleSet + 5))
                    {
                        profileSettingsObj.ProfileVel = SLOW_VEL;
                        profileSettingsObj.ProfileAccel = SLOW_VEL;
                        profileSettingsObj.ProfileDecel = profileSettingsObj.ProfileAccel;
                        ampObj[i].ProfileSettings = profileSettingsObj;
                    }
                    try
                    {
                        ampObj[i].MoveRel(-1);
                    }
                    catch
                    {
                        statusBar.Background = new SolidColorBrush(Color.FromArgb(255, 230, 20, 20));
                        statusInfoTextBlock.Text = "该电机已限位！";
                    }
            
                }
                else
                {
                    ampObj[i].HaltMove();
                    profileSettingsObj.ProfileType = CML_PROFILE_TYPE.PROFILE_TRAP;
                    for(int j = 0; j < ARRAY_COL; j++)
                    { 
                        profileSettingsObj.ProfileVel = 0;
                        profileSettingsObj.ProfileAccel = 0;
                        profileSettingsObj.ProfileDecel = profileSettingsObj.ProfileAccel;
                        ampObj[j].ProfileSettings = profileSettingsObj;
                    }
                    angleSetButton.IsEnabled = true;
                    emergencyStopButton.IsEnabled = false;
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

            angleSetButton.IsEnabled = false;
            emergencyStopButton.IsEnabled = false;
            zeroPointSetButton.IsEnabled = false;

            const double FAST_VEL = 60000; //设置未到目标角度时较快的速度
            const double SLOW_VEL = 25000; //设置快到目标角度时较慢的速度
            const double ORIGIN_POINT = 2; //原点阈值
            const double TURN_POINT = 10; //快速转慢速的转变点

            ampObjAngleActual[0] = (ampObj[0].PositionActual / userUnits[0]) * (360.0 / RATIO);
            ampObjAngleActual[1] = (ampObj[1].PositionActual / userUnits[1]) * (360.0 / RATIO);
            ampObjAngleActual[2] = (ampObj[2].PositionActual / userUnits[2]) * (360.0 / RATIO);
            ampObjAngleActual[3] = (ampObj[3].PositionActual / userUnits[3]) * (360.0 / RATIO);

            profileSettingsObj.ProfileType = CML_PROFILE_TYPE.PROFILE_VELOCITY; // 选择速度模式控制电机

            if (Math.Abs(ampObjAngleActual[0]) > ORIGIN_POINT)//电机1回归原点
            {
                if (ampObjAngleActual[0] > 0)//此时电机1应往后转
                {
                    if (ampObjAngleActual[0] > TURN_POINT)
                    {
                        profileSettingsObj.ProfileVel = FAST_VEL;
                        profileSettingsObj.ProfileAccel = FAST_VEL;
                        profileSettingsObj.ProfileDecel = profileSettingsObj.ProfileAccel;
                        ampObj[0].ProfileSettings = profileSettingsObj;
                    }
                    else
                    {
                        profileSettingsObj.ProfileVel = SLOW_VEL;
                        profileSettingsObj.ProfileAccel = SLOW_VEL;
                        profileSettingsObj.ProfileDecel = profileSettingsObj.ProfileAccel;
                        ampObj[0].ProfileSettings = profileSettingsObj;
                    }

                    try
                    {
                        ampObj[0].MoveRel(-1);
                    }
                    catch
                    {
                        statusBar.Background = new SolidColorBrush(Color.FromArgb(255, 230, 20, 20));
                        statusInfoTextBlock.Text = "左膝已限位！";
                    }
                       
                }
                else//此时电机1应往前转
                {
                    if (ampObjAngleActual[0] < -TURN_POINT)
                    {
                        profileSettingsObj.ProfileVel = FAST_VEL;
                        profileSettingsObj.ProfileAccel = FAST_VEL;
                        profileSettingsObj.ProfileDecel = profileSettingsObj.ProfileAccel;
                        ampObj[0].ProfileSettings = profileSettingsObj;
                    }
                    else
                    {
                        profileSettingsObj.ProfileVel = SLOW_VEL;
                        profileSettingsObj.ProfileAccel = SLOW_VEL;
                        profileSettingsObj.ProfileDecel = profileSettingsObj.ProfileAccel;
                        ampObj[0].ProfileSettings = profileSettingsObj;
                    }

                    try
                    {
                        ampObj[0].MoveRel(1);
                    }
                    catch
                    {
                        statusBar.Background = new SolidColorBrush(Color.FromArgb(255, 230, 20, 20));
                        statusInfoTextBlock.Text = "左膝已限位！";
                    }
                    
                }
            }
            else
            {
                ampObj[0].HaltMove();
            }

            if (Math.Abs(ampObjAngleActual[1]) > ORIGIN_POINT)//电机2回归原点
            {
                if (ampObjAngleActual[1] > 0)//此时电机2应往后转
                {
                    if (ampObjAngleActual[1] > TURN_POINT)
                    {
                        profileSettingsObj.ProfileVel = FAST_VEL;
                        profileSettingsObj.ProfileAccel = FAST_VEL;
                        profileSettingsObj.ProfileDecel = profileSettingsObj.ProfileAccel;
                        ampObj[1].ProfileSettings = profileSettingsObj;
                    }
                    else
                    {
                        profileSettingsObj.ProfileVel = SLOW_VEL;
                        profileSettingsObj.ProfileAccel = SLOW_VEL;
                        profileSettingsObj.ProfileDecel = profileSettingsObj.ProfileAccel;
                        ampObj[1].ProfileSettings = profileSettingsObj;
                    }

                    try
                    {
                        ampObj[1].MoveRel(-1);
                    }
                    catch
                    {
                        statusBar.Background = new SolidColorBrush(Color.FromArgb(255, 230, 20, 20));
                        statusInfoTextBlock.Text = "左髋已限位！";
                    }
                   
                }
                else//此时电机2应往前转
                {
                    if (ampObjAngleActual[1] < -TURN_POINT)
                    {
                        profileSettingsObj.ProfileVel = FAST_VEL;
                        profileSettingsObj.ProfileAccel = FAST_VEL;
                        profileSettingsObj.ProfileDecel = profileSettingsObj.ProfileAccel;
                        ampObj[1].ProfileSettings = profileSettingsObj;
                    }
                    else
                    {
                        profileSettingsObj.ProfileVel = SLOW_VEL;
                        profileSettingsObj.ProfileAccel = SLOW_VEL;
                        profileSettingsObj.ProfileDecel = profileSettingsObj.ProfileAccel;
                        ampObj[1].ProfileSettings = profileSettingsObj;
                    }

                    try
                    {
                        ampObj[1].MoveRel(1);
                    }
                    catch
                    {
                        statusBar.Background = new SolidColorBrush(Color.FromArgb(255, 230, 20, 20));
                        statusInfoTextBlock.Text = "左髋已限位！";
                    }
              
                }
            }
            else
            {
                ampObj[1].HaltMove();
            }

            if (Math.Abs(ampObjAngleActual[2]) > ORIGIN_POINT)//电机3回归原点
            {
                if (ampObjAngleActual[2] > 0)//此时电机3应往前转
                {
                    if (ampObjAngleActual[2] > TURN_POINT)
                    {
                        profileSettingsObj.ProfileVel = FAST_VEL;
                        profileSettingsObj.ProfileAccel = FAST_VEL;
                        profileSettingsObj.ProfileDecel = profileSettingsObj.ProfileAccel;
                        ampObj[2].ProfileSettings = profileSettingsObj;
                    }
                    else
                    {
                        profileSettingsObj.ProfileVel = SLOW_VEL;
                        profileSettingsObj.ProfileAccel = SLOW_VEL;
                        profileSettingsObj.ProfileDecel = profileSettingsObj.ProfileAccel;
                        ampObj[2].ProfileSettings = profileSettingsObj;
                    }

                    try
                    {
                        ampObj[2].MoveRel(-1);
                    }
                    catch
                    {
                        statusBar.Background = new SolidColorBrush(Color.FromArgb(255, 230, 20, 20));
                        statusInfoTextBlock.Text = "右髋已限位！";
                    }
        
                }
                else//此时电机3应往后转
                {
                    if (ampObjAngleActual[2] < -TURN_POINT)
                    {
                        profileSettingsObj.ProfileVel = FAST_VEL;
                        profileSettingsObj.ProfileAccel = FAST_VEL;
                        profileSettingsObj.ProfileDecel = profileSettingsObj.ProfileAccel;
                        ampObj[2].ProfileSettings = profileSettingsObj;
                    }
                    else
                    {
                        profileSettingsObj.ProfileVel = SLOW_VEL;
                        profileSettingsObj.ProfileAccel = SLOW_VEL;
                        profileSettingsObj.ProfileDecel = profileSettingsObj.ProfileAccel;
                        ampObj[2].ProfileSettings = profileSettingsObj;
                    }

                    try
                    {
                        ampObj[2].MoveRel(1);
                    }
                    catch
                    {
                        statusBar.Background = new SolidColorBrush(Color.FromArgb(255, 230, 20, 20));
                        statusInfoTextBlock.Text = "右髋已限位！";
                    }

                }
            }
            else
            {
                ampObj[2].HaltMove();
            }

            if (Math.Abs(ampObjAngleActual[3]) > ORIGIN_POINT)//电机4回归原点
            {
                if (ampObjAngleActual[3] > 0)//此时电机4应往前转
                {
                    if (ampObjAngleActual[3] > TURN_POINT)
                    {
                        profileSettingsObj.ProfileVel = FAST_VEL;
                        profileSettingsObj.ProfileAccel = FAST_VEL;
                        profileSettingsObj.ProfileDecel = profileSettingsObj.ProfileAccel;
                        ampObj[3].ProfileSettings = profileSettingsObj;
                    }
                    else
                    {
                        profileSettingsObj.ProfileVel = SLOW_VEL;
                        profileSettingsObj.ProfileAccel = SLOW_VEL;
                        profileSettingsObj.ProfileDecel = profileSettingsObj.ProfileAccel;
                        ampObj[3].ProfileSettings = profileSettingsObj;
                    }

                    try
                    {
                        ampObj[3].MoveRel(-1);
                    }
                    catch
                    {
                        statusBar.Background = new SolidColorBrush(Color.FromArgb(255, 230, 20, 20));
                        statusInfoTextBlock.Text = "右膝已限位！";
                    }
                 
                }
                else//此时电机4应往后转
                {
                    if (ampObjAngleActual[3] < -TURN_POINT)
                    {
                        profileSettingsObj.ProfileVel = FAST_VEL;
                        profileSettingsObj.ProfileAccel = FAST_VEL;
                        profileSettingsObj.ProfileDecel = profileSettingsObj.ProfileAccel;
                        ampObj[3].ProfileSettings = profileSettingsObj;
                    }
                    else
                    {
                        profileSettingsObj.ProfileVel = SLOW_VEL;
                        profileSettingsObj.ProfileAccel = SLOW_VEL;
                        profileSettingsObj.ProfileDecel = profileSettingsObj.ProfileAccel;
                        ampObj[3].ProfileSettings = profileSettingsObj;
                    }

                    try
                    {
                        ampObj[3].MoveRel(1);
                    }
                    catch
                    {
                        statusBar.Background = new SolidColorBrush(Color.FromArgb(255, 230, 20, 20));
                        statusInfoTextBlock.Text = "右膝已限位！";
                    }

                }
            }
            else
            {
                ampObj[3].HaltMove();
            }

            if (Math.Abs(ampObjAngleActual[0]) < ORIGIN_POINT && Math.Abs(ampObjAngleActual[1]) < ORIGIN_POINT && Math.Abs(ampObjAngleActual[2]) < ORIGIN_POINT && Math.Abs(ampObjAngleActual[3]) < ORIGIN_POINT)
            {
                statusBar.Background = new SolidColorBrush(Color.FromArgb(255, 0, 122, 204));
                statusInfoTextBlock.Text = "回归原点完毕";
                profileSettingsObj.ProfileType = CML_PROFILE_TYPE.PROFILE_TRAP;
                for (int j = 0; j < ARRAY_COL; j++)
                {
                    profileSettingsObj.ProfileVel = 0;
                    profileSettingsObj.ProfileAccel = 0;
                    profileSettingsObj.ProfileDecel = profileSettingsObj.ProfileAccel;
                    ampObj[j].ProfileSettings = profileSettingsObj;
                }
                angleSetButton.IsEnabled = true;
                GetZeroPointTimer.Stop();
            }
        }

        #endregion

        #region 方法

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



        #endregion

    }
}
