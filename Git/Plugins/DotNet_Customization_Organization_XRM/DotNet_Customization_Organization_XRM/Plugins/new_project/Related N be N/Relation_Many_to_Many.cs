using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

//namespaces for d365 interaction
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;

namespace DotNet_Customization_Organization_XRM
{
    public class Relation_Many_to_Many : IPlugin
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

            // Check the Message Name and the Target Entity
            if (context.MessageName.ToLower() == "associate" && (target.LogicalName == "new_project" || target.LogicalName == "new_letter"))
            {
                // Get the ralationships in the context
                Relationship relationShip = (Relationship)context.InputParameters["Relationship"];

                // Get Related Entities in the context
                EntityReferenceCollection relatedentities = (EntityReferenceCollection)context.InputParameters["RelatedEntities"];

                foreach (EntityReference relatedEntity in relatedentities)
                {
                    // Check Related Entity Logical & Schema Name
                    if (relatedEntity.LogicalName == "new_letter" && relationShip.SchemaName == "new_new_project_new_letter")
                    {
                        // Student ID
                        // relatedEntity.Id
                        // Peroform the Task   
                        Entity contactToUpdate = new Entity(relatedEntity.LogicalName, relatedEntity.Id);
                        contactToUpdate["new_score"] = 1500;

                        service.Update(contactToUpdate);
                    }
                }

                EntityReference targetEntity = null;
                string relationshipName = string.Empty;
                EntityReferenceCollection relatedEntity1 = null;

                //Get the Relationship Key from context
                if (context.InputParameters.Contains("Relationship"))
                    relationshipName = context.InputParameters["Relationship"].ToString();

                //Check the Realationship Name with Your intended on
                if (relationshipName != "new_new_project_new_letter.")
                    return;

                //Get Entity 1 Reference From Target Key from Context
                if (context.InputParameters.Contains("Target") && context.InputParameters["Target"] is EntityReference)
                    targetEntity = (EntityReference)context.InputParameters["Target"];

                Entity project = service.Retrieve("new_project", targetEntity.Id, new ColumnSet("new_count"));
                var type = project.GetAttributeValue<int>("new_count");
                //throw new InvalidPluginExecution(Department.ToString());

                //Get Entity 2 Reference From RelatedEntities Key from Context
                var x = 0;
                if (context.InputParameters.Contains("RelatedEntities") &&
                      context.InputParameters["RelatedEntities"] is EntityReferenceCollection)
                {
                    relatedEntity1 = context.InputParameters["RelatedEntities"] as EntityReferenceCollection;
                    x = type + relatedEntity1.Count;
                }

                project["new_count"] = x;
                service.Update(project);
            }
            else if (context.MessageName.ToLower() == "disassociate" && (target.LogicalName == "new_project" || target.LogicalName == "new_letter"))
            {
                // Get the ralationships in the context
                Relationship relationShip = (Relationship)context.InputParameters["Relationship"];

                // Get Related Entities in the context
                EntityReferenceCollection relatedentities = (EntityReferenceCollection)context.InputParameters["RelatedEntities"];

                foreach (EntityReference relatedEntity in relatedentities)
                {
                    // Check Related Entity Logical & Schema Name
                    if (relatedEntity.LogicalName == "new_letter" && relationShip.SchemaName == "new_new_project_new_letter")
                    {
                        // Student ID
                        // relatedEntity.Id
                        // Peroform the Task   
                        Entity contactToUpdate = new Entity(relatedEntity.LogicalName, relatedEntity.Id);
                        contactToUpdate["new_score"] = 1395;

                        service.Update(contactToUpdate);
                    }
                }

                EntityReference targetEntity = null;
                string relationshipName = string.Empty;
                EntityReferenceCollection relatedEntity1 = null;

                //Get the Relationship Key from context
                if (context.InputParameters.Contains("Relationship"))
                    relationshipName = context.InputParameters["Relationship"].ToString();

                //Check the Realationship Name with Your intended on
                if (relationshipName != "new_new_project_new_letter.")
                    return;

                //Get Entity 1 Reference From Target Key from Context
                if (context.InputParameters.Contains("Target") && context.InputParameters["Target"] is EntityReference)
                    targetEntity = (EntityReference)context.InputParameters["Target"];

                Entity project = service.Retrieve("new_project", targetEntity.Id, new ColumnSet("new_count"));
                var type = project.GetAttributeValue<int>("new_count");
                //throw new InvalidPluginExecution(Department.ToString());

                //Get Entity 2 Reference From RelatedEntities Key from Context
                var x = 0;
                if (context.InputParameters.Contains("RelatedEntities") &&
                      context.InputParameters["RelatedEntities"] is EntityReferenceCollection)
                {
                    relatedEntity1 = context.InputParameters["RelatedEntities"] as EntityReferenceCollection;
                    x = type - relatedEntity1.Count;
                }

                project["new_count"] = x;
                service.Update(project);
            }
        }
    }
}
