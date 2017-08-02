using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace test
{
    class Program
    {
        static void Main(string[] args)
        {
            string str1 = "1.01";
            string str2 = "2.02";
            string str3 = "3.03";
            string str4 = "4.04";
            string str5 = "5.05";
 
            StreamWriter sw = new StreamWriter("names.txt", true); //实现不覆盖写入文本

            for (int i = 0; i < 5; i++)
            {
                sw.WriteLine(str1 + '\t' + str2 + '\t' + str3 + '\t' + str4 + '\t' + str5 + '\t');
            }
            sw.Close();  

            
        }
    }
}
