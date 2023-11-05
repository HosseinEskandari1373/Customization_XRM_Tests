using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

//namespaces for d365 interaction
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Crm.Sdk.Messages;
using System.Globalization;
using CrmEarlyBound;
using Customization_XRM.Plugin;
using System.ServiceModel.Description;
using Microsoft.Xrm.Sdk.Client;

namespace Customization_XRM
{
    public class Lead_Compare_Phones : PluginBase
    {
        //Constructor
        public Lead_Compare_Phones() : base(typeof(Lead_Compare_Phones))
        {

        }

        //Override PluginBase
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

                if (context.MessageName.ToLower() != "update") { return; }

                Entity eTarget = null;

                if (context.InputParameters.Contains("Target") && context.InputParameters["Target"] is Entity)
                {
                    Entity entityTarget = (Entity)context.InputParameters["Target"];
                    eTarget = crmService.Retrieve(entityTarget.LogicalName, entityTarget.Id, new ColumnSet(true));

                    if (eTarget != null && eTarget.Contains("statuscode"))
                    {
                        if (eTarget.GetAttributeValue<OptionSetValue>("statuscode").Value == 4 ||
                            eTarget.GetAttributeValue<OptionSetValue>("statuscode").Value == 5 ||
                            eTarget.GetAttributeValue<OptionSetValue>("statuscode").Value == 6 ||
                            eTarget.GetAttributeValue<OptionSetValue>("statuscode").Value == 7) 
                        {
                            return;
                        }
                    }

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
                    var leadRef = (EntityReference)(eTarget.Attributes["new_businessdivisionid"]);
                    var RetriveProject = crmService.Retrieve(leadRef.LogicalName, leadRef.Id, new ColumnSet(true));
                    Guid leadTaghsimatGuid = leadRef.Id;

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
                        var allContracts = crmService.RetrieveMultiple(new FetchExpression(fetchXMLContact)).Entities;
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
                        EntityCollection entityCollectionContact = crmService.RetrieveMultiple(qeContact);

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
                        var allAccounts = crmService.RetrieveMultiple(new FetchExpression(fetchXMLAccount)).Entities;
                        Guid autoNumberRecordIdAccount = Guid.Empty;
                        List<Accounts> taghsimatAccountName = new List<Accounts>();

                        foreach (var itemLookUp in allAccounts)
                        {
                            var accactRef = (EntityReference)(itemLookUp.Attributes["new_businessdivisionid"]);
                            var resAccount = accactRef.Name.ToString();
                            autoNumberRecordIdAccount = accactRef.Id;

                            Accounts accounts = new Accounts()
                            {
                                BusinessDivisionName = resAccount,
                                Business_Phone = itemLookUp.GetAttributeValue<string>("telephone1"),
                                Id = autoNumberRecordIdAccount
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
                        EntityCollection entityCollectionAccount = crmService.RetrieveMultiple(qeAccount);

                        /****************************************************************************************************/
                        //Compare Lead with Lead
                        //get config table row for empty entity 
                        var fetchLead = @"<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='false'>
                                              <entity name='lead'>
                                                <attribute name='fullname' />
                                                <attribute name='leadid' />
                                                <attribute name='mobilephone' />
                                                <attribute name='companyname' />
                                                <attribute name='telephone1' />
                                                <attribute name='new_businessdivisionid' />
                                                <order attribute='createdon' descending='true' />
                                              </entity>
                                            </fetch>";

                        var fetchXMLLead = string.Format(fetchLead, eTarget.Id);
                        var allLeads = crmService.RetrieveMultiple(new FetchExpression(fetchXMLLead)).Entities;
                        Guid autoNumberRecordIdLeads = Guid.Empty;
                        List<Leads> taghsimatLeadName = new List<Leads>();

                        foreach (var itemLookUp in allLeads)
                        {
                            var leadRefe = (EntityReference)(itemLookUp.Attributes["new_businessdivisionid"]);
                            var resLead = leadRefe.Name.ToString();
                            autoNumberRecordIdLeads = leadRefe.Id;

                            Leads leads = new Leads()
                            {
                                BusinessDivisionName = resLead,
                                Business_Phone = itemLookUp.GetAttributeValue<string>("telephone1"),
                                Mobile_Phone = itemLookUp.GetAttributeValue<string>("mobilephone"),
                                Id = autoNumberRecordIdLeads
                            };

                            taghsimatLeadName.Add(leads);
                        }

                        //Stage 1.
                        QueryExpression qeLead = new QueryExpression("lead");

                        //Stage 2.
                        qeLead.ColumnSet.AddColumns("telephone1", "mobilephone", "contactid", "accountid", "new_businessdivisionid");
                        FilterExpression filterMainLead = new FilterExpression(LogicalOperator.And);

                        //Stage 3.
                        FilterExpression filterLead = new FilterExpression(LogicalOperator.Or);
                        filterLead.AddCondition("telephone1", ConditionOperator.Equal, telephone1);
                        filterLead.AddCondition("mobilephone", ConditionOperator.Equal, mobilephone);
                        filterLead.AddCondition("telephone1", ConditionOperator.Equal, mobilephone);
                        filterLead.AddCondition("mobilephone", ConditionOperator.Equal, telephone1);

                        FilterExpression filterTghsimatLead = new FilterExpression();
                        filterTghsimatLead.AddCondition("new_businessdivisionid", ConditionOperator.Equal, leadTaghsimatGuid);

                        filterMainLead.AddFilter(filterLead);
                        filterMainLead.AddFilter(filterTghsimatLead);

                        //Stage 4.
                        qeLead.Criteria.AddFilter(filterMainLead);
                        EntityCollection entityCollectionLead = crmService.RetrieveMultiple(qeLead);

                        if ((entityCollectionContact.Entities.Count == 0 && entityCollectionAccount.Entities.Count == 0 && entityCollectionLead.Entities.Count == 0))
                        {
                            // No Account Found With Similar Account Number.
                            //throw new InvalidPluginExecutionException("شماره تلفن تکراری نیست");
                        }
                        else
                        {
                            throw new InvalidPluginExecutionException("شماره تلفن تکراری است و امکان ثبت آن وجود ندارد.");
                            //Account Records Found With Similar Account Number.
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
