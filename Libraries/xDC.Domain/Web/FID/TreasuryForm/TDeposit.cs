using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace xDC.Domain.Web.FID.TreasuryForm
{
    public class TDeposit
    {
        public int Id { get; set; }
        public string CashflowType { get; set; }
        public string Dealer { get; set; }
        public string Bank { get; set; }
        public DateTime TradeDate { get; set; }
        public DateTime ValueDate { get; set; }
        public DateTime MaturityDate { get; set; }
        public int Tenor { get; set; }
        public double Principal { get; set; }
        public double RatePercent { get; set; }
        public double IntProfitReceivable { get; set; }
        public double PrincipalIntProfitReceivable { get; set; }
        public bool ManualCalc_P_Plus_I { get; set; }
        public string AssetType { get; set; }
        public string RepoTag { get; set; }
        public string ContactPerson { get; set; }
        public string Notes { get; set; }
        public string FcaAccount { get; set; }

        public string ModifiedBy { get; set; }
        public DateTime ModifiedDate { get; set; }
    }
}
