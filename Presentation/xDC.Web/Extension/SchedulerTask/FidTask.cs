using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using xDC.Infrastructure.Application;
using xDC.Logging;
using xDC.Services.App;
using xDC.Utils;

namespace xDC_Web.Extension.SchedulerTask
{
    public class FidTask
    {
        public static void FetchAssetType()
        {
            try
            {
                Logger.LogInfo("Fetch Asset Type Started!");

                using (var db = new kashflowDBEntities())
                {
                    var assetTypeFromEdw = db.EDW_Maturity_Deposit.Select(x => x.Asset_Type).Distinct().ToList();

                    foreach (var assetType in assetTypeFromEdw)
                    {
                        if (!db.Config_Dropdown.Any(x => x.Key == Common.DropdownConfigKey.FID_Treasury_AssetType && x.Value == assetType))
                        {
                            var newAssetType = new Config_Dropdown
                            {
                                Key = Common.DropdownConfigKey.FID_Treasury_AssetType,
                                Value = assetType,
                                CreatedBy = "System",
                                CreatedDate = DateTime.Now,
                            };
                            db.Config_Dropdown.Add(newAssetType);
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
                Logger.LogInfo("Fetch Asset Type Completed!");
            }
        }
    }
}