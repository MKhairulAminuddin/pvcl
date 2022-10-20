using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace xDC.Domain.WebApi.Forms.TradeSettlement
{
    public class TsCreateNewFormRequest
    {
        public int Id { get; set; }
        public int FormType { get; set; }
        public long SettlementDateEpoch { get; set; }
        public string Approver { get; set; }
        public string ApprovalNotes { get; set; }
        public string Currency { get; set; }
        public bool IsSaveAsDraft { get; set; }
        public bool IsSaveAdminEdit { get; set; }

        public List<TsTradeItem> Equity { get; set; }
        public List<TsTradeItem> Bond { get; set; }
        public List<TsTradeItem> Cp { get; set; }
        public List<TsTradeItem> NotesPaper { get; set; }
        public List<TsTradeItem> Repo { get; set; }
        public List<TsTradeItem> Coupon { get; set; }
        public List<TsTradeItem> Fees { get; set; }
        public List<TsTradeItem> Mtm { get; set; }
        public List<TsTradeItem> FxSettlement { get; set; }
        public List<TsTradeItem> ContributionCredited { get; set; }
        public List<TsTradeItem> Altid { get; set; }
        public List<TsTradeItem> Others { get; set; }

        public TsCreateNewFormRequest()
        {
            this.Equity = new List<TsTradeItem>();
            this.Bond = new List<TsTradeItem>();
            this.Cp = new List<TsTradeItem>();
            this.NotesPaper = new List<TsTradeItem>();
            this.Repo = new List<TsTradeItem>();
            this.Coupon = new List<TsTradeItem>();
            this.Fees = new List<TsTradeItem>();
            this.Mtm = new List<TsTradeItem>();
            this.FxSettlement = new List<TsTradeItem>();
            this.ContributionCredited = new List<TsTradeItem>();
            this.Altid = new List<TsTradeItem>();
            this.Others = new List<TsTradeItem>();
        }
    }
}
