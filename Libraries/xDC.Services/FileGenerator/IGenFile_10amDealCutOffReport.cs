using DevExpress.Spreadsheet;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using xDC.Domain.Web.Application;

namespace xDC.Services.FileGenerator
{
    public interface IGenFile_10amDealCutOffReport
    {
        IWorkbook GenerateWorkbook(DateTime selectedDate, bool viewApproved);
        string GenerateFile(DateTime selectedDate, bool isExportAsExcel, bool viewApproved);
        ExportedFile GenFile(string generatedFileName, bool isGenCnEmailTemplate = false);
    }
}
