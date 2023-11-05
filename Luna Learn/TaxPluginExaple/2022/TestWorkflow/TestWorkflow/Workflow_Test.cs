using Microsoft.Xrm.Sdk.Workflow;
using Microsoft.Xrm.Sdk;
using System;
using System.Activities;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xrm.Sdk.Query;
using System.Globalization;
using Xrm;

namespace TestWorkflow
{
    public class Workflow_Test : CodeActivity
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

            var invoice = service.Retrieve(Invoice.EntityLogicalName, InvoiceId.Get(executeContext).Id, new ColumnSet(true));
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
                    var x = Percent.Get(executeContext);
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
            Des.Set(executeContext, des);
        }
        [Output("Description")]
        public OutArgument<string> Des { get; set; }
    }
}
