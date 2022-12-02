using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace xDC.Domain.Web.FID.TreasuryForm
{
    public class TMmi
    {
        public int Id { get; set; }
        public string CashflowType { get; set; }
        public string Dealer { get; set; }
        public string Issuer { get; set; }
        public string ProductType { get; set; }
        public string CounterParty { get; set; }
        public DateTime TradeDate { get; set; }
        public DateTime ValueDate { get; set; }
        public DateTime MaturityDate { get; set; }
        public int HoldingDayTenor { get; set; }
        public double Nominal { get; set; }
        public double SellPurchaseRateYield { get; set; }
        public double Price { get; set; }
        public double IntDividendReceivable { get; set; }
        public double PurchaseProceeds { get; set; }
        public double Proceeds { get; set; }
        public string CertNoStockCode { get; set; }
        public string FcaAccount { get; set; }

        public string ModifiedBy { get; set; }
        public DateTime ModifiedDate { get; set; }
    }
}
