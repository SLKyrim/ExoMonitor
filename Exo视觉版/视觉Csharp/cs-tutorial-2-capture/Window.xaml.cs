using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.IO;
using System.IO.Ports;
using System.Net.Sockets;
using System.Net;
using System.Timers;
//using MathNet.Numerics.LinearAlgebra;
//using MathNet.Numerics.LinearAlgebra.Double;





namespace Intel.RealSense
{
    /// <summary>
    /// Interaction logic for Window.xaml
    /// </summary>
    public partial class CaptureWindow : Window
    {
        private const short WIDTH = 640;//1280;
        private const short HEIGHT = 480;//720;
        static bool Write_Flag = false;
        static int W_H = WIDTH * HEIGHT;


        static Pipeline pipeline;
        static Colorizer colorizer;
        static CancellationTokenSource tokenSource = new CancellationTokenSource();

        //private SerialPort sp = new SerialPort("COM3", 115200, Parity.None, 8, StopBits.One);
        System.Windows.Threading.DispatcherTimer timer1 = new System.Windows.Threading.DispatcherTimer();
        static PipelineProfile pProfile;


        //ushort[] depth = new ushort[W_H];    //640*480
        Intrinsics colorIntrinsics;
        Thread newThread;

        // TCP
        public delegate void showData(string msg);//通信窗口输出
        public delegate void delegate1();//定义委托


        // 定义约束参数
        int dsafe = 0;
        static int dmin = 30;          //预留脚离障碍物的最小距离
        static int hmin = 30;          //预留脚离障碍物的最小距离
        static int lmax = 800;         //机器人允许的最大步长
        static int hmax = 100;         //机器人允许的最大步高
        static int deltal = 20;        //测量的最大误差
        static int deltah = 20;        //测量的最大误差
        static int lnormal = 400;      //正常行走的步长
        static int hnormal = 100;      //正常行走的步高
        static int wbody = 400;
        static int ha = 100;
        static int lf = 350;

        int pStepLen = 0;
        int pStepHgt = 0;
        int Pattern = 0;


        struct IpAndPort
        {
            public string Ip;
            public string Port;
        }

        private TcpClient client;
        private TcpListener server;
        private const int bufferSize = 8000;



        public CaptureWindow()
        {
            InitializeComponent();

            try
            {
                dsafe = dmin + lnormal + deltal;

                Dsafe.Text = dsafe.ToString()+" mm";
                Dmin.Text=dmin.ToString() + " mm";
                Hmin.Text=hmin.ToString() + " mm";
                Wbody.Text=wbody.ToString() + " mm";
                Lmax.Text=lmax.ToString() + " mm";
                Hmax.Text=hmax.ToString() + " mm";
                DeltaL.Text=deltal.ToString() + " mm";
                DeltaH.Text=deltah.ToString() + " mm";
                Ha.Text=ha.ToString() + " mm";
                Lf.Text=lf.ToString() + " mm";
                Ln.Text=lnormal.ToString() + " mm";
                Hn.Text=hnormal.ToString() + " mm";


                //初始化定时器
                timer1.Tick += new EventHandler(timer1_cycle);
                timer1.Interval = new TimeSpan(0, 0, 0, 0, 200);

                pipeline = new Pipeline();
                colorizer = new Colorizer();

                var cfg = new Config();
                cfg.EnableStream(Stream.Depth, WIDTH, HEIGHT);
                cfg.EnableStream(Stream.Color, WIDTH, HEIGHT, Format.Rgb8);

                pProfile = pipeline.Start(cfg);
                //var gyro = pProfile.GetStream(Stream.Gyro);
                newThread = new Thread(ProcessObs); // 开启新线程
                newThread.Priority = ThreadPriority.Highest;
                newThread.Start();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                Application.Current.Shutdown();
            }

        }

        public void timer1_cycle(object sender, EventArgs e)
        {
            try
            {
                var start_time = System.Environment.TickCount;
                timer1.Stop();

                ushort[] depth = new ushort[W_H];
                //获取深度图像
                var frames = pipeline.WaitForFrames();
                var dframe = frames.DepthFrame;
                dframe.CopyTo(depth);
                
                double[] ptx = new double[W_H];    //640*480
                double[] pty = new double[W_H];    //640*480
                double[] ptz = new double[W_H];    //640*480

                //获取相机内参                 
                var stream = (VideoStreamProfile)pProfile.GetStream(Stream.Color);
                var colorIntrinsics = stream.GetIntrinsics();


                //获取有效空间范围内的深度并转换为点云
                int pc_cnt = 0;
                for (int i = 0; i < W_H; i++)
                {
                    if (depth[i] > 500 && depth[i] < 2500)
                    {
                        ptx[pc_cnt] = (((i % WIDTH) - colorIntrinsics.ppx) / colorIntrinsics.fx) * depth[i];
                        pty[pc_cnt] = (((i / WIDTH) - colorIntrinsics.ppy) / colorIntrinsics.fy) * depth[i];
                        ptz[pc_cnt] = depth[i];
                        pc_cnt++;
                    }
                }
                double[] pcx = new double[pc_cnt];    //640*480
                double[] pcy = new double[pc_cnt];    //640*480
                double[] pcz = new double[pc_cnt];    //640*480
                Array.Copy(ptx, pcx, pc_cnt);
                Array.Copy(pty, pcy, pc_cnt);
                Array.Copy(ptz, pcz, pc_cnt);

                //********************************************************
                //RANSAC地面提取
                int ITER = 100;
                double bpa = 0;
                double bpb = 0;
                double bpc = 0;
                int best_count = 0;
                Random rd = new Random(Guid.NewGuid().GetHashCode());
                var rd1 = rd.Next(1, pcx.Length);
                var rd2 = rd.Next(1, pcx.Length);
                var rd3 = rd.Next(1, pcx.Length);
                for (int i = 0; i < ITER; i++)
                {
                    rd1 = rd.Next(1, pcx.Length);
                    rd2 = rd.Next(1, pcx.Length);
                    rd3 = rd.Next(1, pcx.Length);
                    //拟合直线方程z=ax+by+c
                    double[] plane = new double[3];
                    double a = ((pcz[rd1] - pcz[rd2]) * (pcy[rd2] - pcy[rd3]) - (pcz[rd2] - pcz[rd3]) * (pcy[rd1] - pcy[rd2])) / ((pcx[rd1] - pcx[rd2]) * (pcy[rd2] - pcy[rd3]) - (pcx[rd2] - pcx[rd3]) * (pcy[rd1] - pcy[rd2]));
                    double b = ((pcz[rd1] - pcz[rd2]) - a * (pcx[rd1] - pcx[rd2])) / (pcy[rd1] - pcy[rd2]);
                    double c = pcz[rd1] - a * pcx[rd1] - b * pcy[rd1];

                    double[] dist = new double[pcx.Length];
                    int m = 0;
                    for (int k = 0; k < pcx.Length; k++)
                    {
                        dist[k] = Math.Abs(a * pcx[k] + b * pcy[k] + c - pcz[k]);
                        if (dist[k] < 1.0)
                        {
                            m++;
                        }
                    }
                    if (m > best_count)
                    {
                        best_count = m;
                        bpa = a;
                        bpb = b;
                        bpc = c;
                    }
                }

                //将点云分为地面点和其他点
                double[] o_pcx = new double[pcx.Length];
                double[] o_pcy = new double[pcx.Length];
                double[] o_pcz = new double[pcx.Length];
                int o_pc_cnt = 0;
                for (int i = 0; i < pcx.Length; i++)
                {
                    double dist = bpa * pcx[i] + bpb * pcy[i] + bpc - pcz[i];
                    if (dist < -20 || dist > 20)
                    {
                        o_pcx[o_pc_cnt] = pcx[i];
                        o_pcy[o_pc_cnt] = pcy[i];
                        o_pcz[o_pc_cnt] = pcz[i];
                        o_pc_cnt++;
                    }
                }

                double[] on_pcx = new double[o_pc_cnt];
                double[] on_pcy = new double[o_pc_cnt];
                double[] on_pcz = new double[o_pc_cnt];
                Array.Copy(o_pcx, on_pcx, o_pc_cnt);
                Array.Copy(o_pcy, on_pcy, o_pc_cnt);
                Array.Copy(o_pcz, on_pcz, o_pc_cnt);

                //获取新的深度图，不包含地面
                int[,] new_depth = new int[WIDTH, HEIGHT];
                double[,] depth_label = new double[WIDTH, HEIGHT];
                for (int i = 0; i < on_pcx.Length; i++)
                {
                    var x = Math.Round((on_pcx[i] / on_pcz[i]) * colorIntrinsics.fx + colorIntrinsics.ppx);
                    var y = Math.Round((on_pcy[i] / on_pcz[i]) * colorIntrinsics.fy + colorIntrinsics.ppy);
                    if (x < WIDTH && y < HEIGHT)
                    {
                        new_depth[(int)x, (int)y] = 1;
                        depth_label[(int)x, (int)y] = on_pcz[i];
                    }
                }

                //连通域标记
                var cc = CalConnections(new_depth);
                //获取距离人中心最近的地面物体
                double[] dist1 = new double[cc.Count];
                int[] dist_key = new int[cc.Count];
                int dist_cnt = 0;
                foreach (int key in cc.Keys)
                {
                    var cc_values = cc[key];
                    foreach (var val in cc_values)
                    {
                        var d = (val.X - 480) * (val.X - 480) + (val.Y - 320) * (val.Y - 320);
                        //var d = val.Y * val.Y + (val.X - 320) * (val.X - 320);
                        if (dist1[dist_cnt] == 0)
                        {
                            dist1[dist_cnt] = d;
                        }
                        if (d < dist1[dist_cnt])
                        {
                            dist1[dist_cnt] = d;
                        }
                    }
                    dist_key[dist_cnt] = key;
                    dist_cnt++;
                }

                //获取最近区域
                int ii = 0;
                int index = 0;
                while (true)
                {
                    if (dist1[ii] == dist1.Min())
                    {
                        index = ii;
                        break;
                    }
                    ii++;
                }

                //将障碍物转换为点云
                var obs_depth = cc[dist_key[index]];
                //obsize_cnt.Content = obs_depth.Count.ToString();
                double[] obs_pcx = new double[obs_depth.Count];
                double[] obs_pcy = new double[obs_depth.Count];
                double[] obs_pcz = new double[obs_depth.Count];
                byte[,] todisp = new byte[WIDTH, HEIGHT];

                for (int i = 0; i < obs_depth.Count; i++)
                {
                    todisp[obs_depth[i].Y, obs_depth[i].X] = 255;

                    obs_pcx[i] = ((obs_depth[i].Y - colorIntrinsics.ppx) / colorIntrinsics.fx) * depth_label[obs_depth[i].Y, obs_depth[i].X];
                    obs_pcy[i] = ((obs_depth[i].X - colorIntrinsics.ppy) / colorIntrinsics.fy) * depth_label[obs_depth[i].Y, obs_depth[i].X];
                    obs_pcz[i] = depth_label[obs_depth[i].Y, obs_depth[i].X];
                }

                //计算障碍物尺寸、距离等信息
                double[] size_zs = new double[obs_pcz.Length];
                for (int i = 0; i < size_zs.Length; i++)
                {
                    size_zs[i] = Math.Abs((bpa * obs_pcx[i] + bpb * obs_pcy[i] - obs_pcz[i] + bpc) / (Math.Sqrt(bpa * bpa + bpb * bpb + 1)));
                }
                var size_z = (int)Math.Round(size_zs.Max());

                var theta = Math.Acos(1 / Math.Sqrt(bpa * bpa + bpb * bpb + 1));//夹角要加绝对值

                double[] obs_pcx_pj = new double[obs_pcx.Length];
                double[] obs_pcy_pj = new double[obs_pcx.Length];
                double[] obs_pcz_pj = new double[obs_pcx.Length];
                double[] dist_ys = new double[obs_pcx.Length];
                for (int i = 0; i < obs_pcx_pj.Length; i++)
                {
                    var T = (bpa * obs_pcx[i] + bpb * obs_pcy[i] - obs_pcz[i] + bpc) / (bpa * bpa + bpb * bpb + 1);
                    obs_pcx_pj[i] = obs_pcx[i] - bpa * T;
                    obs_pcy_pj[i] = obs_pcy[i] - bpb * T;
                    obs_pcz_pj[i] = obs_pcz[i] + T;
                    dist_ys[i] = Math.Abs(obs_pcz_pj[i] * Math.Sin(theta) - obs_pcy_pj[i] * Math.Cos(theta));
                }

                var size_x = (int)Math.Round(Math.Abs(obs_pcx_pj.Max() - obs_pcx_pj.Min()));
                var size_y = (int)Math.Round(Math.Abs((obs_pcy_pj.Max() - obs_pcy_pj.Min()) * (-1 / Math.Sqrt(bpa * bpa + bpb * bpb + 1))));
                var dist_y = (int)Math.Round(dist_ys.Min());
                //********************************************************

                obsize_x.Content = size_x.ToString() + " mm";
                obsize_y.Content = size_y.ToString() + " mm";
                obsize_z.Content = size_z.ToString() + " mm";
                obdist_y.Content = dist_y.ToString() + " mm";

                //新增加：将障碍物高度、长度和距离转换为下一步的步长和步高
                byte[] ethbuf = new byte[11];
                int cStepLen = 0;
                int cStepHgt = 0;
                int nStepLen = 0;
                int nStepHgt = 0;
                if (dist_y>dsafe)   //当前距离大于安全距离
                {
                    if (Pattern == 33)
                    {
                        Pattern = 3;//Normal模式
                    }
                    else
                    {
                        Pattern = 2;
                    }
                    cStepLen = lnormal;
                    cStepHgt = hnormal;
                    nStepLen = lnormal;
                    nStepHgt = hnormal;
                }
                else
                {
                    var xStepHgt = size_z + hmin + deltah;
                    var xStepLen = dist_y - pStepLen / 2 + size_x + dmin + deltal + lf;
                    if (xStepHgt > hmax || xStepLen > lmax)
                    {   
                        Pattern = 4;    //结束
                        cStepLen = dist_y - dmin-deltal;
                        cStepHgt = hnormal;
                        nStepLen = cStepLen - pStepLen / 2;
                        nStepHgt = hnormal;
                    }
                    else
                    {
                        Pattern = 33;//转换模式
                        cStepLen = dist_y - dmin - deltal;
                        cStepHgt = hnormal;
                        nStepLen = xStepLen;
                        nStepHgt = size_z + hmin + deltah;
                    }
                }


                ComWinTextBox.AppendText("Send to Exoskeleton：" + Pattern.ToString() +"-"+ cStepLen.ToString() + "-" + cStepHgt.ToString() + "-" + nStepLen.ToString() + "-" + nStepHgt.ToString() + "\n");

                //
                ethbuf[0] = 0xAA;   //起始标志
                ethbuf[1] = (byte)Pattern;
                ethbuf[2] = (byte)((cStepLen >> 8) & 0x000000FF);//取最后一步步长的高8位
                ethbuf[3] = (byte)(cStepLen & 0x000000FF);    //取最后一步步长的低8位
                ethbuf[4] = (byte)((cStepHgt >> 8) & 0x000000FF);//取最后一步步高的高8位
                ethbuf[5] = (byte)(cStepHgt & 0x000000FF);    //取最后一步步高的低8位
                ethbuf[6] = (byte)((nStepLen >> 8) & 0x000000FF);//取跨越步长的高8位
                ethbuf[7] = (byte)(nStepLen & 0x000000FF);    //取跨越步长的低8位
                ethbuf[8] = (byte)((nStepHgt >> 8) & 0x000000FF);//取跨越步高的高8位
                ethbuf[9] = (byte)(nStepHgt & 0x000000FF);    //取跨越步高的低8位
                ethbuf[10] = 0xEE;  //结束标志

                //将ethbuf通过网络发送出去
                NetworkStream sendStream = client.GetStream();//获得用于数据传输的流
                sendStream.Write(ethbuf, 0, ethbuf.Length);//最终写入流中


                var bytes = new byte[HEIGHT * WIDTH];
                for (int i = 0; i < HEIGHT; i++)
                {
                    for (int j = 0; j < WIDTH; j++)
                    {
                        bytes[i * WIDTH + j] = todisp[j, i];
                    }
                }
                var bs = BitmapSource.Create(WIDTH, HEIGHT, 300, 300, PixelFormats.Gray8, null, bytes, WIDTH);
                var imgSrc = bs as ImageSource;
                imgObs.Source = imgSrc;


                cal_time.Content = (Environment.TickCount - start_time).ToString() + " ms"; ;
                steplen.Content = cStepLen.ToString()+" mm";
                stephei.Content = cStepHgt.ToString()+" mm";
                pattern.Content = Pattern.ToString();
                steplen_next.Content = nStepLen.ToString() + "mm";
                stephei_next.Content = nStepHgt.ToString() + "mm";

                pStepLen = cStepLen;
                pStepHgt = cStepHgt;
            }
            catch (Exception ex)
            {

            }
        }


        private delegate void Upload_img(VideoFrame frame1, VideoFrame frame2);

        private void ProcessObs()
        {
              
            try
            {
                ushort[] depth = new ushort[W_H];

                while (true)
                {
                    //获取深度图像
                    var frames = pipeline.WaitForFrames();
                    var dframe = frames.DepthFrame;
                    dframe.CopyTo(depth);     
                    var colorized_depth = colorizer.Colorize(frames.DepthFrame);
                    imgColor.Dispatcher.Invoke(new Upload_img(UploadImage), frames.ColorFrame, colorized_depth);
                    //Thread.Sleep(5);
                }
            }
            catch (Exception ex)
            {

            }
        }


        public void UploadImage(VideoFrame frame1, VideoFrame frame2)
        {
            var bytes = new byte[frame1.Stride * frame1.Height];
            frame1.CopyTo(bytes);
            var bs = BitmapSource.Create(frame1.Width, frame1.Height, 300, 300, PixelFormats.Rgb24, null, bytes, frame1.Stride);
            imgColor.Source = bs as ImageSource;

            bytes = new byte[frame2.Stride * frame2.Height];
            frame2.CopyTo(bytes);
            bs = BitmapSource.Create(frame1.Width, frame1.Height, 300, 300, PixelFormats.Rgb24, null, bytes, frame2.Stride);
            imgDepth.Source = bs as ImageSource;
        }


        public void timer2_cycle(object sender, EventArgs e)
        {
            
        }

        private void control_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            tokenSource.Cancel();
        }

        static void write_point_to_txt(double [] ptx, double [] pty, double[] ptz, string tname)
        {
            FileStream fs = new FileStream("E:\\Robotics\\RealSense\\pc_rs\\" + tname + ".txt", FileMode.Create);
            StreamWriter sw = new StreamWriter(fs);
            var L_pc = ptx.Length;
            for (int i = 0; i < L_pc; i++)
            {
                sw.Write(ptx[i].ToString() + ';' + pty[i].ToString() + ';' + ptz[i].ToString() + "\r\n");
            }
            sw.Flush();
            //关闭流
            sw.Close();
            fs.Close();
        }

        static void write_point_to_txt_2(byte[] ptx, string tname)
        {
            FileStream fs = new FileStream("E:\\Robotics\\RealSense\\pc_rs\\" + tname + ".txt", FileMode.Create);
            StreamWriter sw = new StreamWriter(fs);
            var L_pc = ptx.Length;
            for (int i = 0; i < L_pc; i++)
            {
                sw.Write(ptx[i].ToString() + ';');
            }
            sw.Flush();
            //关闭流
            sw.Close();
            fs.Close();
        }

        static void write_point_to_txt_1(double ptx, double pty, double ptz, string tname)
        {
            FileStream fs = new FileStream("E:\\Robotics\\RealSense\\pc_rs\\" + tname + ".txt", FileMode.Create);
            StreamWriter sw = new StreamWriter(fs);
            sw.Write(ptx.ToString() + ';' + pty.ToString() + ';' + ptz.ToString() + "\r\n");
            sw.Flush();
            //关闭流
            sw.Close();
            fs.Close();
        }

        private void button_Click(object sender, RoutedEventArgs e)
        {
            Write_Flag = true;
        }

        static Dictionary<int, List<Point>> CalConnections(int[,] data)
        {
            //一种标记的点的个数  
            Dictionary<int, List<Point>> dic_label_p = new Dictionary<int, List<Point>>();
            //标记  
            int label = 1;
            var Y = data.GetLength(0);
            var X = data.GetLength(1);
            for (int y = 0; y < Y; y++)
            {
                for (int x = 0; x < X; x++)
                {
                    //如果该数据不为0  
                    if (data[y, x] != 0)
                    {
                        List<int> ContainsLabel = new List<int>();
            
                        #region 第一行  
                        if (y == 0)//第一行只看左边  
                        {
                            //第一行第一列，如果不为0，那么填入标记  
                            if (x == 0)
                            {
                                data[y, x] = label;
                                label++;
                            }
                            //第一行，非第一列  
                            else
                            {
                                //如果该列的左侧数据不为0，那么该数据标记填充为左侧的标记  
                                if (data[y, x - 1] != 0)
                                {
                                    data[y, x] = data[y, x - 1];
                                }
                                //否则，填充自增标记  
                                else
                                {
                                    data[y, x] = label;
                                    label++;
                                }
                            }
                        }
                        #endregion

                        #region 非第一行  
                        else
                        {
                            if (x == 0)//最左边  --->不可能出现衔接情况  
                            {
                                /*分析上和右上*/
                                //如果上方数据不为0，则该数据填充上方数据的标记  
                                if (data[y - 1, x] != 0)
                                {
                                    data[y, x] = data[y - 1, x];
                                }
                                //上方数据为0，右上方数据不为0，则该数据填充右上方数据的标记  
                                else if (data[y - 1, x + 1] != 0)
                                {
                                    data[y, x] = data[y - 1, x + 1];
                                }
                                //都为0，则填充自增标记  
                                else
                                {
                                    data[y, x] = label;
                                    label++;
                                }
                            }
                            else if (x == data.GetLength(1) - 1)//最右边   --->不可能出现衔接情况  
                            {
                                /*分析左上和上*/
                                //如果左上数据不为0，则则该数据填充左上方数据的标记  
                                if (data[y - 1, x - 1] != 0)
                                {
                                    data[y, x] = data[y - 1, x - 1];
                                }
                                //左上方数据为0，上方数据不为0，则该数据填充上方数据的标记  
                                else if (data[y - 1, x] != 0)
                                {
                                    data[y, x] = data[y - 1, x];
                                }
                                //左上和上都为0  
                                else
                                {
                                    //如果左侧数据不为0，则该数据填充左侧数据的标记  
                                    if (data[y, x - 1] != 0)
                                    {
                                        data[y, x] = data[y, x - 1];
                                    }
                                    //否则填充自增标记  
                                    else
                                    {
                                        data[y, x] = label;
                                        label++;
                                    }
                                }
                            }
                            else//中间    --->可能出现衔接情况  
                            {
                                //重新实例化需要改变的标记  
                                ContainsLabel = new List<int>();
                                /*分析左上、上和右上*/
                                //上方数据不为0（中间数据），直接填充上方标记  
                                if (data[y - 1, x] != 0)
                                {
                                    data[y, x] = data[y - 1, x];
                                }
                                //上方数据为0  
                                else
                                {
                                    //左上和右上都不为0，填充左上标记  
                                    if (data[y - 1, x - 1] != 0 && data[y - 1, x + 1] != 0)
                                    {
                                        data[y, x] = data[y - 1, x - 1];
                                        //如果右上和左上数据标记不同，则右上标记需要更改  
                                        if (data[y - 1, x + 1] != data[y - 1, x - 1])
                                        {
                                            ContainsLabel.Add(data[y - 1, x + 1]);
                                        }
                                    }
                                    //左上为0，右上不为0  
                                    else if (data[y - 1, x - 1] == 0 && data[y - 1, x + 1] != 0)
                                    {
                                        //左侧不为0，则填充左侧标记  
                                        if (data[y, x - 1] != 0)
                                        {
                                            data[y, x] = data[y, x - 1];
                                            //如果左侧和右上标记不同，，则右上标记需要更改  
                                            if (data[y - 1, x + 1] != data[y, x - 1])
                                            {
                                                ContainsLabel.Add(data[y - 1, x + 1]);
                                            }
                                        }
                                        //左侧为0，则直接填充右上标记  
                                        else
                                        {
                                            data[y, x] = data[y - 1, x + 1];
                                        }
                                    }
                                    //左上不为0，右上为0，填充左上标记  
                                    else if (data[y - 1, x - 1] != 0 && data[y - 1, x + 1] == 0)
                                    {
                                        data[y, x] = data[y - 1, x - 1];
                                    }
                                    //左上和右上都为0  
                                    else if (data[y - 1, x - 1] == 0 && data[y - 1, x + 1] == 0)
                                    {
                                        //如果左侧不为0，则填充左侧标记  
                                        if (data[y, x - 1] != 0)
                                        {
                                            data[y, x] = data[y, x - 1];
                                        }
                                        //否则填充自增标记  
                                        else
                                        {
                                            data[y, x] = label;
                                            label++;
                                        }

                                    }
                                }

                            }
                        }
                        #endregion

                        //如果当前字典不存在该标记，那么创建该标记的Key  
                        if (!dic_label_p.ContainsKey(data[y, x]))
                        {
                            dic_label_p.Add(data[y, x], new List<Point>());
                        }
                        //添加当前标记的点位  
                        dic_label_p[data[y, x]].Add(new Point(x, y));

                        //备份需要更改标记的位置  
                        List<Point> NeedChangedPoints = new List<Point>();
                        //如果有需要更改的标记  
                        for (int i = 0; i < ContainsLabel.Count; i++)
                        {
                            for (int pcount = 0; pcount < dic_label_p[ContainsLabel[i]].Count;)
                            {
                                Point p = dic_label_p[ContainsLabel[i]][pcount];
                                NeedChangedPoints.Add(p);
                                data[p.Y, p.X] = data[y, x];
                                dic_label_p[ContainsLabel[i]].Remove(p);
                                dic_label_p[data[y, x]].Add(p);
                            }
                            dic_label_p.Remove(ContainsLabel[i]);
                        }
                        
                    }
                }
            }

            List<int> remv = new List<int>();
            foreach(int key1 in dic_label_p.Keys)
            {
                if(dic_label_p[key1].Count<500 || dic_label_p[key1].Count>800000)
                {
                    remv.Add(key1);
                }
            }
            foreach(int key1 in remv)
            {
                dic_label_p.Remove(key1);
            }
            return dic_label_p;
        }

        struct Point
        {
            private int x;

            public int X
            {
                get { return x; }
                set { x = value; }
            }
            private int y;

            public int Y
            {
                get { return y; }
                set { y = value; }
            }


            public Point(int x, int y)
            {
                this.x = x;
                this.y = y;
            }
        }

        private void switch_Click(object sender, RoutedEventArgs e)
        {
            //timer1.Start();

            switch_Button.IsEnabled = false;

            if (IPAdressTextBox.Text.Trim() == string.Empty)
            {
                ComWinTextBox.Dispatcher.Invoke(new showData(ComWinTextBox.AppendText), "Please enter the server IP address\n");
                return;
            }
            if (PortTextBox.Text.Trim() == string.Empty)
            {
                ComWinTextBox.Dispatcher.Invoke(new showData(ComWinTextBox.AppendText), "Please enter the server port\n");
                return;
            }
            Thread thread = new Thread(reciveAndListener);
            IpAndPort ipHePort = new IpAndPort();
            ipHePort.Ip = IPAdressTextBox.Text;
            ipHePort.Port = PortTextBox.Text;
            thread.Start((object)ipHePort);
        }

        private void reciveAndListener(object ipAndPort)
        {
            IpAndPort ipHePort = (IpAndPort)ipAndPort;

            IPAddress ip = IPAddress.Parse(ipHePort.Ip);
            server = new TcpListener(ip, int.Parse(ipHePort.Port));
            server.Start();//启动监听

            ComWinTextBox.Dispatcher.Invoke(new showData(ComWinTextBox.AppendText), "Server starts listening...\n");

            //获取连接的客户d端的对象
            client = server.AcceptTcpClient();
            ComWinTextBox.Dispatcher.Invoke(new showData(ComWinTextBox.AppendText), "Client requests connection, connection established!\n");//AcceptTcpClient 是同步方法，会阻塞进程，得到连接对象后才会执行这一步  

            //获得流
            NetworkStream reciveStream = client.GetStream();
            //外骨骼上位机发来连接请求后开始执行图像识别

            do
            {
                byte[] buffer = new byte[bufferSize];
                int msgSize;
                try
                {
                    lock (reciveStream)
                    {
                        msgSize = reciveStream.Read(buffer, 0, bufferSize);
                    }

                    if (msgSize == 9)
                    {
                        timer1.Start();
                    }
                    string msg = Encoding.Default.GetString(buffer, 0, bufferSize);
                    ComWinTextBox.Dispatcher.Invoke(new showData(ReceiveTextBox.AppendText), Encoding.Default.GetString(buffer, 0, msgSize) + "\n");
                }
                catch
                {
                    ComWinTextBox.Dispatcher.Invoke(new showData(ComWinTextBox.AppendText), "\n An exception has occurred: the connection was forced to close");
                    break;
                }
            } while (true);
        }

        private void ComWinTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            ComWinTextBox.ScrollToEnd(); //当通信窗口内容变化时滚动条定位在最下面
        }

        private void ReceiveTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            // 由上位机触发某种条件使得该文本内容变化触发新的视觉反馈返给上位机
            ReceiveTextBox.ScrollToEnd(); 
            Write_Flag = true;
        }
    }
}
