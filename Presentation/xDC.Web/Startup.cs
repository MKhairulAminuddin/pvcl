using Hangfire;
using Hangfire.SqlServer;
using Microsoft.Owin;
using Owin;
using SimpleInjector;
using System;
using System.Collections.Generic;
using System.Web.Mvc;
using xDC.Logging;
using xDC.Services;
using xDC.Services.Application;
using xDC.Services.Audit;
using xDC.Services.FileGenerator;
using xDC.Services.Form;
using xDC.Services.Membership;
using xDC.Services.Notification;
using xDC.Services.Workflow;
using xDC.TaskScheduler;
using xDC.Utils;
using xDC_Web;

[assembly: OwinStartup(typeof(Startup))]
namespace xDC_Web
{
    public partial class Startup
    {
        public static Container Container;
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);

            // DI
            var container = new Container();
            container.Register<ISettingService, SettingService>();
            container.Register<IAuditService, AuditService>();
            container.Register<IWorkflowService, WorkflowService>();
            container.Register<ITrackerService, TrackerService>();

            container.Register<IIfFormService, IfFormService>();
            container.Register<ITsFormService, TsFormService>();
            container.Register<ITreasuryFormService, TreasuryFormService>();
            container.Register<IFcaTaggingFormService, FcaTaggingFormService>();

            container.Register<IGenFile_IfForm, GenFile_IfForm>();
            container.Register<IGenFile_TreasuryForm, GenFile_TreasuryForm>();
            container.Register<IGenFile_TsForm, GenFile_TsForm>();
            container.Register<IGenFile_10amDealCutOffReport, GenFile_10amDealCutOffReport>();
            container.Register<IGenFile_DealCutOffFcyReport, GenFile_DealCutOffFcyReport>();
            container.Register<IGenFile_DealCutOffMyrReport, GenFile_DealCutOffMyrReport>();

            container.Register<IRoleManagementService, RoleManagementService>();
            container.Register<IUserManagementService, UserManagementService>();


            container.Register<INotificationService, NotificationService>();
            container.Register<IEmailNotification, EmailNotification>();
            container.Register<IInAppNotification, InAppNotification>();
            Container = container;
            container.Verify();

            // Hangfire Setup
            app.UseHangfireAspNet(GetHangfireServers);
            app.UseHangfireDashboard();

            #if DEBUG
                        Console.WriteLine("Mode=Debug");
            #else
                RegisterTaskScheduler();
            #endif


        }

        private IEnumerable<IDisposable> GetHangfireServers()
        {
            GlobalConfiguration.Configuration
                .SetDataCompatibilityLevel(CompatibilityLevel.Version_170)
                .UseSimpleAssemblyNameTypeSerializer()
                .UseRecommendedSerializerSettings()
                .UseSqlServerStorage(Config.DbCon, new SqlServerStorageOptions
                {
                    CommandBatchMaxTimeout = TimeSpan.FromMinutes(5),
                    SlidingInvisibilityTimeout = TimeSpan.FromMinutes(5),
                    QueuePollInterval = TimeSpan.Zero,
                    UseRecommendedIsolationLevel = true,
                    DisableGlobalLocks = true
                });

            yield return new BackgroundJobServer();
        }
        
        private void RegisterTaskScheduler()
        {
            RecurringJob.AddOrUpdate("Sync AD", () => new xDcTask().SyncKwapAdData(), Cron.Daily, TimeZoneInfo.Local);
            RecurringJob.AddOrUpdate("Sync User Profile with AD", () => new xDcTask().SyncUserProfileWithAdData(), Cron.Weekly, TimeZoneInfo.Local);

            RecurringJob.AddOrUpdate("ISSD TS - Currency", () => new xDcTask().TsForm_FetchNewCurrency(), Cron.Daily, TimeZoneInfo.Local);
            RecurringJob.AddOrUpdate("FID Treasury - Asset Type", () => new xDcTask().TForm_FetchAssetType(), Cron.Daily, TimeZoneInfo.Local);

            //RecurringJob.AddOrUpdate("[Notification] FCA Tagging to ISSD", () => new xDcTask().NotifyIssd_OnFcaTagged(), Cron.Minutely, TimeZoneInfo.Local);
        }
    }
}
