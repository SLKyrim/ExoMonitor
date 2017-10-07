using System;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;
using CMLCOMLib;
using System.Windows.Controls.Primitives;

namespace ExoGaitMonitorVer2
{
    class SAC
    {
        #region 声明

        private Motors motors = new Motors();
        private Sensors sensors = new Sensors();
        private DispatcherTimer timer;
        private StatusBar statusBar;
        private TextBlock statusInfoTextBlock;

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
        const double G = 9.8; //重力加速度
        const double BATVOL = 26.9; //电池电压
        const double ETA = 0.8; //减速器使用系数
        const double INTERVAL = 20; //SAC执行频率，单位：ms
        const double SHANK_VEL_FAC = 0.1; //小腿速度可執行係數
        const double SHANK_ACC_FAC = 0.1; //小腿加速度可執行係數
        const double THIGH_VEL_FAC = 0.1; //大腿速度可執行係數
        const double THIGH_ACC_FAC = 0.1; //大腿加速度可執行係數
        const double SAC_THRES = 0.8; //SAC模式拉壓力傳感器閾值



        #endregion

        public void StartSAC(Motors motorsIn, Sensors sensorsIn, StatusBar statusBarIn, TextBlock statusInfoTextBlockIn)
        {
            motors = motorsIn;
            sensors = sensorsIn;
            statusBar = statusBarIn;
            statusInfoTextBlock = statusInfoTextBlockIn;

            timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromMilliseconds(10);
            timer.Tick += new EventHandler(SACTimer_Tick);
            timer.Start();
        }

        public  void StopSAC(Motors motorsIn)
        {
            motors = motorsIn;
            for (int i = 0; i < 4; i++)
            {
                motors.ampObj[i].HaltMove();
            }

            timer.Stop();
            timer.Tick -= new EventHandler(SACTimer_Tick);
        }

        private void SACTimer_Tick(object sender, EventArgs e)
        {
            double[] radian = new double[motors.motor_num]; //角度矩阵，单位：rad
            double[] ang_vel = new double[motors.motor_num]; //输出电机角速度矩阵，单位：rad/s
            double[] ang_acc = new double[motors.motor_num]; //输出电机角加速度矩阵，单位：rad/s^2
            double[] inertia = new double[motors.motor_num]; //惯性矩阵
            double[] coriolis = new double[motors.motor_num]; //科里奥利矩阵
            double[] gravity = new double[motors.motor_num]; //重力矩阵
            double[] torque = new double[motors.motor_num]; //减速器扭矩
            //double[] tempVel = new double[motors.motor_num]; //記錄上次的減速器角速度，单位：rad/s

            motors.profileSettingsObj.ProfileType = CML_PROFILE_TYPE.PROFILE_VELOCITY;

            for (int i = 0; i < motors.motor_num; i++)
            {
                //电机的速度默认单位为0.1counts/s；加速度默认单位为10counts/s
                radian[i] = Math.Abs(Math.PI / 180.0 * motors.ampObjAngleActual[i]);//减速器角度转换为弧度，取绝对值
                //tempVel[i] = motors.ampObjAngleVelActual[i];
            }

            //左膝
            inertia[0] = (1.0 / 3.0 * SHANK_WEIGHT * SHANK_LENGTH * SHANK_LENGTH +
                          1.0 / 2.0 * SHANK_WEIGHT * SHANK_LENGTH * THIGH_LENGTH * Math.Cos(radian[0])) * motors.ampObjAngleAccActual[1] +
                          1.0 / 3.0 * SHANK_WEIGHT * SHANK_LENGTH * SHANK_LENGTH * motors.ampObjAngleAccActual[0];
            coriolis[0] = 1.0 / 2.0 * SHANK_WEIGHT * THIGH_LENGTH * SHANK_LENGTH * Math.Sin(radian[0]) * motors.ampObjAngleVelActual[1] * motors.ampObjAngleVelActual[1];
            gravity[0] = 1.0 / 2.0 * SHANK_WEIGHT * G * SHANK_LENGTH * Math.Sin(radian[0] + radian[1]);
            //左髋
            inertia[1] = (1.0 / 3.0 * THIGH_WEIGHT * THIGH_LENGTH * THIGH_LENGTH +
                         SHANK_WEIGHT * THIGH_LENGTH * THIGH_LENGTH +
                         1.0 / 3.0 * SHANK_WEIGHT * SHANK_LENGTH * SHANK_LENGTH +
                         SHANK_WEIGHT * THIGH_LENGTH * SHANK_LENGTH * Math.Cos(radian[0])) * motors.ampObjAngleAccActual[1] +
                         (1.0 / 3.0 * SHANK_WEIGHT * SHANK_LENGTH * SHANK_LENGTH +
                         1.0 / 2.0 * SHANK_WEIGHT * SHANK_LENGTH * THIGH_LENGTH * Math.Cos(radian[0])) * motors.ampObjAngleAccActual[0];
            coriolis[1] = -1.0 * SHANK_WEIGHT * SHANK_LENGTH * THIGH_LENGTH * Math.Sin(radian[0]) * motors.ampObjAngleVelActual[0] * motors.ampObjAngleVelActual[1] -
                          1.0 / 2.0 * SHANK_WEIGHT * SHANK_LENGTH * THIGH_LENGTH * Math.Sin(radian[0]) * motors.ampObjAngleVelActual[0] * motors.ampObjAngleVelActual[0];
            gravity[1] = (1.0 / 2.0 * THIGH_WEIGHT + SHANK_WEIGHT) * G * THIGH_LENGTH * Math.Sin(radian[1]) +
                          1.0 / 2.0 * SHANK_WEIGHT * G * SHANK_LENGTH * Math.Sin(radian[0] + radian[1]);
            //右髋
            inertia[2] = (1.0 / 3.0 * THIGH_WEIGHT * THIGH_LENGTH * THIGH_LENGTH +
                       SHANK_WEIGHT * THIGH_LENGTH * THIGH_LENGTH +
                       1.0 / 3.0 * SHANK_WEIGHT * SHANK_LENGTH * SHANK_LENGTH +
                       SHANK_WEIGHT * THIGH_LENGTH * SHANK_LENGTH * Math.Cos(radian[3])) * motors.ampObjAngleAccActual[2] +
                       (1.0 / 3.0 * SHANK_WEIGHT * SHANK_LENGTH * SHANK_LENGTH +
                       1.0 / 2.0 * SHANK_WEIGHT * SHANK_LENGTH * THIGH_LENGTH * Math.Cos(radian[3])) * motors.ampObjAngleAccActual[3];
            coriolis[2] = -1.0 * SHANK_WEIGHT * SHANK_LENGTH * THIGH_LENGTH * Math.Sin(radian[3]) * motors.ampObjAngleVelActual[3] * motors.ampObjAngleVelActual[2] -
                          1.0 / 2.0 * SHANK_WEIGHT * SHANK_LENGTH * THIGH_LENGTH * Math.Sin(radian[3]) * motors.ampObjAngleVelActual[3] * motors.ampObjAngleVelActual[3];
            gravity[2] = (1.0 / 2.0 * THIGH_WEIGHT + SHANK_WEIGHT) * G * THIGH_LENGTH * Math.Sin(radian[2]) +
                          1.0 / 2.0 * SHANK_WEIGHT * G * SHANK_LENGTH * Math.Sin(radian[3] + radian[2]);
            //右膝
            inertia[3] = (1.0 / 3.0 * SHANK_WEIGHT * SHANK_LENGTH * SHANK_LENGTH +
                       1.0 / 2.0 * SHANK_WEIGHT * SHANK_LENGTH * THIGH_LENGTH * Math.Cos(radian[3])) * motors.ampObjAngleAccActual[2] +
                       1.0 / 3.0 * SHANK_WEIGHT * SHANK_LENGTH * SHANK_LENGTH * motors.ampObjAngleAccActual[3];
            coriolis[3] = 1.0 / 2.0 * SHANK_WEIGHT * THIGH_LENGTH * SHANK_LENGTH * Math.Sin(radian[3]) * motors.ampObjAngleVelActual[2] * motors.ampObjAngleVelActual[2];
            gravity[3] = 1.0 / 2.0 * SHANK_WEIGHT * G * SHANK_LENGTH * Math.Sin(radian[3] + radian[2]);

            for (int i = 0; i < motors.motor_num; i++)
            {
                torque[i] = gravity[i] + (1 - 1.0 / ALPHA) * (inertia[i] + coriolis[i]); //基于SAC计算减速器扭矩
            }

            #region 左膝
            if (Math.Abs(sensors.presN[0]) <= SAC_THRES)
            {
                motors.ampObj[0].HaltMove();
            }
            if (sensors.presN[0] < -SAC_THRES)
            {
                if (torque[0] > 0)
                {
                    //为使presN尽可能小，torque必定与presN同号
                    torque[0] *= -1.0;
                }

                ang_vel[0] = Math.Abs(((9550.0 * (Math.Abs(motors.ampObj[0].CurrentActual * 0.01) * BATVOL / 1000.0) * ETA) / (sensors.presN[0] * SHANK_MOMENT + torque[0])) * (motors.userUnits[0] / 60.0)) * 10; //电机转速，单位：0.1counts/s；SHANK_VEL_FAC为定义可执行调整系数
                ang_acc[0] = Math.Abs(((torque[0] + sensors.presN[0] * SHANK_MOMENT) / motors.RATIO) / MOTOR_INERTIA) / (2.0 * Math.PI) * motors.userUnits[0] * 0.1; //电机角加速度，单位：10counts/s^2；SHANK_ACC_FAC为定义可执行调整系数

                motors.profileSettingsObj.ProfileVel = ang_vel[0];
                motors.profileSettingsObj.ProfileAccel = ang_acc[0];
                motors.profileSettingsObj.ProfileDecel = motors.profileSettingsObj.ProfileAccel;
                motors.ampObj[0].ProfileSettings = motors.profileSettingsObj;

                try
                {
                    motors.ampObj[0].MoveRel(1);
                }
                catch
                {
                    statusBar.Background = new SolidColorBrush(Color.FromArgb(255, 255, 0, 20));
                    statusInfoTextBlock.Text = "左膝限位！";
                }
            }
            if (sensors.presN[0] > SAC_THRES)
            {
                if (torque[0] < 0)
                {
                    //为使presN尽可能小，torque必定与presN同号
                    torque[0] *= -1.0;
                }

                ang_vel[0] = Math.Abs(((9550.0 * (Math.Abs(motors.ampObj[0].CurrentActual * 0.01) * BATVOL / 1000.0) * ETA) / ((sensors.presN[0] * SHANK_MOMENT + torque[0]) / motors.RATIO)) * (motors.userUnits[0] / 60.0)) * 10; //电机转速，单位：0.1counts/s；SHANK_VEL_FAC为定义可执行调整系数
                ang_acc[0] = Math.Abs(((torque[0] + sensors.presN[0] * SHANK_MOMENT) / motors.RATIO) / MOTOR_INERTIA) / (2.0 * Math.PI) * motors.userUnits[0] * 0.1; //电机角加速度，单位：10counts/s^2；SHANK_ACC_FAC为定义可执行调整系数

                motors.profileSettingsObj.ProfileVel = ang_vel[0];
                motors.profileSettingsObj.ProfileAccel = ang_acc[0];
                motors.profileSettingsObj.ProfileDecel = motors.profileSettingsObj.ProfileAccel;
                motors.ampObj[0].ProfileSettings = motors.profileSettingsObj;

                try
                {
                    motors.ampObj[0].MoveRel(-1);
                }
                catch
                {
                    statusBar.Background = new SolidColorBrush(Color.FromArgb(255, 255, 0, 20));
                    statusInfoTextBlock.Text = "左膝限位！";
                }
            }

            #endregion

            #region 左髋
            if (Math.Abs(sensors.presN[1]) <= SAC_THRES)
            {
                motors.ampObj[1].HaltMove();
            }

            if (sensors.presN[1] < -SAC_THRES)
            {
                if (torque[1] > 0)
                {
                    //为使presN尽可能小，torque必定与presN同号
                    torque[1] *= -1.0;
                }

                ang_vel[1] = Math.Abs(((9550.0 * (Math.Abs(motors.ampObj[1].CurrentActual * 0.01) * BATVOL / 1000.0) * ETA) / ((sensors.presN[1] * SHANK_MOMENT + torque[1]) / motors.RATIO)) * (motors.userUnits[1] / 60.0)) * 10; //电机转速，单位：0.1counts/s；SHANK_VEL_FAC为定义可执行调整系数
                ang_acc[1] = Math.Abs(((torque[1] + sensors.presN[1] * SHANK_MOMENT) / motors.RATIO) / MOTOR_INERTIA) / (2.0 * Math.PI) * motors.userUnits[1] * 0.1; //电机角加速度，单位：10counts/s^2；SHANK_ACC_FAC为定义可执行调整系数

                motors.profileSettingsObj.ProfileVel = ang_vel[1];
                motors.profileSettingsObj.ProfileAccel = ang_acc[1];
                motors.profileSettingsObj.ProfileDecel = motors.profileSettingsObj.ProfileAccel;
                motors.ampObj[1].ProfileSettings = motors.profileSettingsObj;

                try
                {
                    motors.ampObj[1].MoveRel(1);
                }
                catch
                {
                    statusBar.Background = new SolidColorBrush(Color.FromArgb(255, 255, 0, 20));
                    statusInfoTextBlock.Text = "左髋限位！";
                }

            }

            if (sensors.presN[1] > SAC_THRES)
            {
                if (torque[1] < 0)
                {
                    //为使presN尽可能小，torque必定与presN同号
                    torque[1] *= -1.0;
                }

                ang_vel[1] = Math.Abs(((9550.0 * (Math.Abs(motors.ampObj[1].CurrentActual * 0.01) * BATVOL / 1000.0) * ETA) / ((sensors.presN[1] * SHANK_MOMENT + torque[1]) / motors.RATIO)) * (motors.userUnits[1] / 60.0)) * 10; //电机转速，单位：0.1counts/s；SHANK_VEL_FAC为定义可执行调整系数
                ang_acc[1] = Math.Abs(((torque[1] + sensors.presN[1] * SHANK_MOMENT) / motors.RATIO) / MOTOR_INERTIA) / (2.0 * Math.PI) * motors.userUnits[1] * 0.1; //电机角加速度，单位：10counts/s^2；SHANK_ACC_FAC为定义可执行调整系数

                motors.profileSettingsObj.ProfileVel = ang_vel[1];
                motors.profileSettingsObj.ProfileAccel = ang_acc[1];
                motors.profileSettingsObj.ProfileDecel = motors.profileSettingsObj.ProfileAccel;
                motors.ampObj[1].ProfileSettings = motors.profileSettingsObj;

                try
                {
                    motors.ampObj[1].MoveRel(-1);
                }
                catch
                {
                    statusBar.Background = new SolidColorBrush(Color.FromArgb(255, 255, 0, 20));
                    statusInfoTextBlock.Text = "左髋限位！";
                }

            }
            #endregion

            #region 右髋
            if (Math.Abs(sensors.presN[2]) <= SAC_THRES)
            {
                motors.ampObj[2].HaltMove();
            }
            if (sensors.presN[2] < -SAC_THRES)
            {
                if (torque[2] > 0)
                {
                    //为使presN尽可能小，torque必定与presN同号
                    torque[2] *= -1.0;
                }

                ang_vel[2] = Math.Abs(((9550.0 * (Math.Abs(motors.ampObj[2].CurrentActual * 0.01) * BATVOL / 1000.0) * ETA) / ((sensors.presN[2] * SHANK_MOMENT + torque[2]) / motors.RATIO)) * (motors.userUnits[2] / 60.0)) * 10; //电机转速，单位：0.1counts/s；SHANK_VEL_FAC为定义可执行调整系数
                ang_acc[2] = Math.Abs(((torque[2] + sensors.presN[2] * SHANK_MOMENT) / motors.RATIO) / MOTOR_INERTIA) / (2.0 * Math.PI) * motors.userUnits[2] * 0.1; //电机角加速度，单位：10counts/s^2；SHANK_ACC_FAC为定义可执行调整系数

                motors.profileSettingsObj.ProfileVel = ang_vel[2];
                motors.profileSettingsObj.ProfileAccel = ang_acc[2];
                motors.profileSettingsObj.ProfileDecel = motors.profileSettingsObj.ProfileAccel;
                motors.ampObj[2].ProfileSettings = motors.profileSettingsObj;

                try
                {
                    motors.ampObj[2].MoveRel(-1);
                }
                catch
                {
                    statusBar.Background = new SolidColorBrush(Color.FromArgb(255, 255, 0, 20));
                    statusInfoTextBlock.Text = "右髋限位！";
                }

            }
            if (sensors.presN[2] > SAC_THRES)
            {
                if (torque[2] < 0)
                {
                    //为使presN尽可能小，torque必定与presN同号
                    torque[2] *= -1.0;
                }

                ang_vel[2] = Math.Abs(((9550.0 * (Math.Abs(motors.ampObj[2].CurrentActual * 0.01) * BATVOL / 1000.0) * ETA) / ((sensors.presN[2] * SHANK_MOMENT + torque[2]) / motors.RATIO)) * (motors.userUnits[2] / 60.0)) * 10; //电机转速，单位：0.1counts/s；SHANK_VEL_FAC为定义可执行调整系数
                ang_acc[2] = Math.Abs(((torque[2] + sensors.presN[2] * SHANK_MOMENT) / motors.RATIO) / MOTOR_INERTIA) / (2.0 * Math.PI) * motors.userUnits[2] * 0.1; //电机角加速度，单位：10counts/s^2；SHANK_ACC_FAC为定义可执行调整系数

                motors.profileSettingsObj.ProfileVel = ang_vel[2];
                motors.profileSettingsObj.ProfileAccel = ang_acc[2];
                motors.profileSettingsObj.ProfileDecel = motors.profileSettingsObj.ProfileAccel;
                motors.ampObj[2].ProfileSettings = motors.profileSettingsObj;

                try
                {
                    motors.ampObj[2].MoveRel(1);
                }
                catch
                {
                    statusBar.Background = new SolidColorBrush(Color.FromArgb(255, 255, 0, 20));
                    statusInfoTextBlock.Text = "右髋限位！";
                }
            }
            #endregion

            #region 右膝
            if (Math.Abs(sensors.presN[3]) <= SAC_THRES)
            {
                motors.ampObj[3].HaltMove();
            }
            if (sensors.presN[3] < -SAC_THRES)
            {
                if (torque[3] > 0)
                {
                    //为使presN尽可能小，torque必定与presN同号
                    torque[3] *= -1.0;
                }

                ang_vel[3] = Math.Abs(((9550.0 * (Math.Abs(motors.ampObj[3].CurrentActual * 0.01) * BATVOL / 1000.0) * ETA) / ((sensors.presN[3] * SHANK_MOMENT + torque[3]) / motors.RATIO)) * (motors.userUnits[3] / 60.0)) * 10; //电机转速，单位：0.1counts/s；SHANK_VEL_FAC为定义可执行调整系数
                ang_acc[3] = Math.Abs(((torque[3] + sensors.presN[3] * SHANK_MOMENT) / motors.RATIO) / MOTOR_INERTIA) / (2.0 * Math.PI) * motors.userUnits[3] * 0.1; //电机角加速度，单位：10counts/s^2；SHANK_ACC_FAC为定义可执行调整系数

                motors.profileSettingsObj.ProfileVel = ang_vel[3];
                motors.profileSettingsObj.ProfileAccel = ang_acc[3];
                motors.profileSettingsObj.ProfileDecel = motors.profileSettingsObj.ProfileAccel;
                motors.ampObj[3].ProfileSettings = motors.profileSettingsObj;

                try
                {
                    motors.ampObj[3].MoveRel(-1);
                }
                catch
                {
                    statusBar.Background = new SolidColorBrush(Color.FromArgb(255, 255, 0, 20));
                    statusInfoTextBlock.Text = "右膝限位！";
                }

            }
            if (sensors.presN[3] > SAC_THRES)
            {
                if (torque[3] < 0)
                {
                    //为使presN尽可能小，torque必定与presN同号
                    torque[3] *= -1.0;
                }

                ang_vel[3] = Math.Abs(((9550.0 * (Math.Abs(motors.ampObj[3].CurrentActual * 0.01) * BATVOL / 1000.0) * ETA) / ((sensors.presN[3] * SHANK_MOMENT + torque[3]) / motors.RATIO)) * (motors.userUnits[3] / 60.0)) * 10; //电机转速，单位：0.1counts/s；SHANK_VEL_FAC为定义可执行调整系数
                ang_acc[3] = Math.Abs(((torque[3] + sensors.presN[3] * SHANK_MOMENT) / motors.RATIO) / MOTOR_INERTIA) / (2.0 * Math.PI) * motors.userUnits[3] * 0.1; //电机角加速度，单位：10counts/s^2；SHANK_ACC_FAC为定义可执行调整系数

                motors.profileSettingsObj.ProfileVel = ang_vel[3];
                motors.profileSettingsObj.ProfileAccel = ang_acc[3];
                motors.profileSettingsObj.ProfileDecel = motors.profileSettingsObj.ProfileAccel;
                motors.ampObj[3].ProfileSettings = motors.profileSettingsObj;

                try
                {
                    motors.ampObj[3].MoveRel(1);
                }
                catch
                {
                    statusBar.Background = new SolidColorBrush(Color.FromArgb(255, 255, 0, 20));
                    statusInfoTextBlock.Text = "右膝限位！";
                }
            }
            #endregion

        }


    }
}
