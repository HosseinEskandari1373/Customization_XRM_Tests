﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

//namespaces for d365 interaction
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;


namespace Update_All_Childs
{
    public class Update_All_Childs : IPlugin
    {
        public void Execute(IServiceProvider serviceProvider)
        {
            if (serviceProvider == null)
                throw new ArgumentNullException("serviceProvider");

            var context = (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));
            var serviceFactory = (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));
            var service = serviceFactory.CreateOrganizationService(context.UserId);

            if (context.InputParameters.Contains("Target") && context.InputParameters["Target"] is Entity)
            {
                Entity targetAccount = (Entity)context.InputParameters["Target"];
                if (targetAccount.Contains("new_dateproject"))
                {
                    // Retrieve all contact for this account
                    var fetch = @"<fetch no-lock='true' >
                                     <entity name='new_contract' >
                                       <attribute name='new_projectid'/>
                                       <filter>
                                         <condition attribute='new_projectid' operator='eq' value='{0}' />
                                       </filter>
                                     </entity>
                                   </fetch>";

                    var fetchXML = string.Format(fetch, targetAccount.Id);
                    var allContacts = service.RetrieveMultiple(new FetchExpression(fetchXML)).Entities;

                    // Iterate through all contact record and update the phone number from account
                    foreach (var contactEnt in allContacts)
                    {
                        Entity contactToUpdate = new Entity(contactEnt.LogicalName, contactEnt.Id);
                        contactToUpdate["new_yearcontract"] = 1395;
                        OptionSetValue CurrencyTypeArzDolati = new OptionSetValue(100000002);
                        //شده است Set حذف مقدار قبلی که
                        contactToUpdate.Attributes.Remove("new_typecontract");
                        contactToUpdate.Attributes.Add("new_typecontract", CurrencyTypeArzDolati);
                        service.Update(contactToUpdate);
                    }
                }
            }
        }
    }
}
