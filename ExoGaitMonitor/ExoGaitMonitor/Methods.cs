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
            try
            {
                int bufferlen = sensor1_SerialPort.BytesToRead;    //先记录下来，避免某种原因，人为的原因，操作几次之间时间长，缓存不一致
                if (bufferlen >= 27)                             //一个电机有使能，方向，转速，电流4个参数，前两个各占1个位，后两个各占2个位，故一个电机数据占6各位，加上一个开始位，两个停止位，故总有1+6*4+2=27位
                {
                    byte[] bytes = new byte[bufferlen];          //声明一个临时数组存储当前来的串口数据
                    sensor1_SerialPort.Read(bytes, 0, bufferlen);  //读取串口内部缓冲区数据到buf数组
                    sensor1_SerialPort.DiscardInBuffer();          //清空串口内部缓存
                    //处理和存储数据
                    Int16 endFlag = BitConverter.ToInt16(bytes, 25);
                    if (endFlag == 2573)                         //停止位0A0D (0D0A?)
                    {
                        if (bytes[0] == 0x23)
                            for (int f = 0; f < 4; f++)
                            {
                                //enable[f] = bytes[f * 6 + 1];
                                //direction[f] = bytes[f * 6 + 2];
                                //speed[f] = bytes[f * 6 + 3] * 256 + bytes[f * 6 + 4];
                                //if (speed[f] >= 2048) speed[f] = (speed[f] - 2048) / 4096 * 5180;          //实际范围-2590~2590r/min,而对应范围是0~4096，故中间值位2048
                                //else speed[f] = (2048 - speed[f]) / 4096 * -5180;
                                //current[f] = bytes[f * 6 + 5] * 256 + bytes[f * 6 + 6];
                                //if (current[f] >= 2048) current[f] = (current[f] - 2048) / 4096 * 30;
                                //else current[f] = (2048 - current[f]) / 4096 * -30;
                            }
                    }
                }
            }
            catch { }
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
            sensor1_SerialPort.Write(command, 0, 19);
        }
    }
}
