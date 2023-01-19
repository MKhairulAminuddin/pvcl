using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace xDC.TaskScheduler
{
    public interface IxDcTask
    {
        void TsForm_FetchNewCurrency();
        void NotifyIssd_OnFcaTagged();

        void TForm_FetchAssetType();

        void SyncKwapAdData();
    }
}
