using System;
using System.Collections.Generic;

namespace xDC_Web.Models
{
    public class TreasuryFormVM
    {
        public int Id { get; set; }
        public long TradeDate { get; set; }
        public string Currency { get; set; }
        public string SubmittedBy { get; set; }
        public string SubmittedDate { get; set; }
        public string Approver { get; set; }
        public string ApprovalNotes { get; set; }

        public List<TreasuryDepositVM> InflowDeposit { get; set; }
        public List<TreasuryDepositVM> OutflowDeposit { get; set; }
        public List<TreasuryMmiVM> InflowMoneyMarket { get; set; }
        public List<TreasuryMmiVM> OutflowMoneyMarket { get; set; }

        public TreasuryFormVM()
        {
            this.InflowDeposit = new List<TreasuryDepositVM>();
            this.OutflowDeposit = new List<TreasuryDepositVM>();
            this.InflowMoneyMarket = new List<TreasuryMmiVM>();
            this.OutflowMoneyMarket = new List<TreasuryMmiVM>();
        }
    }

    public class TreasuryDepositVM
    {
        public int Id { get; set; }
        public string CashflowType { get; set; }
        public string Dealer { get; set; }
        public string Bank { get; set; }
        public DateTime ValueDate { get; set; }
        public DateTime MaturityDate { get; set; }
        public int Tenor { get; set; }
        public double Principal { get; set; }
        public double RatePercent { get; set; }
        public double IntProfitReceivable { get; set; }
        public double PrincipalIntProfitReceivable { get; set; }
        public string AssetType { get; set; }
        public string RepoTag { get; set; }
        public string ContactPerson { get; set; }
        public string Notes { get; set; }

        public string ModifiedBy { get; set; }
        public DateTime ModifiedDate { get; set; }
    }

    public class TreasuryMmiVM
    {
        public int Id { get; set; }
        public string CashflowType { get; set; }
        public string Dealer { get; set; }
        public string Issuer { get; set; }
        public string ProductType { get; set; }
        public string CounterParty { get; set; }
        public DateTime ValueDate { get; set; }
        public DateTime MaturityDate { get; set; }
        public int HoldingDayTenor { get; set; }
        public double Nominal { get; set; }
        public decimal SellPurchaseRateYield { get; set; }
        public double Price { get; set; }
        public double IntDividendReceivable { get; set; }
        public double PurchaseProceeds { get; set; }
        public double Proceeds { get; set; }
        public string CertNoStockCode { get; set; }

        public string ModifiedBy { get; set; }
        public DateTime ModifiedDate { get; set; }
    }
}