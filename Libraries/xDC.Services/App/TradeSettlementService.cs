using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using xDC.Domain.ISSD_TS;
using xDC.Domain.Web.ISSD.TradeSettlementForm;
using xDC.Infrastructure.Application;
using xDC.Utils;

namespace xDC.Services.App
{
    public static class TradeSettlementSvc
    {
        public static void NotifyApprover(string approverUsername, int formId, string submittedBy, string formType, string notes)
        {
            new NotificationService().NotifyApprovalRequest(approverUsername, formId, submittedBy, formType);
            new MailService().SubmitForApproval(formId, formType, approverUsername, notes);
            new WorkflowService().SubmitForApprovalWorkflow(formId, formType, notes);
        }

        public static void NotifyPreparer(int formId, string formType, string formStatus, string preparedBy, string approvedBy, string approvalNotes)
        {
            new NotificationService().NotifyApprovalResult(preparedBy, formId, approvedBy, formType, formStatus);
            new MailService().SendApprovalStatus(formId, formType, formStatus, preparedBy, approvalNotes);
            new WorkflowService().ApprovalResponse(formId, formStatus, approvalNotes, formType, preparedBy, approvedBy);
        }

        public static Form_Workflow GetLatestWorkflow(kashflowDBEntities db, int formId, string formType)
        {
            var result = db.Form_Workflow
                            .Where(x => x.FormId == formId && x.FormType == formType)
                            .OrderByDescending(x => x.RecordedDate)
                            .FirstOrDefault();

            if (result != null && result.WorkflowStatus == Common.FormStatus.PendingApproval)
            {
                return null;
            }
            else
            {
                return result;
            }
        }

        public static List<ISSD_TradeSettlement> GetTradeSettlement(kashflowDBEntities db, int formId)
        {
            var result = db.ISSD_TradeSettlement
                .Where(x => x.FormId == formId)
                .ToList();

            return result;
        }

        public static List<ISSD_TradeSettlement> GetTradeSettlement(kashflowDBEntities db, DateTime settlementDate, string currency)
        {
            var forms = db.ISSD_FormHeader.Where(x =>
                DbFunctions.TruncateTime(x.SettlementDate) == DbFunctions.TruncateTime(settlementDate) &&
                x.Currency == currency);
            
            var trades = new List<ISSD_TradeSettlement>();

            if (forms.Any())
            {
                foreach (var formId in forms.Select(x => x.Id).ToList())
                {
                    var tradesFromId = db.ISSD_TradeSettlement.Where(x => x.FormId == formId).ToList();
                    if (tradesFromId.Any())
                    {
                        trades.AddRange(tradesFromId);
                    }
                }
                
            }

            return trades;
        }

        public static List<TsOpeningBalance> GetOpeningBalance(kashflowDBEntities db, DateTime settlementDate, string currency)
        {
            var result = new List<TsOpeningBalance>();

            var ob = db.EDW_BankBalance
                .AsNoTracking()
                .Where(x =>
                    DbFunctions.TruncateTime(x.SettlementDate) ==
                    DbFunctions.TruncateTime(settlementDate) && x.Currency == currency)
                .GroupBy(x => new { x.SettlementDate, x.InstrumentType })
                .Select(x => new
                {
                    account = x.Key.InstrumentType,
                    total = x.Sum(y => y.Amount ?? 0)
                });

            foreach (var item in ob)
            {
                result.Add(new TsOpeningBalance()
                {
                    Account = item.account,
                    Amount = item.total
                });
            }

            return result;
        }

        public static TS_TotalFlow GetTotalFlow(kashflowDBEntities db, List<int> formId, DateTime settlementDate, string currency)
        {
            var trades = new List<ISSD_TradeSettlement>();

            foreach (var id in formId)
            {
                var tradesFromId = db.ISSD_TradeSettlement.Where(x => x.FormId == id).ToList();
                if (tradesFromId.Any())
                {
                    trades.AddRange(tradesFromId);
                }
            }

            var totalFlow = trades.GroupBy(x => 1)
                .Select(x => new
                {
                    totalMaturity = x.Sum(y => y.Maturity),
                    totalSales = x.Sum(y => y.Sales),
                    totalFirstLeg = x.Sum(y => y.FirstLeg),
                    totalAmountPlus = x.Sum(y => y.AmountPlus),

                    totalPurchase = x.Sum(y => y.Purchase),
                    totalSecondLeg = x.Sum(y => y.SecondLeg),
                    totalAmountMinus = x.Sum(y => y.AmountMinus)
                })
                .FirstOrDefault();

            double cbInflow = 0;
            double cbOutflow = 0;

            if (totalFlow != null)
            {
                cbInflow = totalFlow.totalMaturity + totalFlow.totalSales + totalFlow.totalFirstLeg +
                           totalFlow.totalAmountPlus;
                cbOutflow = totalFlow.totalPurchase + totalFlow.totalSecondLeg + totalFlow.totalAmountMinus;
            }
             
            var result = new TS_TotalFlow()
            {
                Currency = currency,
                SettlementDate = settlementDate,
                Inflow = cbInflow,
                Outflow = cbOutflow
            };

            return result;
        }

        public static bool EnableEdit(string formStatus, string formApprover, string currentUser)
        {
            var isPendingApproval = formStatus == Common.FormStatus.PendingApproval;
            var isFormApprover = formApprover == currentUser;

            return !isPendingApproval && !isFormApprover;
        }

        public static bool EnableDelete(string formStatus)
        {
            var isApproved = (formStatus == Common.FormStatus.Approved);
            var isPendingApproval = formStatus == Common.FormStatus.PendingApproval;

            return !isApproved && !isPendingApproval;
        }

        public static bool EnablePrint(string formStatus)
        {
            var isDraftForm = formStatus == Common.FormStatus.Draft;

            return !isDraftForm;
        }

        public static bool EditFormRules(string formStatus, string approvedBy, string currentUser, out string errorMessage)
        {
            var isPendingApproval = formStatus == Common.FormStatus.PendingApproval;
            var isYouAreTheApprover = approvedBy == currentUser;

            if (isPendingApproval)
            {
                errorMessage = "Form is still in Pending Approval status";
                return true;
            }
            else if (isYouAreTheApprover)
            {
                errorMessage = "You are this form Approver. Then you cannot edit it";
                return true;
            }
            else
            {
                errorMessage = null;
                return false;
            }
        }

        public static bool NewFormRules(kashflowDBEntities db, DateTime settlementDate, string currency, string formType, out string errorMessage)
        {
            var isSameDateFormExist = db.ISSD_FormHeader.Any(x =>
                DbFunctions.TruncateTime(x.SettlementDate) == DbFunctions.TruncateTime(settlementDate.Date) 
                && x.Currency == currency 
                && x.FormType == formType);
            
            if (isSameDateFormExist)
            {
                errorMessage = "Similar form with same Form Part, Currency and Settlement has been already created. Use that instead";
                return true;
            }
            else
            {
                errorMessage = null;
                return false;
            }
        }

        public static double GetTotalInflowByCategory(kashflowDBEntities db, List<int> approvedFormIds, string category)
        {
            var result = db.ISSD_TradeSettlement
                .Where(x => approvedFormIds.Contains(x.FormId) && x.InstrumentType == category)
                .GroupBy(x => x.InstrumentType)
                .Select(x => new
                {
                    InstrumentType = x.Key,
                    AmountPlus = x.Sum(y => y.AmountPlus),
                    Sales = x.Sum(y => y.Sales),
                    Maturity = x.Sum(y => y.Maturity),
                    FirstLeg = x.Sum(y => y.FirstLeg)
                })
                .Select(x => (x.Sales + x.Maturity + x.AmountPlus + x.FirstLeg))
                .FirstOrDefault();

            return result;
        }
        
        public static double GetTotalOutflowByCategory(kashflowDBEntities db, List<int> approvedFormIds, string category)
        {
            var result = db.ISSD_TradeSettlement
                .Where(x => approvedFormIds.Contains(x.FormId) && x.InstrumentType == category)
                .GroupBy(x => x.InstrumentType)
                .Select(x => new
                {
                    InstrumentType = x.Key,
                    AmountMinus = x.Sum(y => y.AmountMinus),
                    Purchase = x.Sum(y => y.Purchase),
                    SecondLeg = x.Sum(y => y.SecondLeg)
                })
                .Select(x => (x.AmountMinus + x.Purchase + x.SecondLeg))
                .FirstOrDefault();

            return result;
        }
        
    }
}
