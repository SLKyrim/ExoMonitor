using System;
using System.Linq;
using System.IO;
using System.Text;
using System.Drawing;


namespace Intel.RealSense
{
    class Program
    {
        static void Main(string[] args)
        {
            FrameQueue q = new FrameQueue();
            //Bitmap bm = new Bitmap(640, 480);
            int W = 1280;
            int H = 720;

            using (var ctx = new Context())
            {
                var devices = ctx.QueryDevices();

                Console.WriteLine("There are " + devices.Count + " connected RealSense devices.");
                if (devices.Count == 0) return;
                var dev = devices[0];

                Console.WriteLine("\nUsing device 0, an {0}", dev.Info[CameraInfo.Name]);
                Console.WriteLine("    Serial number: {0}", dev.Info[CameraInfo.SerialNumber]);
                Console.WriteLine("    Firmware version: {0}", dev.Info[CameraInfo.FirmwareVersion]);

                var depthSensor = dev.Sensors[0];            

                var sp = depthSensor.VideoStreamProfiles.Where(p => p.Stream == Stream.Depth).OrderByDescending(p => p.Framerate).Where(p => p.Width == W && p.Height == H).First();
                depthSensor.Open(sp);
                depthSensor.Start(q);

                int one_meter = (int)(1f / depthSensor.DepthScale);

                var run = true;
                Console.CancelKeyPress += (s, e) =>
                {
                    e.Cancel = true;
                    run = false;
                };

                ushort[] depth = new ushort[sp.Width * sp.Height];
                FileStream fs = new FileStream("G:\\Robotics\\RealSense\\csharp\\1.txt", FileMode.Create);
                StreamWriter sw = new StreamWriter(fs);
                while (run)
                {                   
                    using (var f = q.WaitForFrame() as VideoFrame)
                    {
                        f.CopyTo(depth);
                        foreach(ushort x in depth)
                            sw.Write(x.ToString()+'\t');
                        sw.Flush();
                        //关闭流
                        sw.Close();
                        fs.Close();
                        //for (int i = 0; i < 640; i++)
                        //    for (int j = 0; j < 480; j++)
                        //        bm.SetPixel(1, 1, (Color)(1));
                        //bm.Save("1.png", System.Drawing.Imaging.ImageFormat.Png);
                    }

                    var buffer = new char[(W / 10 + 1) * (H / 20)];
                    var coverage = new int[W/10];
                    int b = 0;
                    for (int y = 0; y < H; ++y)
                    {
                        for (int x = 0; x < W; ++x)
                        {
                            ushort d = depth[x + y * W];
                            if (d > 0 && d < one_meter)
                                ++coverage[x / 10];
                        }

                        if (y % 20 == 19)
                        {
                            for (int i = 0; i < coverage.Length; i++)
                            {
                                int c = coverage[i];
                                buffer[b++] = " .:nhBXWW"[c / 25];
                                coverage[i] = 0;
                            }
                            buffer[b++] = '\n';
                        }
                    }

                    Console.SetCursorPosition(0, 0);
                    Console.WriteLine();
                    Console.Write(buffer);
                }

                depthSensor.Stop();
                depthSensor.Close();
            }

        }
    }
}
