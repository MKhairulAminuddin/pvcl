using System;
using System.Collections.Generic;
using System.DirectoryServices;
using System.DirectoryServices.AccountManagement;
using System.Linq;
using xDC.Infrastructure.Application;
using xDC.Logging;

namespace xDC_Web.Extension.SchedulerTask
{
    public class SyncActiveDirectory
    {
        public static void Sync()
        {
            try
            {
                Logger.LogInfo("Sync AD Started!");
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
                                        adUser.Title = (string) directoryEntry?.Properties["title"].Value;
                                        adUser.Department = (string) directoryEntry?.Properties["department"].Value;
                                        adUser.Office = (string) directoryEntry?.Properties["physicalDeliveryOfficeName"].Value;
                                        adUser.AdAccountCreated =
                                            (DateTime?) directoryEntry?.Properties["whenCreated"].Value;
                                        adUser.AdAccountChanged =
                                            (DateTime?) directoryEntry?.Properties["whenChanged"].Value;
                                    }

                                    adUserList.Add(adUser);
                                }
                            }
                        }
                    }
                    
                    userPrincipal.UserPrincipalName = "crsadm@mykwp.my";

                    using (var searcher = new PrincipalSearcher(userPrincipal))
                    {
                        foreach (var result in searcher.FindAll())
                        {
                            UserPrincipal x = UserPrincipal.FindByIdentity(context, result.UserPrincipalName);

                            if (x != null)
                            {
                                if (!string.IsNullOrEmpty(x.EmailAddress))
                                {
                                    adUserList.Add(new AspNetActiveDirectoryUsers()
                                    {
                                        DisplayName = x.DisplayName,
                                        TelNo = x.VoiceTelephoneNumber,
                                        Username = x.SamAccountName,
                                        Email = x.UserPrincipalName,
                                        AdType = x.StructuralObjectClass,
                                        DistinguishedName = x.DistinguishedName,
                                        LastBadPasswordAttempt = x.LastBadPasswordAttempt,
                                        LastLogon = x.LastLogon,
                                        LastPasswordSet = x.LastPasswordSet
                                    });
                                }
                            }
                        }
                    }
                }
                
                using (var db = new kashflowDBEntities())
                {
                    if (adUserList.Count > 0)
                    {
                        db.Database.ExecuteSqlCommand("TRUNCATE TABLE [AspNetActiveDirectoryUsers]");
                        db.AspNetActiveDirectoryUsers.AddRange(adUserList);
                        db.SaveChanges();
                        Logger.LogInfo("Sync AD Users Completed!");
                    }
                }

                var adGroupList = new List<AspNetActiveDirectoryGroup>();

                using (var context = new PrincipalContext(ContextType.Domain))
                {
                    UserPrincipal userPrincipal = new UserPrincipal(context);
                    userPrincipal.Enabled = false;
                    userPrincipal.EmailAddress = "*@kwap.gov.my";

                    using (var searcher = new PrincipalSearcher(userPrincipal))
                    {
                        foreach (var result in searcher.FindAll())
                        {
                            UserPrincipal group = UserPrincipal.FindByIdentity(context, result.UserPrincipalName);
                            DirectoryEntry directoryEntry = result.GetUnderlyingObject() as DirectoryEntry;

                            if (group != null)
                            {
                                if (!string.IsNullOrEmpty(group.EmailAddress))
                                {
                                    var adGroup = new AspNetActiveDirectoryGroup()
                                    {
                                        DisplayName = group.DisplayName,
                                        TelNo = group.VoiceTelephoneNumber,
                                        Username = group.SamAccountName,
                                        Email = group.EmailAddress,
                                        AdType = group.StructuralObjectClass,
                                        DistinguishedName = group.DistinguishedName,
                                        LastBadPasswordAttempt = group.LastBadPasswordAttempt,
                                        LastLogon = group.LastLogon,
                                        LastPasswordSet = group.LastPasswordSet
                                    };

                                    if (directoryEntry != null)
                                    {
                                        adGroup.Title = (string)directoryEntry?.Properties["title"].Value;
                                        adGroup.Department = (string)directoryEntry?.Properties["department"].Value;
                                        adGroup.Office = (string)directoryEntry?.Properties["physicalDeliveryOfficeName"].Value;
                                        adGroup.AdAccountCreated =
                                            (DateTime?)directoryEntry?.Properties["whenCreated"].Value;
                                        adGroup.AdAccountChanged =
                                            (DateTime?)directoryEntry?.Properties["whenChanged"].Value;
                                    }

                                    adGroupList.Add(adGroup);
                                }
                            }
                        }
                    }
                }

                using (var db = new kashflowDBEntities())
                {
                    if (adGroupList.Count > 0)
                    {
                        db.Database.ExecuteSqlCommand("TRUNCATE TABLE [AspNetActiveDirectoryGroup]");
                        db.AspNetActiveDirectoryGroup.AddRange(adGroupList);
                        db.SaveChanges();
                        Logger.LogInfo("Sync AD Group Completed!");
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.LogError(ex);
            }

        }

        public static void SyncUserProfileWithAd()
        {
            try
            {
                Logger.LogInfo("Sync User Profile with AD Started!");

                using (var db = new kashflowDBEntities())
                {
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

                    Logger.LogInfo("Sync User Profile with AD Completed!");
                }
            }
            catch (Exception ex)
            {
                Logger.LogError(ex);
            }
        }
    }
}