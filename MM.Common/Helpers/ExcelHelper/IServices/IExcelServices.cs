using MM.Common.Helpers.ExcelHelper.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace MM.Common.Helpers.ExcelHelper.IServices
{
    public interface IExcelServices
    {
        ExportExcelResponse ExportXlsFromDataSet(DataSet ds);
        ExportExcelResponse ExportXlsFromDataSet(DataSet ds, string fileName);
    }
}
