using OfficeOpenXml;
using Pipe_Stress_App_V2._5.Common.Models;
using Pipe_Stress_App_V2._5.Extensions;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pipe_Stress_App_V2._5.Common.DataHandlers
{
    public class DataSaveHandler
    {
        public DataSaveHandler()
        {

        }
        public static bool CreateAndSave(DirectoryInfo dir, List<DetData> data)
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            using (var package = new ExcelPackage())
            {


                DataTable dt = GetDataTable(data);
                if (dt.Rows.Count == 0)
                {
                    package.Dispose();
                    return true;
                }

                //Add a new worksheet to the empty workbook
                var worksheet = package.Workbook.Worksheets.Add("Sheet1");

                worksheet.Cells[1, 1].LoadFromDataTable(dt, true);

                worksheet.Cells[2, 26, dt.Rows.Count + 1, 26].Style.Numberformat.Format = "yyyy/MM/dd  HH:mm:ss";

                worksheet.Cells.AutoFitColumns(0);  //Autofit columns for all cells，This will take twice as long

                // Set some document properties
                package.Workbook.Properties.Title = "Experimental Data";
                package.Workbook.Properties.Author = "ying yun";

                // Set some extended property values
                package.Workbook.Properties.Company = "China University of Petroleum,Beijing";



                package.Workbook.Properties.Application = "Microsoft Excel";

                FileOutputUtil.OutputDir = dir;

                string fileName = DateTime.Now.ToString("yyyy年MM月dd日HH时mm分ss秒") + ".xlsx";
                var xlFile = FileOutputUtil.GetFileInfo(fileName);

                // Save our new workbook in the output directory and we are done!
                package.SaveAs(xlFile);
            }
            //GC.Collect();
            return true;

        }
        private static DataTable GetDataTable(List<DetData> data)
        {
            DataTable dt = new DataTable();
            dt.Columns.Add("ID", typeof(int));
            for (int i = 0; i < 24; i++)
            {
                dt.Columns.Add("第" + (i + 1) + "点", typeof(double));
            }
            dt.Columns.Add("测量时间", typeof(DateTime));


            for (int i = 0; i < data.Count; i++)
            {
                var row = dt.NewRow();
                row["ID"] = i + 1;

                for (int j = 0; j < 24; j++)
                {
                    row["第" + (j + 1) + "点"] = data[i].StrainList[j];
                }
                row["测量时间"] = data[i].Time;

                dt.Rows.Add(row);
            }


            return dt;
        }
        public static List<DetData> Read(FileInfo existingFile)
        {
            List<DetData> readData = new List<DetData>();

            Console.WriteLine("开始时间：{0}", DateTime.Now);
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            using (ExcelPackage package = new ExcelPackage(existingFile))
            {

                ExcelWorksheet worksheet = package.Workbook.Worksheets[0];
                int rowNum = worksheet.Dimension.Rows;

                for (int i = 1; i < rowNum; i++)
                {

                    var ts = worksheet.Cells[i + 1, 2, i + 1, 25].ToList().ConvertAll(d => Convert.ToDouble(d.Value));
                    var date = DateTime.ParseExact(worksheet.Cells[i + 1, 26].Text, "yyyy/MM/dd  HH:mm:ss", System.Globalization.CultureInfo.CurrentCulture);

                    readData.Add(new DetData
                    {
                        StrainList = ts,
                        Time = date
                    });
                }
            }
            //GC.Collect();
            Console.WriteLine("结束时间：{0}", DateTime.Now);
            return readData;
        }


    }
}
