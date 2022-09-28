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

            #if DEBUG
                        Console.WriteLine("Mode=Debug");
            #else
                RecurringJob.AddOrUpdate("Sync AD", () => SyncActiveDirectory.Sync(),Cron.Weekly, TimeZoneInfo.Local);
                RecurringJob.AddOrUpdate("Sync User Profile with AD", () => SyncActiveDirectory.SyncUserProfileWithAd(), Cron.Monthly, TimeZoneInfo.Local);
            
                RecurringJob.AddOrUpdate("ISSD TS - Currency", () => IssdTask.FetchNewCurrency(), Cron.Daily, TimeZoneInfo.Local);
                RecurringJob.AddOrUpdate("FID Treasury - Asset Type", () => FidTask.FetchAssetType(), Cron.Daily, TimeZoneInfo.Local);

                RecurringJob.AddOrUpdate("[Notification] FCA Tagging to ISSD", () => NotiTask.FcaTag(), Cron.Minutely, TimeZoneInfo.Local);
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
        
    }
}
