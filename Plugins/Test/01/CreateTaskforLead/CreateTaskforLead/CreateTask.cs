using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

//namespace for d365 interaction
using Microsoft.Xrm.Sdk;

namespace CreateTaskforLead
{
    public class CreateTask : IPlugin
    {
        public void Execute(IServiceProvider serviceProvider)
        {
            // Obtain the execution context from the service provider.
            IPluginExecutionContext context = (IPluginExecutionContext)
                serviceProvider.GetService(typeof(IPluginExecutionContext));

            if (context.InputParameters.Contains("Target") &&
                context.InputParameters["Target"] is Entity)
            {
                // Obtain the target entity from the input parameters.
                Entity entity = (Entity)context.InputParameters["Target"];
                //</snippetFollowupPlugin2>
                 
                // Verify that the target entity represents an account.
                // If not, this plug-in was not registered correctly.
                if (entity.LogicalName != "lead")
                    return;

                try
                {
                    // Create a task activity to follow up with the account customer in 7 days. 
                    Entity followup = new Entity("task");

                    followup["subject"] = "Send e-mail to the new customer.";
                    followup["description"] = "Follow up with the customer.";
                    followup["scheduledstart"] = DateTime.Now.AddDays(7);
                    followup["scheduledend"] = DateTime.Now.AddDays(7);
                    followup["category"] = context.PrimaryEntityName;
                    //followup["curency"] = new Money(23.00M);

                    // Refer to the account in the task activity.
                    if (context.OutputParameters.Contains("id"))
                    {
                        Guid regardingobjectid = new Guid(context.OutputParameters["id"].ToString());
                        string regardingobjectidType = "lead";

                        followup["regardingobjectid"] =
                        new EntityReference(regardingobjectidType, regardingobjectid);
                    }

                    // Obtain the organization service reference.
                    IOrganizationServiceFactory serviceFactory = 
                        (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));
                    IOrganizationService service = 
                        serviceFactory.CreateOrganizationService(context.UserId);

                    service.Create(followup);
                }
                catch (System.ServiceModel.FaultException<OrganizationServiceFault> ex)
                {
                    throw new InvalidPluginExecutionException("An error occurred in the FollowupPlugin plug-in.", ex);
                }
            }
            else { return; }
        }
    }
}
