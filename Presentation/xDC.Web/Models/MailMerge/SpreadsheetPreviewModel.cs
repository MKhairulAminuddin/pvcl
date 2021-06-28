using System;
using DevExpress.Spreadsheet;

namespace xDC_Web.Models.MailMerge
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