using Microsoft.Xrm.Sdk;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Customization_XRM
{
    public class Accounts
    {
        public string Business_Phone { get; set; }
        public string Telephone1 { get; set; }
        public string Telephone2 { get; set; }
        public string Telephone3 { get; set; }
        public string Telephone4 { get; set; }
        public string Telephone5 { get; set; }
        public string Telephone6 { get; set; }
        public string Telephone7 { get; set; }
        public string Telephone8 { get; set; }
        public string Full_Name { get; set; }
        public string Company_Name { get; set; }
        public string BusinessDivisionName { get; set; }
        public Guid Id { get; set; }
        public OptionSetValue State_Code { get; set; }
        public OptionSetValue Status_Code { get; set; }
    }
}
