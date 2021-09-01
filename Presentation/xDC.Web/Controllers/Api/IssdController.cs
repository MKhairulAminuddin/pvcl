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
using xDC.Services.App;
using xDC.Utils;
using xDC_Web.Models;
using xDC_Web.ViewModels.Iisd;

namespace xDC_Web.Controllers.Api
{
    [Authorize(Roles = "Administrator, Power User, ISSD")]
    [RoutePrefix("api/issd")]
    public class IssdController : ApiController
    {
        [HttpGet]
        [Route("TradeSettlement")]
        public HttpResponseMessage TradeSettlement(DataSourceLoadOptions loadOptions)
        {
            try
            {
                using (var db = new kashflowDBEntities())
                {
                    var formTypes = new List<string>()
                    {
                        Common.FormType.ISSD_TS_A,
                        Common.FormType.ISSD_TS_B,
                        Common.FormType.ISSD_TS_C,
                        Common.FormType.ISSD_TS_D,
                        Common.FormType.ISSD_TS_E
                    };

                    var todayDate = DateTime.Now.Date;
                    /*var result = db.ISSD_FormHeader
                        .Where(x => formTypes.Contains(x.FormType) && DbFunctions.TruncateTime(x.SettlementDate) >= todayDate);*/
                    var result = db.ISSD_FormHeader
                        .Where(x => formTypes.Contains(x.FormType));

                    var getApprover = db.Config_Approver.Where(x => x.Username == User.Identity.Name);
                    var isMeApprover = getApprover.Any();

                    var resultVM = new List<ViewFormHeaderModel>();

                    foreach (var item in result)
                    {
                        resultVM.Add(new ViewFormHeaderModel
                        {
                            Id = item.Id,
                            FormType = item.FormType,
                            FormDate = item.SettlementDate,
                            FormStatus = item.FormStatus,
                            Currency = item.Currency,
                            PreparedBy = item.PreparedBy,
                            PreparedDate = item.PreparedDate,
                            ApprovedBy = item.ApprovedBy,
                            ApprovedDate = item.ApprovedDate,
                            AdminEditted = item.AdminEditted,
                            AdminEdittedBy = item.AdminEdittedBy,
                            AdminEdittedDate = item.AdminEdittedDate,

                            IsDraft = (item.FormStatus != Common.FormStatus.PendingApproval),
                            IsMeCanEditDraft = (User.IsInRole(Config.Acl.Issd) && !isMeApprover),

                            IsPendingApproval = (item.FormStatus == Common.FormStatus.PendingApproval),

                            IsMyFormRejected = (User.Identity.Name == item.PreparedBy && item.FormStatus == Common.FormStatus.Rejected),
                            IsFormPendingMyApproval = (User.Identity.Name == item.ApprovedBy && item.FormStatus == Common.FormStatus.PendingApproval),


                            
                            IsCanAdminEdit = (User.IsInRole(Config.Acl.PowerUser) && !isMeApprover && item.FormStatus != Common.FormStatus.PendingApproval),
                            
                        });
                    }

                    return Request.CreateResponse(DataSourceLoader.Load(resultVM, loadOptions));
                }
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest, ex.Message);
            }

        }

        [HttpGet]
        [Route("TradeSettlement/Approved")]
        public HttpResponseMessage ApprovedTradeSettlement(DataSourceLoadOptions loadOptions)
        {
            try
            {
                using (var db = new xDC.Infrastructure.Application.kashflowDBEntities())
                {
                    var formTypes = new List<string>()
                    {
                        Common.FormTypeMapping(3),
                        Common.FormTypeMapping(4),
                        Common.FormTypeMapping(5),
                        Common.FormTypeMapping(6),
                        Common.FormTypeMapping(7)
                    };

                    var formStatus  = new List<string>()
                    {
                        Common.FormStatus.Approved
                    };

                    var result = db.ISSD_FormHeader
                        .Where(x => formTypes.Contains(x.FormType) && formStatus.Contains(x.FormStatus))
                        .GroupBy(x => new { x.SettlementDate, x.Currency})
                        .Select(x => new
                        {
                            SettlementDate = x.Key.SettlementDate,
                            Currency = x.Key.Currency,
                            ApprovedDate = x.Max(i=>i.ApprovedDate)
                        });

                    var resultVM = new List<ViewFormHeaderModel>();

                    foreach (var item in result)
                    {
                        resultVM.Add(new ViewFormHeaderModel
                        {
                            FormDate = item.SettlementDate,
                            Currency = item.Currency,
                            ApprovedDate = item.ApprovedDate
                        });
                    }

                    return Request.CreateResponse(DataSourceLoader.Load(resultVM, loadOptions));
                }
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest, ex.Message);
            }

        }

        [HttpGet]
        [Route("ts/approvedTrades/{settlementDateEpoch}/{currency}")]
        public HttpResponseMessage TS_ApprovedTrades(long settlementDateEpoch, string currency, DataSourceLoadOptions loadOptions)
        {
            try
            {
                using (var db = new kashflowDBEntities())
                {
                    var settlementDate = Common.ConvertEpochToDateTime(settlementDateEpoch);
                    
                    var trades = TradeSettlementService.GetTradeSettlement(db, settlementDate.Value.Date, currency);

                    var result = trades
                        .GroupBy(i => 1)
                        .Select(x => new TS_ApprovedTradeVM()
                        {
                            SettlementDate = settlementDate.Value,
                            Currency = currency,

                            TotalEquity = x.Count(y => y.InstrumentType == Common.TsItemCategory.Equity),
                            TotalBond = x.Count(y => y.InstrumentType == Common.TsItemCategory.Bond),
                            TotalCp = x.Count(y => y.InstrumentType == Common.TsItemCategory.Cp),
                            TotalNotesPapers = x.Count(y => y.InstrumentType == Common.TsItemCategory.NotesPapers),
                            TotalRepo = x.Count(y => y.InstrumentType == Common.TsItemCategory.Repo),
                            TotalCoupon = x.Count(y => y.InstrumentType == Common.TsItemCategory.Coupon),
                            TotalFees = x.Count(y => y.InstrumentType == Common.TsItemCategory.Fees),
                            TotalMtm = x.Count(y => y.InstrumentType == Common.TsItemCategory.Mtm),
                            TotalFx = x.Count(y => y.InstrumentType == Common.TsItemCategory.Fx),
                            TotalCn = x.Count(y => y.InstrumentType == Common.TsItemCategory.Cn),
                            TotalAltid = x.Count(y => y.InstrumentType == Common.TsItemCategory.Altid),
                            TotalOthers = x.Count(y => y.InstrumentType == Common.TsItemCategory.Others),

                            FormIdEquity = x.FirstOrDefault(y => y.InstrumentType == Common.TsItemCategory.Equity)?.FormId,
                            FormIdBond = x.FirstOrDefault(y => y.InstrumentType == Common.TsItemCategory.Bond)?.FormId,
                            FormIdCp = x.FirstOrDefault(y => y.InstrumentType == Common.TsItemCategory.Cp)?.FormId,
                            FormIdNotesPapers = x.FirstOrDefault(y => y.InstrumentType == Common.TsItemCategory.NotesPapers)?.FormId,
                            FormIdRepo = x.FirstOrDefault(y => y.InstrumentType == Common.TsItemCategory.Repo)?.FormId,
                            FormIdCoupon = x.FirstOrDefault(y => y.InstrumentType == Common.TsItemCategory.Coupon)?.FormId,
                            FormIdFees = x.FirstOrDefault(y => y.InstrumentType == Common.TsItemCategory.Fees)?.FormId,
                            FormIdMtm = x.FirstOrDefault(y => y.InstrumentType == Common.TsItemCategory.Mtm)?.FormId,
                            FormIdFx = x.FirstOrDefault(y => y.InstrumentType == Common.TsItemCategory.Fx)?.FormId,
                            FormIdCn = x.FirstOrDefault(y => y.InstrumentType == Common.TsItemCategory.Cn)?.FormId,
                            FormIdAltid = x.FirstOrDefault(y => y.InstrumentType == Common.TsItemCategory.Altid)?.FormId,
                            FormIdOthers = x.FirstOrDefault(y => y.InstrumentType == Common.TsItemCategory.Others)?.FormId,

                        }).ToList();

                    return Request.CreateResponse(DataSourceLoader.Load(result, loadOptions));
                }
            }
            catch (Exception ex)
            {
                Logger.LogError(ex);
                return Request.CreateResponse(HttpStatusCode.BadRequest, ex.Message);
            }
        }

        [HttpGet]
        [Route("ts/approvedTradeItems/{formId}/{instrumentType}")]
        public HttpResponseMessage TS_ApprovedTradeItems(int formId, string instrumentType, DataSourceLoadOptions loadOptions)
        {
            try
            {
                using (var db = new kashflowDBEntities())
                {
                    var tradeCategory = Common.TsCategoryUrlParamMapping(instrumentType);

                    if (tradeCategory != null)
                    {
                        var result = db.ISSD_TradeSettlement.AsNoTracking()
                            .Where(x => x.InstrumentType == tradeCategory && x.FormId == formId).ToList();

                        return Request.CreateResponse(DataSourceLoader.Load(result, loadOptions));
                    }
                    else
                    {
                        return Request.CreateResponse(HttpStatusCode.BadRequest, "Failed convert to actual date");
                    }
                }
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest, ex.Message);
            }

        }

        [HttpGet]
        [Route("GetIssdForm/{formType}")]
        public HttpResponseMessage GetTradeSettlementForm(int formType, DataSourceLoadOptions loadOptions)
        {
            try
            {
                using (var db = new xDC.Infrastructure.Application.kashflowDBEntities())
                {
                    var selectedFormType = Common.FormTypeMapping(formType);

                    var result = db.ISSD_FormHeader
                        .Where(x => x.FormType == selectedFormType);

                    var getApprover = db.Config_Approver.Where(x => x.Username == User.Identity.Name);
                    var isMeApprover = getApprover.Any();

                    var resultVM = new List<ViewTradeSettlementFormHeaderModel>();

                    foreach (var item in result)
                    {
                        resultVM.Add(new ViewTradeSettlementFormHeaderModel()
                        {
                            Id = item.Id,
                            FormType = item.FormType,
                            SettlementDate = item.SettlementDate,
                            FormStatus = item.FormStatus,
                            Currency = item.Currency,
                            PreparedBy = item.PreparedBy,
                            PreparedDate = item.PreparedDate,
                            ApprovedBy = item.ApprovedBy,
                            ApprovedDate = item.ApprovedDate,
                            AdminEditted = item.AdminEditted,
                            AdminEdittedBy = item.AdminEdittedBy,
                            AdminEdittedDate = item.AdminEdittedDate,

                            IsDraft = (item.FormStatus == Common.FormStatus.Draft),

                            IsMeCanEditDraft = (User.IsInRole(Config.Acl.Issd) && !isMeApprover),


                            IsMyFormRejected = (User.Identity.Name == item.PreparedBy && item.FormStatus == Common.FormStatus.Rejected),
                            IsFormPendingMyApproval = (User.Identity.Name == item.ApprovedBy && item.FormStatus == Common.FormStatus.PendingApproval),
                            IsFormOwner = (User.Identity.Name == item.PreparedBy),
                            IsCanAdminEdit = (User.IsInRole(Config.Acl.PowerUser)),
                            IsResubmitEnabled = (item.FormStatus == "Rejected" && User.IsInRole(Config.Acl.Amsd) && User.Identity.Name != item.ApprovedBy)
                        });
                    }

                    return Request.CreateResponse(DataSourceLoader.Load(resultVM, loadOptions));
                }
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest, ex.Message);
            }

        }


        #region Trade Settlement Form

        [HttpPost]
        [Route("TradeSettlement/New")]
        public HttpResponseMessage NewTradeSettlement([FromBody] TradeSettlementModel inputs)
        {
            try
            {
                var settlementDateConverted = Common.ConvertEpochToDateTime(inputs.SettlementDateEpoch);
                inputs.Currency = inputs.Currency.ToUpper();

                using (var db = new kashflowDBEntities())
                {
                    if (TradeSettlementService.IsSameDateAndCurrencyExist(db, settlementDateConverted.Value.Date, inputs.Currency, Common.FormTypeMapping(inputs.FormType)))
                    {
                        return Request.CreateResponse(HttpStatusCode.BadRequest, "Similar form has been created. Use that instead.");
                    }

                    /*if (TradeSettlementService.IsTMinus(settlementDateConverted.Value.Date))
                    {
                        return Request.CreateResponse(HttpStatusCode.BadRequest, "Submission for Settlement Date T-n is not allowed");
                    }*/

                    var newFormHeader = new ISSD_FormHeader()
                    {
                        FormType = Common.FormTypeMapping(inputs.FormType),
                        PreparedBy = User.Identity.Name,
                        PreparedDate = DateTime.Now,
                        FormStatus = (inputs.IsSaveAsDraft) ? Common.FormStatus.Draft : Common.FormStatus.PendingApproval,
                        SettlementDate = Common.ConvertEpochToDateTime(inputs.SettlementDateEpoch)?.Date,
                        Currency = inputs.Currency,
                        ApprovedBy = (inputs.IsSaveAsDraft) ? null : inputs.Approver
                    };
                    
                    db.ISSD_FormHeader.Add(newFormHeader);
                    db.SaveChanges();
                    
                    var newTrades = new List<ISSD_TradeSettlement>();

                    if (inputs.Equity.Any())
                    {
                        foreach (var item in inputs.Equity)
                        {
                            newTrades.Add(new ISSD_TradeSettlement()
                            {
                                FormId = newFormHeader.Id,
                                InstrumentType = Common.TsItemCategory.Equity,
                                InstrumentCode = item.InstrumentCode,
                                StockCode = item.StockCode,
                                Maturity = item.Maturity,
                                Sales = item.Sales,
                                Purchase = item.Purchase,
                                Remarks = item.Remarks,
                                ModifiedBy = User.Identity.Name,
                                ModifiedDate = DateTime.Now
                            });
                        }
                    }

                    if (inputs.Bond.Any())
                    {
                        foreach (var item in inputs.Bond)
                        {
                            newTrades.Add(new ISSD_TradeSettlement()
                            {
                                FormId = newFormHeader.Id,
                                InstrumentType = Common.TsItemCategory.Bond,
                                InstrumentCode = item.InstrumentCode,
                                StockCode = item.StockCode,
                                Maturity = item.Maturity,
                                Sales = item.Sales,
                                Purchase = item.Purchase,
                                Remarks = item.Remarks,
                                ModifiedBy = User.Identity.Name,
                                ModifiedDate = DateTime.Now
                            });
                        }
                    }

                    if (inputs.Cp.Any())
                    {
                        foreach (var item in inputs.Cp)
                        {
                            newTrades.Add(new ISSD_TradeSettlement()
                            {
                                FormId = newFormHeader.Id,
                                InstrumentType = Common.TsItemCategory.Cp,
                                InstrumentCode = item.InstrumentCode,
                                StockCode = item.StockCode,
                                Maturity = item.Maturity,
                                Sales = item.Sales,
                                Purchase = item.Purchase,
                                Remarks = item.Remarks,
                                ModifiedBy = User.Identity.Name,
                                ModifiedDate = DateTime.Now
                            });
                        }
                    }

                    if (inputs.NotesPaper.Any())
                    {
                        foreach (var item in inputs.NotesPaper)
                        {
                            newTrades.Add(new ISSD_TradeSettlement()
                            {
                                FormId = newFormHeader.Id,
                                InstrumentType = Common.TsItemCategory.NotesPapers,
                                InstrumentCode = item.InstrumentCode,
                                StockCode = item.StockCode,
                                Maturity = item.Maturity,
                                Sales = item.Sales,
                                Purchase = item.Purchase,
                                Remarks = item.Remarks,
                                ModifiedBy = User.Identity.Name,
                                ModifiedDate = DateTime.Now
                            });
                        }
                    }

                    if (inputs.Repo.Any())
                    {
                        foreach (var item in inputs.Repo)
                        {
                            newTrades.Add(new ISSD_TradeSettlement()
                            {
                                FormId = newFormHeader.Id,
                                InstrumentType = Common.TsItemCategory.Repo,
                                InstrumentCode = item.InstrumentCode,
                                StockCode = item.StockCode,
                                FirstLeg = item.FirstLeg,
                                SecondLeg = item.SecondLeg,
                                Remarks = item.Remarks,
                                ModifiedBy = User.Identity.Name,
                                ModifiedDate = DateTime.Now
                            });
                        }
                    }

                    if (inputs.Coupon.Any())
                    {
                        foreach (var item in inputs.Coupon)
                        {
                            newTrades.Add(new ISSD_TradeSettlement()
                            {
                                FormId = newFormHeader.Id,
                                InstrumentType = Common.TsItemCategory.Coupon,
                                InstrumentCode = item.InstrumentCode,
                                StockCode = item.StockCode,
                                AmountPlus = item.AmountPlus,
                                Remarks = item.Remarks,
                                ModifiedBy = User.Identity.Name,
                                ModifiedDate = DateTime.Now
                            });
                        }
                    }

                    if (inputs.Fees.Any())
                    {
                        foreach (var item in inputs.Fees)
                        {
                            newTrades.Add(new ISSD_TradeSettlement()
                            {
                                FormId = newFormHeader.Id,
                                InstrumentType = Common.TsItemCategory.Fees,
                                InstrumentCode = item.InstrumentCode,
                                AmountPlus = item.AmountPlus,
                                AmountMinus = item.AmountMinus,
                                Remarks = item.Remarks,
                                ModifiedBy = User.Identity.Name,
                                ModifiedDate = DateTime.Now
                            });
                        }
                    }

                    if (inputs.Mtm.Any())
                    {
                        foreach (var item in inputs.Mtm)
                        {
                            newTrades.Add(new ISSD_TradeSettlement()
                            {
                                FormId = newFormHeader.Id,
                                InstrumentType = Common.TsItemCategory.Mtm,
                                InstrumentCode = item.InstrumentCode,
                                StockCode = item.StockCode,
                                AmountPlus = item.AmountPlus,
                                AmountMinus = item.AmountMinus,
                                Remarks = item.Remarks,
                                ModifiedBy = User.Identity.Name,
                                ModifiedDate = DateTime.Now
                            });
                        }
                    }

                    if (inputs.FxSettlement.Any())
                    {
                        foreach (var item in inputs.FxSettlement)
                        {
                            newTrades.Add(new ISSD_TradeSettlement()
                            {
                                FormId = newFormHeader.Id,
                                InstrumentType = Common.TsItemCategory.Fx,
                                InstrumentCode = item.InstrumentCode,
                                AmountPlus = item.AmountPlus,
                                AmountMinus = item.AmountMinus,
                                Remarks = item.Remarks,
                                ModifiedBy = User.Identity.Name,
                                ModifiedDate = DateTime.Now
                            });
                        }
                    }

                    if (inputs.ContributionCredited.Any())
                    {
                        foreach (var item in inputs.ContributionCredited)
                        {
                            newTrades.Add(new ISSD_TradeSettlement()
                            {
                                FormId = newFormHeader.Id,
                                InstrumentType = Common.TsItemCategory.Cn,
                                InstrumentCode = item.InstrumentCode,
                                AmountPlus = item.AmountPlus,
                                Remarks = item.Remarks,
                                ModifiedBy = User.Identity.Name,
                                ModifiedDate = DateTime.Now
                            });
                        }
                    }

                    if (inputs.Altid.Any())
                    {
                        foreach (var item in inputs.Altid)
                        {
                            newTrades.Add(new ISSD_TradeSettlement()
                            {
                                FormId = newFormHeader.Id,
                                InstrumentType = Common.TsItemCategory.Altid,
                                InstrumentCode = item.InstrumentCode,
                                AmountPlus = item.AmountPlus,
                                AmountMinus = item.AmountMinus,
                                Remarks = item.Remarks,
                                ModifiedBy = User.Identity.Name,
                                ModifiedDate = DateTime.Now
                            });
                        }
                    }

                    if (inputs.Others.Any())
                    {
                        foreach (var item in inputs.Others)
                        {
                            newTrades.Add(new ISSD_TradeSettlement()
                            {
                                FormId = newFormHeader.Id,
                                InstrumentType = Common.TsItemCategory.Others,
                                InstrumentCode = item.InstrumentCode,
                                AmountPlus = item.AmountPlus,
                                AmountMinus = item.AmountMinus,
                                Remarks = item.Remarks,
                                ModifiedBy = User.Identity.Name,
                                ModifiedDate = DateTime.Now
                            });
                        }
                    }
                    
                    db.ISSD_TradeSettlement.AddRange(newTrades);
                    db.SaveChanges();
                    
                    if (inputs.OpeningBalance.Any())
                    {
                        foreach (var item in inputs.OpeningBalance)
                        {
                            db.ISSD_Balance.Add(new ISSD_Balance()
                            {
                                FormId = newFormHeader.Id,
                                BalanceCategory = item.InstrumentType,
                                Amount = item.Amount,
                                CreatedBy = User.Identity.Name,
                                CreatedDate = DateTime.Now
                            });
                        }
                        db.SaveChanges();
                    }

                    if (inputs.Approver != null)
                    {
                        TradeSettlementService.NotifyApprover(inputs.Approver, newFormHeader.Id, User.Identity.Name, newFormHeader.FormType, inputs.ApprovalNotes);
                    }
                    
                    return Request.CreateResponse(HttpStatusCode.Created, newFormHeader.Id);
                }
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest, ex.Message);
            }

        }

        [HttpPost]
        [Route("TradeSettlement/Edit")]
        public HttpResponseMessage EditTradeSettlement([FromBody] TradeSettlementModel inputs)
        {
            try
            {
                using (var db = new kashflowDBEntities())
                {
                    var getForm = db.ISSD_FormHeader.FirstOrDefault(x => x.Id == inputs.Id);

                    if (getForm == null)
                    {
                        return Request.CreateResponse(HttpStatusCode.BadRequest, "Form not found!");
                    }

                    if (inputs.IsSaveAdminEdit)
                    {
                        getForm.AdminEditted = true;
                        getForm.AdminEdittedBy = User.Identity.Name;
                        getForm.AdminEdittedDate = DateTime.Now;
                    }

                    if (inputs.IsSaveAsDraft)
                    {
                        getForm.PreparedBy = User.Identity.Name;
                        getForm.PreparedDate = DateTime.Now;
                    }

                    if (inputs.Approver != null)
                    {
                        getForm.ApprovedBy = inputs.Approver;
                        getForm.ApprovedDate = null; // empty the date as this is new submission
                        getForm.FormStatus = Common.FormStatus.PendingApproval;

                        TradeSettlementService.NotifyApprover(getForm.ApprovedBy, getForm.Id, User.Identity.Name,
                            getForm.FormType, inputs.ApprovalNotes);
                    }
                    
                    var getTradeItems = db.ISSD_TradeSettlement.Where(x =>
                        x.FormId == getForm.Id);

                    if (inputs.Equity != null)
                    {
                        foreach (var item in inputs.Equity)
                        {
                            var foundItem = getTradeItems.FirstOrDefault(x => x.Id == item.Id);
                            if (foundItem != null)
                            {
                                if (foundItem.Remarks != item.Remarks)
                                {
                                    foundItem.Remarks = item.Remarks;
                                    foundItem.ModifiedBy = User.Identity.Name;
                                    foundItem.ModifiedDate = DateTime.Now;
                                }
                            }
                        }
                    }

                    if (inputs.Bond != null)
                    {
                        foreach (var item in inputs.Bond)
                        {
                            var foundItem = getTradeItems.FirstOrDefault(x => x.Id == item.Id);
                            if (foundItem != null)
                            {
                                if (foundItem.Remarks != item.Remarks)
                                {
                                    foundItem.Remarks = item.Remarks;
                                    foundItem.ModifiedBy = User.Identity.Name;
                                    foundItem.ModifiedDate = DateTime.Now;
                                }
                            }
                        }
                    }

                    if (inputs.Cp != null)
                    {
                        foreach (var item in inputs.Cp)
                        {
                            var foundItem = getTradeItems.FirstOrDefault(x => x.Id == item.Id);
                            if (foundItem != null)
                            {
                                if (foundItem.Remarks != item.Remarks)
                                {
                                    foundItem.Remarks = item.Remarks;
                                    foundItem.ModifiedBy = User.Identity.Name;
                                    foundItem.ModifiedDate = DateTime.Now;
                                }
                            }
                        }
                    }

                    if (inputs.NotesPaper != null)
                    {
                        foreach (var item in inputs.NotesPaper)
                        {
                            var foundItem = getTradeItems.FirstOrDefault(x => x.Id == item.Id);
                            if (foundItem != null)
                            {
                                if (foundItem.Remarks != item.Remarks)
                                {
                                    foundItem.Remarks = item.Remarks;
                                    foundItem.ModifiedBy = User.Identity.Name;
                                    foundItem.ModifiedDate = DateTime.Now;
                                }
                            }
                        }
                    }

                    if (inputs.Repo != null)
                    {
                        foreach (var item in inputs.Repo)
                        {
                            var foundItem = getTradeItems.FirstOrDefault(x => x.Id == item.Id);
                            if (foundItem != null)
                            {
                                if (foundItem.InstrumentCode != item.InstrumentCode ||
                                    foundItem.StockCode != item.StockCode ||
                                    foundItem.FirstLeg != item.FirstLeg ||
                                    foundItem.SecondLeg != item.SecondLeg ||
                                    foundItem.Remarks != item.Remarks)
                                {
                                    foundItem.InstrumentCode = item.InstrumentCode;
                                    foundItem.StockCode = item.StockCode;
                                    foundItem.FirstLeg = item.FirstLeg;
                                    foundItem.SecondLeg = item.SecondLeg;
                                    foundItem.Remarks = item.Remarks;
                                    foundItem.ModifiedBy = User.Identity.Name;
                                    foundItem.ModifiedDate = DateTime.Now;
                                }
                            }
                            else
                            {
                                db.ISSD_TradeSettlement.Add(new ISSD_TradeSettlement()
                                {
                                    FormId = inputs.Id,
                                    InstrumentType = Common.TsItemCategory.Repo,
                                    InstrumentCode = item.InstrumentCode,
                                    StockCode = item.StockCode,
                                    FirstLeg = item.FirstLeg,
                                    SecondLeg = item.SecondLeg,
                                    Remarks = item.Remarks,
                                    ModifiedBy = User.Identity.Name,
                                    ModifiedDate = DateTime.Now
                                });
                            }
                        }
                    }

                    if (inputs.Coupon != null)
                    {
                        foreach (var item in inputs.Coupon)
                        {
                            var foundItem = getTradeItems.FirstOrDefault(x => x.Id == item.Id);
                            if (foundItem != null)
                            {
                                if (foundItem.InstrumentCode != item.InstrumentCode ||
                                    foundItem.StockCode != item.StockCode ||
                                    foundItem.AmountPlus != item.AmountPlus ||
                                    foundItem.Remarks != item.Remarks)
                                {
                                    foundItem.InstrumentCode = item.InstrumentCode;
                                    foundItem.StockCode = item.StockCode;
                                    foundItem.AmountPlus = item.AmountPlus;
                                    foundItem.Remarks = item.Remarks;
                                    foundItem.ModifiedBy = User.Identity.Name;
                                    foundItem.ModifiedDate = DateTime.Now;
                                }
                            }
                            else
                            {
                                db.ISSD_TradeSettlement.Add(new ISSD_TradeSettlement()
                                {
                                    FormId = inputs.Id,
                                    InstrumentType = Common.TsItemCategory.Coupon,
                                    InstrumentCode = item.InstrumentCode,
                                    StockCode = item.StockCode,
                                    AmountPlus = item.AmountPlus,
                                    Remarks = item.Remarks,
                                    ModifiedBy = User.Identity.Name,
                                    ModifiedDate = DateTime.Now
                                });
                            }
                        }
                    }

                    if (inputs.Mtm != null)
                    {
                        foreach (var item in inputs.Mtm)
                        {
                            var foundItem = getTradeItems.FirstOrDefault(x => x.Id == item.Id);
                            if (foundItem != null)
                            {
                                if (foundItem.InstrumentCode != item.InstrumentCode ||
                                    foundItem.StockCode != item.StockCode ||
                                    foundItem.AmountPlus != item.AmountPlus ||
                                    foundItem.AmountMinus != item.AmountMinus ||
                                    foundItem.Remarks != item.Remarks)
                                {
                                    foundItem.InstrumentCode = item.InstrumentCode;
                                    foundItem.StockCode = item.StockCode;
                                    foundItem.AmountPlus = item.AmountPlus;
                                    foundItem.AmountMinus = item.AmountMinus;
                                    foundItem.Remarks = item.Remarks;
                                    foundItem.ModifiedBy = User.Identity.Name;
                                    foundItem.ModifiedDate = DateTime.Now;
                                }
                                    
                            }
                            else
                            {
                                db.ISSD_TradeSettlement.Add(new ISSD_TradeSettlement()
                                {
                                    FormId = inputs.Id,
                                    InstrumentType = Common.TsItemCategory.Mtm,
                                    InstrumentCode = item.InstrumentCode,
                                    StockCode = item.StockCode,
                                    AmountPlus = item.AmountPlus,
                                    AmountMinus = item.AmountMinus,
                                    Remarks = item.Remarks,
                                    ModifiedBy = User.Identity.Name,
                                    ModifiedDate = DateTime.Now
                                });
                            }
                        }
                    }

                    if (inputs.FxSettlement != null)
                    {
                        foreach (var item in inputs.FxSettlement)
                        {
                            var foundItem = getTradeItems.FirstOrDefault(x => x.Id == item.Id);
                            if (foundItem != null)
                            {
                                if (foundItem.InstrumentCode != item.InstrumentCode ||
                                    foundItem.StockCode != item.StockCode ||
                                    foundItem.AmountPlus != item.AmountPlus ||
                                    foundItem.AmountMinus != item.AmountMinus ||
                                    foundItem.Remarks != item.Remarks)
                                {
                                    foundItem.InstrumentCode = item.InstrumentCode;
                                    foundItem.StockCode = item.StockCode;
                                    foundItem.AmountPlus = item.AmountPlus;
                                    foundItem.AmountMinus = item.AmountMinus;
                                    foundItem.Remarks = item.Remarks;
                                    foundItem.ModifiedBy = User.Identity.Name;
                                    foundItem.ModifiedDate = DateTime.Now;
                                }
                                    
                            }
                            else
                            {
                                db.ISSD_TradeSettlement.Add(new ISSD_TradeSettlement()
                                {
                                    FormId = inputs.Id,
                                    InstrumentType = Common.TsItemCategory.Fx,
                                    InstrumentCode = item.InstrumentCode,
                                    StockCode = item.StockCode,
                                    AmountPlus = item.AmountPlus,
                                    AmountMinus = item.AmountMinus,
                                    Remarks = item.Remarks,
                                    ModifiedBy = User.Identity.Name,
                                    ModifiedDate = DateTime.Now
                                });
                            }
                        }
                    }

                    if (inputs.Altid != null)
                    {
                        foreach (var item in inputs.Altid)
                        {
                            var foundItem = getTradeItems.FirstOrDefault(x => x.Id == item.Id);
                            if (foundItem != null)
                            {
                                if (foundItem.InstrumentCode != item.InstrumentCode ||
                                    foundItem.StockCode != item.StockCode ||
                                    foundItem.AmountPlus != item.AmountPlus ||
                                    foundItem.AmountMinus != item.AmountMinus ||
                                    foundItem.Remarks != item.Remarks)
                                {
                                    foundItem.InstrumentCode = item.InstrumentCode;
                                    foundItem.StockCode = item.StockCode;
                                    foundItem.AmountPlus = item.AmountPlus;
                                    foundItem.AmountMinus = item.AmountMinus;
                                    foundItem.Remarks = item.Remarks;
                                    foundItem.ModifiedBy = User.Identity.Name;
                                    foundItem.ModifiedDate = DateTime.Now;
                                }
                            }
                            else
                            {
                                db.ISSD_TradeSettlement.Add(new ISSD_TradeSettlement()
                                {
                                    FormId = inputs.Id,
                                    InstrumentType = Common.TsItemCategory.Altid,
                                    InstrumentCode = item.InstrumentCode,
                                    StockCode = item.StockCode,
                                    AmountPlus = item.AmountPlus,
                                    AmountMinus = item.AmountMinus,
                                    Remarks = item.Remarks,
                                    ModifiedBy = User.Identity.Name,
                                    ModifiedDate = DateTime.Now
                                });
                            }
                        }
                    }

                    if (inputs.ContributionCredited != null)
                    {
                        foreach (var item in inputs.ContributionCredited)
                        {
                            var foundItem = getTradeItems.FirstOrDefault(x => x.Id == item.Id);
                            if (foundItem != null)
                            {
                                if (foundItem.InstrumentCode != item.InstrumentCode ||
                                    foundItem.StockCode != item.StockCode ||
                                    foundItem.AmountPlus != item.AmountPlus ||
                                    foundItem.Remarks != item.Remarks)
                                {
                                    foundItem.InstrumentCode = item.InstrumentCode;
                                    foundItem.StockCode = item.StockCode;
                                    foundItem.AmountPlus = item.AmountPlus;
                                    foundItem.Remarks = item.Remarks;

                                    foundItem.ModifiedBy = User.Identity.Name;
                                    foundItem.ModifiedDate = DateTime.Now;
                                }
                            }
                                    
                            else
                            {
                                db.ISSD_TradeSettlement.Add(new ISSD_TradeSettlement()
                                {
                                    FormId = inputs.Id,
                                    InstrumentType = Common.TsItemCategory.Cn,
                                    InstrumentCode = item.InstrumentCode,
                                    StockCode = item.StockCode,
                                    AmountPlus = item.AmountPlus,
                                    Remarks = item.Remarks,

                                    ModifiedBy = User.Identity.Name,
                                    ModifiedDate = DateTime.Now
                                });
                            }
                        }
                    }

                    if (inputs.Fees != null)
                    {
                        foreach (var item in inputs.Fees)
                        {
                            var foundItem = getTradeItems.FirstOrDefault(x => x.Id == item.Id);
                            if (foundItem != null)
                            {
                                if (foundItem.InstrumentCode != item.InstrumentCode ||
                                    foundItem.StockCode != item.StockCode ||
                                    foundItem.AmountPlus != item.AmountPlus ||
                                    foundItem.AmountMinus != item.AmountMinus ||
                                    foundItem.Remarks != item.Remarks)
                                {
                                    foundItem.InstrumentCode = item.InstrumentCode;
                                    foundItem.StockCode = item.StockCode;
                                    foundItem.AmountPlus = item.AmountPlus;
                                    foundItem.AmountMinus = item.AmountMinus;
                                    foundItem.Remarks = item.Remarks;

                                    foundItem.ModifiedBy = User.Identity.Name;
                                    foundItem.ModifiedDate = DateTime.Now;
                                }
                            }
                            else
                            {
                                db.ISSD_TradeSettlement.Add(new ISSD_TradeSettlement()
                                {
                                    FormId = inputs.Id,
                                    InstrumentType = Common.TsItemCategory.Fees,
                                    InstrumentCode = item.InstrumentCode,
                                    StockCode = item.StockCode,
                                    AmountPlus = item.AmountPlus,
                                    AmountMinus = item.AmountMinus,
                                    Remarks = item.Remarks,

                                    ModifiedBy = User.Identity.Name,
                                    ModifiedDate = DateTime.Now
                                });
                            }
                        }
                    }

                    if (inputs.Others != null)
                    {
                        foreach (var item in inputs.Others)
                        {
                            var foundItem = getTradeItems.FirstOrDefault(x => x.Id == item.Id);
                            if (foundItem != null)
                            {
                                if (foundItem.InstrumentCode != item.InstrumentCode ||
                                    foundItem.StockCode != item.StockCode||
                                    foundItem.AmountPlus != item.AmountPlus||
                                    foundItem.AmountMinus != item.AmountMinus ||
                                    foundItem.Remarks != item.Remarks)
                                {
                                    foundItem.InstrumentCode = item.InstrumentCode;
                                    foundItem.StockCode = item.StockCode;
                                    foundItem.AmountPlus = item.AmountPlus;
                                    foundItem.AmountMinus = item.AmountMinus;
                                    foundItem.Remarks = item.Remarks;

                                    foundItem.ModifiedBy = User.Identity.Name;
                                    foundItem.ModifiedDate = DateTime.Now;
                                }
                                
                            }
                            else
                            {
                                db.ISSD_TradeSettlement.Add(new ISSD_TradeSettlement()
                                {
                                    FormId = inputs.Id,
                                    InstrumentType = Common.TsItemCategory.Others,
                                    InstrumentCode = item.InstrumentCode,
                                    StockCode = item.StockCode,
                                    AmountPlus = item.AmountPlus,
                                    AmountMinus = item.AmountMinus,
                                    Remarks = item.Remarks,

                                    ModifiedBy = User.Identity.Name,
                                    ModifiedDate = DateTime.Now
                                });
                            }
                        }
                    }
                    
                    db.SaveChanges();

                    return Request.CreateResponse(HttpStatusCode.Created, getForm.Id);
                }
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest, ex.Message);
            }

        }

        [HttpPost]
        [Route("TradeSettlement/Approval")]
        public HttpResponseMessage ApprovalTradeSettlement([FromBody] ApprovalInflowFundsModel inputs)
        {
            try
            {
                using (var db = new kashflowDBEntities())
                {
                    var formId = Convert.ToInt32(inputs.FormId);
                    var form = db.ISSD_FormHeader.FirstOrDefault(x => x.Id == formId);

                    if (form != null)
                    {
                        if (form.ApprovedBy == User.Identity.Name)
                        {
                            form.ApprovedDate = DateTime.Now;
                            form.FormStatus = (inputs.ApprovalStatus)
                                ? Common.FormStatus.Approved
                                : Common.FormStatus.Rejected;

                            db.SaveChanges();

                            TradeSettlementService.NotifyPreparer(form.PreparedBy, form.Id, User.Identity.Name, form.FormType, inputs.ApprovalStatus,
                                inputs.ApprovalNote);

                            return Request.CreateResponse(HttpStatusCode.Accepted, formId);
                        }
                        else
                        {
                            return Request.CreateResponse(HttpStatusCode.BadRequest, "Unauthorized Approver!");
                        }
                    }
                    else
                    {
                        return Request.CreateResponse(HttpStatusCode.BadRequest, "Form not Found!");
                    }
                }
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest, ex.Message);
            }
        }
        
        [HttpDelete]
        [Authorize(Roles = "Administrator, ISSD")]
        [Route("TradeSettlement")]
        public HttpResponseMessage DeleteTradeSettlement(FormDataCollection form)
        {
            try
            {
                using (var db = new kashflowDBEntities())
                {
                    var key = Convert.ToInt32(form.Get("id"));

                    var formHeader = db.ISSD_FormHeader.FirstOrDefault(x => x.Id == key);

                    if (formHeader != null)
                    {
                        db.ISSD_FormHeader.Remove(formHeader);

                        var tradeItems = db.ISSD_TradeSettlement.Where(x => x.FormId == key);
                        if (tradeItems.Any())
                        {
                            db.ISSD_TradeSettlement.RemoveRange(tradeItems);
                        }

                        var openingBalances = db.ISSD_Balance.Where(x => x.FormId == key);
                        if (openingBalances.Any())
                        {
                            db.ISSD_Balance.RemoveRange(openingBalances);
                        }

                        db.SaveChanges();

                        return Request.CreateResponse(HttpStatusCode.OK);
                    }
                    else
                    {
                        return Request.CreateResponse(HttpStatusCode.BadRequest, "Form not found!");
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.LogError(ex);
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex);
            }

        }

        #endregion

        #region Bank Balance
        
        [HttpGet]
        [Route("GetBalance/{formId}")]
        public HttpResponseMessage GetBalance(int formId, DataSourceLoadOptions loadOptions)
        {
            try
            {
                using (var db = new kashflowDBEntities())
                {
                    var result = db.ISSD_Balance.Where(x => x.FormId == formId).ToList();

                    return Request.CreateResponse(DataSourceLoader.Load(result, loadOptions));
                }
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest, ex.Message);
            }
        }

        [HttpGet]
        [Route("GetBalanceConsolidated/{settlementDateEpoch}/{currency}")]
        public HttpResponseMessage GetBalanceConsolidated(long settlementDateEpoch, string currency, DataSourceLoadOptions loadOptions)
        {
            try
            {
                var settlementDate = Common.ConvertEpochToDateTime(settlementDateEpoch);
                var settlementDateOnly = settlementDate.Value.Date;

                using (var db = new kashflowDBEntities())
                {
                    var result = new List<ISSD_Balance>();

                    var getForm = db.ISSD_FormHeader.FirstOrDefault(x =>
                        DbFunctions.TruncateTime(x.SettlementDate) == settlementDateOnly && x.Currency == currency && x.FormStatus == "Approved");

                    if (getForm != null)
                    {
                        result = db.ISSD_Balance.Where(x => x.FormId == getForm.Id).ToList();
                        return Request.CreateResponse(DataSourceLoader.Load(result, loadOptions));
                    }
                    else
                    {
                        return Request.CreateResponse(DataSourceLoader.Load(result, loadOptions));
                    }
                }
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest, ex.Message);
            }
        }

        #endregion
        
        [HttpGet]
        [Route("TradeItemDW/{type}/{settlementDateEpoch}")]
        public HttpResponseMessage TradeItemDW(string type, long settlementDateEpoch, DataSourceLoadOptions loadOptions)
        {
            try
            {
                using (var db = new kashflowDBEntities())
                {
                    var settlementDate = Common.ConvertEpochToDateTime(settlementDateEpoch);

                    if (settlementDate != null)
                    {
                        type = type.ToUpper();
                        settlementDate = settlementDate.Value.Date;

                        var result = db.EDW_TradeItem.AsNoTracking().Where(x => x.InstrumentType == type && x.SettlementDate == settlementDate).ToList();
                        
                        return Request.CreateResponse(DataSourceLoader.Load(result, loadOptions));
                    }
                    else
                    {
                        return Request.CreateResponse(HttpStatusCode.BadRequest, "Failed convert to actual date");
                    }
                }
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest, ex.Message);
            }

        }

        [HttpGet]
        [Route("GetOpeningBalanceEdw/{settlementDateEpoch}/{currency}")]
        public HttpResponseMessage GetOpeningBalance(long settlementDateEpoch, string currency, DataSourceLoadOptions loadOptions)
        {
            try
            {
                using (var db = new kashflowDBEntities())
                {
                    var settlementDate = Common.ConvertEpochToDateTime(settlementDateEpoch);

                    if (settlementDate != null)
                    {
                        settlementDate = settlementDate.Value.Date;

                        var result =
                            db.EDW_BankBalance.AsNoTracking()
                                .Where(x =>
                                    x.SettlementDate == settlementDate && x.Currency == currency)
                                .GroupBy(x => new { x.Currency, x.InstrumentType })
                                .Select(x => new
                                {
                                    instrumentType = x.Key.InstrumentType,
                                    currency = x.Key.Currency,
                                    amount = x.Sum(y => y.Amount)
                                }).ToList();

                        return Request.CreateResponse(HttpStatusCode.OK, DataSourceLoader.Load(result, loadOptions));
                    }
                    else
                    {
                        return Request.CreateResponse(HttpStatusCode.BadRequest, "Failed convert to actual date");
                    }
                }
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest, ex.Message);
            }
        }



        #region Trade Settlement Grid

        [HttpGet]
        [Route("GetTradeSettlementFromEdw/{type}/{settlementDateEpoch}/{currency}")]
        public HttpResponseMessage GetTradeSettlementFromEdw(string type, long settlementDateEpoch, string currency, DataSourceLoadOptions loadOptions)
        {
            try
            {
                using (var db = new kashflowDBEntities())
                {
                    var settlementDate = Common.ConvertEpochToDateTime(settlementDateEpoch);

                    if (settlementDate != null)
                    {
                        type = type.ToUpper();
                        settlementDate = settlementDate.Value.Date;

                        var result = db.EDW_TradeItem.AsNoTracking().Where(x =>
                            x.InstrumentType == type && x.SettlementDate == settlementDate &&
                            x.Currency == currency);

                        var finalResult = new List<ISSD_TradeSettlement>();
                        foreach (var item in result)
                        {
                            var tradeItem = new ISSD_TradeSettlement
                            {
                                InstrumentType = item.InstrumentType,
                                InstrumentCode = item.InstrumentName,
                                StockCode = string.IsNullOrEmpty(item.ISIN) ? item.StockCode : string.Concat(item.StockCode, " / " + item.ISIN),
                                Maturity = (decimal?)((item.Type == "M" && item.InstrumentType != Common.TsItemCategory.Repo) ? item.Amount : 0),
                                Sales = (decimal?)((item.Type == "S" && item.InstrumentType != Common.TsItemCategory.Repo) ? item.Amount : 0),
                                Purchase = (decimal?)((item.Type == "P" && item.InstrumentType != Common.TsItemCategory.Repo) ? item.Amount : 0),
                                SecondLeg = (decimal?)((item.InstrumentType == Common.TsItemCategory.Repo) ? item.Amount : 0),
                                AmountPlus = (decimal?)((item.InstrumentType == Common.TsItemCategory.Coupon) ? item.Amount : 0),
                            };

                            finalResult.Add(tradeItem);
                        }

                        return Request.CreateResponse(DataSourceLoader.Load(finalResult, loadOptions));
                    }
                    else
                    {
                        return Request.CreateResponse(HttpStatusCode.BadRequest, "Failed convert to actual date");
                    }
                }
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest, ex.Message);
            }
        }

        [HttpGet]
        [Route("TradeSettlement/TradeItem/{formId}/{tradeType}")]
        public HttpResponseMessage GetTradeItem(string formId, string tradeType, DataSourceLoadOptions loadOptions)
        {
            try
            {
                using (var db = new kashflowDBEntities())
                {
                    var result = new List<ISSD_TradeSettlement>();

                    if (!string.IsNullOrEmpty(formId))
                    {
                        var formIdParsed = Convert.ToInt32(formId);
                        var instrumentType = Common.TsCategoryUrlParamMapping(tradeType);
                        result = db.ISSD_TradeSettlement
                            .Where(x => x.FormId == formIdParsed && x.InstrumentType == instrumentType).ToList();
                        return Request.CreateResponse(DataSourceLoader.Load(result, loadOptions));
                    }
                    else
                    {
                        return Request.CreateResponse(DataSourceLoader.Load(result, loadOptions));
                    }
                }
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest, ex.Message);
            }
        }

        [HttpGet]
        [Route("TradeSettlement/TradeItemConsolidated/{tradeType}/{settlementDateEpoch}/{currency}")]
        public HttpResponseMessage GetTradeItemConsolidated(string tradeType, long settlementDateEpoch, string currency, DataSourceLoadOptions loadOptions)
        {
            try
            {
                var settlementDate = Common.ConvertEpochToDateTime(settlementDateEpoch);
                var settlementDateOnly = settlementDate.Value.Date;
                var formType = Common.TsUrlParamInstrumentTypeMapFormType(tradeType);

                using (var db = new kashflowDBEntities())
                {
                    var result = new List<ISSD_TradeSettlement>();

                    var getForm = db.ISSD_FormHeader.FirstOrDefault(x =>
                        DbFunctions.TruncateTime(x.SettlementDate) == settlementDateOnly && x.Currency == currency &&
                        x.FormStatus == "Approved" && x.FormType == formType);

                    if (getForm != null)
                    {
                        var instrumentType = Common.TsCategoryUrlParamMapping(tradeType);
                        result = db.ISSD_TradeSettlement
                            .Where(x => x.FormId == getForm.Id && x.InstrumentType == instrumentType).ToList();
                        return Request.CreateResponse(DataSourceLoader.Load(result, loadOptions));
                    }
                    else
                    {
                        return Request.CreateResponse(DataSourceLoader.Load(result, loadOptions));
                    }
                }
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest, ex.Message);
            }
        }


        [HttpPut]
        [Route("TradeSettlement/TradeItem/")]
        public HttpResponseMessage UpdateTradeItem(FormDataCollection form)
        {
            using (var db = new kashflowDBEntities())
            {
                var key = Convert.ToInt32(form.Get("key"));
                var values = form.Get("values");
                var existingRecord = db.ISSD_TradeSettlement.SingleOrDefault(o => o.Id == key);

                JsonConvert.PopulateObject(values, existingRecord);

                if (existingRecord != null)
                {
                    existingRecord.ModifiedBy = User.Identity.Name;
                    existingRecord.ModifiedDate = DateTime.Now;
                }

                Validate(existingRecord);

                if (!ModelState.IsValid)
                    return Request.CreateErrorResponse(HttpStatusCode.BadRequest, ModelState);

                db.SaveChanges();

                return Request.CreateResponse(HttpStatusCode.OK);
            }

        }
        
        #endregion

    }
}
