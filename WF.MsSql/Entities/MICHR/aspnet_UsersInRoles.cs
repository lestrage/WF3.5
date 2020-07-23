namespace WF.MsSql
{
    using System;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;


    [Table("aspnet_UsersInRoles")]
    public partial class aspnet_UsersInRoles
    {   
        public Guid RoleId { get; set; }
        public Guid UserId { get; set; }
        public Guid ApplicationId { get; set; }
        
        public DateTime CreateDate { get; set; }
        
        public DateTime DeleteDate { get; set; }
    }
}
