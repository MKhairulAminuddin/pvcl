using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using DevExtreme.AspNet.Data;
using DevExtreme.AspNet.Mvc;
using xDC.Infrastructure.Application;
using xDC.Logging;
using xDC_Web.ViewModels.User;

namespace xDC_Web.Controllers.Api
{
    [Authorize]
    [RoutePrefix("api/user")]
    public class UserController : ApiController
    {
        [HttpGet]
        public HttpResponseMessage Profile(DataSourceLoadOptions loadOptions)
        {
            try
            {
                using (var db = new kashflowDBEntities())
                {
                    var currentUsername = User.Identity.Name;

                    db.Configuration.LazyLoadingEnabled = false;
                    db.Configuration.ProxyCreationEnabled = false;
                    var userGroup = db.AspNetUsers.Include(x => x.AspNetRoles).Where(x => x.UserName == currentUsername).Select(y => new Profile()
                    {
                        Username = y.UserName,
                        DisplayName = y.FullName,
                        Email = y.Email,
                        TelNo = y.TelephoneNumber,
                        Title = y.Title,
                        Department = y.Department,
                        KashflowUserGroup = y.AspNetRoles.Select(z => z.Name).FirstOrDefault(),
                        KashflowApprover = db.Config_Approver.Any(z => z.Username == currentUsername)
                    }).ToList();

                    var result = new List<Profile>();
                    result.AddRange(userGroup);


                    return Request.CreateResponse(DataSourceLoader.Load(result, loadOptions));
                }
            }
            catch (Exception ex)
            {
                Logger.LogError(ex);
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }
    }
}
