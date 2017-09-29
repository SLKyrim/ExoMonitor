using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace ExoGaitMonitorVer2
{
    class SAC
    {
        #region 声明

        static DispatcherTimer sacTimer;

        #endregion

        public void sacStart()
        {
            if (!sacTimer.IsEnabled)
                sacTimer.Start();
        }


    }
}
