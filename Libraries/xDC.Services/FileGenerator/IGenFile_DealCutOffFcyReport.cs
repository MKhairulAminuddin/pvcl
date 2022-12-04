using DevExpress.Spreadsheet;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace xDC.Services.FileGenerator
{
    public interface IGenFile_DealCutOffFcyReport
    {
        IWorkbook GenerateWorkbook(DateTime? selectedDate, bool viewApproved);
        string GenerateFile(DateTime selectedDate, bool isExportAsExcel, bool viewApproved);
        FileStream GenFile(string generatedFileName);
    }
}
