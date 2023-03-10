using System;
using System.Collections.Generic;
using System.Linq;
using System.ComponentModel;
using System.ComponentModel.Composition;

using Rock;
using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;
using Rock.Workflow;
using Rock.Financial;
using Rock.Transactions;
using static System.Collections.Specialized.BitVector32;


namespace com.kevinrutledge.Workflow.Action
{
    [ActionCategory("com_kevinrutledge: Workflow Power Pack")]
    [Description("Manually creates a financial transaction using supplied information.")]
    [Export(typeof(Rock.Workflow.ActionComponent))]
    [ExportMetadata("ComponentName", "Create Financial Transaction")]

    [WorkflowAttribute("Authorized Person Attribute", "The person attribute to assign the transaction to",
       true, "", "", 0, AttributeKey.PERSON_ATTRIBUTE_KEY, new string[] { "Rock.Field.Types.PersonFieldType" })]
    [WorkflowAttribute("Currency Type Value", "The currency type for the transaction.", false, "", "", 1, AttributeKey.Currency_Type_Attribute_Key, new String[] { "Rock.Field.Types.DefinedValueFieldType" })]
    [WorkflowAttribute("Financial Account", "Workflow attribute that contains the target account.", true, "", "", 2, AttributeKey.FINANCIAL_ACCOUNT_ATTRIBUTE_KEY, new string[] { "Rock.Field.Types.AccountFieldType" })]
    [WorkflowAttribute("Amount", "Workflow attribute that contains the amount to charge.", true, "", "", 3, AttributeKey.AMOUNT_ATTRIBUTE_KEY, new string[] { "Rock.Field.Types.CurrencyFieldType" })]
    [WorkflowAttribute("Total Fee Amount", "The Total Fee Amount", false, "", "", 4, AttributeKey.Fee_Amount_Attribute_Key, new string[] { "Rock.Field.Types.CurrencyFieldType" })]
    [WorkflowAttribute("Transaction Date Time", "Workflow Attribute that contains the transaction date/time.", true, "", "", 5, AttributeKey.Transaction_Date_Time_Attribute_Key, new string[] { "Rock.Field.Types.DateTimeFieldType" })]
    [WorkflowAttribute("Deposit Date", "Workflow Attribute that contains the transaction deposit date date/time.", true, "", "", 5, AttributeKey.Deposit_Date_Attribute_Key, new string[] { "Rock.Field.Types.DateTimeFieldType" })]
    [WorkflowAttribute("Transaction Summary", "The summary to add to the transaction.", false, "", "", 14, AttributeKey.SUMMARY_DETAILS_ATTRIBUTE_KEY, new string[] { "Rock.Field.Types.TextFieldType" })]
    [WorkflowAttribute("Transaction Source", "The defined value for the transaction source.", false, "", "", 15, AttributeKey.Source_Value_Attribute_Key, new string[] { "Rock.Field.Types.DefinedValueFieldType" })]
    [WorkflowTextOrAttribute("Transaction Code", "Attribute Value", "The code to add to the transaction. <span class='tip tip-lava'></span>",
       false, "", "", 16, AttributeKey.Transaction_Code_Attribute_Key, new string[] { "Rock.Field.Types.TextFieldType" })]
    [WorkflowTextOrAttribute("Batch Prefix", "Attribute Value", "The prefix to use for any batches created. <span class='tip tip-lava'></span>",
       false, "", "", 17, AttributeKey.BATCH_PREFIX_ATTRIBUTE_KEY, new string[] { "Rock.Field.Types.TextFieldType" })]
    [WorkflowAttribute("Created Transaction Attribute", "The text attribute you want to save the created transaction's Guid", false, "", "", 18, AttributeKey.Created_Transaction_Attribute_Key, new string[] { "Rock.Field.Types.TextFieldType" })]
    [BooleanField("Continue On Error", "Should processing continue even if SQL Error occurs?", false, "",19,AttributeKey.Continue_On_Error_Attribute_Key)]
    [WorkflowAttribute("Where to Save HTML Formatted Error List","The Attribute to store an html formatted error list for later display.",false,"","",20,AttributeKey.HTML_ERROR_LIST_ATTRIBUTE_KEY, new string[] { "Rock.Field.Types.MemoFieldType" })]
    /// <summary>
    /// Manually Creates a financial transaction with values passed to the workflow action.
    /// </summary>
    public class CreateFinancialTransaction : ActionComponent
    {
        private static class AttributeKey
        {

            public const string FINANCIAL_BATCH_ATTRIBUTE_KEY = "FinancialBatchIdGuid";
            public const string FINANCIAL_ACCOUNT_ATTRIBUTE_KEY = "FinancialAccount";
            public const string AMOUNT_ATTRIBUTE_KEY = "TransactionAmount";
            public const string PERSON_ATTRIBUTE_KEY = "PersonAttribute";
            public const string BATCH_PREFIX_ATTRIBUTE_KEY = "BatchPrefix";
            public const string Transaction_Date_Time_Attribute_Key = "TransactionDateTime";
            public const string SUMMARY_DETAILS_ATTRIBUTE_KEY = "Summary";
            public const string Currency_Type_Attribute_Key = "CurrencyType";
            public const string Fee_Amount_Attribute_Key = "FeeAmount";
            public const string Deposit_Date_Attribute_Key = "DepositDate";
            public const string Transaction_Code_Attribute_Key = "TransactionCode";
            public const string Source_Value_Attribute_Key = "SourceValue";
            public const string Created_Transaction_Attribute_Key = "CreatedFinancialTransaction";
            public const string Continue_On_Error_Attribute_Key = "ContinueOnError";
            public const string HTML_ERROR_LIST_ATTRIBUTE_KEY = "HTMLErrorList";
        }
        /// <summary>
        /// Executes the specified workflow.
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        /// <param name="action">The action.</param>
        /// <param name="entity">The entity.</param>
        /// <param name="errorMessages">The error messages.</param>
        /// <returns></returns>

        public override bool Execute(RockContext rockContext, WorkflowAction action, Object entity, out List<string> errorMessages)
        {
            errorMessages = new List<string>();
            var mergeFields = GetMergeFields(action);




            // Get the amount
            decimal? amount = GetAttributeValue(action, AttributeKey.AMOUNT_ATTRIBUTE_KEY, true).AsDecimalOrNull();


            if (!amount.HasValue || amount.Value < 1m)
            {
                errorMessages.Add("A valid amount was not provided.");
            }

            // Get the account
            var account = GetEntityFromAttributeValue<FinancialAccount>(action, AttributeKey.FINANCIAL_ACCOUNT_ATTRIBUTE_KEY, true, rockContext);

            if (account == null)
            {
                errorMessages.Add("The account is not valid");
            }

            // get person
            Person person = null;

            string personAttributeValue = GetAttributeValue(action, AttributeKey.PERSON_ATTRIBUTE_KEY);
            Guid? guidPersonAttribute = personAttributeValue.AsGuidOrNull();
            if (guidPersonAttribute.HasValue)
            {
                var attributePerson = AttributeCache.Get(guidPersonAttribute.Value, rockContext);
                if (attributePerson != null || attributePerson.FieldType.Class != "Rock.Field.Types.PersonFieldType")
                {
                    string attributePersonValue = action.GetWorkflowAttributeValue(guidPersonAttribute.Value);
                    if (!string.IsNullOrWhiteSpace(attributePersonValue))
                    {
                        Guid personAliasGuid = attributePersonValue.AsGuid();
                        if (!personAliasGuid.IsEmpty())
                        {
                            person = new PersonAliasService(rockContext).Queryable()
                                .Where(a => a.Guid.Equals(personAliasGuid))
                                .Select(a => a.Person)
                                .FirstOrDefault();
                        }
                    }
                }
            }

            if (person.PrimaryAliasId == null)
            {
                errorMessages.Add(string.Format("Person could not be found for selected value ('{0}')!", guidPersonAttribute.ToString()));

            }

            //Gets the Transaction Code
            String transactionCode = GetAttributeValue(action, AttributeKey.Transaction_Code_Attribute_Key, true).ToString();

            var findTransaction = new FinancialTransactionService(rockContext).GetByTransactionCode(null, transactionCode);

            if (findTransaction != null)
            {
                errorMessages.Add(string.Format("A Transaction with Transaction Code '{0}' already exists.", transactionCode));

            }


            if (errorMessages.Any())
            {
                return HandleExit(action, errorMessages);
            }

            // Create Transaction and Batch
            FinancialTransaction transaction = new FinancialTransaction();
            FinancialPaymentDetail financialPaymentDetail = new FinancialPaymentDetail();

            /*
            Guid currencyTypeGuid = GetAttributeValue(action, AttributeKey.Currency_Type_Attribute_Key, true).AsGuid();
            Guid sourceTypeGuid = GetAttributeValue(action, AttributeKey.Source_Value_Attribute_Key, true).AsGuid();

            var currencyType = new DefinedValueService(rockContext).GetByGuid(currencyTypeGuid);
            var sourceType = new DefinedValueService(rockContext).GetByGuid(sourceTypeGuid);

            */


            var currencyType = DefinedValueCache.Get(GetAttributeValue(action, AttributeKey.Currency_Type_Attribute_Key ,true).AsGuid());
            var sourceType = DefinedValueCache.Get(GetAttributeValue(action, AttributeKey.Source_Value_Attribute_Key, true).AsGuid());

            transaction.AuthorizedPersonAliasId = person.PrimaryAliasId;
            transaction.TransactionDateTime = DateTime.Parse(GetAttributeValue(action, AttributeKey.Transaction_Date_Time_Attribute_Key, true));
            var transactionType = DefinedValueCache.Get(Rock.SystemGuid.DefinedValue.TRANSACTION_TYPE_CONTRIBUTION.AsGuid());
            transaction.TransactionTypeValueId = transactionType.Id;

            transaction.TransactionCode = transactionCode;
            transaction.SourceTypeValueId = sourceType.Id;
            transaction.FinancialPaymentDetail = financialPaymentDetail;
            transaction.SettledDate = DateTime.Parse(GetAttributeValue(action, AttributeKey.Deposit_Date_Attribute_Key, true));
            transaction.Status = "Complete";

            financialPaymentDetail.CurrencyTypeValueId = currencyType.Id;
            
            var transactionDetail = new FinancialTransactionDetail();

            transactionDetail.Amount = Convert.ToDecimal(amount);
            transactionDetail.AccountId = account.Id;

            transactionDetail.FeeAmount = GetAttributeValue(action, AttributeKey.Fee_Amount_Attribute_Key).AsDecimalOrNull();
            transactionDetail.Summary = GetAttributeValue(action, AttributeKey.SUMMARY_DETAILS_ATTRIBUTE_KEY, true);
            DateTime? settledDate = transaction.SettledDate; // Assuming SettledDate is nullable DateTime
            String batchName = GetTextFromSelectedAttribute(AttributeKey.BATCH_PREFIX_ATTRIBUTE_KEY) + ' ' + (settledDate != null ? settledDate.Value.ToString("MM/dd/yyyy") : "");
            transaction.TransactionDetails.Add(transactionDetail);
            

            FinancialBatchService financialBatchService = new FinancialBatchService(rockContext);

            // Get the batch
            var batch = financialBatchService.Get(


                batchName,
                null,
                null,
                transaction.SettledDate.Value,
                TimeSpan.Zero
                );

            var batchChanges = new History.HistoryChangeList();

            if (batch.Id == 0)
            {
                batchChanges.AddChange(History.HistoryVerb.Add, History.HistoryChangeType.Record, "Batch");
                History.EvaluateChange(batchChanges, "Batch Name", string.Empty, batch.Name);
                History.EvaluateChange(batchChanges, "Batch Status", string.Empty, batch.Status.ToString());
                History.EvaluateChange(batchChanges, "Start Date/Time", null, batch.BatchStartDateTime.ToString());
                History.EvaluateChange(batchChanges, "End Date/Time", null, batch.BatchEndDateTime.ToString());
            }

            FinancialTransactionService financialTransactionService = new FinancialTransactionService(rockContext);

            // If this is a new Batch, SaveChanges so that we can get the Batch.Id
            if (batch.Id == 0)
            {
                rockContext.SaveChanges();
            }

            transaction.BatchId = batch.Id;

            // use the financialTransactionService to add the transaction instead of batch.Transactions to avoid lazy-loading the transactions already associated with the batch
            financialTransactionService.Add(transaction);

            rockContext.SaveChanges();
            transaction.SaveAttributeValues();

            financialBatchService.IncrementControlAmount(batch.Id, transaction.TotalAmount, batchChanges);
            rockContext.SaveChanges();

            HistoryService.SaveChanges(
            rockContext,
            typeof(FinancialBatch),
            Rock.SystemGuid.Category.HISTORY_FINANCIAL_BATCH.AsGuid(),
            batch.Id,
            batchChanges
        );
            action.AddLogEntry(string.Format("The following transaction id was created ('{0}')!", transaction.Id.ToString()));

            var query = GetAttributeValue(action, AttributeKey.Created_Transaction_Attribute_Key);
            var attribute = SetWorkflowAttributeValue(action, AttributeKey.Created_Transaction_Attribute_Key, transaction.Guid.ToString());




            return HandleExit(action, errorMessages);
            string GetTextFromSelectedAttribute(string attributeKey)
            {
                var attributeGuid = GetAttributeValue(action, attributeKey).AsGuidOrNull();

                if (!attributeGuid.HasValue)
                {
                    return string.Empty;
                }

                return action.GetWorkflowAttributeValue(attributeGuid.Value);
            }
        }
        private bool HandleExit(WorkflowAction action, List<string> errorMessages)
        {
            bool continueOnError = GetAttributeValue(action, AttributeKey.Continue_On_Error_Attribute_Key).AsBoolean();
            errorMessages.ForEach(m => action.AddLogEntry(m, true));

            // Create HTML Message Alert to Show
            string htmlList = "<div class='alert alert-danger'><ul>";
            htmlList += string.Join("", errorMessages.Select(msg => $"<li>{msg}</li>"));
            htmlList += "</ul></div>";


            var attribute = SetWorkflowAttributeValue(action, AttributeKey.HTML_ERROR_LIST_ATTRIBUTE_KEY, htmlList);
            var hasError = errorMessages.Any();
            return continueOnError || !hasError;
        }
      
    }

}
