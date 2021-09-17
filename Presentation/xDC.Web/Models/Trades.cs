namespace xDC_Web.Models
{
    public class Trades
    {
        public int Id { get; set; }
        public int FormId { get; set; }
        public string InstrumentType { get; set; }
        public string InstrumentCode { get; set; }
        public string StockCode { get; set; }
        public double Maturity { get; set; }
        public double Sales { get; set; }
        public double Purchase { get; set; }
        public double FirstLeg { get; set; }
        public double SecondLeg { get; set; }
        public double AmountPlus { get; set; }
        public double AmountMinus { get; set; }

        public string Remarks { get; set; }
    }
}