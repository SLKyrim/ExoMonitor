using System;
using System.Text;
using System.IO;
using CMLCOMLib;
using ClassLib.Motor;


namespace ClassLib.Walk
{
    public class PVT

    {
        public void StartPVT(Motors motors)
        {
            #region 计算轨迹位置，速度和时间间隔序列
            //原始数据
            string[] ral = File.ReadAllLines(@"....\Debug\gaittest.txt", Encoding.Default); //相对目录是在bin/Debug下，所以要回溯到上两级目录
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
            #region
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
            double[,] vel = new double[rich2Line, colCounter]; //速度
            int[] times = new int[rich2Line]; //时间间隔
            for (int i = 0; i < rich2Line; i++)
            {
                times[i] =9; //【设置】时间间隔
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
            //int rich3Line = rich2Line * 2 - 1;
           
            //double[,] pos3 = new double[rich2Line, colCounter]; //三次扩充位置数据

            //for (int i = 0; i < rich3Line; i++)
            //{

            //    for (int j = 0; j < colCounter; j++)
            //    {
            //        if (i % 2 == 0)//偶数位存放原始数据
            //        {
            //            pos3[i, j] = pos2[i / 2, j];
            //        }
            //        else//奇数位存放扩充数据
            //        {
            //            pos3[i, j] = (pos2[i / 2 + 1, j] + pos2[i / 2, j]) / 2.0;

            //        }
            //        //pos3[i, j] = pos3[i, j] - pos3[0, j];
            //    }
            //}
            #endregion
            //四次扩充
            //int rich4Line = 2 * rich3Line - 1;
            //double[,] pos4 = new double[rich4Line, colCounter];//四次扩充位置数据


            //for (int i = 0; i < rich4Line; i++)
            //{
            //    
            //    for (int j = 0; j < colCounter; j++)
            //    {
            //        if (i % 2 == 0)// 偶数位存放原始数据
            //        {
            //            pos4[i, j] = pos3[i / 2, j];
            //        }
            //        else//奇数位存放扩充数据
            //        {
            //            pos4[i, j] = (pos3[i / 2, j] + pos3[i / 2 + 1, j]) / 2.0;
            //        }
            //    }
            //    //}
            for (int i = 0; i < rich2Line - 1; i++)
                {
                    for (int j = 0; j < colCounter; j++)
                    {

                        vel[i, j] = (pos2[i + 1, j] - pos2[i, j]) * 1000.0 / times[i];
                    }
                }
                vel[rich2Line - 1, 0] = 0;
                vel[rich2Line - 1, 1] = 0;
                vel[rich2Line - 1, 2] = 0;
                vel[rich2Line - 1, 3] = 0;
                #endregion
                for (int i = 0; i < motors.motor_num; i++)//开始步态前各电机回到轨迹初始位置
                {
                    motors.profileSettingsObj = motors.ampObj[i].ProfileSettings;
                    motors.profileSettingsObj.ProfileAccel = (motors.ampObj[i].VelocityLoopSettings.VelLoopMaxAcc );
                    motors.profileSettingsObj.ProfileDecel = (motors.ampObj[i].VelocityLoopSettings.VelLoopMaxDec );
                    motors.profileSettingsObj.ProfileVel = (motors.ampObj[i].VelocityLoopSettings.VelLoopMaxVel *0.8);
                    motors.profileSettingsObj.ProfileType = CML_PROFILE_TYPE.PROFILE_TRAP; //PVT模式下的控制模式类型
                    motors.ampObj[i].ProfileSettings = motors.profileSettingsObj;
                    motors.ampObj[i].MoveAbs(pos0[0, i]); //PVT模式开始后先移动到各关节初始位置
                    motors.ampObj[i].WaitMoveDone(10000); //等待各关节回到初始位置的最大时间
                }
                motors.Linkage.TrajectoryInitialize(pos2, vel, times, 50); //开始步态
            }
        }
    }
