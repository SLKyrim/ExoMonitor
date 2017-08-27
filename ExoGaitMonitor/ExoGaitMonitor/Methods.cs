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
        public double presN = new double(); //传感器接收数据，单位N
        public string presVolt = ""; //传感器电压
        public int presVoltDec; //传感器电压十进制
        const int FILTERCOUNT = 5; //滤波器计数设置常数
        private int countor = 0; //滤波需要的计数器,到 FILTERCOUNT时归零
        private double[] tempPressN = new double[FILTERCOUNT];//滤波取 FILTERCOUNT个接收数据的平均值
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
                presVolt = "";
                for (int i = 3; i < 5; i++)
                {
                    presVolt += bytes[i].ToString("X2");
                }
                //presVolt = bytes[4].ToString("X2");
                presVoltDec = Int16.Parse(presVolt, System.Globalization.NumberStyles.HexNumber);
                //presN = presVoltDec * 5.0 / 4095;
                //presN = 5.0 / 1.65 * (1.65 - 5.0 / 4095 * presVoltDec);
                //presN = (presVoltDec - 128) * 5.0 / 127 * 50.0 / 1.65;
                tempPressN[countor] = (1.40 - presVoltDec * 5.0 / 4095) * 50.0 / 1.40; //拉压力传感器输出，单位N

                countor++;

                //滤波
                if(countor == FILTERCOUNT)
                {
                    countor = 0;//计数器归零
                    for (int i = 0; i < FILTERCOUNT; i++)
                    {
                        presN += tempPressN[i];
                    }
                    presN /= FILTERCOUNT; //取平均值作为最终输出
                }
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
            byte[] clearBytes = new byte[8] { 0, 0, 0, 0, 0, 0, 0, 0 };
            SendControlCMD(clearBytes);//避免不规范操作造成再开机时电机自启动

            if (sensor1_SerialPort != null)
            {
                sensor1_SerialPort.DataReceived -= new System.IO.Ports.SerialDataReceivedEventHandler(sensor1_DataReceived);

                sensor1_SerialPort.Close();
            }

            return true;
        }

        public void SendControlCMD(byte[] command)//串口写入字节命令
        {
            //byte[] command = new byte[19];
            //command[0] = 0x23;//开始字符
            //command[1] = 0x01;//电机1 使能端
            //command[2] = 0x01;//电机1 方向
            //command[3] = 0x08;//电机1 转速高位
            //command[4] = 0x88;//电机1 转速低位（范围1800-16200）对应速度范围（0-2590r/min）
            //command[5] = 0x01;//电机2
            //command[6] = 0x01;//电机2
            //command[7] = 0x08;//电机2
            //command[8] = 0x88;//电机2
            //command[9] = 0x01;//电机3
            //command[10] = 0x01;//电机3
            //command[11] = 0x08;//电机3
            //command[12] = 0x88;//电机3
            //command[13] = 0x01;//电机4
            //command[14] = 0x01;//电机4
            //command[15] = 0x08;//电机4
            //command[16] = 0x88;//电机4
            //command[17] = 0x0D;//结束字符
            //command[18] = 0x0A;
            sensor1_SerialPort.Write(command, 0, 8);
        }
    }
}
