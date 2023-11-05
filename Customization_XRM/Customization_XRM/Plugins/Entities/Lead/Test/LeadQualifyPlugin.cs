using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

//namespaces for d365 interaction
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Crm.Sdk.Messages;
using System.Globalization;
using Customization_XRM.Plugin;

namespace Customization_XRM
{
    class LeadQualifyPlugin : PluginBase
    {
        //constructor
        public LeadQualifyPlugin() : base(typeof(LeadQualifyPlugin))
        {
            //constructor implementation here

        }

        protected override void ExecutePluginLogic(LocalPluginExecution localPluginExecution)
        {
            if (localPluginExecution == null)
            {
                throw new InvalidPluginExecutionException("Invalied Local Plugin execution object.");
            }

            try
            {
                //IPlugin components
                IPluginExecutionContext context = localPluginExecution.pluginContext;
                IOrganizationService crmService = localPluginExecution.orgService;
                ITracingService tracingService = localPluginExecution.tracingService;

                if (context.InputParameters.Contains("LeadId") && context.InputParameters["LeadId"] is EntityReference)
                {
                    if (context.MessageName.ToLower() != "qualifylead" && context.Stage != 20) { return; }
                    EntityReference leadRef = (EntityReference)context.InputParameters["LeadId"];

                    Entity leadEntity = crmService.Retrieve(leadRef.LogicalName, leadRef.Id, new ColumnSet("leadsourcecode"));
                    if (leadEntity != null && leadEntity.Contains("leadsourcecode"))
                    {
                        if (leadEntity.GetAttributeValue<OptionSetValue>("leadsourcecode").Value == 8) //lead sources is web
                        {
                            context.InputParameters["CreateAccount"] = false;
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
