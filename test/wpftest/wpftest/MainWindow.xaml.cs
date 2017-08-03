using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
using System.IO.Ports;
using System.IO;
using System.Windows.Threading;
using System.Xml.Serialization;
using System.Diagnostics;

namespace wpftest
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private DispatcherTimer ShowCurTimeTimer;

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            ShowCurTimeTimer = new DispatcherTimer();
            ShowCurTimeTimer.Tick += new EventHandler(testTimer);
            ShowCurTimeTimer.Interval = TimeSpan.FromMilliseconds(100);
            ShowCurTimeTimer.Start();
        }


        
        int count = 0;
        string str = "fuck";
        public void testTimer(object sender, EventArgs e)
        {
            StreamWriter toText = new StreamWriter("data.txt", true);

            if (count < 699)
            {
                toText.WriteLine(str + '\t' + str);
            }

            toText.Close();

            count++;
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            
        }
    }
}
