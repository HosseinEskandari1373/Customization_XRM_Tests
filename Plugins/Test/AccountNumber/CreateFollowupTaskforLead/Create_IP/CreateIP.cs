using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Linq;

namespace Create_IP
{
    public class CreateIP : IPlugin
    {
        public void Execute(IServiceProvider serviceProvider)
        {
            try
            {
                // Obtain the execution context from the service provider.
                IPluginExecutionContext context = (IPluginExecutionContext)
                    serviceProvider.GetService(typeof(IPluginExecutionContext));

                // Obtain the organization service reference.
                IOrganizationServiceFactory serviceFactory =
                    (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));

                IOrganizationService service =
                    serviceFactory.CreateOrganizationService(context.UserId);

                if (context.InputParameters.Contains("Target") && context.InputParameters["Target"] is Entity)
                {
                    Entity entity = (Entity)context.InputParameters["Target"];

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
                    Entity entAuto = ecAuto[0];
                    var autoNumberRecordId = entAuto.Id;

                    //initiate a Update LOCK on counter entity
                    Entity counterTable = new Entity("account");
                    counterTable.Attributes["telephone1"] = "lock" + DateTime.Now;
                    counterTable.Id = autoNumberRecordId;
                    service.Update(counterTable);

                    Entity AutoPost = service.Retrieve("account", autoNumberRecordId, new ColumnSet(true));
                    var currentrecordcounternumber = AutoPost.GetAttributeValue<string>("accountnumber");

                    //initialize counter
                    var newCounterValue = Convert.ToInt32(currentrecordcounternumber) + 1;

                    //update the counter in revenue
                    Entity newupdate = new Entity();
                    newupdate.LogicalName = entity.LogicalName;
                    newupdate.Id = entity.Id;
                    newupdate["accountnumber"] = newCounterValue.ToString();
                    service.Update(newupdate);

                    //update the config
                    Entity newupdateconfig = new Entity();
                    newupdateconfig.LogicalName = "soft_autocountertable";
                    newupdateconfig.Id = autoNumberRecordId;
                    newupdateconfig["accountnumber"] = newCounterValue.ToString();
                    service.Update(newupdateconfig);

                }
            }
            catch (Exception)
            {

                throw new InvalidPluginExecutionException("The account number can only be set by the system.");
            }

        }
    }
}