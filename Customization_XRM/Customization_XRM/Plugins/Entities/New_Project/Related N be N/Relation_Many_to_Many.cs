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
    public class Relation_Many_to_Many : PluginBase
    {
        public Relation_Many_to_Many() : base(typeof(Relation_Many_to_Many))
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

                // Get the Entity Reference as the Target
                EntityReference target = (EntityReference)context.InputParameters["Target"];
                Entity project = new new_Project();
                Entity letter = new new_letter();

                // Check the Message Name and the Target Entity
                if (context.MessageName.ToLower() == "associate" && (target.LogicalName == project.LogicalName || target.LogicalName == letter.LogicalName))
                {
                    // Get the ralationships in the context
                    Relationship relationShip = (Relationship)context.InputParameters["Relationship"];

                    // Get Related Entities in the context
                    EntityReferenceCollection relatedentities = (EntityReferenceCollection)context.InputParameters["RelatedEntities"];

                    foreach (EntityReference relatedEntity in relatedentities)
                    {
                        // Check Related Entity Logical & Schema Name
                        if (relatedEntity.LogicalName == letter.LogicalName && relationShip.SchemaName == "new_new_project_new_letter")
                        {
                            // Student ID
                            // relatedEntity.Id
                            // Peroform the Task   
                            Entity contactToUpdate = new Entity(relatedEntity.LogicalName, relatedEntity.Id);
                            contactToUpdate["new_score"] = 1500;

                            crmService.Update(contactToUpdate);
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

                    Entity projects = crmService.Retrieve(project.LogicalName, targetEntity.Id, new ColumnSet("new_count"));
                    var type = projects.GetAttributeValue<int>("new_count");
                    //throw new InvalidPluginExecution(Department.ToString());

                    //Get Entity 2 Reference From RelatedEntities Key from Context
                    var x = 0;
                    if (context.InputParameters.Contains("RelatedEntities") &&
                          context.InputParameters["RelatedEntities"] is EntityReferenceCollection)
                    {
                        relatedEntity1 = context.InputParameters["RelatedEntities"] as EntityReferenceCollection;
                        x = type + relatedEntity1.Count;
                    }

                    projects["new_count"] = x;
                    crmService.Update(projects);
                }
                else if (context.MessageName.ToLower() == "disassociate" && (target.LogicalName == project.LogicalName || target.LogicalName == letter.LogicalName))
                {
                    // Get the ralationships in the context
                    Relationship relationShip = (Relationship)context.InputParameters["Relationship"];

                    // Get Related Entities in the context
                    EntityReferenceCollection relatedentities = (EntityReferenceCollection)context.InputParameters["RelatedEntities"];

                    foreach (EntityReference relatedEntity in relatedentities)
                    {
                        // Check Related Entity Logical & Schema Name
                        if (relatedEntity.LogicalName == letter.LogicalName && relationShip.SchemaName == "new_new_project_new_letter")
                        {
                            // Student ID
                            // relatedEntity.Id
                            // Peroform the Task   
                            Entity contactToUpdate = new Entity(relatedEntity.LogicalName, relatedEntity.Id);
                            contactToUpdate["new_score"] = 1395;

                            crmService.Update(contactToUpdate);
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

                    Entity projects = crmService.Retrieve(project.LogicalName, targetEntity.Id, new ColumnSet("new_count"));
                    var type = projects.GetAttributeValue<int>("new_count");
                    //throw new InvalidPluginExecution(Department.ToString());

                    //Get Entity 2 Reference From RelatedEntities Key from Context
                    var x = 0;
                    if (context.InputParameters.Contains("RelatedEntities") &&
                          context.InputParameters["RelatedEntities"] is EntityReferenceCollection)
                    {
                        relatedEntity1 = context.InputParameters["RelatedEntities"] as EntityReferenceCollection;
                        x = type - relatedEntity1.Count;
                    }

                    projects["new_count"] = x;
                    crmService.Update(projects);
                }
            }
            catch (Exception ex)
            {
                throw new InvalidPluginExecutionException(ex.Message);
            }
        }
    }
}
