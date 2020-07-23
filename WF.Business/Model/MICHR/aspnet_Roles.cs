using System;

namespace WF.Business.Model
{
    public class aspnet_Roles
    {
        public Guid RoleId { get; set; }
        public string RoleName { get; set; }
        public string LoweredRoleName { get; set; }
        public string Description { get; set; }
        public Guid CreatedByUserID { get; set; }
        public DateTime CreatedOnDate { get; set; }
        public Guid LastModifiedByUserID { get; set; }
        public DateTime LastModifiedOnDate { get; set; }
        public int TrangThaiSuDung { get; set; }
        public Guid? ParentRoleId { get; set; }
    }
}
