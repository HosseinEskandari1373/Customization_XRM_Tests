using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

//namespaces for d365 interaction
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using CrmEarlyBound;
using Customization_XRM;

namespace DotNet_Customization_Organization_XRM
{
    public class LeadUpdateTest2 : IPlugin
    {
        public void Execute(IServiceProvider serviceProvider)
        {
            //Initializing Service Context.
            IPluginExecutionContext context = (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));
            IOrganizationServiceFactory factory = (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));
            IOrganizationService service = factory.CreateOrganizationService(context.UserId);
            try
            {
                //Defining Entity Object.
                Entity eTarget = null;
                if (context.MessageName == "Create")
                {
                    //Retrieving Target Entity.
                    eTarget = (context.InputParameters.Contains("Target") && context.InputParameters["Target"] != null) ?
                    context.InputParameters["Target"] as Entity : null;

                    //Retrieving Target Entity.
                    //leadNew = (context.InputParameters.Contains("Target") && context.InputParameters["Target"] != null) ?
                    //context.InputParameters["Target"] as Entity : null;

                    //string telephone1_New = leadNew.GetAttributeValue<string>("telephone1");//(leadNew.GetAttributeValue<string>("telephone1") != "") ? leadNew.GetAttributeValue<string>("telephone1") : null;
                    //string mobilephone_New = leadNew.GetAttributeValue<string>("mobilephone");//(leadNew.GetAttributeValue<string>("mobilephone") != "") ? leadNew.GetAttributeValue<string>("mobilephone") : null; 
                    //Triggering The Logic If Target Is Not Null.
                    if (eTarget != null)
                    {
                        //Stage 1.
                        QueryExpression qeAccount = new QueryExpression("account");
                        //Stage 2.
                        qeAccount.ColumnSet.AddColumns("accountnumber", "accountname", "accountid");
                        //Stage 3.
                        FilterExpression filter = new FilterExpression(LogicalOperator.And);
                        filter.AddCondition("accountnumber", ConditionOperator.Equal, eTarget.GetAttributeValue<string>("accountnumber"));
                        filter.AddCondition("accountname", ConditionOperator.NotNull);
                        qeAccount.Criteria.AddFilter(filter);
                        //Stage 4.
                        EntityCollection entityCollection = service.RetrieveMultiple(qeAccount);
                        if (entityCollection.Entities.Count == 0)
                        {
                            // No Account Found With Similar Account Number.
                        }
                        else
                        {
                            //Stage 5.
                            int TotalAccountCount = entityCollection.Entities.Count;
                            //Account Records Found With Similar Account Number.
                            foreach (var entity in entityCollection.Entities)
                            {
                                //Stage 6.
                                Entity eAccountNew = new Entity("account");
                                eAccountNew.Id = entity.Id;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new InvalidPluginExecutionException(ex.Message);
            }


            //// Obtain the execution context from the service provider. 
            //IPluginExecutionContext context = (IPluginExecutionContext)
            //    serviceProvider.GetService(typeof(IPluginExecutionContext));

            //// Obtain the organization service reference. 
            //IOrganizationServiceFactory serviceFactory =
            //    (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));

            //IOrganizationService service =
            //    serviceFactory.CreateOrganizationService(context.UserId);

            //if (context.MessageName != "Update") { return; }

            //if (context.InputParameters.Contains("Target") && context.InputParameters["Target"] is Entity)
            //{
            //    Entity entityTarget = (Entity)context.InputParameters["Target"];
            //    Entity leadEntity = service.Retrieve(entityTarget.LogicalName, entityTarget.Id, new ColumnSet(true));
            //    Entity leadEarlyBound = new Lead();

            //    if (context.InputParameters.Contains("Target") && context.InputParameters["Target"] is Entity)
            //    {
            //        List<LeadPhone> listLeadPhone = new List<LeadPhone>();
            //        LeadPhone leadContactPhone =
            //            new LeadPhone
            //            {
            //                Business_Phone = leadEntity.GetAttributeValue<string>("telephone1"),
            //                Mobile_Phone = leadEntity.GetAttributeValue<string>("mobilephone")
            //            };

            //        listLeadPhone.Add(leadContactPhone);

            //        //get config table row
            //        QueryExpression qe = new QueryExpression("contact");
            //        FilterExpression fe = new FilterExpression();
            //        qe.ColumnSet = new ColumnSet("telephone1", "mobilephone");
            //        var countContact = service.RetrieveMultiple(qe).Entities;

            //        ConditionExpression x = new ConditionExpression(leadContactPhone.Mobile_Phone, ConditionOperator.In, countContact);
            //        var res = x.Values.Count;
            //    }
            //}  
        }
    }
}
