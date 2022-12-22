using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using xDC.Domain.Web.Application;

namespace xDC.Services.FileGenerator
{
    public interface IGenFile_TsForm
    {
        string GenId_TsForm(int formId, string currentUser, bool isExportAsExcel);
        string GenId_ConsolidatedTsForm(DateTime settlementDate, string currency, string currentUser, bool isExportAsExcel);
        ExportedFile GenFile(string generatedFilename, bool isGenCnEmailTemplate = false);

        string Gen_CnEmailFile(int formId, string senderEmail);

    }
}
