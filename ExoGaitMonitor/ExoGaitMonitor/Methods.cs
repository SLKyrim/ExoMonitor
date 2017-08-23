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
        private SerialPort sensor1_SerialPort = new SerialPort(); //传感器1串口

        //获取可用串口名
        private string[] IsOpenSerialPortCount = null;

        #endregion

        #region 传感器1串口
        FunctionPage functionpage = new FunctionPage();
        public double presKg = new double();
        public string presVolt = "";
        public Int16 presVoltDec;

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
            sensor1_SerialPort.DataReceived += new System.IO.Ports.SerialDataReceivedEventHandler(sensor1_DataReceived);
        }

        private void sensor1_DataReceived(object sender, SerialDataReceivedEventArgs e)//传感器1串口接收数据
        {
            byte[] bytes = new byte[7];          //声明一个临时数组存储当前来的串口数据
            sensor1_SerialPort.Read(bytes, 0, 7);  //读取串口内部缓冲区数据到buf数组
            sensor1_SerialPort.DiscardInBuffer();          //清空串口内部缓存
            //string presVolt = bytes[4].ToString();
            presVolt = "";
            for (int i = 3; i < 5; i++)
            {
                presVolt += bytes[i].ToString("X2");
            }
            //presVolt = bytes[4].ToString("X2");
            presVoltDec = Int16.Parse(presVolt, System.Globalization.NumberStyles.HexNumber);
            //presKg = presVoltDec * 5.0 / 4095;
            //presKg = 5.0 / 1.65 * (1.65 - 5.0 / 4095 * presVoltDec);
            //presKg = (presVoltDec - 128) * 5.0 / 127 * 50.0 / 1.65;
            presKg = (1.40 - presVoltDec * 5.0 / 4095) * 50.0 / 1.40;

        }

        #endregion

        public string[] CheckSerialPortCount()//获取可用串口名
        {
            IsOpenSerialPortCount = SerialPort.GetPortNames();
            return IsOpenSerialPortCount;
        }

        public bool SerialPortClose()//关闭窗口时执行
        {
            byte[] clearBytes = new byte[19] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
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
            //01 03 00 00 00 01 84 0A
            //byte[] command = new byte[19];
            //command[0] = 0x01;//开始字符
            //command[1] = 0x03;//电机1 使能端
            //command[2] = 0x00;//电机1 方向
            //command[3] = 0x00;//电机1 转速高位
            //command[4] = 0x88;//电机1 转速低位（范围1800-16200）对应速度范围（0-2590r/min）
            //command[5] = 0x01;//电机2
            //command[6] = 0x01;//电机2
            //command[7] = 0x0A;//电机2
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

        public void WriteCMD(object sender, EventArgs e)//串口写入字节命令
        {
            //01 03 00 00 00 01 84 0A 读AI0口的指令
            byte[] command = new byte[8];
            command[0] = 0x01;//#设备地址
            command[1] = 0x03;//#功能代码，读寄存器的值
            command[2] = 0x00;
            command[3] = 0x00;
            command[4] = 0x00;//从第AIn号口开始读数据
            command[5] = 0x01;//读几个口
            command[6] = 0x84;//#CRC 校验的低 8 位
            command[7] = 0x0A;//#CRC 校验的高 8 位

            //用于查看提交的指令
            //string returnStr = "";
            //for (int i = 0; i < command.Length; i++)
            //{
            //    returnStr += command[i].ToString("X2");
            //}
            //WritetextBox.Text = returnStr;

            sensor1_SerialPort.Write(command, 0, 8);
            functionpage.presstextBox.Text = presKg.ToString("F");
            //functionpage.presstextBox.Text = presVoltDec.ToString();
        }

    }
}
