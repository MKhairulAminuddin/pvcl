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
        string GenId_TsForm(int formId, bool isExportAsExcel);
        string GenId_ConsolidatedTsForm(DateTime settlementDate, string currency, bool isExportAsExcel);
        FileStream GenFile(string generatedFilename);

    }
}
