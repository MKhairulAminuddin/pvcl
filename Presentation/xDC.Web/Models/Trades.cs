namespace xDC_Web.Models
{
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