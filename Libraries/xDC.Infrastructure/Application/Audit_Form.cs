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
    
    public partial class Audit_Form
    {
        public int Id { get; set; }
        public int FormId { get; set; }
        public string FormType { get; set; }
        public Nullable<System.DateTime> FormDate { get; set; }
        public string ActionType { get; set; }
        public string Remarks { get; set; }
        public string ValueBefore { get; set; }
        public string ValueAfter { get; set; }
        public string ModifiedBy { get; set; }
        public Nullable<System.DateTime> ModifiedOn { get; set; }
    }
}
