using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

//namespaces for d365 interaction
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;


namespace Update_All_Childs
{
    public class Update_All_Childs : IPlugin
    {
        IOrganizationService service = null;

        public void Execute(IServiceProvider serviceProvider)
        {
            ITracingService tracingService = (ITracingService)serviceProvider.GetService(typeof(ITracingService));
            IPluginExecutionContext context = (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));
            IOrganizationServiceFactory serviceFactory = (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));
            service = serviceFactory.CreateOrganizationService(context.UserId);

            Entity primaryEntity = null;

            if (context.InputParameters.Contains("Target") && context.InputParameters["Target"] is Entity)
            {
                try
                {
                    primaryEntity = (Entity)context.InputParameters["Target"];

                    //check if record is deactivated
                    if (primaryEntity.Contains("new_dateproject") &&
                      primaryEntity.GetAttributeValue<OptionSetValue>("statecode").Value == 1)
                    {
                        //for disable send operation as 0
                        EnableDisableOneToManyRelatedEntities("new_contract", "new_projectid", primaryEntity.Id, 0);
                    }
                    //check if record is activated
                    else if (primaryEntity.Contains("statecode") &&
                       primaryEntity.GetAttributeValue<OptionSetValue>("statecode").Value == 0)
                    {
                        //for disable send operation as 1
                        EnableDisableOneToManyRelatedEntities("new_contract", "new_projectid", primaryEntity.Id, 1);
                    }
                }
                catch (Exception ex)
                {
                    tracingService.Trace("Error occured while deactivating " + primaryEntity.LogicalName + " child records Detials" + ex.Message);
                }
            }
        }

        private void EnableDisableOneToManyRelatedEntities(string entityname, string parententityfieldname, Guid parentfieldvalue, int operation)
        {
            EntityCollection results = null;
            QueryExpression query = new QueryExpression()
            {
                EntityName = entityname,
                Criteria =
                    {
             FilterOperator = LogicalOperator.And,
                        Filters =
                        {
                            new FilterExpression
                            {
                                FilterOperator = LogicalOperator.And,
                                Conditions =
                                {
                                  new ConditionExpression(parententityfieldname,ConditionOperator.Equal,parentfieldvalue),
                                  new ConditionExpression("statecode",ConditionOperator.Equal,operation)
                                },
                            }

                        }
                    }
            };
            results = service.RetrieveMultiple(query);

            #region Disable records
            if (operation == 0)
            {
                //deactivate records
                foreach (Entity relatedentity in results.Entities)
                {
                    Entity deactivateRecord = new Entity(entityname);
                    deactivateRecord.Id = relatedentity.Id;
                    deactivateRecord["statecode"] = new OptionSetValue(1);
                    deactivateRecord["statuscode"] = new OptionSetValue(2);

                    service.Update(deactivateRecord);

                }
            }
            #endregion
            #region Enable records
            else if (operation == 1)
            {
                //activate them
                foreach (Entity relatedentity in results.Entities)
                {
                    Entity activateRecord = new Entity(entityname);
                    activateRecord.Id = relatedentity.Id;
                    activateRecord["statecode"] = new OptionSetValue(0);
                    activateRecord["statuscode"] = new OptionSetValue(1);

                    service.Update(activateRecord);

                }
            }
            #endregion
        }
    }
}
