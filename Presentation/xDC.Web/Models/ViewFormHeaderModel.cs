using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace xDC_Web.Models
{
    public class AmsdInflowFundGridModel
    {
        public int Id { get; set; }
        public string FormType { get; set; }
        public string FormStatus { get; set; }
        public DateTime? FormDate { get; set; }
        public string Currency { get; set; }
        public string PreparedBy { get; set; }
        public DateTime? PreparedDate { get; set; }
        public string ApprovedBy { get; set; }
        public DateTime? ApprovedDate { get; set; }


        public bool EnableEdit { get; set; }
        public bool EnableDelete { get; set; }
        public bool EnablePrint { get; set; }
        public bool EnableRetractSubmission { get; set; }

        public bool IsPendingMyApproval { get; set; }
        public bool IsRejected { get; set; }

    }
    
    public class ViewTradeSettlementFormHeaderModel
    {
        public int Id { get; set; }
        public string FormType { get; set; }
        public string FormStatus { get; set; }
        public Nullable<System.DateTime> SettlementDate { get; set; }
        public string Currency { get; set; }
        public string PreparedBy { get; set; }
        public Nullable<System.DateTime> PreparedDate { get; set; }
        public string ApprovedBy { get; set; }
        public Nullable<System.DateTime> ApprovedDate { get; set; }
        public bool AdminEditted { get; set; }
        public string AdminEdittedBy { get; set; }
        public Nullable<System.DateTime> AdminEdittedDate { get; set; }




        public bool IsFormOwner { get; set; }
        public bool IsDraft { get; set; }

        public bool IsMeCanEditDraft { get; set; }
        public bool IsCanAdminEdit { get; set; }
        public bool IsResubmitEnabled { get; set; }

        public bool IsFormPendingMyApproval { get; set; }
        public bool IsMyFormRejected { get; set; }
    }
}