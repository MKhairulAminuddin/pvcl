namespace xDC_Web.ViewModels.Fid
{
    public class TenAmCutOffItemVM
    {
        public string Id { get; set; }
        public string Currency { get; set; }
        public string Account { get; set; }
        public string AccountX { get; set; }
        public double OpeningBalance { get; set; }
        public double TotalInflow { get; set; }
        public double TotalOutflow { get; set; }
        public double Net { get; set; }

        public double ClosingBalance { get; set; }
    }
}