using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace xDC.Domain.Web.FID.TreasuryForm
{
    public class TForm
    {
        public int Id { get; set; }
        public long ValueDate { get; set; }
        public string Currency { get; set; }
        public string SubmittedBy { get; set; }
        public string SubmittedDate { get; set; }
        public string Approver { get; set; }
        public string ApprovalNotes { get; set; }

        public List<TDeposit> InflowDeposit { get; set; }
        public List<TDeposit> OutflowDeposit { get; set; }
        public List<TMmi> InflowMoneyMarket { get; set; }
        public List<TMmi> OutflowMoneyMarket { get; set; }

        public TForm()
        {
            this.InflowDeposit = new List<TDeposit>();
            this.OutflowDeposit = new List<TDeposit>();
            this.InflowMoneyMarket = new List<TMmi>();
            this.OutflowMoneyMarket = new List<TMmi>();
        }
    }
}
