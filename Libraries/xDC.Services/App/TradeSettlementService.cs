using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using xDC.Domain.ISSD_TS;
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
            new MailService().ContributionCreditedTradeSettlement(formId);
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

        public static List<TS_OpeningBalance> GetOpeningBalance(kashflowDBEntities db, DateTime settlementDate, string currency)
        {
            var result = new List<TS_OpeningBalance>();

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
                result.Add(new TS_OpeningBalance()
                {
                    Account = item.account,
                    Amount = (decimal)item.total
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
                    totalMaturity = x.Sum(y => y.Maturity??0),
                    totalSales = x.Sum(y => y.Sales ?? 0),
                    totalFirstLeg = x.Sum(y => y.FirstLeg ?? 0),
                    totalAmountPlus = x.Sum(y => y.AmountPlus ?? 0),

                    totalPurchase = x.Sum(y => y.Purchase ?? 0),
                    totalSecondLeg = x.Sum(y => y.SecondLeg ?? 0),
                    totalAmountMinus = x.Sum(y => y.AmountMinus ?? 0)
                })
                .FirstOrDefault();

            decimal cbInflow = 0;
            decimal cbOutflow = 0;

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

            var isTMinus = settlementDate.Date < DateTime.Now.Date;

            if (isSameDateFormExist)
            {
                errorMessage = "Similar form has been created. Use that instead";
                return true;
            }
            else if (isTMinus)
            {
                errorMessage = "Submission for Settlement Date T-n is not allowed";
                return true;
            }
            else
            {
                errorMessage = null;
                return false;
            }
        }

    }
}
