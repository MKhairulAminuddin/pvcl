﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using xDC.Infrastructure.Application;
using xDC.Logging;

namespace xDC.Services
{
    public static class TrackerService
    {
        public static void TrackUserLogin(string username, string ipadress, string browser)
        {
            try
            {
                using (var db = new kashflowDBEntities())
                {
                    var newItem = new Log_UserAccess()
                    {
                        UserName = username,
                        ClientAddress = ipadress,
                        ClientBrowser = browser,
                        RecordedDate = DateTime.Now
                    };
                    var result = db.Log_UserAccess.Add(newItem);
                    db.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                Logger.LogError(ex);
            }
        }
    }
}