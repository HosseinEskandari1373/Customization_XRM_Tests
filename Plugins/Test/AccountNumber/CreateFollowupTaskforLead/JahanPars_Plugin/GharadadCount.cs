using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

//namespaces for d365 interaction
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;

namespace JahanPars_Plugin
{
    public class GharadadCount : IPlugin
    {
        public void Execute(IServiceProvider serviceProvider)
        {
            // Obtain the execution context from the service provider.
            IPluginExecutionContext context = (IPluginExecutionContext)
                serviceProvider.GetService(typeof(IPluginExecutionContext));

            // Obtain the organization service reference.
            IOrganizationServiceFactory serviceFactory =
                (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));

            IOrganizationService service =
                serviceFactory.CreateOrganizationService(context.UserId);

            if (context.InputParameters.Contains("Target") &&
                context.InputParameters["Target"] is Entity)
            {
                Entity entity = (Entity)context.InputParameters["Target"];

                if (entity.LogicalName == "new_contract")
                {
                    if (entity.Attributes.Contains("new_countcontract") == false)
                    {
                        if (context.InputParameters.Contains("Target") && context.InputParameters["Target"] is Entity)
                        {
                            //get config table row
                            var fetch = @"<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='false'>
                                              <entity name='new_contract'>
                                                <attribute name='new_name' />
                                                <attribute name='createdon' />
                                                <attribute name='new_typecontract' />
                                                <attribute name='new_projectid' />
                                                <attribute name='new_countcontract' />
                                                <attribute name='new_yearcontract' />
                                                <attribute name='new_contractid' />
                                                <order attribute='new_name' descending='false' />
                                                <filter type='and'>
                                                  <condition attribute='statecode' operator='eq' value='0' />
                                                  <condition attribute='createdon' operator='today' />
                                                </filter>
                                              </entity>
                                           </fetch>";

                            EntityCollection ecAuto = service.RetrieveMultiple(new FetchExpression(fetch));
                            Guid autoNumberRecordId = Guid.Empty;

                            foreach (var itemLookUp in ecAuto.Entities)
                            {
                                autoNumberRecordId = itemLookUp.Id;
                            }

                            if (autoNumberRecordId == Guid.Empty)
                            {
                                entity.Attributes.Add("new_countcontract", "JP-MES-H-02-001");
                            }

                            Entity AutoPost = service.Retrieve("new_contract", autoNumberRecordId, new ColumnSet(true));
                            var currentrecordcounternumber = AutoPost.GetAttributeValue<string>("new_countcontract");

                            //initialize counter
                            var numbers = string.Concat(currentrecordcounternumber.Where(char.IsNumber));
                            var lenNum = numbers.Length;
                            var chars = string.Concat(currentrecordcounternumber.Where(char.IsLetter));

                            var newCounterValue = (Convert.ToInt32(numbers) + 1).ToString().PadLeft(lenNum, '0');

                            //new id;
                            entity.Attributes.Add("new_countcontract", chars + "-" + newCounterValue.ToString());
                        }
                    }
                    else
                    {
                        throw new InvalidPluginExecutionException("The account number can only be set by the system.");
                    }
                }
            }
        }
    }
}
