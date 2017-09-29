using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO.Ports;
using System.Windows.Threading;

namespace ExoGaitMonitorVer2
{
    class Sensors
    {
        #region 声明
        //传感器类，包括拉压力传感器的初始化和关闭；向拉压力传感器写命令的线程

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

        static DispatcherTimer sensorsTimer; //向传感器发送命令的线程

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

        public void writeCommandTimer_Tick(object sender, EventArgs e)//向传感器写命令以及向传感器接收数据的委托
        {
            //一路的CRC校验位84 0A; 二路的是C4 0B; 三路的是 05 CB; 四路的是 44 09.
            byte[] command = new byte[8];
            command[0] = 0x01;//#设备地址
            command[1] = 0x03;//#功能代码，读寄存器的值
            command[2] = 0x00;//
            command[3] = 0x00;//
            command[4] = 0x00;//从第AI0号口开始读数据
            command[5] = 0x04;//读四个口
            command[6] = 0x44;//读四个口时的 CRC 校验的低 8 位
            command[7] = 0x09;//读四个口时的 CRC 校验的高 8 位 
            
            forceSensor_SerialPort.Write(command, 0, 8);
        }

        public void writeCommandStart()//开始向传感器写入命令
        {
            sensorsTimer = new DispatcherTimer();
            sensorsTimer.Interval = TimeSpan.FromMilliseconds(10);
            sensorsTimer.Tick += new EventHandler(writeCommandTimer_Tick);

            if (!sensorsTimer.IsEnabled)
                sensorsTimer.Start();
        }

        public void writeCommandStop()//开始向传感器写入命令
        {
            if (!sensorsTimer.IsEnabled)
            {
                sensorsTimer.Tick -= new EventHandler(writeCommandTimer_Tick);
                sensorsTimer.Stop();
            }
              
        }
        #endregion
    }
}
