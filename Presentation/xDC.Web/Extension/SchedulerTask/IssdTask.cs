﻿using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using xDC.Infrastructure.Application;
using xDC.Logging;
using xDC.Utils;

namespace xDC_Web.Extension.SchedulerTask
{
    public class IssdTask
    {
        public static void FetchNewCurrency()
        {
            try
            {
                Logger.LogInfo("Fetch New Currency Started!");

                using (var db = new kashflowDBEntities())
                {
                    var currencyFromEdw = db.EDW_TradeItem.Select(x => x.Currency).Distinct().ToList();
                    var currencyConfigKey = Common.DropdownConfigKeyMapping(3);


                    foreach (var cdw in currencyFromEdw)
                    {
                        if (!db.Config_Dropdown.Any(x => x.Key == currencyConfigKey && x.Value == cdw ))
                        {
                            var newCurrency = new Config_Dropdown
                            {
                                Key = currencyConfigKey,
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
                Logger.LogError(ex);
            }
            finally
            {
                Logger.LogInfo("Fetch New Currency Completed!");
            }
        }
    }
}