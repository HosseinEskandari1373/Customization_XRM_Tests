using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms.VisualStyles;

//namespaces for d365 interaction
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;

namespace All_Organization_Plugins.Plugins.TestLab_Org
{
    public class CreateConstractNumberAuto : IPlugin
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
                            QueryExpression qe = new QueryExpression("new_contract");
                            FilterExpression fe = new FilterExpression();
                            qe.ColumnSet = new ColumnSet(true);
                            qe.Orders.Add(new OrderExpression("createdon", OrderType.Descending));
                            var countContract = service.RetrieveMultiple(qe).Entities.First();

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

                            if (countContract.Attributes["new_countcontract"] == null)
                            {
                                entity.Attributes.Add("new_countcontract", charFix + "-" + nameProject + "-" + optionVal + "-" + resultYear + "-" + "001");
                            }

                            Entity AutoPost = service.Retrieve("new_contract", countContract.Id, new ColumnSet(true));
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
