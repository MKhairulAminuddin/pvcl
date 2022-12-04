using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading;
using System.Web;
using xDC.Infrastructure.Application;
using xDC.Logging;
using xDC.Services;
using xDC.Services.Form;
using xDC.Utils;

namespace xDC_Web.Extension.SchedulerTask
{
    public class NotiTask
    {
        public static void FcaTag()
        {
            try
            {
                //Logger.LogInfo("FCA Tagging Notification Started!");

                using (var db = new kashflowDBEntities())
                {
                    var dateAssigned = DateTime.Now.AddMinutes(-1);

                    var isSent = db.App_Notification.FirstOrDefault(x =>
                        DbFunctions.DiffMinutes(x.CreatedOn, dateAssigned) == 0
                        && DbFunctions.DiffHours(x.CreatedOn, dateAssigned) == 0
                        && DbFunctions.TruncateTime(x.CreatedOn) == dateAssigned.Date
                        && x.NotificationType == "FcaTaggingToIssd");

                    if (isSent == null)
                    {
                        db.App_Notification.Add(new App_Notification
                        {
                            UserId = "System",
                            NotificationType = "FcaTaggingToIssd",
                            CreatedOn = dateAssigned
                        });
                        db.SaveChanges();
                        
                        var issdTaggedItems = db.ISSD_TradeSettlement.Where(x => x.AssignedDate >= dateAssigned).ToList();

                        if (issdTaggedItems.Any())
                        {
                            // TODO: fix this
                            //EmailNotificationService.FcaBankTaggingToIssd(issdTaggedItems);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
            }
            finally
            {
            }
        }
    }
}