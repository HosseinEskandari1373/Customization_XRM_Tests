using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

//namespaces for d365 interaction
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;

namespace Relation_Many_to_Many_Lock_Entity_Creation
{
    public class Lock_Entity_Creation : IPlugin
    {
        public void Execute(IServiceProvider serviceProvider)
        {
            if (serviceProvider == null)
                throw new ArgumentNullException("serviceProvider");

            //Tracing Object
            ITracingService tracing = (ITracingService)serviceProvider.GetService(typeof(ITracingService));

            //ExecutionContext Object
            IPluginExecutionContext context = (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));

            //OrganizationService Object
            IOrganizationServiceFactory serviceFactory = (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));

            //OrganizationService Object
            var service = serviceFactory.CreateOrganizationService(context.UserId);

            // Get the Entity Reference as the Target
            EntityReference target = (EntityReference)context.InputParameters["Target"];

            EntityReference targetEntity = null;
            string relationshipName = string.Empty;
            EntityReferenceCollection relatedEntity = null;

            //Get the Relationship Key from context
            if (context.InputParameters.Contains("Relationship"))
                relationshipName = context.InputParameters["Relationship"].ToString();

            //Check the Realationship Name with Your intended on
            if (relationshipName != "new_new_project_new_letter.")
                return;

            //Get Entity 1 Reference From Target Key from Context
            if (context.InputParameters.Contains("Target") && context.InputParameters["Target"] is EntityReference)
                targetEntity = (EntityReference)context.InputParameters["Target"];

            Entity project = service.Retrieve("new_project", targetEntity.Id, new ColumnSet("new_project_type"));
            var type = project.GetAttributeValue<OptionSetValue>("new_project_type").Value;
            //throw new InvalidPluginExecution(Department.ToString());

            //Get Entity 2 Reference From RelatedEntities Key from Context
            if (context.InputParameters.Contains("RelatedEntities") &&
                  context.InputParameters["RelatedEntities"] is EntityReferenceCollection)
            {
                relatedEntity = context.InputParameters["RelatedEntities"] as EntityReferenceCollection;
                foreach (var ent in relatedEntity)
                {
                    Entity letter = service.Retrieve("new_letter", ent.Id, new ColumnSet("new_letter_type"));
                    var Category = letter.GetAttributeValue<OptionSetValue>("new_letter_type").Value;

                    //throw new InvalidPluginExecution(category.ToString());
                    Entity letters = service.Retrieve("new_letter", ent.Id, new ColumnSet("new_score"));
                    if (type == 100000000 && Category != 100000000)
                    {
                        //if department is diabetes and medicine category is not diabetes
                        throw new InvalidPluginExecutionException("you can not add amaliati letter for this project.");
                    }
                    else if (type == 100000001 && Category != 100000001)
                    {
                        //if department is Urology and Medicine category is not Urology
                        throw new InvalidPluginExecutionException("you can not add barnamerizi letter for this project.");
                    }
                }
            }
        }
    }
}
