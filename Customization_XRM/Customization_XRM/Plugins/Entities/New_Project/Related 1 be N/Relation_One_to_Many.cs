using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

//namespaces for d365 interaction
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using Customization_XRM.Plugin;
using CrmEarlyBound;

namespace Customization_XRM
{
    public class Relation_One_to_Many : PluginBase
    {
        public Relation_One_to_Many() : base(typeof(Relation_One_to_Many))
        {

        }
        protected override void ExecutePluginLogic(LocalPluginExecution localPluginExecution)
        {
            try
            {
                if (localPluginExecution == null)
                {
                    throw new InvalidPluginExecutionException("Local Plugin Execution is not initialized correctly.");
                }

                //initialize plugin basec components
                IPluginExecutionContext context = localPluginExecution.pluginContext;
                IOrganizationService crmService = localPluginExecution.orgService;
                ITracingService tracingService = localPluginExecution.tracingService;

                if (context.InputParameters.Contains("Target") && context.InputParameters["Target"] is Entity)
                {
                    Entity targetAccount = (Entity)context.InputParameters["Target"];
                    if (targetAccount.Contains("new_dateproject" /*تاریخ پروژه*/))
                    {
                        // Retrieve all contract for this project
                        var fetch = @"<fetch no-lock='true' >
                                     <entity name='new_contract' >
                                       <attribute name='new_projectid'/>
                                       <filter>
                                         <condition attribute='new_projectid' operator='eq' value='{0}' />
                                       </filter>
                                     </entity>
                                   </fetch>";

                        var fetchXML = string.Format(fetch, targetAccount.Id);
                        var allContracts = crmService.RetrieveMultiple(new FetchExpression(fetchXML)).Entities;

                        // Iterate through all contact record and update the phone number from account
                        foreach (var contractEnt in allContracts)
                        {
                            Entity contractToUpdate = new Entity(contractEnt.LogicalName, contractEnt.Id);
                            contractToUpdate["new_yearcontract" /*تعداد قرارداد*/] = 1400;

                            OptionSetValue OptionVal = new OptionSetValue(100000002);
                            //شده است Set حذف مقدار قبلی که
                            contractToUpdate.Attributes.Remove("new_typecontract" /*نوع قرارداد*/);
                            contractToUpdate.Attributes.Add("new_typecontract", OptionVal);
                            crmService.Update(contractToUpdate);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new InvalidPluginExecutionException(ex.Message);
            }
        }
    }
}
