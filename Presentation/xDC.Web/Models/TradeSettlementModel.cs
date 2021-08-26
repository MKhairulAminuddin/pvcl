using System;
using System.Collections.Generic;

namespace xDC_Web.Models
{
    public class TradeSettlementModel
    {
        public int Id { get; set; }
        public int FormType { get; set; }
        public long SettlementDateEpoch { get; set; }
        public string Approver { get; set; }
        public string ApprovalNotes { get; set; }
        public string Currency { get; set; }
        public bool IsSaveAsDraft { get; set; }
        public bool IsSaveAdminEdit { get; set; }

        public List<BankBalance> OpeningBalance { get; set; }

        public List<Trades> Equity { get; set; }
        public List<Trades> Bond { get; set; }
        public List<Trades> Cp { get; set; }
        public List<Trades> NotesPaper { get; set; }
        public List<Trades> Repo { get; set; }
        public List<Trades> Coupon { get; set; }
        public List<Trades> Fees { get; set; }
        public List<Trades> Mtm { get; set; }
        public List<Trades> FxSettlement { get; set; }
        public List<Trades> ContributionCredited { get; set; }
        public List<Trades> Altid { get; set; }
        public List<Trades> Others { get; set; }
        
        public TradeSettlementModel()
        {
            this.Equity = new List<Trades>();
            this.Bond = new List<Trades>();
            this.Cp = new List<Trades>();
            this.NotesPaper = new List<Trades>();
            this.Repo = new List<Trades>();
            this.Coupon = new List<Trades>();
            this.Fees = new List<Trades>();
            this.Mtm = new List<Trades>();
            this.FxSettlement = new List<Trades>();
            this.ContributionCredited = new List<Trades>();
            this.Altid = new List<Trades>();
            this.Others = new List<Trades>();

            this.OpeningBalance = new List<BankBalance>();
        }
    }

    public class TreasuryFormModel
    {
        public int Id { get; set; }
        public long TradeDate { get; set; }
        public string Currency { get; set; }
        public string SubmittedBy { get; set; }
        public string SubmittedDate { get; set; }

        public List<TreasuryDeposit> InflowDeposit { get; set; }
        public List<TreasuryDeposit> OutflowDeposit { get; set; }
        public List<TreasuryItems> InflowMoneyMarket { get; set; }
        public List<TreasuryItems> OutflowMoneyMarket { get; set; }
    }

    public class TreasuryDeposit
    {
        public int Id { get; set; }
        public string CashflowType { get; set; }
        public string Dealer { get; set; }
        public string Bank { get; set; }
        public DateTime ValueDate { get; set; }
        public DateTime MaturityDate { get; set; }
        public int Tenor { get; set; }
        public decimal RatePercent { get; set; }
        public decimal IntProfitReceivable { get; set; }
        public decimal PrincipalIntProfitReceivable { get; set; }
        public string AssetType { get; set; }
        public string RepoTag { get; set; }
        public string ContactPerson { get; set; }
        public string Notes { get; set; }

        public string ModifiedBy { get; set; }
        public DateTime ModifiedDate { get; set; }
    }

    public class TreasuryItems
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
        public decimal Nominal { get; set; }
        public decimal SellPurchaseRateYield { get; set; }
        public decimal Price { get; set; }
        public decimal IntDividendReceivable { get; set; }
        public decimal Proceeds { get; set; }
        public decimal CertNoStockCode { get; set; }

        public string ModifiedBy { get; set; }
        public DateTime ModifiedDate { get; set; }
    }
}