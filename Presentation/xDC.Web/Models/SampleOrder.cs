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
        public string FormType { get; set; }
        public List<AmsdInflowFunds> AmsdInflowFunds { get; set; }
    }

    public class AmsdInflowFunds
    {
        public string FundType { get; set; }
        public string Bank { get; set; }
        public decimal Amount { get; set; }
    }
}
