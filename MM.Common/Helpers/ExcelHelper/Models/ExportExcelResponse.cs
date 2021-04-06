using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace MM.Common.Helpers.ExcelHelper.Models
{
   public class ExportExcelResponse
    {
        public Stream FileStream { get; set; }
        public string FileName { get; set; }
    }
}
