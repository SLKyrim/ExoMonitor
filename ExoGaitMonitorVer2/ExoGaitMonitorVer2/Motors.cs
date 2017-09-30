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
    public class Motors
    {
        #region 声明
        //电机类，包括实例化电机对象和电机联动对象Linkage；
        //包括操作电机向设置方向移动到设置角度；设置外骨骼原点及回归原点

        //CMO
        public AmpObj[] ampObj; //声明驱动器
        public ProfileSettingsObj profileSettingsObj; //声明驱动器属性
        public canOpenObj canObj; //声明网络接口
        //public LinkageObj Linkage; //连接一组电机，能够按输入序列同时操作

        private const int MOTOR_NUM = 4; //设置电机个数
        public int motor_num = MOTOR_NUM; //供调用
        public int RATIO = 160; //减速比

        public double[] userUnits = new double[MOTOR_NUM]; // 用户定义单位：编码器每圈计数
        #endregion

        public void motors_Init()//电机初始化
        {
            canObj = new canOpenObj(); //实例化网络接口
            profileSettingsObj = new ProfileSettingsObj(); //实例化驱动器属性

            canObj.BitRate = CML_BIT_RATES.BITRATE_1_Mbit_per_sec; //设置CAN传输速率为1M/s
            canObj.Initialize(); //网络接口初始化

            ampObj = new AmpObj[MOTOR_NUM]; //实例化四个驱动器（盘式电机）

            for (int i = 0; i < MOTOR_NUM; i++)//初始化四个驱动器
            {
                ampObj[i] = new AmpObj();
                ampObj[i].Initialize(canObj, (short)(i + 1));
                ampObj[i].HaltMode = CML_HALT_MODE.HALT_DECEL; //选择通过减速来停止电机的方式
                ampObj[i].CountsPerUnit = 1; //电机转一圈编码器默认计数25600次，设置为4后转一圈计数6400次
                userUnits[i] = ampObj[i].MotorInfo.ctsPerRev / ampObj[i].CountsPerUnit; //用户定义单位，counts/圈
            }
        }
    }
}
