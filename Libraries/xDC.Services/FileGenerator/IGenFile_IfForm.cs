using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace xDC.Services.FileGenerator
{
    public interface IGenFile_IfForm
    {
        string GenId_IfForm(int formId, string currentUser, bool isExportAsExcel);
        FileStream GenFile(string generatedFileName);
    }
}
