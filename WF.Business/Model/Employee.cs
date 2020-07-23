using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WF.Business.Model
{
    public class Employee
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public Guid StructDivisionId { get; set; }
        public StructDivision StructDivision { get; set; }
        public bool IsHead { get; set; }

        public List<EmployeeRole> EmployeeRoles { get; set; }
    }

    public class UserInfo
    {
        public Guid UserId { get; set; }
        public string UserName { get; set; }
        public string FullName { get; set; }
        public string RoleName { get; set; }
        public string OrgName { get; set; }
    }
}
