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
    public class Relation_One_to_Many : IPlugin
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
                    var allContracts = service.RetrieveMultiple(new FetchExpression(fetchXML)).Entities;

                    // Iterate through all contact record and update the phone number from account
                    foreach (var contractEnt in allContracts)
                    {
                        Entity contractToUpdate = new Entity(contractEnt.LogicalName, contractEnt.Id);
                        contractToUpdate["new_yearcontract" /*تعداد قرارداد*/] = 1400;

                        OptionSetValue OptionVal = new OptionSetValue(100000002);
                        //شده است Set حذف مقدار قبلی که
                        contractToUpdate.Attributes.Remove("new_typecontract" /*نوع قرارداد*/);
                        contractToUpdate.Attributes.Add("new_typecontract", OptionVal);
                        service.Update(contractToUpdate);
                    }
                }
            }
        }
    }
}
