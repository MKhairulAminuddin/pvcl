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
    
    public partial class Audit_10AMDCO_ClosingBalance
    {
        public int Id { get; set; }
        public Nullable<System.DateTime> ReportDate { get; set; }
        public string Currency { get; set; }
        public string Account { get; set; }
        public double ValueBefore { get; set; }
        public double ValueAfter { get; set; }
        public string Operation { get; set; }
        public string ModifiedBy { get; set; }
        public Nullable<System.DateTime> ModifiedOn { get; set; }
    }
}
