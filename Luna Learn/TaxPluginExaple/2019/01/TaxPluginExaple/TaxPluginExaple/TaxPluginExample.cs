using Microsoft.Xrm.Sdk;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;
using CrmEarlyBound;
using Microsoft.Xrm.Sdk.Query;

namespace TaxPluginExaple
{
    public class TaxPluginExample : IPlugin
    {
        public void Execute(IServiceProvider serviceProvider)
        {
            // Obtain the tracing service
            ITracingService tracingService =
            (ITracingService)serviceProvider.GetService(typeof(ITracingService));

            // Obtain the execution context from the service provider.  
            IPluginExecutionContext context = (IPluginExecutionContext)
                serviceProvider.GetService(typeof(IPluginExecutionContext));

            // The InputParameters collection contains all the data passed in the message request.  
            if (context.InputParameters.Contains("Target") &&
                context.InputParameters["Target"] is Entity)
            {
                // Obtain the target entity from the input parameters.  
                Entity entity = (Entity)context.InputParameters["Target"];

                // Obtain the organization service reference which you will need for  
                // web service calls.  
                IOrganizationServiceFactory serviceFactory =
                    (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));
                IOrganizationService service = serviceFactory.CreateOrganizationService(context.UserId);
                
                try
                {
                    var c = new ColumnSet(true);
                    var invoice = service.Retrieve(Invoice.EntityLogicalName, entity.Id, c) as Invoice;

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
                    foreach (var item in invoiceItems)
                    {
                        if (invoice.new_FactorType == true)
                        {
                            item.Tax = new Money(item.BaseAmount.Value * 0.09M);
                            service.Update(new InvoiceDetail { Id = item.Id, Tax = item.Tax });
                        }
                        else
                        {
                            service.Update(new InvoiceDetail { Id = item.Id, Tax = new Money(0) });
                        }
                    }
                }

                catch (FaultException<OrganizationServiceFault> ex)
                {
                    throw new InvalidPluginExecutionException("An error occurred in FollowUpPlugin.", ex);
                }

                catch (Exception ex)
                {
                    tracingService.Trace("FollowUpPlugin: {0}", ex.ToString());
                    throw;
                }
            }
        }
    }
}
