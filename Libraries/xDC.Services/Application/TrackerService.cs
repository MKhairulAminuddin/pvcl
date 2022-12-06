using System;
using xDC.Infrastructure.Application;
using xDC.Logging;

namespace xDC.Services.Application
{
    public class TrackerService : ITrackerService
    {
        

        public void TrackUserLogin(string username, string ipadress, string browser)
        {
            try
            {
                using (var db = new kashflowDBEntities())
                {
                    var newItem = new Audit_UserAccess()
                    {
                        UserName = username,
                        ClientAddress = ipadress,
                        ClientBrowser = browser,
                        RecordedDate = DateTime.Now
                    };
                    var result = db.Audit_UserAccess.Add(newItem);
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
