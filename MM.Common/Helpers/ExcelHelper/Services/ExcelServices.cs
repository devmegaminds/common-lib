using MM.Common.Helpers.ExcelHelper.IServices;
using MM.Common.Helpers.ExcelHelper.Models;
using NPOI.HSSF.UserModel;
using NPOI.SS.UserModel;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Text;

namespace MM.Common.Helpers.ExcelHelper.Services
{
    public class ExcelServices : IExcelServices
    {
        public ExportExcelResponse ExportXlsFromDataSet(DataSet ds)
        {
            return ExportXlsFromDataSet(ds, string.Empty);
        }
        public ExportExcelResponse ExportXlsFromDataSet(DataSet ds, string fileName)
        {
            ExportExcelResponse exportXlsFromDataSetResponse = new ExportExcelResponse();

            MemoryStream memoryStream = new MemoryStream();
            IWorkbook workbook = new HSSFWorkbook();

            foreach (DataTable dt in ds.Tables)
            {
                ISheet sheet = workbook.CreateSheet(dt.TableName);

                var row0 = sheet.CreateRow(0);//header
                for (int j = 0; j < dt.Columns.Count; j++)
                {
                    row0.CreateCell(j).SetCellValue(dt.Columns[j].ColumnName);
                }

                for (int i = 0; i < dt.Rows.Count; i++)//rest
                {
                    var row = sheet.CreateRow(1 + i);
                    for (int j = 0; j < dt.Columns.Count; j++)
                        row.CreateCell(j).SetCellValue(dt.Rows[i][j].ToString());
                }
            }

            workbook.Write(memoryStream);
            memoryStream.Flush();
            memoryStream.Position = 0;
            exportXlsFromDataSetResponse.FileStream = memoryStream;

            if (string.IsNullOrEmpty(fileName))
            {
                string dateString = string.Format("{0:MMddyyyy}", DateTime.UtcNow);
                string defaultFileName = string.Format("rpt_{0}_{1}.xls", ds.DataSetName, dateString);
                exportXlsFromDataSetResponse.FileName = defaultFileName;
            }
            return exportXlsFromDataSetResponse;
        }
    }
}
