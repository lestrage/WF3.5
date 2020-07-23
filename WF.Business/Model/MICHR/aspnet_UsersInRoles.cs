using System;

namespace WF.Business.Model
{
    public partial class aspnet_UsersInRoles
    {
        public Guid RoleId { get; set; }
        public Guid UserId { get; set; }
        public Guid ApplicationId { get; set; }        
        public DateTime CreateDate { get; set; }        
        public DateTime DeleteDate { get; set; }
    }
}
