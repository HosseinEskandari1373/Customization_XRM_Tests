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
using CrmEarlyBound;
using Customization_XRM.Plugin;

namespace Customization_XRM
{
    class PluginQualifY_PostOperation : PluginBase
    {
        //constructor
        public PluginQualifY_PostOperation() : base(typeof(PluginQualifY_PostOperation))
        {

        }
        protected override void ExecutePluginLogic(LocalPluginExecution localPluginExecution)
        {
            //plugin logic
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

                //Target is present or not
                if (context.InputParameters.Contains("Target") && context.InputParameters["Target"] is Entity)
                {
                    if (context.MessageName.ToLower() != "create" && context.Stage != 40) { return; }
                    Entity targetLead = context.InputParameters["Target"] as Entity;

                    QualifyLeadRequest qualifyLead = new QualifyLeadRequest
                    {
                        LeadId = new EntityReference(targetLead.LogicalName, targetLead.Id),
                        Status = new OptionSetValue(3), //Qualify
                        CreateAccount = true,
                        CreateContact = true,
                        CreateOpportunity = true
                    };

                    QualifyLeadResponse leadResponse = (QualifyLeadResponse)crmService.Execute(qualifyLead);
                    tracingService.Trace("Lead is qualified");
                }
            }
            catch (Exception ex)
            {
                throw new InvalidPluginExecutionException(ex.Message);
            }
        }
    }
}
