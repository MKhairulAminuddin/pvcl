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
    
    public partial class FID_Treasury_Deposit
    {
        public int Id { get; set; }
        public int FormId { get; set; }
        public string CashflowType { get; set; }
        public string Dealer { get; set; }
        public string Bank { get; set; }
        public Nullable<System.DateTime> ValueDate { get; set; }
        public Nullable<System.DateTime> MaturityDate { get; set; }
        public Nullable<int> Tenor { get; set; }
        public double Principal { get; set; }
        public double RatePercent { get; set; }
        public double IntProfitReceivable { get; set; }
        public double PrincipalIntProfitReceivable { get; set; }
        public string AssetType { get; set; }
        public string RepoTag { get; set; }
        public string ContactPerson { get; set; }
        public string Notes { get; set; }
        public string FcaAccount { get; set; }
        public string ModifiedBy { get; set; }
        public Nullable<System.DateTime> ModifiedDate { get; set; }
    }
}
