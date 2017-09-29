using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO.Ports;

namespace ExoGaitMonitorVer2
{
    class Sensors
    {
        #region 参数定义

        //串口
        public SerialPort forceSensor_SerialPort = new SerialPort(); //传感器1串口

        //获取可用串口名
        private string[] IsOpenSerialPortCount = null;

        //拉压力传感器参数
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

        #region 拉压力传感器串口

        public void forceSensor_SerialPort_Init(string comstring)//拉压力传感器串口初始化
        {
            if (forceSensor_SerialPort != null)
            {
                if (forceSensor_SerialPort.IsOpen)
                {
                    forceSensor_SerialPort.Close();
                }
            }

            forceSensor_SerialPort = new SerialPort();
            forceSensor_SerialPort.PortName = comstring;
            forceSensor_SerialPort.BaudRate = 115200;
            forceSensor_SerialPort.Parity = Parity.None;
            forceSensor_SerialPort.StopBits = StopBits.One;
            forceSensor_SerialPort.Open();
            countor = 0;
            forceSensor_SerialPort.DataReceived += new System.IO.Ports.SerialDataReceivedEventHandler(sensor1_DataReceived);
        }

        private void sensor1_DataReceived(object sender, SerialDataReceivedEventArgs e)//拉压力传感器串口接收数据
        {
            int bufferlen = forceSensor_SerialPort.BytesToRead;
            if (bufferlen >= 13)
            {
                byte[] bytes = new byte[bufferlen];          //声明一个临时数组存储当前来的串口数据
                forceSensor_SerialPort.Read(bytes, 0, bufferlen);  //读取串口内部缓冲区数据到buf数组
                forceSensor_SerialPort.DiscardInBuffer();          //清空串口内部缓存

                for (int i = 0; i < SENSOR_NUM; i++)
                {
                    tempPresN[i] = 0;
                    presVolt[i] = bytes[3 + 2 * i].ToString("X2") + bytes[4 + 2 * i].ToString("X2");
                    presVoltDec[i] = Int16.Parse(presVolt[i], System.Globalization.NumberStyles.HexNumber);
                    tempPresNs[countor, i] = ((1.40 - presVoltDec[i] * 5.0 / 4095) * 50.0 / 1.40) - _pressInitialization[i]; //拉压力传感器输出，单位N
                }

                countor++;

                if (countor == FILTER_COUNT) //滤波
                {
                    countor = 0;//计数器归零

                    for (int i = 0; i < SENSOR_NUM; i++)
                    {
                        for (int j = 0; j < FILTER_COUNT; j++)
                        {
                            tempPresN[i] += tempPresNs[j, i];
                        }
                        presN[i] = tempPresN[i] / FILTER_COUNT;//取平均值作为最终输出，单位N
                    }
                }
            }
        }

        public void pressInit()//压力初始化
        {
            int numberOfGather = 5;

            for (int i = 0; i < SENSOR_NUM; i++)
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
            if (forceSensor_SerialPort != null)
            {
                forceSensor_SerialPort.DataReceived -= new System.IO.Ports.SerialDataReceivedEventHandler(sensor1_DataReceived);

                forceSensor_SerialPort.Close();
            }
            return true;
        }

    }
}
