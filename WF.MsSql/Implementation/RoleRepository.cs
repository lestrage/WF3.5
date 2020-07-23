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
    public class RoleRepository : IRoleRepository
    {
        private SampleContext _sampleContext;
        private MICHRContext _michrContext;

        public RoleRepository(SampleContext sampleContext, MICHRContext michrContext)
        {
            _sampleContext = sampleContext;
            _michrContext = michrContext;
        }

        public bool CheckRole(string name)
        {
            try
            {
                var r = _michrContext.aspnet_Roles.FirstOrDefault(x => x.RoleName == name);
                if (r != null)
                    return true;
                else
                    return false;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public bool CheckRole(string name, Guid userId)
        {
            try
            {
                var r = (from rol in _michrContext.aspnet_Roles
                         join rolu in _michrContext.aspnet_UsersInRoles
                         on rol.RoleId equals rolu.RoleId
                         where rol.RoleName == name 
                         && rolu.UserId == userId
                         select rol).FirstOrDefault();
                if (r != null)
                    return true;
                else
                    return false;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public List<Business.Model.Role> GetAll()
        {
            try
            {
                var data = (from r in _sampleContext.Roles
                            orderby r.Name
                            select new Business.Model.Role()
                            {
                                Id = r.Id,
                                Name = r.Name
                            }).ToList();
                return data;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public List<Business.Model.Employee> GetAllEmployeeByRole(string roleName)
        {
            try
            {
                var data = (from r in _sampleContext.Roles
                            join re in _sampleContext.EmployeeRoles
                            on r.Id equals re.RoleId
                            join e in _sampleContext.Employees
                            on re.EmployeeId equals e.Id
                            where r.Name == roleName
                            orderby e.Name
                            select new Business.Model.Employee()
                            {
                                Id = r.Id,
                                Name = r.Name
                            }).ToList();
                return data;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public List<Business.Model.aspnet_Roles> GetAllRole()
        {
            try
            {
                var data = (from r in _michrContext.aspnet_Roles
                            orderby r.RoleName
                            select new Business.Model.aspnet_Roles()
                            {
                                RoleId = r.RoleId,
                                RoleName = r.RoleName
                            }).ToList();
                return data;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public List<Business.Model.aspnet_Users> GetAllUserByRole(string roleName)
        {
            try
            {
                var data = (from r in _michrContext.aspnet_Roles
                            join ru in _michrContext.aspnet_UsersInRoles
                            on r.RoleId equals ru.RoleId
                            join u in _michrContext.aspnet_Users
                            on ru.UserId equals u.UserId
                            join m in _michrContext.aspnet_Memberships
                            on u.UserId equals m.UserId
                            where r.RoleName == roleName && m.IsLockedOut == false
                            orderby u.UserName
                            select new Business.Model.aspnet_Users()
                            {
                                UserId = u.UserId,
                                UserName = u.UserName
                            }).ToList();
                return data;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}
