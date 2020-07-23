namespace WF.MsSql
{
    using System;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;


    [Table("aspnet_Users")]
    public partial class aspnet_Users
    {
        [Key]
        public Guid UserId { get; set; }

        [Required]
        [StringLength(256)]
        public string UserName { get; set; }

        [Required]
        [StringLength(256)]
        public string LoweredUserName { get; set; }

        [StringLength(16)]
        public string MobileAlias { get; set; }

        [Required]
        public bool IsAnonymous { get; set; }

        [Required]
        public DateTime LastActivityDate { get; set; }
    }
}
