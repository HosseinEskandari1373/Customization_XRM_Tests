using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

//namespaces for d365 interaction
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;

namespace ClassLibrary1
{
    public class JahanPars : IPlugin
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

            if (context.InputParameters.Contains("Target") &&
                context.InputParameters["Target"] is Entity)
            {
                Entity entity = (Entity)context.InputParameters["Target"];

                if (entity.LogicalName == "new_contract")
                {
                    if (entity.Attributes.Contains("new_countcontract") == false)
                    {
                        if (context.InputParameters.Contains("Target") && context.InputParameters["Target"] is Entity)
                        {
                            //get config table row
                            var fetch = @"<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='false'>
                                              <entity name='new_contract'>
                                                <attribute name='new_name' />
                                                <attribute name='createdon' />
                                                <attribute name='new_typecontract' />
                                                <attribute name='new_projectid' />
                                                <attribute name='new_countcontract' />
                                                <attribute name='new_yearcontract' />
                                                <attribute name='new_contractid' />
                                                <order attribute='new_name' descending='false' />
                                                <filter type='and'>
                                                  <condition attribute='statecode' operator='eq' value='0' />
                                                </filter>
                                              </entity>
                                           </fetch>";

                            EntityCollection ecAuto = service.RetrieveMultiple(new FetchExpression(fetch));
                            Guid autoNumberRecordId = Guid.Empty;

                            foreach (var itemLookUp in ecAuto.Entities)
                            {
                                autoNumberRecordId = itemLookUp.Id;
                            }

                            //مقدار پیش فرض
                            var charFix = "JP";

                            //سه حرف اول نام پروژه
                            var ProjectRef = (EntityReference)(entity.Attributes["new_projectid"]);
                            var RetriveProject = service.Retrieve(ProjectRef.LogicalName, ProjectRef.Id, new ColumnSet(true));
                            var nameProject = RetriveProject["new_name"].ToString().Substring(0, 3);

                            //مقدار option set
                            int value = ((OptionSetValue)entity["new_typecontract"]).Value;

                            var optionVal = "";

                            if (value == 100000000)
                            {
                                optionVal = "H";
                            }
                            else if (value == 100000001)
                            {
                                optionVal = "L";
                            }
                            else
                            {
                                optionVal = "E";
                            }

                            //خواندن تاریخ
                            var datetimes = Convert.ToDateTime(entity["new_datecontract"]);
                            var calendar = new PersianCalendar();
                            var persianDate = new DateTime(calendar.GetYear(datetimes), calendar.GetMonth(datetimes), calendar.GetDayOfMonth(datetimes));
                            var persianDateYear = calendar.GetYear(datetimes).ToString();
                            var resultYear = persianDateYear.Substring(2, 2);

                            //دو عدد آخر سال
                            var dates = entity["new_yearcontract"].ToString();
                            var lenDate = dates.Length;
                            var charDate = dates.Substring(2, 2);

                            if (autoNumberRecordId == Guid.Empty)
                            {
                                entity.Attributes.Add("new_countcontract", charFix + "-" + nameProject + "-" + optionVal + "-" + resultYear + "-" + "001");
                            }

                            Entity AutoPost = service.Retrieve("new_contract", autoNumberRecordId, new ColumnSet(true));
                            var currentrecordcounternumber = AutoPost.GetAttributeValue<string>("new_countcontract");

                            var lencurrentrecordcounternumber = currentrecordcounternumber.Length - 3;
                            var currentrecordcounternumbers = currentrecordcounternumber.Substring(lencurrentrecordcounternumber, 3);

                            //initialize counter
                            var numbers = string.Concat(currentrecordcounternumbers.Where(char.IsNumber));
                            var lenNum = numbers.Length;

                            var newCounterValue = (Convert.ToInt32(numbers) + 1).ToString().PadLeft(lenNum, '0');

                            //new id;
                            entity.Attributes.Add("new_countcontract", charFix + "-" + nameProject + "-" + optionVal + "-" + resultYear + "-" + newCounterValue.ToString());
                        }
                    }
                    else
                    {
                        throw new InvalidPluginExecutionException("The account number can only be set by the system.");
                    }
                }
            }
        }
    }
}
