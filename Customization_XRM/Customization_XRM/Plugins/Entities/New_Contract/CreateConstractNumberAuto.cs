using System;
using System.Collections.Generic;
using System.Globalization;
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
    public class CreateConstractNumberAuto : PluginBase
    {
        public CreateConstractNumberAuto() : base(typeof(CreateConstractNumberAuto))
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

                if (context.InputParameters.Contains("Target") &&
                        context.InputParameters["Target"] is Entity)
                {
                    Entity entity = (Entity)context.InputParameters["Target"];
                    Entity contract = new new_Contract();

                    if (entity.LogicalName == contract.LogicalName /*فرم قرارداد*/)
                    {
                        if (context.InputParameters.Contains("Target") && context.InputParameters["Target"] is Entity)
                        {
                            //get config table row for empty entity 
                            var fetch = @"<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='false'>
                                          <entity name='new_contract'>
                                            <attribute name='new_countcontract' />
                                            <order attribute='new_name' descending='false' />
                                            <filter type='and'>
                                              <condition attribute='statecode' operator='eq' value='0' />
                                            </filter>
                                          </entity>
                                      </fetch>";

                            EntityCollection ecAuto = crmService.RetrieveMultiple(new FetchExpression(fetch));
                            Guid autoNumberRecordId = Guid.Empty;

                            foreach (var itemLookUp in ecAuto.Entities)
                            {
                                autoNumberRecordId = itemLookUp.Id;
                            }

                            //------------------------------------------------

                            //مقدار پیش فرض 
                            var charFix = "JP";

                            //سه حرف اول نام پروژه 
                            var ProjectRef = (EntityReference)(entity.Attributes["new_projectid"] /*پروژه مرتبط*/);
                            var RetriveProject = crmService.Retrieve(ProjectRef.LogicalName, ProjectRef.Id, new ColumnSet(true));
                            var nameProject = RetriveProject["new_name"].ToString().Substring(0, 3);

                            //مقدار option set 
                            int value;
                            var optionVal = "";
                            if (entity.Attributes.Contains("new_typecontract" /*نوع قرارداد*/) == false)
                            {
                                throw new InvalidPluginExecutionException("لطفا نوع قرارداد را تکمیل نمائید.");
                            }
                            else
                            {
                                value = ((OptionSetValue)entity["new_typecontract"]).Value;

                                if (value == 100000000)
                                {
                                    optionVal = "H";
                                }
                                else if (value == 100000001)
                                {
                                    optionVal = "L";
                                }
                                else if (value == 100000002)
                                {
                                    optionVal = "E";
                                }
                            }

                            //خواندن تاریخ
                            DateTime datetimes;
                            var resultYear = "";
                            if (entity.Attributes.Contains("new_datecontract" /*تاریخ قرارداد*/) == false)
                            {
                                throw new InvalidPluginExecutionException("لطفا تاریخ قرارداد را تکمیل نمائید.");
                            }
                            else
                            {
                                datetimes = Convert.ToDateTime(entity["new_datecontract" /*تاریخ قرارداد*/]);
                                var calendar = new PersianCalendar();
                                var persianDate = new DateTime(calendar.GetYear(datetimes), calendar.GetMonth(datetimes), calendar.GetDayOfMonth(datetimes));
                                var persianDateYear = calendar.GetYear(datetimes).ToString();
                                resultYear = persianDateYear.Substring(2, 2);
                            }

                            if (autoNumberRecordId == Guid.Empty)
                            {
                                entity.Attributes.Add("new_countcontract" /*شماره قرارداد*/, charFix + "-" + nameProject + "-" + optionVal + "-" + resultYear + "-" + "001");
                            }
                            else
                            {
                                //get config table row
                                QueryExpression qe = new QueryExpression("new_contract");
                                FilterExpression fe = new FilterExpression();
                                qe.ColumnSet = new ColumnSet(true);
                                qe.Orders.Add(new OrderExpression("createdon", OrderType.Descending));
                                var countContract = crmService.RetrieveMultiple(qe).Entities.First();

                                Entity AutoPost = crmService.Retrieve(contract.LogicalName /*فرم قرارداد*/, countContract.Id, new ColumnSet(true));
                                var currentrecordcounternumber = AutoPost.GetAttributeValue<string>("new_countcontract" /*شماره قرارداد*/);

                                var lencurrentrecordcounternumber = currentrecordcounternumber.Length - 3;
                                var currentrecordcounternumbers = currentrecordcounternumber.Substring(lencurrentrecordcounternumber, 3);

                                //initialize counter 
                                var numbers = string.Concat(currentrecordcounternumbers.Where(char.IsNumber));
                                var lenNum = numbers.Length;

                                var newCounterValue = (Convert.ToInt32(numbers) + 1).ToString().PadLeft(lenNum, '0');

                                //new id; 
                                entity.Attributes.Add("new_cIountcontract" /*شماره قرارداد*/, charFix + "-" + nameProject + "-" + optionVal + "-" + resultYear + "-" + newCounterValue.ToString());
                            }
                        }
                        else
                        {
                            throw new InvalidPluginExecutionException("The account number can only be set by the system.");
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
