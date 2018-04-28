using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CMLCOMLib;
using System.IO;

namespace ExoGaitMonitorVer2
{
    class Sitdown
    {
        public void StartSitdown(Motors motor)
        {
            //原始数据
                string[] ral = File.ReadAllLines(@"..\..\InputData\彭工坐姿1.3的1.3倍.txt", Encoding.Default);
            int lineCounter = ral.Length; //获取步态数据行数
            string[] col = (ral[0] ?? string.Empty).Split(new char[] { '\t' }, StringSplitOptions.RemoveEmptyEntries);
            int colCounter = col.Length; //获取步态数据列数
            double[,] pos0 = new double[lineCounter, colCounter]; //原始位置数据
            for (int i = 0; i < lineCounter; i++)
            {
                string[] str = (ral[i] ?? string.Empty).Split(new char[] { '\t' }, StringSplitOptions.RemoveEmptyEntries);
                for (int j = 0; j < colCounter; j++)
                {
                    pos0[i, j] = double.Parse(str[j]) / (360.0 / motor.RATIO) * motor.userUnits[j] * -1;
                }
            }
            #region
            //一次扩充
            int line1Counter = lineCounter * 2 - 1;
            double[,] pos1 = new double[line1Counter, colCounter];
            for (int i = 0; i < line1Counter; i++)
            {
                for (int j = 0; j < colCounter; j++)
                {
                    if (i % 2 == 0)
                    {
                        pos1[i, j] = pos0[i / 2, j];
                    }
                    else
                    {
                        pos1[i, j] = (pos0[i / 2, j] + pos0[i / 2 + 1, j]) / 2.0;
                    }
                }
            }
            //二次扩充
            int line2Counter = 2 * line1Counter - 1;
            double[,] pos2 = new double[line2Counter, colCounter];
            for (int i = 0; i < line2Counter; i++)
            {
                for (int j = 0; j < colCounter; j++)
                {
                    if (i % 2 == 0)
                    {
                        pos2[i, j] = pos1[i / 2, j];
                    }
                    else
                    {
                        pos2[i, j] = (pos1[i / 2, j] + pos1[i / 2 + 1, j]) / 2.0;
                    }
                }
            }
            #endregion
            int line3Counter = 2 * line2Counter - 1;
            double[,] pos3 = new double[line3Counter, colCounter];
            int[] times = new int[line3Counter];//时间间隔
            double[,] vel = new double[line3Counter, colCounter];//速度
            for (int i = 0; i < line3Counter; i++)
            {
                times[i] = 3;//设置时间间隔
                for (int j = 0; j < colCounter; j++)
                {
                    if (i % 2 == 0)
                    {
                        pos3[i, j] = pos2[i / 2, j];
                    }
                    else
                    {
                        pos3[i, j] = (pos2[i / 2, j] + pos2[i / 2 + 1, j]) / 2.0;
                    }

                }
            }

            for (int i = 0; i < line3Counter - 1; i++)
            {
                for (int j = 0; j < colCounter; j++)
                {
                    vel[i, j] = (pos3[i + 1, j] - pos3[i, j]) * 1000.0 / times[i];
                }

            }
            vel[line3Counter - 1, 0] = 0;
            vel[line3Counter - 1, 1] = 0;
            vel[line3Counter - 1, 2] = 0;
            vel[line3Counter - 1, 3] = 0;
            for (int i = 0; i < motor.motor_num; i++)
            {
                motor.profileSettingsObj = motor.ampObj[i].ProfileSettings;
                motor.profileSettingsObj.ProfileAccel = (motor.ampObj[i].VelocityLoopSettings.VelLoopMaxAcc);
                motor.profileSettingsObj.ProfileDecel = (motor.ampObj[i].VelocityLoopSettings.VelLoopMaxDec);
                motor.profileSettingsObj.ProfileVel = (motor.ampObj[i].VelocityLoopSettings.VelLoopMaxVel);
                motor.profileSettingsObj.ProfileType = CML_PROFILE_TYPE.PROFILE_TRAP; //PVT模式下的控制模式类型
                motor.ampObj[i].ProfileSettings = motor.profileSettingsObj;
                motor.ampObj[i].MoveAbs(pos0[0, i]); //PVT模式开始后先移动到各关节初始位置
                motor.ampObj[i].WaitMoveDone(10000); //等待各关节回到初始位置的最大时间
            }
            motor.Linkage.TrajectoryInitialize(pos3, vel, times, 100);
        }
    }
}
