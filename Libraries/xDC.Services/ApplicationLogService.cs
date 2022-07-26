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
                    Log_UserAccess newLog = new Log_UserAccess()
                    {
                        UserName = username,
                        ClientAddress = ip,
                        ClientBrowser = client,
                        RecordedDate = DateTime.Now
                    };
                    var item = db.Log_UserAccess.Add(newLog);
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
