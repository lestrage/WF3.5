using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using WF.Business.Model;

namespace WF.Web.Models
{
    public class SettingsModel
    {
        public string WFSchema { get; set; }

        public List<Employee> Employees { get; set; }
        public List<Role> Roles { get; set; }
        public List<StructDivision> StructDivision { get; set; }
        public string SchemeName { get; set; }
    }
}