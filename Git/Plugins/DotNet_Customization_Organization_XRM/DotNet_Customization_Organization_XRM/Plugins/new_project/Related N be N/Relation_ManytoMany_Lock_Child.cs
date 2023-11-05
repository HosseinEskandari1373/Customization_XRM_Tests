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
    public class Relation_ManytoMany_Lock_Child : IPlugin
    {
        public void Execute(IServiceProvider serviceProvider)
        {
            //Tracing Object
            ITracingService tracing = (ITracingService)serviceProvider.GetService(typeof(ITracingService));

            //ExecutionContext Object
            IPluginExecutionContext context = (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));

            //OrganizationService Object
            IOrganizationServiceFactory serviceFactory = (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));

            //OrganizationService Object
            var service = serviceFactory.CreateOrganizationService(context.UserId);

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
            var projectType = project.GetAttributeValue<OptionSetValue>("new_project_type").Value;
            //throw new InvalidPluginExecution(Department.ToString());

            //Get Entity 2 Reference From RelatedEntities Key from Context
            if (context.InputParameters.Contains("RelatedEntities") &&
                  context.InputParameters["RelatedEntities"] is EntityReferenceCollection)
            {
                relatedEntity = context.InputParameters["RelatedEntities"] as EntityReferenceCollection;
                foreach (var ent in relatedEntity)
                {
                    Entity letter = service.Retrieve("new_letter", ent.Id, new ColumnSet("new_letter_type"));
                    var letterType = letter.GetAttributeValue<OptionSetValue>("new_letter_type").Value;
                    //throw new InvalidPluginExecution(category.ToString());
                    if (projectType == 100000000 && letterType != 100000000)
                    {
                        //if department is diabetes and medicine category is not diabetes
                        throw new InvalidPluginExecutionException("امکان اضافه کردن نامه با نوع غیر تحلیلی روی پروژه وجود ندارد.");
                    }
                    else if (projectType == 100000001 && letterType != 100000001)
                    {
                        //if department is Urology and Medicine category is not Urology
                        throw new InvalidPluginExecutionException("امکان اضافه کردن نامه غیر عملیاتی روی فرم پروژه وجود ندارد.");
                    }
                }
            }
        }
    }
}
