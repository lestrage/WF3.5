using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WF.Business.DataAccess
{
    public interface IRoleRepository
    {
        List<Model.Role> GetAll();
        List<Model.aspnet_Roles> GetAllRole();
        List<Model.Employee> GetAllEmployeeByRole(string roleName);
        List<Model.aspnet_Users> GetAllUserByRole(string roleName);
        bool CheckRole(string name);
        bool CheckRole(string name, Guid userId);
    }
}
