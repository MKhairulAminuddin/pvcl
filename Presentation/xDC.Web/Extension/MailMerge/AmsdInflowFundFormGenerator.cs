using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Web;
using DevExpress.Spreadsheet;
using xDC_Web.Models.MailMerge;

namespace xDC_Web.Extension.MailMerge
{
    public class AmsdInflowFundFormGenerator
    {
        IWorkbook workbook;

        public AmsdInflowFundFormGenerator()
        {
            IWorkbook workbook = new Workbook();
            workbook.Options.Culture = new CultureInfo("en-US");
            workbook.LoadDocument(MapPath("~/App_Data/AMSD - Inflow Funds Template.xltx"));

            this.workbook = workbook;
        }

        #region Properties
        Worksheet Sheet { get { return workbook.Worksheets[0]; } }
        #endregion

        public IWorkbook GenerateDocument(AmsdInflowFundFormPreviewModel model)
        {
            workbook.BeginUpdate();
            try
            {
                GenerateHeaderRange(model);
                AdjustPrintOptions();
            }
            finally
            {
                workbook.EndUpdate();
            }

            return workbook;
        }

        void GenerateHeaderRange(AmsdInflowFundFormPreviewModel model)
        {
            Sheet["E2"].Value = model.Preparer;
            Sheet["E3"].Value = model.PreparedDate;

            int startingRownumber = 11;
            foreach (var item in model.InflowFunds)
            {
                Sheet["A" + startingRownumber].Value = item.FundType;
                Sheet["B" + startingRownumber].Value = item.Bank;
                Sheet["C" + startingRownumber].Value = item.Amount;

                ++startingRownumber;
            }
            Sheet["A" + startingRownumber + ":B" + startingRownumber].Merge();
            Sheet["A" + startingRownumber].Value = "Total";
            Sheet["A" + startingRownumber].Font.Bold = true;

            Sheet["C11:C" + startingRownumber].NumberFormat =
                "_(#,##0.00_);_((#,##0.00);_(\" - \"??_);_(@_)";

            Sheet["C" + startingRownumber].FormulaInvariant = "=SUM($C$11:$C$" + (startingRownumber - 1) + ")";

            Sheet["A" + startingRownumber + ":C" + startingRownumber].Borders.TopBorder.Color = Color.Black;
            Sheet["A" + startingRownumber + ":C" + startingRownumber].Borders.TopBorder.LineStyle = BorderLineStyle.Thick;


            workbook.Calculate();
        }



        void AdjustPrintOptions()
        {
            Sheet.SetPrintRange(Sheet.GetDataRange());
            Sheet.PrintOptions.FitToPage = true;
            Sheet.PrintOptions.FitToWidth = 1;
            Sheet.PrintOptions.FitToHeight = 0; // automatic
        }

        protected internal string MapPath(string path)
        {
            return System.Web.HttpContext.Current.Request.MapPath(path);
        }
    }
}