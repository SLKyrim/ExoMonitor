using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO.Ports;

namespace ExoGaitMonitor
{
    class Methods
    {
        #region 参数定义

        //串口
        public SerialPort sensor1_SerialPort = new SerialPort(); //传感器1串口

        //获取可用串口名
        private string[] IsOpenSerialPortCount = null;

        //传感器参数
        const int FILTER_COUNT = 5; //滤波器计数设置常数
        const int SENSOR_NUM = 4;//使用传感器的个数

        public double[] presN = new double[SENSOR_NUM]; //传感器接收数据，单位N
        public string[] presVolt = new string[SENSOR_NUM]; //传感器电压
        public double[] presVoltDec = new double[SENSOR_NUM]; //传感器电压十进制
        public double[] tempPresN = new double[SENSOR_NUM]; //临时存储tempPresNs的和值

        private int countor = 0; //滤波需要的计数器,到 FILTERCOUNT时归零
        private double[,] tempPresNs = new double[FILTER_COUNT, SENSOR_NUM];//滤波取 FILTERCOUNT个接收数据的平均值
        private double[] _pressInitialization = new double[SENSOR_NUM];// 初始化所用压力值
        #endregion

        #region 传感器1串口

        public void sensor1_SerialPort_Init(string comstring)//传感器1串口初始化
        {
            if (sensor1_SerialPort != null)
            {
                if (sensor1_SerialPort.IsOpen)
                {
                    sensor1_SerialPort.Close();
                }
            }

            sensor1_SerialPort = new SerialPort();
            sensor1_SerialPort.PortName = comstring;
            sensor1_SerialPort.BaudRate = 115200;
            sensor1_SerialPort.Parity = Parity.None;
            sensor1_SerialPort.StopBits = StopBits.One;
            sensor1_SerialPort.Open();
            countor = 0;
            sensor1_SerialPort.DataReceived += new System.IO.Ports.SerialDataReceivedEventHandler(sensor1_DataReceived);
        }

        private void sensor1_DataReceived(object sender, SerialDataReceivedEventArgs e)//传感器1串口接收数据
        {
            int bufferlen = sensor1_SerialPort.BytesToRead;
            if (bufferlen >= 13)
            {
                byte[] bytes = new byte[bufferlen];          //声明一个临时数组存储当前来的串口数据
                sensor1_SerialPort.Read(bytes, 0, bufferlen);  //读取串口内部缓冲区数据到buf数组
                sensor1_SerialPort.DiscardInBuffer();          //清空串口内部缓存
                                                               //string presVolt = bytes[4].ToString();

                //presVolt[0] = bytes[3].ToString("X2") + bytes[4].ToString("X2");//接收AI0的信号
                //presVolt[1] = bytes[5].ToString("X2") + bytes[6].ToString("X2");//接收AI1的信号
                //presVolt[2] = bytes[7].ToString("X2") + bytes[8].ToString("X2");//接收AI2的信号
                //presVolt[3] = bytes[9].ToString("X2") + bytes[10].ToString("X2");//接收AI3的信号

                for (int i = 0; i < SENSOR_NUM; i++)
                {
                    tempPresN[i] = 0;
                    presVolt[i] = bytes[3 + 2 * i].ToString("X2") + bytes[4 + 2 * i].ToString("X2");
                    presVoltDec[i] = Int16.Parse(presVolt[i], System.Globalization.NumberStyles.HexNumber);
                    tempPresNs[countor, i] = ((1.40 - presVoltDec[i] * 5.0 / 4095) * 50.0 / 1.40) - _pressInitialization[i]; //拉压力传感器输出，单位N
                }
               
                countor++;

                //滤波
                if(countor == FILTER_COUNT)
                {
                    countor = 0;//计数器归零

                    for (int i = 0; i < SENSOR_NUM; i++)
                    {
                        for (int j = 0; j < FILTER_COUNT; j++)
                        {
                            tempPresN[i] += tempPresNs[j, i];
                        }
                        presN[i] = tempPresN[i] / FILTER_COUNT;//取平均值作为最终输出
                    }                 
                }
            }    
        }

        public void pressInit()//压力初始化
        {
            int numberOfGather = 5;

            for (int i = 0; i< SENSOR_NUM; i++)
            {
                _pressInitialization[i] = 0;

                for (int j = 0; j < numberOfGather; j++)
                {
                    _pressInitialization[i] += presN[i];
                }

                _pressInitialization[i] /= numberOfGather;
            }
        }

        #endregion

        public string[] CheckSerialPortCount()//获取可用串口名
        {
            IsOpenSerialPortCount = SerialPort.GetPortNames();
            return IsOpenSerialPortCount;
        }

        public bool SerialPortClose()//关闭窗口时执行
        {
            //byte[] clearBytes = new byte[8] { 0, 0, 0, 0, 0, 0, 0, 0 };
            //SendControlCMD(clearBytes);//避免不规范操作造成再开机时电机自启动

            if (sensor1_SerialPort != null)
            {
                sensor1_SerialPort.DataReceived -= new System.IO.Ports.SerialDataReceivedEventHandler(sensor1_DataReceived);

                sensor1_SerialPort.Close();
            }

            return true;
        }

        //public void SendControlCMD(byte[] command)//串口写入字节命令
        //{
        //    //byte[] command = new byte[19];
        //    //command[0] = 0x23;//开始字符
        //    //command[1] = 0x01;//电机1 使能端
        //    //command[2] = 0x01;//电机1 方向
        //    //command[3] = 0x08;//电机1 转速高位
        //    //command[4] = 0x88;//电机1 转速低位（范围1800-16200）对应速度范围（0-2590r/min）
        //    //command[5] = 0x01;//电机2
        //    //command[6] = 0x01;//电机2
        //    //command[7] = 0x08;//电机2
        //    //command[8] = 0x88;//电机2
        //    //command[9] = 0x01;//电机3
        //    //command[10] = 0x01;//电机3
        //    //command[11] = 0x08;//电机3
        //    //command[12] = 0x88;//电机3
        //    //command[13] = 0x01;//电机4
        //    //command[14] = 0x01;//电机4
        //    //command[15] = 0x08;//电机4
        //    //command[16] = 0x88;//电机4
        //    //command[17] = 0x0D;//结束字符
        //    //command[18] = 0x0A;
        //    sensor1_SerialPort.Write(command, 0, 8);
        //}
    }
}
