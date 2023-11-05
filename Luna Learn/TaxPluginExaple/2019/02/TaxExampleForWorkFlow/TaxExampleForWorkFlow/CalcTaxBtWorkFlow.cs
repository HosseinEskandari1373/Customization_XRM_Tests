using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using Microsoft.Xrm.Sdk.Workflow;
using System;
using System.Activities;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CrmEarlyBound;
using System.Globalization;

namespace TaxExampleForWorkFlow
{
    public class CalcTaxBtWorkFlow : CodeActivity
    {
        [Input("Invoice Id")]
        [ReferenceTarget("invoice")]
        public InArgument<EntityReference> InvoiceId { get; set; }

        [Input("Tax Percent")]
        public InArgument<decimal> Percent { get; set; }
        protected override void Execute(CodeActivityContext executeContext)
        {
            ITracingService tracingService = executeContext.GetExtension<ITracingService>();

            //Create the context
            IWorkflowContext context = executeContext.GetExtension<IWorkflowContext>();
            IOrganizationServiceFactory serviceFactory = executeContext.GetExtension<IOrganizationServiceFactory>();
            IOrganizationService service = serviceFactory.CreateOrganizationService(context.UserId);

            var c = new ColumnSet(true);

            var invoice = service.Retrieve(Invoice.EntityLogicalName, InvoiceId.Get(executeContext).Id, new ColumnSet(true)) as Invoice;
            var invoiceItemsQuery = new QueryExpression
            {
                EntityName = InvoiceDetail.EntityLogicalName,
                ColumnSet = new ColumnSet(true),
                Criteria = new FilterExpression
                {
                    Conditions =
                    {
                        new ConditionExpression
                        {
                            AttributeName = "invoiceid",
                            Operator = ConditionOperator.Equal,
                            Values = {invoice.Id}
                        }
                    }
                }
            };

            var invoiceItems = service.RetrieveMultiple(invoiceItemsQuery).Entities.Cast<InvoiceDetail>().ToList();
            string des = "";
            bool IsCatch = false;
            foreach (var item in invoiceItems)
            {
                try
                {
                    service.Update(new InvoiceDetail { Id = item.Id, Tax = new Money(item.BaseAmount.Value * Percent.Get(executeContext)) });
                }
                catch (Exception)
                {
                    IsCatch = true;
                    des = $"محاسبه مالیات برای {item.ProductId.Name} انجام نگردید.";
                }
            }
            if (!IsCatch)
            {
                des = $"محاسبه مالیات در تاریخ {DateTime.Now.ToString("yyyy/MM/dd HH", CultureInfo.GetCultureInfo("fa-ir", "fa"))} با موفقیت انجام گردید.";
            }
            Des.Set(executeContext, Des);
        }
        [Output ("Description")]
        public OutArgument<string> Des { get; set; }
    }
}
