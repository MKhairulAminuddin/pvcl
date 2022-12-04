using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using DevExpress.Spreadsheet;
using DevExpress.XtraSpreadsheet.Export;

namespace xDC_Web.Extension.MailMerge
{
    public class HtmlContentGenerator
    {
        Stream stream;
        public HtmlContentGenerator(Stream stream)
        {
            this.stream = stream;
        }
        public void Generate(IWorkbook workbook, int sheetIndex)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                HtmlDocumentExporterOptions options = new HtmlDocumentExporterOptions
                {
                    SheetIndex = sheetIndex,
                    EmbedImages = true,
                    AnchorImagesToPage = true
                };
                workbook.ExportToHtml(ms, options);
                ms.Seek(0, SeekOrigin.Begin);
                ms.CopyTo(this.stream);
            }
        }
    }
}