using System;

namespace xDC_Web.ViewModels.Fid.Treasury
{
    public class TreasuryDepositGridVm
    {
        public int Id { get; set; }
        public string Dealer { get; set; }
        public string Bank { get; set; }
        public DateTime ValueDate { get; set; }
        public DateTime MaturityDate { get; set; }
        public double Principal { get; set; }
        public int Tenor { get; set; }
        public double RatePercent { get; set; }
        public double IntProfitReceivable { get; set; }
        public double PrincipalIntProfitReceivable { get; set; }
        public string AssetType { get; set; }
        public string RepoTag { get; set; }
        public string ContactPerson { get; set; }
        public string Notes { get; set; }

    }
}