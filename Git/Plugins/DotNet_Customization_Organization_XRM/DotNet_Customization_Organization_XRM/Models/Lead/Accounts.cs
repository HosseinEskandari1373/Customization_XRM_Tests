using Microsoft.Xrm.Sdk;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DotNet_Customization_Organization_XRM
{
    public class Accounts
    {
        public string Business_Phone { get; set; }
        public string Full_Name { get; set; }
        public string Company_Name { get; set; }
        public string BusinessDivisionName { get; set; }
        public Guid Id { get; set; }
        public OptionSetValue State_Code { get; set; }
        public OptionSetValue Status_Code { get; set; }
    }
}
