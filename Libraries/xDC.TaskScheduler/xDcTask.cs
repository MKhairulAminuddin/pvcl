using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using xDC.Infrastructure.Application;
using xDC.Logging;
using xDC.Utils;
using System.DirectoryServices;
using System.DirectoryServices.AccountManagement;
using xDC.Services.Notification;
using System.IO;

namespace xDC.TaskScheduler
{
    public class xDcTask : IxDcTask
    {

        #region ISSD Related Task

        public void TsForm_FetchNewCurrency()
        {
            try
            {
                Logger.LogInfo("Scheduler - TsForm_FetchNewCurrency started");

                using (var db = new kashflowDBEntities())
                {
                    var currencyFromEdw = db.EDW_TradeItem.Select(x => x.Currency).Distinct().ToList();

                    foreach (var cdw in currencyFromEdw)
                    {
                        if (!db.Config_Dropdown.Any(x => x.Key == Common.DropdownConfigKey.ISSD_TS_Currency && x.Value == cdw))
                        {
                            var newCurrency = new Config_Dropdown
                            {
                                Key = Common.DropdownConfigKey.ISSD_TS_Currency,
                                Value = cdw,
                                CreatedBy = "System",
                                CreatedDate = DateTime.Now,
                            };
                            db.Config_Dropdown.Add(newCurrency);
                        }
                    }
                    db.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                Logger.LogError(ex.Message);
            }
            finally
            {
                Logger.LogInfo("Scheduler - TsForm_FetchNewCurrency ended");
            }
        }

        public void NotifyIssd_OnFcaTagged()
        {
            try
            {
                Logger.LogInfo("Scheduler - NotifyIssd_OnFcaTagged Started!");

                new EmailNotification().OnFcaTagging();
            }
            catch (Exception ex)
            {
                Logger.LogError(ex.Message);
            }
            finally
            {
                Logger.LogInfo("Scheduler - NotifyIssd_OnFcaTagged ended");
            }
        }

        #endregion

        #region FID Related Task

        public void TForm_FetchAssetType()
        {
            try
            {
                Logger.LogInfo("Scheduler - TForm_FetchAssetType started");

                using (var db = new kashflowDBEntities())
                {
                    var assetTypeFromEdw = db.EDW_Maturity_Deposit.Select(x => x.AssetType).Distinct().ToList();

                    foreach (var assetType in assetTypeFromEdw)
                    {
                        if (!db.Config_Dropdown.Any(x => x.Key == Common.DropdownConfigKey.FID_Treasury_AssetType && x.Value == assetType))
                        {
                            var newAssetType = new Config_Dropdown
                            {
                                Key = Common.DropdownConfigKey.FID_Treasury_AssetType,
                                Value = assetType,
                                CreatedBy = "System",
                                CreatedDate = DateTime.Now,
                            };
                            db.Config_Dropdown.Add(newAssetType);
                        }
                    }
                    db.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                Logger.LogError(ex.Message);
            }
            finally
            {
                Logger.LogInfo("Scheduler - TForm_FetchAssetType ended");
            }
        }


        #endregion

        #region Application Specific

        public void SyncKwapAdData()
        {
            try
            {
                Logger.LogInfo("Scheduler - SyncKwapAdData started");

                var adUserList = new List<AspNetActiveDirectoryUsers>();

                using (var context = new PrincipalContext(ContextType.Domain))
                {
                    UserPrincipal userPrincipal = new UserPrincipal(context);
                    userPrincipal.Enabled = true;
                    userPrincipal.EmailAddress = "*@kwap.gov.my";

                    using (var searcher = new PrincipalSearcher(userPrincipal))
                    {
                        foreach (var result in searcher.FindAll())
                        {
                            UserPrincipal user = UserPrincipal.FindByIdentity(context, result.UserPrincipalName);
                            DirectoryEntry directoryEntry = result.GetUnderlyingObject() as DirectoryEntry;

                            if (user != null)
                            {
                                if (!string.IsNullOrEmpty(user.EmailAddress))
                                {
                                    var adUser = new AspNetActiveDirectoryUsers()
                                    {
                                        DisplayName = user.DisplayName,
                                        TelNo = user.VoiceTelephoneNumber,
                                        Username = user.SamAccountName,
                                        Email = user.EmailAddress,
                                        AdType = user.StructuralObjectClass,
                                        DistinguishedName = user.DistinguishedName,
                                        LastBadPasswordAttempt = user.LastBadPasswordAttempt,
                                        LastLogon = user.LastLogon,
                                        LastPasswordSet = user.LastPasswordSet
                                    };

                                    if (directoryEntry != null)
                                    {
                                        adUser.Title = (string)directoryEntry?.Properties["title"].Value;
                                        adUser.Department = (string)directoryEntry?.Properties["department"].Value;
                                        adUser.Office = (string)directoryEntry?.Properties["physicalDeliveryOfficeName"].Value;
                                        adUser.AdAccountCreated =
                                            (DateTime?)directoryEntry?.Properties["whenCreated"].Value;
                                        adUser.AdAccountChanged =
                                            (DateTime?)directoryEntry?.Properties["whenChanged"].Value;
                                    }

                                    adUserList.Add(adUser);
                                }
                            }
                        }
                    }
                }

                using (var db = new kashflowDBEntities())
                {
                    if (adUserList.Count > 0)
                    {
                        // truncate existing AD accounts in database and refill with latest one
                        db.Database.ExecuteSqlCommand("TRUNCATE TABLE [AspNetActiveDirectoryUsers]");
                        db.AspNetActiveDirectoryUsers.AddRange(adUserList);
                        db.SaveChanges();

                        // update user profile
                        var applicationUsers = db.AspNetUsers.ToList();
                        foreach (var appUser in applicationUsers)
                        {
                            var adInformation = db.AspNetActiveDirectoryUsers.FirstOrDefault(x => x.Username == appUser.UserName);
                            if (adInformation != null)
                            {
                                appUser.Email = adInformation.Email;
                                appUser.Department = adInformation.Department;
                                appUser.Title = adInformation.Title;
                                appUser.FullName = adInformation.DisplayName;
                                appUser.TelephoneNumber = adInformation.TelNo;
                            }
                        }
                        db.SaveChanges();


                        // update user profile
                        var approverDetails = db.Config_Approver.ToList();
                        foreach (var approver in approverDetails)
                        {
                            var adInformation = db.AspNetActiveDirectoryUsers.FirstOrDefault(x => x.Username == approver.Username);
                            if (adInformation != null)
                            {
                                approver.Email = adInformation.Email;
                                approver.Department = adInformation.Department;
                                approver.Title = adInformation.Title;
                            }
                        }
                        db.SaveChanges();


                    }
                }
            }
            catch (Exception ex)
            {
                Logger.LogError(ex.Message);
            }
            finally
            {
                Logger.LogInfo("Scheduler - SyncKwapAdData ended");
            }

        }

        public void ClearTempFolder()
        {
            try
            {
                var tempFolder = Path.Combine(Config.TempFolderPath);

                if (Directory.Exists(tempFolder))
                {
                    Directory.Delete(tempFolder, true);
                }
            }
            catch (Exception ex)
            {
                Logger.LogError(ex);
            }

        }

        #endregion


    }
}
