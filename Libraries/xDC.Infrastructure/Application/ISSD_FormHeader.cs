//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace xDC.Infrastructure.Application
{
    using System;
    using System.Collections.Generic;
    
    public partial class ISSD_FormHeader
    {
        public int Id { get; set; }
        public string FormType { get; set; }
        public string FormStatus { get; set; }
        public string Currency { get; set; }
        public Nullable<System.DateTime> SettlementDate { get; set; }
        public string PreparedBy { get; set; }
        public Nullable<System.DateTime> PreparedDate { get; set; }
        public string ApprovedBy { get; set; }
        public Nullable<System.DateTime> ApprovedDate { get; set; }
        public bool AdminEditted { get; set; }
        public string AdminEdittedBy { get; set; }
        public Nullable<System.DateTime> AdminEdittedDate { get; set; }
    }
}
