using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using xDC.Domain.Web.Application;

namespace xDC.Services.FileGenerator
{
    public interface IGenFile_TreasuryForm
    {
        string GenId_TreasuryForm(int formId, string currentUser, bool isExportAsExcel);
        ExportedFile GenFile(string generatedFileName, bool isGenCnEmailTemplate = false);
    }
}
