using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Threading;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Controls.Primitives;
using CMLCOMLib;

namespace ExoGaitMonitorVer2
{
    class Force
    { 
        private Sensors sensors;
        private Motors motors;
        private DispatcherTimer timer;
        private StatusBar statusBar;
        private TextBlock statusInfoTextBlock;

        private const double FORCE_THRES = 0.5; //启动阈值
        private const double VOL = 80000; //速度
        private const double ACC = 100000; //加速度

        public void StartForce(Motors motorsIn, Sensors sensorsIn, StatusBar statusBarIn, TextBlock statusInfoTextBlockIn)
        {
            motors = motorsIn;
            sensors = sensorsIn;
            statusBar = statusBarIn;
            statusInfoTextBlock = statusInfoTextBlockIn;

            timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromMilliseconds(10);
            timer.Tick += new EventHandler(ForceTime_Tick);
            timer.Start();
        }
        public void StopForce(Motors motorsIn)
        {
            motors = motorsIn;
            for (int i = 0; i < 4; i++)
            {
                motors.ampObj[i].HaltMove();
            }
            timer.Tick -= new EventHandler(ForceTime_Tick);
            timer.Stop();
        }
        public void ForceTime_Tick(object sender, EventArgs e)
        {
            motors.profileSettingsObj.ProfileType = CML_PROFILE_TYPE.PROFILE_VELOCITY;

            #region 左膝
            if (Math.Abs(sensors.presN[0]) < FORCE_THRES)
            {
                motors.ampObj[0].HaltMove();
            }
            if (sensors.presN[0] < -FORCE_THRES)
            {
                motors.profileSettingsObj.ProfileVel = VOL;
                motors.profileSettingsObj.ProfileAccel = ACC;
                motors.profileSettingsObj.ProfileDecel = motors.profileSettingsObj.ProfileAccel;
                motors.ampObj[0].ProfileSettings = motors.profileSettingsObj;
                try
                {
                    motors.ampObj[0].MoveRel(1);
                }
                catch
                {
                    statusBar.Background = new SolidColorBrush(Color.FromArgb(255, 255, 0, 20));
                    statusInfoTextBlock.Text = "左膝限位";
                }
            }
            if (sensors.presN[0] > FORCE_THRES)
            {
                motors.profileSettingsObj.ProfileVel = VOL;
                motors.profileSettingsObj.ProfileAccel = ACC;
                motors.profileSettingsObj.ProfileDecel = motors.profileSettingsObj.ProfileAccel;
                motors.ampObj[0].ProfileSettings = motors.profileSettingsObj;

                try
                {
                    motors.ampObj[0].MoveRel(-1);
                }
                catch
                {
                    statusBar.Background = new SolidColorBrush(Color.FromArgb(255, 255, 0, 20));
                    statusInfoTextBlock.Text = "左膝限位";
                }
            }
            #endregion

            #region 左髋
            if (Math.Abs(sensors.presN[1]) < FORCE_THRES)
            {
                motors.ampObj[1].HaltMove();
            }
            if (sensors.presN[1] < -FORCE_THRES)
            {
                motors.profileSettingsObj.ProfileVel = VOL;
                motors.profileSettingsObj.ProfileAccel = ACC;
                motors.profileSettingsObj.ProfileDecel = motors.profileSettingsObj.ProfileAccel;
                motors.ampObj[1].ProfileSettings = motors.profileSettingsObj;

                try
                {
                    motors.ampObj[1].MoveRel(1);
                }
                catch
                {
                    statusBar.Background = new SolidColorBrush(Color.FromArgb(255, 255, 0, 20));
                    statusInfoTextBlock.Text = "左髋限位";
                }
            }
            if (sensors.presN[1] > FORCE_THRES)
            {
                motors.profileSettingsObj.ProfileVel = VOL;
                motors.profileSettingsObj.ProfileAccel = ACC;
                motors.profileSettingsObj.ProfileDecel = motors.profileSettingsObj.ProfileAccel;
                motors.ampObj[1].ProfileSettings = motors.profileSettingsObj;

                try
                {
                    motors.ampObj[1].MoveRel(-1);
                }
                catch
                {
                    statusBar.Background = new SolidColorBrush(Color.FromArgb(255, 255, 0, 20));
                    statusInfoTextBlock.Text = "左髋限位";
                }
            }
            #endregion

            #region 右髋
            if (Math.Abs(sensors.presN[2]) < FORCE_THRES)
            {
                motors.ampObj[2].HaltMove();
            }
            if (sensors.presN[2] < -FORCE_THRES)
            {
                motors.profileSettingsObj.ProfileVel = VOL;
                motors.profileSettingsObj.ProfileAccel = ACC;
                motors.profileSettingsObj.ProfileDecel = motors.profileSettingsObj.ProfileAccel;
                motors.ampObj[2].ProfileSettings = motors.profileSettingsObj;
                try
                {
                    motors.ampObj[2].MoveRel(-1);
                }
                catch
                {
                    statusBar.Background = new SolidColorBrush(Color.FromArgb(255, 255, 0, 20));
                    statusInfoTextBlock.Text = "右髋限位";
                }
            }
            if (sensors.presN[2] > FORCE_THRES)
            {
                motors.profileSettingsObj.ProfileVel = VOL;
                motors.profileSettingsObj.ProfileAccel = ACC;
                motors.profileSettingsObj.ProfileDecel = motors.profileSettingsObj.ProfileAccel;
                motors.ampObj[2].ProfileSettings = motors.profileSettingsObj;
                try
                {
                    motors.ampObj[2].MoveRel(11);
                }
                catch
                {
                    statusBar.Background = new SolidColorBrush(Color.FromArgb(255, 255, 0, 20));
                    statusInfoTextBlock.Text = "右髋限位";
                }
            }
            #endregion

            #region 右膝
            if (Math.Abs(sensors.presN[3]) < FORCE_THRES)
            {
                motors.ampObj[3].HaltMove();
            }
            if (sensors.presN[3] < -FORCE_THRES)
            {
                motors.profileSettingsObj.ProfileVel = VOL;
                motors.profileSettingsObj.ProfileAccel = ACC;
                motors.profileSettingsObj.ProfileDecel = motors.profileSettingsObj.ProfileAccel;
                motors.ampObj[3].ProfileSettings = motors.profileSettingsObj;

                try
                {
                    motors.ampObj[3].MoveRel(-1);
                }
                catch
                {
                    statusBar.Background = new SolidColorBrush(Color.FromArgb(255, 255, 0, 20));
                    statusInfoTextBlock.Text = "右膝限位";
                }
            }
            if (sensors.presN[3] > FORCE_THRES)
            {
                motors.profileSettingsObj.ProfileVel = VOL;
                motors.profileSettingsObj.ProfileAccel = ACC;
                motors.profileSettingsObj.ProfileDecel = motors.profileSettingsObj.ProfileAccel;
                motors.ampObj[3].ProfileSettings = motors.profileSettingsObj;
                try
                {
                    motors.ampObj[3].MoveRel(1);
                }
                catch
                {
                    statusBar.Background = new SolidColorBrush(Color.FromArgb(255, 255, 0, 20));
                    statusInfoTextBlock.Text = "右膝限位";
                }
            }
            #endregion
        }
    }
}
