﻿using System;
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
using xDC.Utils;
using xDC_Web.Models;

namespace xDC_Web.Controllers.Api
{
    [Authorize(Roles = "Administrator, Power User, ISSD")]
    [RoutePrefix("api/issd")]
    public class IssdController : ApiController
    {
        [HttpGet]
        [Route("GetIssdForm")]
        public HttpResponseMessage GetIssdForm(DataSourceLoadOptions loadOptions)
        {
            try
            {
                using (var db = new xDC.Infrastructure.Application.kashflowDBEntities())
                {
                    var issdFormTypes = new List<string>()
                    {
                        Common.FormTypeMapping(2)
                    };

                    var result = db.Form_Header
                        .Where(x => issdFormTypes.Contains(x.FormType)).ToList();

                    var getApprover = db.Config_Approver.Where(x => x.Username == User.Identity.Name);
                    var isMeApprover = getApprover.Any();

                    var resultVM = new List<ViewFormHeaderModel>();

                    foreach (var item in result)
                    {
                        resultVM.Add(new ViewFormHeaderModel
                        {
                            Id = item.Id,
                            FormType = item.FormType,
                            FormDate = item.FormDate,
                            FormStatus = item.FormStatus,
                            Currency = item.Currency,
                            PreparedBy = item.PreparedBy,
                            PreparedDate = item.PreparedDate,
                            ApprovedBy = item.ApprovedBy,
                            ApprovedDate = item.ApprovedDate,
                            AdminEditted = item.AdminEditted,
                            AdminEdittedBy = item.AdminEdittedBy,
                            AdminEdittedDate = item.AdminEdittedDate,

                            IsDraft = (item.FormStatus == Common.FormStatusMapping(0)),

                            IsMeCanEditDraft = (User.IsInRole(Config.AclIssd) && !isMeApprover),


                            IsMyFormRejected = (User.Identity.Name == item.PreparedBy && item.FormStatus == Common.FormStatusMapping(4)),
                            IsFormPendingMyApproval = (User.Identity.Name == item.ApprovedBy && item.FormStatus == Common.FormStatusMapping(2)),
                            IsFormOwner = (User.Identity.Name == item.PreparedBy),
                            IsCanAdminEdit = (User.IsInRole(Config.AclPowerUser)),
                            IsResubmitEnabled = (item.FormStatus == "Rejected" && User.IsInRole(Config.AclAmsd) && User.Identity.Name != item.ApprovedBy)
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

                            IsDraft = (item.FormStatus == Common.FormStatusMapping(0)),

                            IsMeCanEditDraft = (User.IsInRole(Config.AclIssd) && !isMeApprover),


                            IsMyFormRejected = (User.Identity.Name == item.PreparedBy && item.FormStatus == Common.FormStatusMapping(4)),
                            IsFormPendingMyApproval = (User.Identity.Name == item.ApprovedBy && item.FormStatus == Common.FormStatusMapping(2)),
                            IsFormOwner = (User.Identity.Name == item.PreparedBy),
                            IsCanAdminEdit = (User.IsInRole(Config.AclPowerUser)),
                            IsResubmitEnabled = (item.FormStatus == "Rejected" && User.IsInRole(Config.AclAmsd) && User.Identity.Name != item.ApprovedBy)
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
                using (var db = new kashflowDBEntities())
                {
                    var newFormHeader = new ISSD_FormHeader()
                    {
                        FormType = Common.FormTypeMapping(inputs.FormType),
                        PreparedBy = User.Identity.Name,
                        PreparedDate = DateTime.Now,
                        FormStatus = (inputs.IsSaveAsDraft) ? Common.FormStatusMapping(0) : Common.FormStatusMapping(2),
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
                                InstrumentType = Common.TradeSettlementMapping(1),
                                InstrumentCode = item.InstrumentCode,
                                StockCode = item.StockCode,
                                Maturity = item.Maturity,
                                Sales = item.Sales,
                                Purchase = item.Purchase,
                                Remarks = item.Remarks,
                                CreatedBy = User.Identity.Name,
                                CreatedDate = DateTime.Now
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
                                InstrumentType = Common.TradeSettlementMapping(2),
                                InstrumentCode = item.InstrumentCode,
                                StockCode = item.StockCode,
                                Maturity = item.Maturity,
                                Sales = item.Sales,
                                Purchase = item.Purchase,
                                Remarks = item.Remarks,
                                CreatedBy = User.Identity.Name,
                                CreatedDate = DateTime.Now
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
                                InstrumentType = Common.TradeSettlementMapping(3),
                                InstrumentCode = item.InstrumentCode,
                                StockCode = item.StockCode,
                                Maturity = item.Maturity,
                                Sales = item.Sales,
                                Purchase = item.Purchase,
                                Remarks = item.Remarks,
                                CreatedBy = User.Identity.Name,
                                CreatedDate = DateTime.Now
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
                                InstrumentType = Common.TradeSettlementMapping(4),
                                InstrumentCode = item.InstrumentCode,
                                StockCode = item.StockCode,
                                Maturity = item.Maturity,
                                Sales = item.Sales,
                                Purchase = item.Purchase,
                                Remarks = item.Remarks,
                                CreatedBy = User.Identity.Name,
                                CreatedDate = DateTime.Now
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
                                InstrumentType = Common.TradeSettlementMapping(5),
                                InstrumentCode = item.InstrumentCode,
                                StockCode = item.StockCode,
                                FirstLeg = item.FirstLeg,
                                SecondLeg = item.SecondLeg,
                                Remarks = item.Remarks,
                                CreatedBy = User.Identity.Name,
                                CreatedDate = DateTime.Now
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
                                InstrumentType = Common.TradeSettlementMapping(6),
                                InstrumentCode = item.InstrumentCode,
                                StockCode = item.StockCode,
                                AmountPlus = item.AmountPlus,
                                Remarks = item.Remarks,
                                CreatedBy = User.Identity.Name,
                                CreatedDate = DateTime.Now
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
                                InstrumentType = Common.TradeSettlementMapping(7),
                                InstrumentCode = item.InstrumentCode,
                                AmountPlus = item.AmountPlus,
                                Remarks = item.Remarks,
                                CreatedBy = User.Identity.Name,
                                CreatedDate = DateTime.Now
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
                                InstrumentType = Common.TradeSettlementMapping(8),
                                InstrumentCode = item.InstrumentCode,
                                StockCode = item.StockCode,
                                AmountPlus = item.AmountPlus,
                                AmountMinus = item.AmountMinus,
                                Remarks = item.Remarks,
                                CreatedBy = User.Identity.Name,
                                CreatedDate = DateTime.Now
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
                                InstrumentType = Common.TradeSettlementMapping(9),
                                InstrumentCode = item.InstrumentCode,
                                AmountPlus = item.AmountPlus,
                                AmountMinus = item.AmountMinus,
                                Remarks = item.Remarks,
                                CreatedBy = User.Identity.Name,
                                CreatedDate = DateTime.Now
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
                                InstrumentType = Common.TradeSettlementMapping(10),
                                InstrumentCode = item.InstrumentCode,
                                AmountPlus = item.AmountPlus,
                                Remarks = item.Remarks,
                                CreatedBy = User.Identity.Name,
                                CreatedDate = DateTime.Now
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
                                InstrumentType = Common.TradeSettlementMapping(11),
                                InstrumentCode = item.InstrumentCode,
                                AmountPlus = item.AmountPlus,
                                AmountMinus = item.AmountMinus,
                                Remarks = item.Remarks,
                                CreatedBy = User.Identity.Name,
                                CreatedDate = DateTime.Now
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
                                InstrumentType = Common.TradeSettlementMapping(12),
                                InstrumentCode = item.InstrumentCode,
                                AmountPlus = item.AmountPlus,
                                AmountMinus = item.AmountMinus,
                                Remarks = item.Remarks,
                                CreatedBy = User.Identity.Name,
                                CreatedDate = DateTime.Now
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
                        getForm.FormStatus = Common.FormStatusMapping(2);
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
                                    foundItem.CreatedBy = User.Identity.Name;
                                    foundItem.CreatedDate = DateTime.Now;
                                }
                            }
                        }
                    }

                    /*
                    if (inputs.Coupon != null)
                    {
                        foreach (var item in inputs.Coupon)
                        {
                            db.ISSD_TradeSettlement.Add(new ISSD_TradeSettlement()
                            {
                                FormId = getForm.Id,
                                InstrumentType = Common.TradeSettlementMapping(6),
                                InstrumentCode = item.InstrumentCode,
                                StockCode = item.StockCode,
                                Maturity = item.Maturity,
                                Sales = item.Sales,
                                Purchase = item.Purchase,
                                AmountPlus = item.AmountPlus,
                                AmountMinus = item.AmountMinus,
                                SecondLeg = item.SecondLeg,
                                FirstLeg = item.FirstLeg,
                                CreatedBy = User.Identity.Name,
                                CreatedDate = DateTime.Now
                            });
                        }
                    }

                    if (inputs.Fees != null)
                    {
                        foreach (var item in inputs.Fees)
                        {
                            db.ISSD_TradeSettlement.Add(new ISSD_TradeSettlement()
                            {
                                FormId = getForm.Id,
                                InstrumentType = Common.TradeSettlementMapping(7),
                                InstrumentCode = item.InstrumentCode,
                                StockCode = item.StockCode,
                                Maturity = item.Maturity,
                                Sales = item.Sales,
                                Purchase = item.Purchase,
                                AmountPlus = item.AmountPlus,
                                AmountMinus = item.AmountMinus,
                                SecondLeg = item.SecondLeg,
                                FirstLeg = item.FirstLeg,
                                CreatedBy = User.Identity.Name,
                                CreatedDate = DateTime.Now
                            });
                        }
                    }

                    if (inputs.Mtm != null)
                    {
                        foreach (var item in inputs.Mtm)
                        {
                            db.ISSD_TradeSettlement.Add(new ISSD_TradeSettlement()
                            {
                                FormId = getForm.Id,
                                InstrumentType = Common.TradeSettlementMapping(8),
                                InstrumentCode = item.InstrumentCode,
                                StockCode = item.StockCode,
                                Maturity = item.Maturity,
                                Sales = item.Sales,
                                Purchase = item.Purchase,
                                AmountPlus = item.AmountPlus,
                                AmountMinus = item.AmountMinus,
                                SecondLeg = item.SecondLeg,
                                FirstLeg = item.FirstLeg,
                                CreatedBy = User.Identity.Name,
                                CreatedDate = DateTime.Now
                            });
                        }
                    }

                    if (inputs.FxSettlement != null)
                    {
                        foreach (var item in inputs.FxSettlement)
                        {
                            db.ISSD_TradeSettlement.Add(new ISSD_TradeSettlement()
                            {
                                FormId = getForm.Id,
                                InstrumentType = Common.TradeSettlementMapping(9),
                                InstrumentCode = item.InstrumentCode,
                                StockCode = item.StockCode,
                                Maturity = item.Maturity,
                                Sales = item.Sales,
                                Purchase = item.Purchase,
                                AmountPlus = item.AmountPlus,
                                AmountMinus = item.AmountMinus,
                                SecondLeg = item.SecondLeg,
                                FirstLeg = item.FirstLeg,
                                CreatedBy = User.Identity.Name,
                                CreatedDate = DateTime.Now
                            });
                        }
                    }

                    if (inputs.ContributionCredited != null)
                    {
                        foreach (var item in inputs.ContributionCredited)
                        {
                            db.ISSD_TradeSettlement.Add(new ISSD_TradeSettlement()
                            {
                                FormId = getForm.Id,
                                InstrumentType = Common.TradeSettlementMapping(10),
                                InstrumentCode = item.InstrumentCode,
                                StockCode = item.StockCode,
                                Maturity = item.Maturity,
                                Sales = item.Sales,
                                Purchase = item.Purchase,
                                AmountPlus = item.AmountPlus,
                                AmountMinus = item.AmountMinus,
                                SecondLeg = item.SecondLeg,
                                FirstLeg = item.FirstLeg,
                                CreatedBy = User.Identity.Name,
                                CreatedDate = DateTime.Now
                            });
                        }
                    }

                    if (inputs.Altid != null)
                    {
                        foreach (var item in inputs.Altid)
                        {
                            db.ISSD_TradeSettlement.Add(new ISSD_TradeSettlement()
                            {
                                FormId = getForm.Id,
                                InstrumentType = Common.TradeSettlementMapping(11),
                                InstrumentCode = item.InstrumentCode,
                                StockCode = item.StockCode,
                                Maturity = item.Maturity,
                                Sales = item.Sales,
                                Purchase = item.Purchase,
                                AmountPlus = item.AmountPlus,
                                AmountMinus = item.AmountMinus,
                                SecondLeg = item.SecondLeg,
                                FirstLeg = item.FirstLeg,
                                CreatedBy = User.Identity.Name,
                                CreatedDate = DateTime.Now
                            });
                        }
                    }

                    if (inputs.Others != null)
                    {
                        foreach (var item in inputs.Others)
                        {
                            db.ISSD_TradeSettlement.Add(new ISSD_TradeSettlement()
                            {
                                FormId = getForm.Id,
                                InstrumentType = Common.TradeSettlementMapping(12),
                                InstrumentCode = item.InstrumentCode,
                                StockCode = item.StockCode,
                                Maturity = item.Maturity,
                                Sales = item.Sales,
                                Purchase = item.Purchase,
                                AmountPlus = item.AmountPlus,
                                AmountMinus = item.AmountMinus,
                                SecondLeg = item.SecondLeg,
                                FirstLeg = item.FirstLeg,
                                CreatedBy = User.Identity.Name,
                                CreatedDate = DateTime.Now
                            });
                        }
                    }*/
                    
                    db.SaveChanges();

                    return Request.CreateResponse(HttpStatusCode.Created, getForm.Id);
                }
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest, ex.Message);
            }

        }

        /*
         * To delete draft form
         *
         */
        [HttpDelete]
        [Authorize(Roles = "Administrator, ISSD")]
        [Route("TradeSettlement/")]
        public HttpResponseMessage DeleteTradeSettlement(FormDataCollection form)
        {
            try
            {
                using (var db = new kashflowDBEntities())
                {
                    var key = Convert.ToInt32(form.Get("id"));

                    var formHeader = db.Form_Header.FirstOrDefault(x => x.Id == key);
                    var inflowFunds = db.ISSD_TradeSettlement.Where(x => x.FormId == formHeader.Id);
                    
                    db.Form_Header.Remove(formHeader);
                    db.ISSD_TradeSettlement.RemoveRange(inflowFunds);
                    db.SaveChanges();

                    return Request.CreateResponse(HttpStatusCode.OK);
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

        #endregion


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
                                StockCode = string.IsNullOrEmpty(item.ISIN)?item.StockCode:string.Concat(item.StockCode, " / " + item.ISIN),
                                Maturity = (decimal?)((item.Type == "M" && item.InstrumentType != Common.TradeSettlementMapping(5)) ? item.Amount : 0),
                                Sales = (decimal?)((item.Type == "S" && item.InstrumentType != Common.TradeSettlementMapping(5)) ? item.Amount : 0),
                                Purchase = (decimal?)((item.Type == "P" && item.InstrumentType != Common.TradeSettlementMapping(5)) ? item.Amount : 0),
                                SecondLeg = (decimal?)((item.InstrumentType == Common.TradeSettlementMapping(5)) ? item.Amount : 0),
                                AmountPlus = (decimal?)((item.InstrumentType == Common.TradeSettlementMapping(6)) ? item.Amount : 0),
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
                        var instrumentType = Common.TradeSettlementUrlParamMapping(tradeType);
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
                    existingRecord.UpdatedBy = User.Identity.Name;
                    existingRecord.UpdatedDate = DateTime.Now;
                }

                Validate(existingRecord);

                if (!ModelState.IsValid)
                    return Request.CreateErrorResponse(HttpStatusCode.BadRequest, ModelState);

                db.SaveChanges();

                return Request.CreateResponse(HttpStatusCode.OK);
            }

        }

        [HttpPost]
        [Route("TradeSettlement/TradeItem/")]
        public HttpResponseMessage InsertTradeItem(FormDataCollection form)
        {
            using (var db = new kashflowDBEntities())
            {
                var values = form.Get("values");

                var newRecord = new AMSD_InflowFund();
                JsonConvert.PopulateObject(values, newRecord);

                newRecord.CreatedBy = User.Identity.Name;
                newRecord.CreatedDate = DateTime.Now;

                Validate(newRecord);

                if (!ModelState.IsValid)
                    return Request.CreateErrorResponse(HttpStatusCode.BadRequest, ModelState);

                db.AMSD_InflowFund.Add(newRecord);
                db.SaveChanges();

                return Request.CreateResponse(HttpStatusCode.Created, newRecord);
            }
        }

        [HttpDelete]
        [Route("TradeSettlement/TradeItem/")]
        public HttpResponseMessage DeleteTradeItem(FormDataCollection form)
        {
            using (var db = new kashflowDBEntities())
            {
                var key = Convert.ToInt32(form.Get("key"));
                var foundRecord = db.AMSD_InflowFund.First(x => x.Id == key);

                db.AMSD_InflowFund.Remove(foundRecord);
                db.SaveChanges();

                return Request.CreateResponse(HttpStatusCode.OK);
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

    }
}
