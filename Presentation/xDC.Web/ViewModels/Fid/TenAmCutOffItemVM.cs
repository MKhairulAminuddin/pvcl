namespace xDC_Web.ViewModels.Fid
{
    public class TenAmCutOffItemVM
    {
        public string Currency { get; set; }
        public string Account { get; set; }
        public decimal OpeningBalance { get; set; }
        public decimal TotalInflow { get; set; }
        public decimal TotalOutflow { get; set; }
        public decimal Net { get; set; }
    }
}