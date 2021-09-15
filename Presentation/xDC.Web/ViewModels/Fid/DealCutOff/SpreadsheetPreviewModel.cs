using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using DevExpress.Spreadsheet;

namespace xDC_Web.ViewModels.Fid.DealCutOff
{
    public class SpreadsheetPreviewModel
    {
        public SpreadsheetPreviewModel()
        {
            HeightInPixels = 1200;
            CanShowOnlyFirstWorksheet = true;
        }
        public string OwnerPropertyName { get; set; }
        public bool CanShowOnlyFirstWorksheet { get; set; }
        public string PreviewDocumentAction { get; set; }
        public int PreviewSheetIndex { get; set; }
        public int HeightInPixels { get; set; }
        public IWorkbook Workbook { get; set; }
    }
}