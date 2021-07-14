namespace xDC_Web.Models
{
    public class AmsdInflowFunds
    {
        public int Id { get; set; }
        public int FormId { get; set; }
        public string FundType { get; set; }
        public string Bank { get; set; }
        public decimal Amount { get; set; }
    }
}