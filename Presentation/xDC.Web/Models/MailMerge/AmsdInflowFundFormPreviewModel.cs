using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace xDC_Web.Models.MailMerge
{
    public class AmsdInflowFundFormPreviewModel : SpreadsheetModelBase
    {
        public AmsdInflowFundFormPreviewModel()
        {
            PreviewModel.PreviewDocumentAction = "SpreadsheetMailMergePreview";
        }

        public int Id { get; set; }
        public string Preparer { get; set; }
        public DateTime PreparedDate { get; set; }

        public string Approver { get; set; }
        public DateTime ApprovedDate { get; set; }

        public string FormStatus { get; set; }

        public List<AmsdInflowFundItems> InflowFunds { get; set; }
    }

    public class AmsdInflowFundItems
    {
        public int Id { get; set; }
        public string FundType { get; set; }
        public string Bank { get; set; }
        public decimal Amount { get; set; }
    }
}