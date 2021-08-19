using Hangfire;
using Hangfire.SqlServer;
using Microsoft.Owin;
using Owin;
using System;
using System.Collections.Generic;
using xDC.Utils;
using xDC_Web;
using xDC_Web.Extension.SchedulerTask;

[assembly: OwinStartup(typeof(Startup))]
namespace xDC_Web
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);

            // Hangfire Setup
            app.UseHangfireAspNet(GetHangfireServers);
            app.UseHangfireDashboard();
            
            RecurringJob.AddOrUpdate(
                () => SyncActiveDirectory.Sync(),
                Cron.Weekly, TimeZoneInfo.Local);
            RecurringJob.AddOrUpdate(
                () => SyncActiveDirectory.SyncUserProfileWithAd(),
                Cron.Monthly, TimeZoneInfo.Local);
            RecurringJob.AddOrUpdate(
                () => FidTask.FetchToday10AmTradeSettlement(),
                "0 10 * * *", TimeZoneInfo.Local);
            RecurringJob.AddOrUpdate(
                () => FidTask.Fetch10AmTradeSettlementOnDemand(),
                "0 0 31 2 0", TimeZoneInfo.Local);
            RecurringJob.AddOrUpdate(
                () => IssdTask.FetchNewCurrency(),
                Cron.Daily, TimeZoneInfo.Local);

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
        
    }
}
