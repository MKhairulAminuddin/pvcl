using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using xDC.Infrastructure.Application;

namespace xDC.Services
{
    public class ApplicationLogService
    {
        public void UserAccess(string username, string ip, string client)
        {
            try
            {
                using(var db = new kashflowDBEntities())
                {
                    Audit_UserAccess newLog = new Audit_UserAccess()
                    {
                        UserName = username,
                        ClientAddress = ip,
                        ClientBrowser = client,
                        RecordedDate = DateTime.Now
                    };
                    var item = db.Audit_UserAccess.Add(newLog);
                    db.SaveChanges();
                }
            } 
            catch(Exception ex)
            {
                Logging.Logger.LogError(ex);
            }
        }
    }
}
