namespace WF.MsSql
{
    using System;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;


    [Table("aspnet_Roles")]
    public partial class aspnet_Roles
    {
        [Key]
        public Guid RoleId { get; set; }

        [Required]
        [StringLength(256)]
        public string RoleName { get; set; }

        [Required]
        [StringLength(256)]
        public string LoweredRoleName { get; set; }

        [StringLength(256)]
        public string Description { get; set; }

        [Required]
        public Guid CreatedByUserID { get; set; }

        [Required]
        public DateTime CreatedOnDate { get; set; }

        [Required]
        public Guid LastModifiedByUserID { get; set; }

        [Required]
        public DateTime LastModifiedOnDate { get; set; }
        
        [Required]
        public int TrangThaiSuDung { get; set; }
        public Guid? ParentRoleId { get; set; }
    }
}
