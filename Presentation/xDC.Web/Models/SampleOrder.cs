using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace xDC_Web.Models {
    public class SampleOrder {
        public int OrderID { get; set; }
        public DateTime OrderDate { get; set; }
        public string CustomerID { get; set; }
        public string CustomerName { get; set; }
        public string ShipCountry { get; set; }
        public string ShipCity { get; set; }
    }

    public class InflowFundsModel
    {
        public int Id { get; set; }
        public string FormType { get; set; }
        public List<AmsdInflowFunds> AmsdInflowFunds { get; set; }
    }

    public class AmsdInflowFunds
    {
        public int Id { get; set; }
        public int FormId { get; set; }
        public string FundType { get; set; }
        public string Bank { get; set; }
        public decimal Amount { get; set; }
    }



    public class TradeSettlementModel
    {
        public string currency { get; set; }
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
    }

    public class Trades
    {
        public int Id { get; set; }
        public int FormId { get; set; }
        public string InstrumentType { get; set; }
        public string InstrumentCode { get; set; }
        public string StockCode { get; set; }
        public decimal Maturity { get; set; }
        public decimal Sales { get; set; }
        public decimal Purchase { get; set; }
        public decimal FirstLeg { get; set; }
        public decimal SecondLeg { get; set; }
        public decimal AmountPlus { get; set; }
        public decimal AmountMinus { get; set; }
    }

}
