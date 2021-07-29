using System;
using System.Collections.Generic;

namespace xDC_Web.Models
{
    public class TradeSettlementModel
    {
        public int Id { get; set; }
        public string FormType { get; set; }
        public long SettlementDateEpoch { get; set; }
        public string Approver { get; set; }
        public string Currency { get; set; }

        public decimal RentasOpeningBalance { get; set; }
        public decimal MmaOpeningBalance { get; set; }

        public decimal RentasClosingBalance { get; set; }
        public decimal MmaClosingBalance { get; set; }


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

        public bool IsSaveAsDraft { get; set; }

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
        }
    }
}