using System;
using System.Collections.Generic;
using System.DirectoryServices;
using System.DirectoryServices.AccountManagement;
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
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.LogError(ex);
            }

        }
    }
}