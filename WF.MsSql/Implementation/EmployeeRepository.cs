using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WF.Business.DataAccess;
using WF.Business.Model;

namespace WF.MsSql.Implementation
{
    public class EmployeeRepository : IEmployeeRepository
    {
        private readonly SampleContext _sampleContext;
        private readonly MICHRContext _michrContext;

        public EmployeeRepository(SampleContext sampleContext, MICHRContext michrContext)
        {
            _sampleContext = sampleContext;
            _michrContext = michrContext;
        }

        public bool CheckRole(Guid employeeId, string roleName)
        {
            return _sampleContext.EmployeeRoles.Any(r => r.EmployeeId == employeeId && r.Role.Name == roleName);
        }

        public List<Business.Model.Employee> GetAll()
        {

            return _sampleContext.Employees
                                 .Include(x => x.StructDivision)
                                 .Include(x => x.EmployeeRoles)
                                 .ThenInclude(x => x.Role)
                                 .ToList().Select(e => Mappings.Mapper.Map<Business.Model.Employee>(e))
                                 .OrderBy(c => c.Name).ToList();
        }

        public bool CheckUser(string userName)
        {
            return _michrContext.aspnet_Users.Any(r => r.UserName == userName);
        }

        public IEnumerable<string> GetInRole(string roleName)
        {
            return
                  _sampleContext.EmployeeRoles.Where(r => r.Role.Name == roleName).ToList()
                      .Select(r => r.EmployeeId.ToString()).ToList();
        }

        public string GetNameById(Guid id)
        {
            string res = "Unknown";

            var item = _sampleContext.Employees.Find(id);
            if (item != null)
                res = item.Name;

            return res;
        }

        public List<Business.Model.aspnet_Users> GetAllUser()
        {
            try
            {
                return _michrContext.aspnet_Users.ToList()
                                    .Select(e => Mappings.Mapper.Map<Business.Model.aspnet_Users>(e))
                                    .OrderBy(x => x.UserName).ToList();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public List<Business.Model.UserInfo> GetAllUserById(List<string> dataId)
        {
            var data = (from u in _michrContext.aspnet_Memberships
                        join us in _michrContext.aspnet_Users
                        on u.UserId equals us.UserId
                        join ru in _michrContext.aspnet_UsersInRoles on u.UserId equals ru.UserId
                        into ruFirst
                        from ru in ruFirst.DefaultIfEmpty()
                        join r in _michrContext.aspnet_Roles on ru.RoleId equals r.RoleId
                        into rFirst
                        from r in rFirst.DefaultIfEmpty()
                        where dataId.Contains(u.UserId.ToString()) && u.IsLockedOut == false
                        orderby us.UserName
                        select new Business.Model.UserInfo()
                        {
                            UserId = u.UserId,
                            FullName = u.FullName,
                            OrgName = "",
                            RoleName = r.RoleName,
                            UserName = ""
                        }).ToList();
            List<UserInfo> dataReturn = new List<UserInfo>();
            foreach (var item in data)
            {
                if (!dataReturn.Any(x => x.UserId == item.UserId))
                    dataReturn.Add(item);
            }
            return dataReturn;
        }
    }
}
