using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace xDC_Web.ViewModels.Iisd
{
    public class EDW_TradeItemVM
    {
        public int Id { get; set; }
        public string InstrumentType { get; set; }
        public string InstrumentName { get; set; }
        public string StockCode { get; set; }
        public string ISIN { get; set; }
        public string Type { get; set; }
        public double Amount { get; set; }
        public Nullable<System.DateTime> TradeDate { get; set; }
        public Nullable<System.DateTime> SettlementDate { get; set; }
        public string Currency { get; set; }
        public string CreatedBy { get; set; }
        public System.DateTime CreatedDate { get; set; }
        public string UpdatedBy { get; set; }
        public Nullable<System.DateTime> UpdatedDate { get; set; }
    }
}