using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace xDC.Services.FileGenerator
{
    public interface IGenFile_TsForm
    {
        string GenId_TsForm(int formId, string currentUser, bool isExportAsExcel);
        string GenId_ConsolidatedTsForm(DateTime settlementDate, string currency, string currentUser, bool isExportAsExcel);
        FileStream GenFile(string generatedFilename);

        string Gen_CnEmailFile(int formId, string senderEmail);

    }
}
