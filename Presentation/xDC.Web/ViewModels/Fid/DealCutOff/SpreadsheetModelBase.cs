using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace xDC_Web.ViewModels.Fid.DealCutOff
{
    public class SpreadsheetModelBase
    {
        public SpreadsheetModelBase()
        {
            PreviewModel = new SpreadsheetPreviewModel();
            PreviewModel.OwnerPropertyName = "PreviewModel";
        }
        public int ExportToDocumentType { get; set; }
        public SpreadsheetPreviewModel PreviewModel { get; internal set; }
    }
}