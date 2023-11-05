using Microsoft.Xrm.Sdk;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

//namespaces for d365 interaction
using Microsoft.Xrm.Sdk.Query;
using CrmEarlyBound;
using Customization_XRM;

namespace Customization_XRM
{
    public class LeadPhone
    {
        public string Business_Phone { get; set; }
        public string Mobile_Phone { get; set; }
        public string Full_Name { get; set; }
        public string Company_Name { get; set; }
        public string BusinessDivisionName { get; set; }
        public Guid Id { get; set; }
        public OptionSetValue State_Code { get; set; }
        public OptionSetValue Status_Code { get; set; }
    }
}
