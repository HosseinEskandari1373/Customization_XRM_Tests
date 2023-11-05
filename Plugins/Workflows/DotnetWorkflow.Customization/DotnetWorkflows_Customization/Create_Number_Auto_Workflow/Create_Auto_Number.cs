using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

//Library
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Workflow;
using System.Activities;
using Microsoft.Xrm.Sdk.Query;

namespace Create_Number_Auto_Workflow
{
    public class Create_Auto_Number : CodeActivity
    {
        
        [Input("Entity Name")]
        [RequiredArgument]
        public InArgument<string> entityName { get; set; }

        [Input("Field Name")]
        [RequiredArgument]
        public InArgument<string> fieldName { get; set; }

        [Input("Organization Name")]
        [RequiredArgument]
        public InArgument<string> organizationName { get; set; }
        protected override void Execute(CodeActivityContext context)
        {
            IWorkflowContext workflowContext = context.GetExtension<IWorkflowContext>();

            IOrganizationServiceFactory serviceFactory =
                context.GetExtension<IOrganizationServiceFactory>();

            IOrganizationService service =
                serviceFactory.CreateOrganizationService(workflowContext.InitiatingUserId);

            try
            {
                Entity ent = new Entity(entityName.Get(context));

                if (ent.Attributes.Contains(fieldName.Get(context)) == false)
                {
                    //get config table row
                    QueryExpression qe = new QueryExpression(ent.LogicalName);
                    FilterExpression fe = new FilterExpression();
                    qe.ColumnSet = new ColumnSet(true);
                    qe.Orders.Add(new OrderExpression("createdon", OrderType.Descending));
                    var countContract = service.RetrieveMultiple(qe).Entities.First();

                    //مقدار پیش فرض
                    var charFix = organizationName;

                    Entity en = new Entity(ent.LogicalName);
                    en.Id = ent.Id;

                    if (countContract.Attributes[fieldName.Get(context)] == null)
                    {
                        en.Attributes.Add(fieldName.Get(context), charFix + "-" + "001");
                    }
                    else
                    {
                        Entity AutoPost = service.Retrieve(ent.LogicalName, countContract.Id, new ColumnSet(true));
                        var currentrecordcounternumber = AutoPost.GetAttributeValue<string>(fieldName.Get(context));

                        var lencurrentrecordcounternumber = currentrecordcounternumber.Length - 3;
                        var currentrecordcounternumbers = currentrecordcounternumber.Substring(lencurrentrecordcounternumber, 3);

                        //initialize counter
                        var numbers = string.Concat(currentrecordcounternumbers.Where(char.IsNumber));
                        var lenNum = numbers.Length;

                        var newCounterValue = (Convert.ToInt32(numbers) + 1).ToString().PadLeft(lenNum, '0');

                        //new id;
                        en.Attributes.Add(fieldName.Get(context), charFix + "-" + newCounterValue.ToString());
                    }
                }
                else
                {
                    throw new InvalidPluginExecutionException("The number can only be set by the system.");
                }
            }
            catch (Exception ex)
            {
                new InvalidPluginExecutionException (ex.Message);
            }
        }
    }
}
