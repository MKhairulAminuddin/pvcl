using System;
using xDC.Utils;

namespace xDC_Web.ViewModels.Fid.DealCutOff
{
    public class DealCutOffMyrTemplateModel : SpreadsheetModelBase
    {
        public DealCutOffMyrTemplateModel()
        {
            PreviewModel.PreviewDocumentAction = "DealCutOffMyrPreview";
            TradeDate = Common.ConvertEpochToDateTime(SelectedDate);
        }
        public long SelectedDate { get; set; }
        public DateTime? TradeDate { get; set; }
    }
}