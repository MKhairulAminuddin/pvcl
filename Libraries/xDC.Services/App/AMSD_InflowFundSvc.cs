using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using xDC.Infrastructure.Application;
using xDC.Utils;

namespace xDC.Services.App
{
    public static class AMSD_InflowFundSvc
    {
        public static bool EnableEdit(string formStatus, string formApprover, string currentUser)
        {
            var isPendingApproval = formStatus == Common.FormStatus.PendingApproval;
            var isFormApprover = formApprover == currentUser;

            return !isPendingApproval || !isFormApprover;
        }

        public static bool EnableDelete(string formStatus)
        {
            var isPendingApproval = formStatus == Common.FormStatus.PendingApproval;

            return !isPendingApproval;
        }

        public static bool EnablePrint(string formStatus)
        {
            var isDraftForm = formStatus == Common.FormStatus.Draft;

            return !isDraftForm;
        }
    }
}
