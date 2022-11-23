using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Web.Http;
using DevExtreme.AspNet.Data;
using DevExtreme.AspNet.Mvc;
using Newtonsoft.Json;
using xDC.Infrastructure.Application;
using xDC.Logging;
using xDC.Services.Form;
using xDC.Utils;
using xDC_Web.Extension.CustomAttribute;
using xDC_Web.ViewModels.Fid;
using xDC_Web.ViewModels.TenAmCutOff;

namespace xDC_Web.Controllers.Api
{
    [KflowApiAuthorize(Common.PermissionKey.Report_DCO_10am)]
    [RoutePrefix("api/TenAmDealCutOff")]
    public class TenAmDealCutOffController : ApiController
    {
        [HttpGet]
        [Route("Summary/{reportDate}/{viewType}")]
        public HttpResponseMessage SummaryReport(long reportDate, string viewType, DataSourceLoadOptions loadOptions)
        {
            try
            {
                var reportDateParsed = Common.ConvertEpochToDateTime(reportDate);
                reportDateParsed = reportDateParsed.Value.Date;

                bool viewApprovedOnly = (viewType.ToLower() == Common.FormStatus.Approved.ToLower());

                using (var db = new kashflowDBEntities())
                {
                    var result = new List<TenAmCutOffItemVM>();
                    var resultRaw = new List<TenAmCutOffItemVM>();

                    var configAccount = db.Config_FcaBankAccount.Where(x => x.Currency != "MYR").ToList();
                    configAccount.Add(new Config_FcaBankAccount
                    {
                        AccountName1 = "RENTAS",
                        AccountName2 = "RENTAS",
                        AccountName3 = "RENTAS",
                        Currency = "MYR"
                    });
                    configAccount.Add(new Config_FcaBankAccount
                    {
                        AccountName1 = "MMA",
                        AccountName2 = "MMA",
                        AccountName3 = "MMA",
                        Currency = "MYR"
                    });

                    #region 1 - ISSD Trade Settlement TRX

                    foreach (var account in configAccount)
                    {
                        var item = new TenAmCutOffItemVM
                        {
                            Account = account.AccountName1,
                            Currency = account.Currency
                        };
                        result.Add(item);

                        var tsFormIds = (viewApprovedOnly)
                            ? db.ISSD_FormHeader
                                .Where(x => x.FormStatus == Common.FormStatus.Approved
                                            && x.SettlementDate != null
                                            && x.Currency == item.Currency
                                            && DbFunctions.TruncateTime(x.SettlementDate) == DbFunctions.TruncateTime(reportDateParsed))
                                .Select(x => x.Id)
                                .ToList()
                            : db.ISSD_FormHeader
                                .Where(x => x.FormStatus != Common.FormStatus.Rejected
                                            && x.SettlementDate != null
                                            && x.Currency == item.Currency
                                            && DbFunctions.TruncateTime(x.SettlementDate) ==
                                            DbFunctions.TruncateTime(reportDateParsed))
                                .Select(x => x.Id)
                                .ToList();

                        if (tsFormIds.Any())
                        {
                            var tradeItemInflow = db.ISSD_TradeSettlement
                                .Where(x => tsFormIds.Contains(x.FormId)
                                            && x.InflowTo != null
                                            && x.InflowTo == account.AccountName3
                                            && x.InflowAmount > 0)
                                .Select(l => l.InflowAmount)
                                .DefaultIfEmpty(0)
                                .Sum();

                            item.TotalInflow = tradeItemInflow;

                            // get total inflow based on assigned outflow account
                            var tradeItemOutflow = db.ISSD_TradeSettlement
                                .Where(x => tsFormIds.Contains(x.FormId)
                                            && x.OutflowFrom != null
                                            && x.OutflowFrom == account.AccountName3
                                            && x.OutflowAmount > 0)
                                .Select(l => l.OutflowAmount)
                                .DefaultIfEmpty(0)
                                .Sum();

                            item.TotalOutflow = tradeItemOutflow;
                        }

                        resultRaw.Add(item);
                    }

                    #endregion

                    #region 2 - FID Treasury TRX (MYR)

                    var fidTreasuryMyr = new TenAmCutOffItemVM
                    {
                        Account = "RENTAS",
                        Currency = "MYR"
                    };

                    var treasuryMyrFormIds = (viewApprovedOnly)
                        ? db.FID_Treasury
                            .Where(x => x.FormStatus == Common.FormStatus.Approved
                                        && x.ValueDate != null
                                        && x.Currency == "MYR"
                                        && DbFunctions.TruncateTime(x.ValueDate) ==
                                        DbFunctions.TruncateTime(reportDateParsed))
                            .Select(x => x.Id)
                            .ToList()
                        : db.FID_Treasury
                            .Where(x => x.FormStatus != Common.FormStatus.Rejected
                                        && x.ValueDate != null
                                        && x.Currency == "MYR"
                                        && DbFunctions.TruncateTime(x.ValueDate) ==
                                        DbFunctions.TruncateTime(reportDateParsed))
                            .Select(x => x.Id)
                            .ToList();

                    if (treasuryMyrFormIds.Any())
                    {
                        var inflowDeposit = db.FID_Treasury_Deposit
                            .Where(x => treasuryMyrFormIds.Contains(x.FormId)
                                        && x.PrincipalIntProfitReceivable > 0
                                        && x.CashflowType == Common.Cashflow.Inflow)
                            .Select(l => l.PrincipalIntProfitReceivable)
                            .DefaultIfEmpty(0)
                            .Sum();

                        var inflowMmi = db.FID_Treasury_MMI
                            .Where(x => treasuryMyrFormIds.Contains(x.FormId)
                                        && x.CashflowType == Common.Cashflow.Inflow
                                        && x.Proceeds > 0)
                            .Select(l => l.Proceeds)
                            .DefaultIfEmpty(0)
                            .Sum();

                        var outflowDeposit = db.FID_Treasury_Deposit
                            .Where(x => treasuryMyrFormIds.Contains(x.FormId)
                                        && x.Principal > 0
                                        && x.CashflowType == Common.Cashflow.Outflow)
                            .Select(l => l.Principal)
                            .DefaultIfEmpty(0)
                            .Sum();

                        var outflowMmi = db.FID_Treasury_MMI
                            .Where(x => treasuryMyrFormIds.Contains(x.FormId)
                                        && x.CashflowType == Common.Cashflow.Outflow
                                        && x.Proceeds > 0)
                            .Select(l => l.Proceeds)
                            .DefaultIfEmpty(0)
                            .Sum();

                        fidTreasuryMyr.TotalInflow += inflowDeposit;
                        fidTreasuryMyr.TotalInflow += inflowMmi;
                        fidTreasuryMyr.TotalOutflow += outflowDeposit;
                        fidTreasuryMyr.TotalOutflow += outflowMmi;
                    }

                    resultRaw.Add(fidTreasuryMyr);

                    #endregion

                    #region 3 - FID Treasury TRX (FCY)

                    foreach (var account in configAccount.Where(x => x.Currency != "MYR"))
                    {
                        var fidTreasuryFcy = new TenAmCutOffItemVM
                        {
                            Account = account.AccountName1,
                            Currency = account.Currency
                        };

                        var treasuryFcyFormIds = (viewApprovedOnly)
                            ? db.FID_Treasury
                                .Where(x => x.FormStatus == Common.FormStatus.Approved
                                            && x.ValueDate != null
                                            && x.Currency == account.Currency
                                            && DbFunctions.TruncateTime(x.ValueDate) ==
                                            DbFunctions.TruncateTime(reportDateParsed))
                                .Select(x => x.Id)
                                .ToList()
                            : db.FID_Treasury
                                .Where(x => x.FormStatus != Common.FormStatus.Rejected
                                            && x.ValueDate != null
                                            && x.Currency == account.Currency
                                            && DbFunctions.TruncateTime(x.ValueDate) ==
                                            DbFunctions.TruncateTime(reportDateParsed))
                                .Select(x => x.Id)
                                .ToList();

                        if (treasuryFcyFormIds.Any())
                        {
                            var inflowDeposit = db.FID_Treasury_Deposit
                                .Where(x => treasuryFcyFormIds.Contains(x.FormId)
                                            && x.PrincipalIntProfitReceivable > 0
                                            && x.CashflowType == Common.Cashflow.Inflow
                                            && x.FcaAccount == account.AccountName3)
                                .Select(l => l.PrincipalIntProfitReceivable)
                                .DefaultIfEmpty(0)
                                .Sum();

                            var inflowMmi = db.FID_Treasury_MMI
                                .Where(x => treasuryFcyFormIds.Contains(x.FormId)
                                            && x.CashflowType == Common.Cashflow.Inflow
                                            && x.Proceeds > 0
                                            && x.FcaAccount == account.AccountName3)
                                .Select(l => l.Proceeds)
                                .DefaultIfEmpty(0)
                                .Sum();

                            var outflowDeposit = db.FID_Treasury_Deposit
                                .Where(x => treasuryFcyFormIds.Contains(x.FormId)
                                            && x.Principal > 0
                                            && x.CashflowType == Common.Cashflow.Outflow
                                            && x.FcaAccount == account.AccountName3)
                                .Select(l => l.Principal)
                                .DefaultIfEmpty(0)
                                .Sum();

                            var outflowMmi = db.FID_Treasury_MMI
                                .Where(x => treasuryFcyFormIds.Contains(x.FormId)
                                            && x.CashflowType == Common.Cashflow.Outflow
                                            && x.Proceeds > 0
                                            && x.FcaAccount == account.AccountName3)
                                .Select(l => l.Proceeds)
                                .DefaultIfEmpty(0)
                                .Sum();

                            fidTreasuryFcy.TotalInflow += inflowDeposit;
                            fidTreasuryFcy.TotalInflow += inflowMmi;
                            fidTreasuryFcy.TotalOutflow += outflowDeposit;
                            fidTreasuryFcy.TotalOutflow += outflowMmi;
                        }

                        resultRaw.Add(fidTreasuryFcy);
                    }


                    #endregion

                    #region 4 - AMSD Inflow Fund

                    var amsdFormIds = (viewApprovedOnly)
                        ? db.AMSD_IF
                            .Where(x => DbFunctions.TruncateTime(x.FormDate) == DbFunctions.TruncateTime(reportDateParsed)
                                        && x.FormStatus == Common.FormStatus.Approved)
                            .Select(x => x.Id)
                            .ToList()
                        : db.AMSD_IF
                            .Where(x => DbFunctions.TruncateTime(x.FormDate) == DbFunctions.TruncateTime(reportDateParsed)
                                        && x.FormStatus != Common.FormStatus.Rejected)
                            .Select(x => x.Id)
                            .ToList();

                    if (amsdFormIds.Any())
                    {
                        var inflowFunds = db.AMSD_IF_Item
                            .Where(x => amsdFormIds.Contains(x.FormId) && x.Bank != null)
                            .GroupBy(x => new
                            {
                                x.Bank
                            })
                            .Select(x => new
                            {
                                Bank = x.Key.Bank,
                                Amount = x.Sum(y => y.Amount)
                            });

                        foreach (var fund in inflowFunds)
                        {
                            var inflowFundsFromAmsd = new TenAmCutOffItemVM()
                            {
                                Account = fund.Bank,
                                Currency = "MYR",
                                TotalInflow = fund.Amount,
                                Net = fund.Amount
                            };
                            resultRaw.Add(inflowFundsFromAmsd);
                        }
                    }
                    #endregion

                    #region 5 - Opening Balance

                    result = resultRaw
                        .GroupBy(x => new
                        {
                            x.Account,
                            x.Currency
                        })
                        .Select(x => new TenAmCutOffItemVM
                        {
                            Currency = x.Key.Currency,
                            Account = x.Key.Account,
                            OpeningBalance = 0,
                            TotalInflow = x.Sum(y => y.TotalInflow),
                            TotalOutflow = x.Sum(y => y.TotalOutflow)
                        })
                        .ToList();

                    foreach (var item in result)
                    {
                        var accountName2 = configAccount.Where(x => x.AccountName1 == item.Account)
                            .Select(x => x.AccountName2).Distinct().FirstOrDefault();

                        if (accountName2 != null)
                        {
                            // plug in opening balance
                            var ob = FcaTaggingSvc.GetOpeningBalance(db, reportDateParsed.Value, item.Currency, accountName2);

                            item.OpeningBalance = ob;
                        }
                    }

                    #endregion

                    result = result.GroupBy(x => new
                    {
                        x.Account,
                        x.Currency
                    })
                        .Select(x => new TenAmCutOffItemVM
                        {
                            Id = $"{x.Key.Currency};{x.Key.Account}",
                            Currency = x.Key.Currency,
                            Account = x.Key.Account,
                            OpeningBalance = x.Sum(y => y.OpeningBalance),
                            TotalInflow = x.Sum(y => y.TotalInflow),
                            TotalOutflow = x.Sum(y => y.TotalOutflow),
                            Net = x.Sum(y => y.OpeningBalance) + x.Sum(y => y.TotalInflow) - x.Sum(y => y.TotalOutflow),
                            ClosingBalance = 0
                        })
                        .ToList();

                    #region Editable Closing Balance

                    var closingBalances = db.TenAmDealCutOff_ClosingBalance.Where(x => x.Date == reportDateParsed);

                    if (closingBalances.Any())
                    {
                        foreach (var item in closingBalances)
                        {
                            var getResult = result.FirstOrDefault(x => x.Account == item.Account && x.Currency == item.Currency);
                            if (getResult != null)
                            {
                                getResult.ClosingBalance = item.ClosingBalance;
                                getResult.ClosingBalanceModifiedDate = item.ModifiedDate;
                                getResult.ClosingBalanceModifiedBy = item.ModifiedBy;
                            }
                        }
                    }

                    #endregion

                    return Request.CreateResponse(DataSourceLoader.Load(result, loadOptions));
                }
            }
            catch (Exception ex)
            {
                Logger.LogError(ex);
                return Request.CreateResponse(HttpStatusCode.BadRequest, ex.Message);
            }
        }

        [HttpPut]
        [Route("Summary/ClosingBalance/{reportDate}")]
        [KflowApiAuthorize(Common.PermissionKey.Report_DCO_10am_EditClosingBalance)]
        public HttpResponseMessage UpdateClosingBalance(long reportDate, FormDataCollection form)
        {
            try
            {
                var reportDateParsed = Common.ConvertEpochToDateTime(reportDate);
                reportDateParsed = reportDateParsed.Value.Date;

                var key = Convert.ToString(form.Get("key"));
                var values = form.Get("values");

                var vm = new UpdateClosingBalanceVM();
                
                JsonConvert.PopulateObject(values, vm);

                

                using (var db = new kashflowDBEntities())
                {
                    if (!string.IsNullOrEmpty(key))
                    {
                        var popCurrencyAccount = key.Split(';');
                        vm.Currency = popCurrencyAccount.FirstOrDefault();
                        vm.Account = popCurrencyAccount.LastOrDefault();

                        var record = db.TenAmDealCutOff_ClosingBalance.FirstOrDefault(x =>
                            x.Account == vm.Account
                            && x.Currency == vm.Currency && DbFunctions.TruncateTime(x.Date) == DbFunctions.TruncateTime(reportDateParsed));

                        if (record != null)
                        {
                            record.ClosingBalance = vm.ClosingBalance;
                            record.ModifiedBy = User.Identity.Name;
                            record.ModifiedDate = DateTime.Now;
                            db.SaveChanges();
                        }
                        else
                        {
                            var newRecord = new TenAmDealCutOff_ClosingBalance()
                            {
                                Date = reportDateParsed,
                                Currency = vm.Currency,
                                Account = vm.Account,
                                ClosingBalance = vm.ClosingBalance,
                                ModifiedBy = User.Identity.Name,
                                ModifiedDate = DateTime.Now
                            };
                            db.TenAmDealCutOff_ClosingBalance.Add(newRecord);
                            db.SaveChanges();
                        }
                    }
                    else
                    {
                        var newRecord = new TenAmDealCutOff_ClosingBalance()
                        {
                            Date = reportDateParsed,
                            Currency = vm.Currency,
                            Account = vm.Account,
                            ClosingBalance = vm.ClosingBalance,
                            ModifiedBy = User.Identity.Name,
                            ModifiedDate = DateTime.Now
                        };
                        db.TenAmDealCutOff_ClosingBalance.Add(newRecord);
                        db.SaveChanges();
                    }

                    return Request.CreateResponse(HttpStatusCode.OK);
                }

            }
            catch (Exception ex)
            {
                Logger.LogError(ex);
                return Request.CreateResponse(HttpStatusCode.BadRequest, ex.Message); 
            }
        }

        [HttpGet]
        [Route("Audit/ClosingBalance/{dateEpoch}")]
        [KflowApiAuthorize(Common.PermissionKey.Report_DCO_10am_EditClosingBalance)]
        public HttpResponseMessage AuditClosingBalance(long dateEpoch, DataSourceLoadOptions loadOptions)
        {
            try
            {
                using (var db = new kashflowDBEntities())
                {
                    var selectedDate = Common.ConvertEpochToDateTime(dateEpoch);

                    var result = db.Audit_10AMDCO_ClosingBalance
                         .Where(x => DbFunctions.TruncateTime(x.ReportDate) == DbFunctions.TruncateTime(selectedDate))
                         .ToList();

                    return Request.CreateResponse(DataSourceLoader.Load(result, loadOptions));
                }
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest, ex.Message);
            }

        }

    }
}
