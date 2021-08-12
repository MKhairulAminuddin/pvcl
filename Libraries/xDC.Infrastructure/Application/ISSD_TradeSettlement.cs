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
    
    public partial class ISSD_TradeSettlement
    {
        public int Id { get; set; }
        public int FormId { get; set; }
        public string InstrumentType { get; set; }
        public string InstrumentCode { get; set; }
        public string StockCode { get; set; }
        public Nullable<decimal> Maturity { get; set; }
        public Nullable<decimal> Sales { get; set; }
        public Nullable<decimal> Purchase { get; set; }
        public Nullable<decimal> FirstLeg { get; set; }
        public Nullable<decimal> SecondLeg { get; set; }
        public Nullable<decimal> AmountPlus { get; set; }
        public Nullable<decimal> AmountMinus { get; set; }
        public string Remarks { get; set; }
        public string ModifiedBy { get; set; }
        public Nullable<System.DateTime> ModifiedDate { get; set; }
    }
}
