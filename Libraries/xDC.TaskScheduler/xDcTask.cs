using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using xDC.Infrastructure.Application;
using xDC.Logging;
using xDC.Utils;

namespace xDC.TaskScheduler
{
    public class xDcTask : IxDcTask
    {
        private readonly IXDcLogger _logger;

        public xDcTask(IXDcLogger logger)
        {
            _logger = logger;
        }

        public void FetchNewCurrency()
        {
            try
            {
                _logger.LogInfo("Scheduler - FetchNewCurrency started");

                using (var db = new kashflowDBEntities())
                {
                    var currencyFromEdw = db.EDW_TradeItem.Select(x => x.Currency).Distinct().ToList();

                    foreach (var cdw in currencyFromEdw)
                    {
                        if (!db.Config_Dropdown.Any(x => x.Key == Common.DropdownConfigKey.ISSD_TS_Currency && x.Value == cdw))
                        {
                            var newCurrency = new Config_Dropdown
                            {
                                Key = Common.DropdownConfigKey.ISSD_TS_Currency,
                                Value = cdw,
                                CreatedBy = "System",
                                CreatedDate = DateTime.Now,
                            };
                            db.Config_Dropdown.Add(newCurrency);
                        }
                    }
                    db.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
            }
            finally
            {
                _logger.LogInfo("Scheduler - FetchNewCurrency ended");
            }
        }
    }
}
