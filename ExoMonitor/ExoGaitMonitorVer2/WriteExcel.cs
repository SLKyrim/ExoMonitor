using System;
using Excel = Microsoft.Office.Interop.Excel;
using System.Windows.Controls.Primitives;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;

namespace ExoGaitMonitorVer2
{
    class WriteExcel
    {
        #region 声明

        private StatusBar statusBar;
        private TextBlock statusInfoTextBlock;
        private Motors motors = new Motors();
        private DispatcherTimer timer;

        //Excel settings
        Excel.Application ExcelApp = null;
        Excel.Workbooks ExcelWorkbooks = null;
        Excel.Workbook ExcelWorkbook = null;
        Excel.Worksheet ExcelWorksheet = null;
        object misValue = System.Reflection.Missing.Value;
        int excelCnt = 3;
        private const int PARA_NUM = 1; //每个电机写入参数个数

        #endregion

        public bool writeStart(StatusBar statusBarIn, TextBlock statusInfoTextBlockIn, Motors motorsIn)
        {
            statusBar = statusBarIn;
            statusInfoTextBlock = statusInfoTextBlockIn;
            motors = motorsIn;

            ExcelApp = new Excel.Application();
            if (ExcelApp == null)
            {
                statusBar.Background = new SolidColorBrush(Color.FromArgb(255, 230, 20, 20));
                statusInfoTextBlock.Text = "该计算机可能未安装Excel！";
                return false;
            }

            ExcelApp.Visible = true;
            ExcelApp.Application.DisplayAlerts = false;
            ExcelWorkbooks = ExcelApp.Workbooks;
            try
            {
                ExcelWorkbook = ExcelWorkbooks.Add(misValue);
                ExcelWorksheet = (Excel.Worksheet)ExcelWorkbook.Worksheets["Sheet1"];
                ExcelWorksheet.Range[ExcelWorksheet.Cells[1, 1], ExcelWorksheet.Cells[1, PARA_NUM]].Merge();
                ExcelWorksheet.Range[ExcelWorksheet.Cells[1, 1], ExcelWorksheet.Cells[1, PARA_NUM]].Interior.ColorIndex = 39;
                ExcelWorksheet.Cells[1, 1] = "电机1左膝";
                ExcelWorksheet.Cells[2, 1] = "关节角度";

                ExcelWorksheet.Range[ExcelWorksheet.Cells[1, PARA_NUM + 1], ExcelWorksheet.Cells[1, 2 * PARA_NUM]].Merge();
                ExcelWorksheet.Range[ExcelWorksheet.Cells[1, PARA_NUM + 1], ExcelWorksheet.Cells[1, 2 * PARA_NUM]].Interior.ColorIndex = 35;
                ExcelWorksheet.Cells[1, PARA_NUM + 1] = "电机2左髋";
                ExcelWorksheet.Cells[2, PARA_NUM + 1] = "关节角度";

                ExcelWorksheet.Range[ExcelWorksheet.Cells[1, 2 * PARA_NUM + 1], ExcelWorksheet.Cells[1, 3 * PARA_NUM]].Merge();
                ExcelWorksheet.Range[ExcelWorksheet.Cells[1, 2 * PARA_NUM + 1], ExcelWorksheet.Cells[1, 3 * PARA_NUM]].Interior.ColorIndex = 37;
                ExcelWorksheet.Cells[1, 2 * PARA_NUM + 1] = "电机3右髋";
                ExcelWorksheet.Cells[2, 2 * PARA_NUM + 1] = "关节角度";

                ExcelWorksheet.Range[ExcelWorksheet.Cells[1, 3 * PARA_NUM + 1], ExcelWorksheet.Cells[1, 4 * PARA_NUM]].Merge();
                ExcelWorksheet.Range[ExcelWorksheet.Cells[1, 3 * PARA_NUM + 1], ExcelWorksheet.Cells[1, 4 * PARA_NUM]].Interior.ColorIndex = 36;
                ExcelWorksheet.Cells[1, 3 * PARA_NUM + 1] = "电机4右膝";
                ExcelWorksheet.Cells[2, 3 * PARA_NUM + 1] = "关节角度";

                excelCnt = 3;

                timer = new DispatcherTimer();
                timer.Interval = TimeSpan.FromMilliseconds(10);
                timer.Tick += new EventHandler(writeExcel_Tick);
                timer.Start();
            }
            catch
            {
                statusBar.Background = new SolidColorBrush(Color.FromArgb(255, 230, 20, 20));
                statusInfoTextBlock.Text = "打开文件时发生错误！";
                writeStop();
                return false;
            }
            return true;
        }

        public bool writeStop()
        {
            try
            {
                if (ExcelWorkbook != null)
                {
                    ExcelWorkbook.SaveAs(@"C:\Users\Administrator\Desktop\GaitData.xlsx"); //默认保存路径为C:\Users\Administrator\Documents

                    timer.Stop();
                    timer.Tick -= new EventHandler(writeExcel_Tick);
                }
            }
            catch
            {
                statusBar.Background = new SolidColorBrush(Color.FromArgb(255, 230, 20, 20));
                statusInfoTextBlock.Text = "保存失败！";
                return false;
            }

            if (ExcelApp != null)
            {
                if (ExcelWorkbooks != null)
                {
                    if (ExcelWorkbook != null)
                    {
                        if (ExcelWorksheet != null)
                        {
                            System.Runtime.InteropServices.Marshal.ReleaseComObject(ExcelWorksheet);
                            ExcelWorksheet = null;
                        }
                        System.Runtime.InteropServices.Marshal.ReleaseComObject(ExcelWorkbook);
                        ExcelWorkbook = null;
                    }
                    System.Runtime.InteropServices.Marshal.ReleaseComObject(ExcelWorkbooks);
                    ExcelWorkbooks = null;
                }
                ExcelApp.Application.Workbooks.Close();
                ExcelApp.Quit();
                System.Runtime.InteropServices.Marshal.ReleaseComObject(ExcelApp);
                ExcelApp = null;
            }
            return true;
        }

        private void writeExcel_Tick(object sender, EventArgs e)
        {
            ExcelWorksheet.Cells[excelCnt, 1] = motors.ampObjAngleActual[0];
            ExcelWorksheet.Range[ExcelWorksheet.Cells[excelCnt, 1], ExcelWorksheet.Cells[excelCnt, PARA_NUM]].Interior.ColorIndex = 39;

            ExcelWorksheet.Cells[excelCnt, PARA_NUM + 1] = motors.ampObjAngleActual[1];
            ExcelWorksheet.Range[ExcelWorksheet.Cells[excelCnt, PARA_NUM + 1], ExcelWorksheet.Cells[excelCnt, 2 * PARA_NUM]].Interior.ColorIndex = 35;

            ExcelWorksheet.Cells[excelCnt, 2 * PARA_NUM + 1] = motors.ampObjAngleActual[2];
            ExcelWorksheet.Range[ExcelWorksheet.Cells[excelCnt, 2 * PARA_NUM + 1], ExcelWorksheet.Cells[excelCnt, 3 * PARA_NUM]].Interior.ColorIndex = 37;

            ExcelWorksheet.Cells[excelCnt, 3 * PARA_NUM + 1] = motors.ampObjAngleActual[3];
            ExcelWorksheet.Range[ExcelWorksheet.Cells[excelCnt, 3 * PARA_NUM + 1], ExcelWorksheet.Cells[excelCnt, 4 * PARA_NUM]].Interior.ColorIndex = 36;

            excelCnt++;
        }
    }
}
