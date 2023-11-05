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
    public class LeadUpdateTest : IPlugin
    {
        public void Execute(IServiceProvider serviceProvider)
        {
            // Obtain the execution context from the service provider. 
            IPluginExecutionContext context = (IPluginExecutionContext)
                serviceProvider.GetService(typeof(IPluginExecutionContext));

            // Obtain the organization service reference. 
            IOrganizationServiceFactory serviceFactory =
                (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));

            IOrganizationService service =
                serviceFactory.CreateOrganizationService(context.UserId);

            if (context.MessageName != "Update") { return; }

            if (context.InputParameters.Contains("Target") && context.InputParameters["Target"] is Entity)
            {
                Entity entity = (Entity)context.InputParameters["Target"];
                Entity leadEntity = service.Retrieve(entity.LogicalName, entity.Id, new ColumnSet(true));
                Entity lead = new Lead();

                if (leadEntity.LogicalName == lead.LogicalName)
                {
                    if (context.InputParameters.Contains("Target") && context.InputParameters["Target"] is Entity)
                    {
                        List<LeadPhone> listLeadPhone = new List<LeadPhone>();
                        LeadPhone liPhone =
                            new LeadPhone
                            {
                                Business_Phone = leadEntity.GetAttributeValue<string>("telephone1"),
                                Mobile_Phone = leadEntity.GetAttributeValue<string>("mobilephone"),
                                Company_Name = entity.GetAttributeValue<string>("companyname"),
                                Full_Name = entity.GetAttributeValue<string>("fullname"),
                                State_Code = entity.GetAttributeValue<OptionSetValue>("statecode"),
                                Status_Code = entity.GetAttributeValue<OptionSetValue>("statuscode")
                            };

                        listLeadPhone.Add(liPhone);

                        // Retrieve all contact
                        Entity contact = new Contact();
                        var fetchContact = @"<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='false'>
                                      <entity name='contact'>
                                        <attribute name='fullname' />
                                        <attribute name='parentcustomerid' />
                                        <attribute name='telephone1' />
                                        <attribute name='emailaddress1' />
                                        <attribute name='contactid' />
                                        <attribute name='telephone3' />
                                        <attribute name='address2_telephone3' />
                                        <attribute name='address2_telephone2' />
                                        <attribute name='address2_telephone1' />
                                        <attribute name='address2_name' />
                                        <attribute name='address1_telephone3' />
                                        <attribute name='address1_telephone2' />
                                        <attribute name='address1_telephone1' />
                                        <attribute name='address1_name' />
                                        <order attribute='fullname' descending='false' />
                                        <filter type='and'>
                                          <condition attribute='ownerid' operator='eq-userid' />
                                          <condition attribute='statecode' operator='eq' value='0' />
                                        </filter>
                                      </entity>
                                    </fetch>";

                        var fetchXMLContact = string.Format(fetchContact, contact.Id);
                        var allContacts = service.RetrieveMultiple(new FetchExpression(fetchXMLContact)).Entities;

                        List<ContactPhone> listContactPhone = new List<ContactPhone>();

                        //get config table row
                        QueryExpression qe = new QueryExpression("contact");
                        FilterExpression fe = new FilterExpression();
                        qe.ColumnSet = new ColumnSet("telephone1", "mobilephone");
                        var countContact = service.RetrieveMultiple(qe).Entities;

                        foreach (var item in allContacts)
                        {
                            Entity conPhone = new Entity(item.LogicalName, item.Id);
                            conPhone = service.Retrieve(item.LogicalName, item.Id, new ColumnSet(true));

                            


                            ////Entity conPhone = new Entity(item.LogicalName, item.Id);
                            //var contactTelephone1 = conPhone.GetAttributeValue<string>("telephone1");
                            //var contactMobilePhone = conPhone.GetAttributeValue<string>("mobilephone");

                            //var x = leadEntity.GetAttributeValue<string>("telephone1");
                            //var y = leadEntity.GetAttributeValue<string>("mobilephone");

                            //var x1 = entity.Attributes["telephone1"];
                            //var y1 = entity.Attributes["mobilephone"];

                            //for (int i = 0; i < allContacts.Count; i++)
                            //{
                            //    if (
                            //          x == contactTelephone1 /*||
                            //          x == contactMobilePhone ||
                            //          y == contactTelephone1 ||
                            //          y == contactMobilePhone*/
                            //       )
                            //    {
                            //        //throw new InvalidPluginExecutionException("شماره تلفن تکراری است");
                            //        //valContactPhone = valContactPhone + 1;
                            //        entity["companyname"] = "09090909";
                            //        service.Update(conPhone);
                            //    }
                            //    else if (
                            //                x == contactTelephone1 /*||
                            //                x == contactMobilePhone ||
                            //                y == contactTelephone1 ||
                            //                y == contactMobilePhone*/
                            //            )
                            //    {
                            //        throw new InvalidPluginExecutionException("شماره تلفن تکراری نیست");
                            //    }
                            //}

                            //valContactPhone = listLeadPhone.Any(
                            //                                       s => s.Business_Phone == conPhone.GetAttributeValue<string>("telephone1") ||
                            //                                       s.Business_Phone == conPhone.GetAttributeValue<string>("mobilephone") ||
                            //                                       s.Mobile_Phone == conPhone.GetAttributeValue<string>("telephone1") ||
                            //                                       s.Mobile_Phone == conPhone.GetAttributeValue<string>("mobilephone")
                            //                                    );

                            //if (valContactPhone >= 1)
                            //{
                            //    throw new InvalidPluginExecutionException("شماره تلفن تکراری است");
                            //}
                        }

                        //if (3 >= 1)
                        //{
                        //    throw new InvalidPluginExecutionException("شماره تلفن تکراری است");
                        //}
                    }
                }
            }
        }
    }
}
