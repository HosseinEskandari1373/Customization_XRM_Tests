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


namespace DotNet_Customization_Organization_XRM
{
    public class Letter_NextDateOfBirth : CodeActivity
    {
        //[Input("SomeInput")]
        //[RequiredArgument]
        //public InArgument<bool> MyInputString { get; set; }

        //[Output("SomeOutput")]
        //public OutArgument<> MyOutputArgument { get; set; }

        [Input("Date of Birth for Record")]
        [RequiredArgument]
        [ReferenceTarget("new_letter")]
        public InArgument<EntityReference> Letter { get; set; }

        protected override void Execute(CodeActivityContext context)
        {
            try
            {
                IWorkflowContext workflowContext = context.GetExtension<IWorkflowContext>();

                IOrganizationServiceFactory serviceFactory =
                    context.GetExtension<IOrganizationServiceFactory>();

                IOrganizationService service =
                    serviceFactory.CreateOrganizationService(workflowContext.InitiatingUserId);

                Guid letterId = this.Letter.Get(context).Id;
                Entity letterEntity = service.Retrieve("new_letter", letterId, new ColumnSet("new_date_of_birth"));

                DateTime? dateOfBirth; //1/1/0001
                if (letterEntity.Contains("new_date_of_birth"))
                {
                    dateOfBirth = (DateTime?)letterEntity["new_date_of_birth"];
                    if (dateOfBirth.Value.Year > DateTime.Now.Year)
                    {
                        throw new Exception("Date of Birth can not have year greater than current year.");
                    }
                    else if (dateOfBirth.Value.Year == DateTime.Now.Year)
                    {
                        if (dateOfBirth.Value.Month > DateTime.Now.Month)
                        {
                            throw new Exception("Date of Birth can not have mount greater than curent mounth & year");
                        }
                        else if (dateOfBirth.Value.Month == DateTime.Now.Month)
                        {
                            if (dateOfBirth.Value.Day > DateTime.Now.Day)
                            {
                                throw new Exception("Date of Birth can not be greater than curent day.");
                            }
                        }
                    }
                }
                else
                {
                    dateOfBirth = null;
                }
                if (dateOfBirth == null)
                {
                    return;
                }

                DateTime nextDateOfBirth = GetNextDateOfBirth(dateOfBirth.Value);
                Entity updateLetter = new Entity("new_letter");
                updateLetter.Id = letterId;
                updateLetter["new_next_date_of_birth"] = nextDateOfBirth;
                service.Update(updateLetter);
            }
            catch (Exception ex)
            {

                throw new InvalidPluginExecutionException(ex.Message);
            }
        }

        private DateTime GetNextDateOfBirth(DateTime dateofbirth)
        {
            DateTime nextDOB = new DateTime(dateofbirth.Year, dateofbirth.Month, dateofbirth.Day);
            if (nextDOB.Month == 2 && nextDOB.Day == 29)
            {
                //Todo
                if (!DateTime.IsLeapYear(DateTime.Now.Year)) //Not leap year
                {
                    if (nextDOB.Month < DateTime.Now.Month)
                    {
                        if (DateTime.IsLeapYear(DateTime.Now.Year + 1))
                        {
                            nextDOB = new DateTime(DateTime.Now.Year + 1, nextDOB.Month, nextDOB.Day);
                        }
                        else
                        {
                            nextDOB = new DateTime(DateTime.Now.Year + 1, nextDOB.Month, nextDOB.Day - 1);
                        }
                    }
                    else
                    {
                        nextDOB = new DateTime(DateTime.Now.Year, nextDOB.Month, nextDOB.Day - 1);
                    }
                }
                else //leap year
                {
                    if (nextDOB.Month < DateTime.Now.Month)
                    {
                        nextDOB = new DateTime(DateTime.Now.Year + 1, nextDOB.Month, nextDOB.Day - 1);
                    }
                    else
                    {
                        nextDOB = new DateTime(DateTime.Now.Year, nextDOB.Month, nextDOB.Day);
                    }
                }
            }
            else
            {
                if (nextDOB.Month < DateTime.Now.Month)
                {
                    nextDOB = new DateTime(DateTime.Now.Year + 1, nextDOB.Month, nextDOB.Day);
                }
                else if (nextDOB.Month == DateTime.Now.Month)
                {
                    if (nextDOB.Day < DateTime.Now.Day)
                    {
                        nextDOB = new DateTime(DateTime.Now.Year + 1, nextDOB.Month, nextDOB.Day);
                    }
                    else
                    {
                        nextDOB = new DateTime(DateTime.Now.Year, nextDOB.Month, nextDOB.Day);
                    }
                }
                else
                {
                    nextDOB = new DateTime(DateTime.Now.Year, nextDOB.Month, nextDOB.Day);
                }
            }

            return nextDOB;
        }
    }
}
