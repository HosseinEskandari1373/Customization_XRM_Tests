using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

//namespaces for d365 interaction
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;

namespace PluginTest01
{
    public class Advanced : IPlugin
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

                if (entity.LogicalName == "account")
                {
                    if (entity.Attributes.Contains("accountnumber") == false)
                    {
                        if (context.InputParameters.Contains("Target") && context.InputParameters["Target"] is Entity)
                        {
                            //get config table row
                            var fetch = @"<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='false'>
                                  <entity name='account'>
                                    <attribute name='name' />
                                    <attribute name='address1_city' />
                                    <attribute name='primarycontactid' />
                                    <attribute name='telephone1' />
                                    <attribute name='accountid' />
                                    <order attribute='name' descending='false' />
                                    <filter type='and'>
                                      <condition attribute='ownerid' operator='eq-userid' />
                                      <condition attribute='statecode' operator='eq' value='0' />
                                    </filter>
                                    <link-entity name='contact' from='contactid' to='primarycontactid' visible='false' link-type='outer' alias='accountprimarycontactidcontactcontactid'>
                                      <attribute name='emailaddress1' />
                                    </link-entity>
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
                                entity.Attributes.Add("accountnumber", "RQI-00001");
                            }

                            Entity AutoPost = service.Retrieve("account", autoNumberRecordId, new ColumnSet(true));
                            var currentrecordcounternumber = AutoPost.GetAttributeValue<string>("accountnumber");

                            //initialize counter
                            var numbers = string.Concat(currentrecordcounternumber.Where(char.IsNumber));
                            var lenNum = numbers.Length;
                            var chars = string.Concat(currentrecordcounternumber.Where(char.IsLetter));

                            var newCounterValue = (Convert.ToInt32(numbers) + 1).ToString().PadLeft(lenNum, '0');

                            //new id;
                            entity.Attributes.Add("accountnumber", chars + "-" + newCounterValue.ToString());
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
