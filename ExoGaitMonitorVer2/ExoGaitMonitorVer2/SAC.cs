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
        private DispatcherTimer timer;
        

        #endregion

        public void StartSAC(Motors motorsIn)
        {
            motors = motorsIn;

            timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromMilliseconds(10);
            timer.Tick += new EventHandler(SACTimer_Tick);
            timer.Start();
        }

        private void SACTimer_Tick(object sender, EventArgs e)
        {
            motors.profileSettingsObj.ProfileType = CML_PROFILE_TYPE.PROFILE_VELOCITY;


        }


    }
}
