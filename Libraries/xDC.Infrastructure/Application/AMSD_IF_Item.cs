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
    
    public partial class AMSD_IF_Item
    {
        public int Id { get; set; }
        public int FormId { get; set; }
        public string FundType { get; set; }
        public string Bank { get; set; }
        public double Amount { get; set; }
        public Nullable<System.DateTime> LatestSubmission { get; set; }
        public string ModifiedBy { get; set; }
        public Nullable<System.DateTime> ModifiedDate { get; set; }
    }
}
