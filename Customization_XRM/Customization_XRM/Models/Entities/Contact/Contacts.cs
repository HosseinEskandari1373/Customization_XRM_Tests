using Microsoft.Xrm.Sdk;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Customization_XRM
{
    public class Contacts
    {
        public string Business_Phone { get; set; }
        public string Mobile_Phone { get; set; }
        public string Phone1 { get; set; }
        public string Phone2 { get; set; }
        public string Phone3 { get; set; }
        public string Phone4 { get; set; }
        public string Phone5 { get; set; }
        public string Phone6 { get; set; }
        public string Phone7 { get; set; }
        public string Phone8 { get; set; }
        public string Full_Name { get; set; }
        public string Company_Name { get; set; }
        public string BusinessDivisionName { get; set; }
        public Guid Id { get; set; }
        public OptionSetValue State_Code { get; set; }
        public OptionSetValue Status_Code { get; set; }
    }
}
