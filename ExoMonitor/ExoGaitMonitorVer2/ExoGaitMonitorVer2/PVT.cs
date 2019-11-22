using System;
using System.Text;
using System.IO;
using CMLCOMLib;
using System.Windows.Threading;
using System.Threading;
using System.Threading.Tasks;


namespace ExoGaitMonitorVer2
{
    class PVT
    {

        public void ZeroPoints(Motors motors, double[,] pos, double unit, int timevalue1, int timevalue2)
        {
            for (int i = 0; i < motors.motor_num; i++)//开始步态前各电机回到轨迹初始位置
            {
                motors.profileSettingsObj = motors.ampObj[i].ProfileSettings;
                motors.profileSettingsObj.ProfileAccel = (motors.ampObj[i].VelocityLoopSettings.VelLoopMaxAcc);
                motors.profileSettingsObj.ProfileDecel = (motors.ampObj[i].VelocityLoopSettings.VelLoopMaxDec);
                motors.profileSettingsObj.ProfileVel = (motors.ampObj[i].VelocityLoopSettings.VelLoopMaxVel * 0.8);
                motors.profileSettingsObj.ProfileType = CML_PROFILE_TYPE.PROFILE_TRAP; //PVT模式下的控制模式类型
                motors.ampObj[i].ProfileSettings = motors.profileSettingsObj;
                motors.ampObj[i].MoveAbs(pos[0, i]); //PVT模式开始后先移动到各关节初始位置
                motors.ampObj[i].WaitMoveDone(10000); //等待各关节回到初始位置的最大时间
            }
        }

        public void PVT_Action(Motors motors, double[,] pos, double unit, int timevalue1, int timevalue2)
        {
            //计算轨迹位置，速度和时间间隔序列
            //原始数据

            int lineCounter = pos.GetLength(0);
            int colCounter = pos.GetLength(1); ;
            double[,] vel = new double[lineCounter, colCounter]; //速度
            int[] times = new int[lineCounter]; //时间间隔
            for (int i = 0; i < lineCounter; i++)
            {
                if (i < lineCounter - 300)
                {
                    times[i] = timevalue1; //【设置】时间间隔
                }
                else
                {
                    times[i] = timevalue2; //【设置】时间间隔
                }
            }
            times[lineCounter - 1] = 0;

            for (int i = 0; i < lineCounter - 1; i++)
            {
                for (int j = 0; j < colCounter; j++)
                {

                    vel[i, j] = (pos[i + 1, j] - pos[i, j]) * 1000.0 / (times[i]);
                }
            }
            vel[lineCounter - 1, 0] = 0;
            vel[lineCounter - 1, 1] = 0;
            vel[lineCounter - 1, 2] = 0;
            vel[lineCounter - 1, 3] = 0;

            motors.Linkage.TrajectoryInitialize(pos, vel, times, 500); //开始步态
        }
    }
}
