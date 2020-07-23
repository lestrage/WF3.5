using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WF.Business.DataAccess
{
    public interface IEmployeeRepository
    {
        List<Model.Employee> GetAll();
        List<Model.aspnet_Users> GetAllUser();
        bool CheckUser(string userName);
        string GetNameById(Guid id);
        IEnumerable<string> GetInRole(string roleName);
        bool CheckRole(Guid employeeId, string roleName);
        List<Model.UserInfo> GetAllUserById(List<string> dataId);
    }
}
