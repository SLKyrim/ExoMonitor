using System;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.IO;
using System.Windows.Threading;
using CMLCOMLib;
using System.Windows.Input;
using Microsoft.Research.DynamicDataDisplay;
using Microsoft.Research.DynamicDataDisplay.DataSources;

namespace ExoGaitMonitorVer2
{
    class PVT
    {
        #region 声明

        private Motors motors = new Motors();
        //private Sensors sensors = new Sensors();

        LinkageObj Linkage = new LinkageObj(); //连接一组电机，能够按输入序列同时操作
        #endregion

        public void StartPVT()//执行PVT
        {
            motors.motors_Init();
            Linkage = new LinkageObj();

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
                    pos0[i, j] = double.Parse(str[j]) / (360.0 / motors.RATIO) * motors.userUnits[j] * -1;
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

            Linkage.Initialize(motors.ampObj);
            Linkage.SetMoveLimits(200000, 3000000, 3000000, 200000);
            for (int i = 0; i < motors.motor_num; i++)//开始步态前各电机回到轨迹初始位置
            {
                motors.profileSettingsObj = motors.ampObj[i].ProfileSettings;
                motors.profileSettingsObj.ProfileAccel = (motors.ampObj[i].VelocityLoopSettings.VelLoopMaxAcc) / 10;
                motors.profileSettingsObj.ProfileDecel = (motors.ampObj[i].VelocityLoopSettings.VelLoopMaxDec) / 10;
                motors.profileSettingsObj.ProfileVel = (motors.ampObj[i].VelocityLoopSettings.VelLoopMaxVel) / 10;
                motors.profileSettingsObj.ProfileType = CML_PROFILE_TYPE.PROFILE_TRAP; //PVT模式下的控制模式类型
                motors.ampObj[i].ProfileSettings = motors.profileSettingsObj;
                motors.ampObj[i].MoveAbs(pos0[0, i]); //PVT模式开始后先移动到各关节初始位置
                motors.ampObj[i].WaitMoveDone(10000); //等待各关节回到初始位置的最大时间
            }

            Linkage.TrajectoryInitialize(pos3, vel, times, 100); //开始步态

            File.WriteAllText(@"C:\Users\Administrator\Desktop\龙兴国\ExoGaitMonitor\ExoGaitMonitor\ExoGaitMonitor\bin\Debug\PVT_ExoGaitData.txt", string.Empty);
        }

        public void StopPVT()
        {
            Linkage.HaltMove();
        }
    }
}
