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
    public class LeadUpdateTest3 : IPlugin
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

            try
            {
                if (context.MessageName.ToLower() == "update")
                {
                    //Defining Entity Object.
                    Entity eTarget = null;

                    if (context.InputParameters.Contains("Target") && context.InputParameters["Target"] is Entity)
                    {
                        Entity entityTarget = (Entity)context.InputParameters["Target"];
                        eTarget = service.Retrieve(entityTarget.LogicalName, entityTarget.Id, new ColumnSet(true));

                        if (eTarget.GetAttributeValue<string>("telephone1") is null && eTarget.GetAttributeValue<string>("mobilephone") != null)
                        {
                            eTarget.Attributes.Add("telephone1", "###");
                        }
                        else if (eTarget.GetAttributeValue<string>("mobilephone") is null && eTarget.GetAttributeValue<string>("telephone1") != null)
                        {
                            eTarget.Attributes.Add("mobilephone", "###");
                        }
                        else if (eTarget.GetAttributeValue<string>("telephone1") is null && eTarget.GetAttributeValue<string>("mobilephone") is null)
                        {
                            eTarget.Attributes.Add("telephone1", "###");
                            eTarget.Attributes.Add("mobilephone", "###");
                        }

                        var telephone1 = eTarget.GetAttributeValue<string>("telephone1");
                        var mobilephone = eTarget.GetAttributeValue<string>("mobilephone");

                        /****************************************************************************************************/
                        //Compare Lead with Contact
                        var leadRefUpdate = (EntityReference)(eTarget.Attributes["new_businessdivisionid"]);
                        var RetriveProject = service.Retrieve(leadRefUpdate.LogicalName, leadRefUpdate.Id, new ColumnSet(true));
                        Guid leadTaghsimatGuid = leadRefUpdate.Id;

                        //Triggering The Logic If Target Is Not Null.
                        if (eTarget != null)
                        {
                            //get config table row for empty entity 
                            var fetchContact = @"<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='false'>
                                          <entity name='contact'>
                                            <attribute name='fullname' />
                                            <attribute name='telephone1' />
                                            <attribute name='mobilephone' />
                                            <attribute name='contactid' />
                                            <attribute name='new_businessdivisionid' />
                                            <order attribute='fullname' descending='false' />
                                            <filter type='and'>
                                              <condition attribute='ownerid' operator='eq-userid' />
                                              <condition attribute='statecode' operator='eq' value='0' />
                                            </filter>
                                          </entity>
                                       </fetch>";

                            var fetchXMLContact = string.Format(fetchContact, eTarget.Id);
                            var allContracts = service.RetrieveMultiple(new FetchExpression(fetchXMLContact)).Entities;
                            Guid autoNumberRecordIdContact = Guid.Empty;
                            List<Contacts> taghsimatContactName = new List<Contacts>();

                            foreach (var itemLookUp in allContracts)
                            {
                                var contactRef = (EntityReference)(itemLookUp.Attributes["new_businessdivisionid"]);
                                var resContact = contactRef.Name.ToString();
                                autoNumberRecordIdContact = contactRef.Id;

                                Contacts contacts = new Contacts()
                                {
                                    BusinessDivisionName = resContact,
                                    Business_Phone = itemLookUp.GetAttributeValue<string>("telephone1"),
                                    Mobile_Phone = itemLookUp.GetAttributeValue<string>("mobilephone"),
                                    Id = autoNumberRecordIdContact
                                };

                                taghsimatContactName.Add(contacts);
                            }

                            //Stage 1.
                            QueryExpression qeContact = new QueryExpression("contact");

                            //Stage 2.
                            qeContact.ColumnSet.AddColumns("telephone1", "mobilephone", "contactid", "new_businessdivisionid");
                            FilterExpression filterMainContact = new FilterExpression(LogicalOperator.And);

                            //Stage 3.
                            FilterExpression filterContact = new FilterExpression(LogicalOperator.Or);
                            filterContact.AddCondition("telephone1", ConditionOperator.Equal, telephone1);
                            filterContact.AddCondition("mobilephone", ConditionOperator.Equal, mobilephone);
                            filterContact.AddCondition("telephone1", ConditionOperator.Equal, mobilephone);
                            filterContact.AddCondition("mobilephone", ConditionOperator.Equal, telephone1);

                            FilterExpression filterTghsimatContact = new FilterExpression();
                            filterTghsimatContact.AddCondition("new_businessdivisionid", ConditionOperator.Equal, leadTaghsimatGuid);

                            filterMainContact.AddFilter(filterContact);
                            filterMainContact.AddFilter(filterTghsimatContact);

                            qeContact.Criteria.AddFilter(filterMainContact);
                            EntityCollection entityCollectionContact = service.RetrieveMultiple(qeContact);

                            /****************************************************************************************************/
                            //Compare Lead with Account
                            //get config table row for empty entity 
                            var fetchAccount = @"<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='false'>
                                          <entity name='contact'>
                                            <attribute name='fullname' />
                                            <attribute name='telephone1' />
                                            <attribute name='mobilephone' />
                                            <attribute name='contactid' />
                                            <attribute name='new_businessdivisionid' />
                                            <order attribute='fullname' descending='false' />
                                            <filter type='and'>
                                              <condition attribute='ownerid' operator='eq-userid' />
                                              <condition attribute='statecode' operator='eq' value='0' />
                                            </filter>
                                          </entity>
                                       </fetch>";

                            var fetchXMLAccount = string.Format(fetchAccount, eTarget.Id);
                            var allAccounts = service.RetrieveMultiple(new FetchExpression(fetchXMLAccount)).Entities;
                            Guid autoNumberRecordId = Guid.Empty;
                            List<Accounts> taghsimatAccountName = new List<Accounts>();

                            foreach (var itemLookUp in allAccounts)
                            {
                                var accactRef = (EntityReference)(itemLookUp.Attributes["new_businessdivisionid"]);
                                var resAccount = accactRef.Name.ToString();
                                autoNumberRecordId = accactRef.Id;

                                Accounts accounts = new Accounts()
                                {
                                    BusinessDivisionName = resAccount,
                                    Business_Phone = itemLookUp.GetAttributeValue<string>("telephone1"),
                                    Id = autoNumberRecordId
                                };

                                taghsimatAccountName.Add(accounts);
                            }

                            //Stage 1.
                            QueryExpression qeAccount = new QueryExpression("account");

                            //Stage 2.
                            qeAccount.ColumnSet.AddColumns("telephone1", "accountid", "new_businessdivisionid");
                            FilterExpression filterMainAccount = new FilterExpression(LogicalOperator.And);

                            //Stage 3.
                            FilterExpression filterAccount = new FilterExpression();
                            filterAccount.AddCondition("telephone1", ConditionOperator.Equal, telephone1);
                            filterAccount.AddCondition("telephone1", ConditionOperator.Equal, mobilephone);

                            FilterExpression filterTghsimatAccount = new FilterExpression();
                            filterTghsimatAccount.AddCondition("new_businessdivisionid", ConditionOperator.Equal, leadTaghsimatGuid);

                            filterMainAccount.AddFilter(filterAccount);
                            filterMainAccount.AddFilter(filterTghsimatAccount);

                            //Stage 4.
                            qeAccount.Criteria.AddFilter(filterMainAccount);
                            EntityCollection entityCollectionAccount = service.RetrieveMultiple(qeAccount);

                            if ((entityCollectionContact.Entities.Count == 0 && entityCollectionAccount.Entities.Count == 0))
                            {
                                // No Account Found With Similar Account Number.
                                throw new InvalidPluginExecutionException("شماره تلفن تکراری نیست");
                            }
                            else
                            {
                                throw new InvalidPluginExecutionException("شماره تلفن تکراری است و امکان ثبت آن وجود ندارد.");
                                //Account Records Found With Similar Account Number.
                            }
                        }
                    }
                }
                else if (context.MessageName.ToLower() == "qualifylead")
                {
                    //Defining Entity Object.
                    Entity leadEntity = null;

                    if (context.InputParameters.Contains("LeadId") && context.InputParameters["LeadId"] is EntityReference)
                    {
                        EntityReference leadRef = (EntityReference)context.InputParameters["LeadId"];
                        leadEntity = service.Retrieve(leadRef.LogicalName, leadRef.Id, new ColumnSet(true));

                        if (leadEntity.GetAttributeValue<string>("telephone1") is null && leadEntity.GetAttributeValue<string>("mobilephone") != null)
                        {
                            leadEntity.Attributes.Add("telephone1", "###");
                        }
                        else if (leadEntity.GetAttributeValue<string>("mobilephone") is null && leadEntity.GetAttributeValue<string>("telephone1") != null)
                        {
                            leadEntity.Attributes.Add("mobilephone", "###");
                        }
                        else if (leadEntity.GetAttributeValue<string>("telephone1") is null && leadEntity.GetAttributeValue<string>("mobilephone") is null)
                        {
                            leadEntity.Attributes.Add("telephone1", "###");
                            leadEntity.Attributes.Add("mobilephone", "###");
                        }

                        var telephone1 = leadEntity.GetAttributeValue<string>("telephone1");
                        var mobilephone = leadEntity.GetAttributeValue<string>("mobilephone");

                        /****************************************************************************************************/
                        //Compare Lead with Contact
                        var leadRefQuality = (EntityReference)(leadEntity.Attributes["new_businessdivisionid"]);
                        var RetriveProject = service.Retrieve(leadRefQuality.LogicalName, leadRefQuality.Id, new ColumnSet(true));
                        Guid leadTaghsimatGuid = leadRefQuality.Id;

                        //Triggering The Logic If Target Is Not Null.
                        if (leadEntity != null)
                        {
                            //get config table row for empty entity 
                            var fetchContact = @"<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='false'>
                                                  <entity name='contact'>
                                                    <attribute name='fullname' />
                                                    <attribute name='parentcustomerid' />
                                                    <attribute name='telephone1' />
                                                    <attribute name='emailaddress1' />
                                                    <attribute name='contactid' />
                                                    <order attribute='fullname' descending='false' />
                                                    <filter type='and'>
                                                      <condition attribute='ownerid' operator='eq-userid' />
                                                      <condition attribute='statecode' operator='eq' value='0' />
                                                      <condition attribute='originatingleadid' operator='null' />
                                                    </filter>
                                                  </entity>
                                                </fetch>";

                            var fetchXMLContact = string.Format(fetchContact, leadEntity.Id);
                            var allContracts = service.RetrieveMultiple(new FetchExpression(fetchXMLContact)).Entities;
                            Guid autoNumberRecordIdContact = Guid.Empty;
                            List<Contacts> taghsimatContactName = new List<Contacts>();

                            foreach (var itemLookUp in allContracts)
                            {
                                var contactRef = (EntityReference)(itemLookUp.Attributes["new_businessdivisionid"]);
                                var resContact = contactRef.Name.ToString();
                                autoNumberRecordIdContact = contactRef.Id;

                                Contacts contacts = new Contacts()
                                {
                                    BusinessDivisionName = resContact,
                                    Business_Phone = itemLookUp.GetAttributeValue<string>("telephone1"),
                                    Mobile_Phone = itemLookUp.GetAttributeValue<string>("mobilephone"),
                                    Id = autoNumberRecordIdContact
                                };

                                taghsimatContactName.Add(contacts);
                            }

                            //Stage 1.
                            QueryExpression qeContact = new QueryExpression("contact");

                            //Stage 2.
                            qeContact.ColumnSet.AddColumns("telephone1", "mobilephone", "contactid", "new_businessdivisionid");
                            FilterExpression filterMainContact = new FilterExpression(LogicalOperator.And);

                            //Stage 3.
                            FilterExpression filterContact = new FilterExpression(LogicalOperator.Or);
                            filterContact.AddCondition("telephone1", ConditionOperator.Equal, telephone1);
                            filterContact.AddCondition("mobilephone", ConditionOperator.Equal, mobilephone);
                            filterContact.AddCondition("telephone1", ConditionOperator.Equal, mobilephone);
                            filterContact.AddCondition("mobilephone", ConditionOperator.Equal, telephone1);

                            FilterExpression filterTghsimatContact = new FilterExpression();
                            filterTghsimatContact.AddCondition("new_businessdivisionid", ConditionOperator.Equal, leadTaghsimatGuid);

                            filterMainContact.AddFilter(filterContact);
                            filterMainContact.AddFilter(filterTghsimatContact);

                            qeContact.Criteria.AddFilter(filterMainContact);
                            EntityCollection entityCollectionContact = service.RetrieveMultiple(qeContact);

                            /****************************************************************************************************/
                            //Compare Lead with Account
                            //get config table row for empty entity 
                            var fetchAccount = @"<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='false'>
                                          <entity name='contact'>
                                            <attribute name='fullname' />
                                            <attribute name='telephone1' />
                                            <attribute name='mobilephone' />
                                            <attribute name='contactid' />
                                            <attribute name='new_businessdivisionid' />
                                            <order attribute='fullname' descending='false' />
                                            <filter type='and'>
                                              <condition attribute='ownerid' operator='eq-userid' />
                                              <condition attribute='statecode' operator='eq' value='0' />
                                            </filter>
                                          </entity>
                                       </fetch>";

                            var fetchXMLAccount = string.Format(fetchAccount, leadEntity.Id);
                            var allAccounts = service.RetrieveMultiple(new FetchExpression(fetchXMLAccount)).Entities;
                            Guid autoNumberRecordId = Guid.Empty;
                            List<Accounts> taghsimatAccountName = new List<Accounts>();

                            foreach (var itemLookUp in allAccounts)
                            {
                                var accactRef = (EntityReference)(itemLookUp.Attributes["new_businessdivisionid"]);
                                var resAccount = accactRef.Name.ToString();
                                autoNumberRecordId = accactRef.Id;

                                Accounts accounts = new Accounts()
                                {
                                    BusinessDivisionName = resAccount,
                                    Business_Phone = itemLookUp.GetAttributeValue<string>("telephone1"),
                                    Id = autoNumberRecordId
                                };

                                taghsimatAccountName.Add(accounts);
                            }

                            //Stage 1.
                            QueryExpression qeAccount = new QueryExpression("account");

                            //Stage 2.
                            qeAccount.ColumnSet.AddColumns("telephone1", "accountid", "new_businessdivisionid");
                            FilterExpression filterMainAccount = new FilterExpression(LogicalOperator.And);

                            //Stage 3.
                            FilterExpression filterAccount = new FilterExpression();
                            filterAccount.AddCondition("telephone1", ConditionOperator.Equal, telephone1);
                            filterAccount.AddCondition("telephone1", ConditionOperator.Equal, mobilephone);

                            FilterExpression filterTghsimatAccount = new FilterExpression();
                            filterTghsimatAccount.AddCondition("new_businessdivisionid", ConditionOperator.Equal, leadTaghsimatGuid);

                            filterMainAccount.AddFilter(filterAccount);
                            filterMainAccount.AddFilter(filterTghsimatAccount);

                            //Stage 4.
                            qeAccount.Criteria.AddFilter(filterMainAccount);
                            EntityCollection entityCollectionAccount = service.RetrieveMultiple(qeAccount);

                            if ((entityCollectionContact.Entities.Count == 0 && entityCollectionAccount.Entities.Count == 0))
                            {
                                // No Account Found With Similar Account Number.
                                throw new InvalidPluginExecutionException("شماره تلفن تکراری نیست");
                            }
                            else
                            {
                                throw new InvalidPluginExecutionException("شماره تلفن تکراری است و امکان ثبت آن وجود ندارد.");
                                //Account Records Found With Similar Account Number.
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new InvalidPluginExecutionException(ex.Message);
            }
        }
    }
}
