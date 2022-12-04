using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using xDC.Infrastructure.Application;
using xDC.Logging;

namespace xDC.Services.Application
{
    public class TrackerService : ITrackerService
    {
        private readonly IXDcLogger _logger;

        public TrackerService(IXDcLogger logger)
        {
            _logger = logger;
        }

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
                _logger.LogError(ex);
            }
        }
    }
}
