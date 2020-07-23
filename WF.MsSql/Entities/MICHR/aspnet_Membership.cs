namespace WF.MsSql
{
    using System;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;


    [Table("aspnet_Membership")]
    public partial class aspnet_Membership
    {
        [Key]
        public Guid UserId { get; set; }

        [Required]
        [StringLength(128)]
        public string Password { get; set; }

        [Required]
        public int PasswordFormat { get; set; }

        [Required]
        [StringLength(128)]
        public string PasswordSalt { get; set; }

        [StringLength(16)]
        public string MobilePIN { get; set; }

        [StringLength(256)]
        public string Email { get; set; }

        [StringLength(256)]
        public string LoweredEmail { get; set; }

        [StringLength(256)]
        public string PasswordQuestion { get; set; }

        [StringLength(128)]
        public string PasswordAnswer { get; set; }

        [Required]
        public bool IsApproved { get; set; }

        [Required]
        public bool IsLockedOut { get; set; }

        [Required]
        public DateTime CreateDate { get; set; }

        [Required]
        public DateTime LastLoginDate { get; set; }

        [Required]
        public DateTime LastPasswordChangedDate { get; set; }

        [Required]
        public DateTime LastLockoutDate { get; set; }
        
        [Required]
        public int FailedPasswordAttemptCount { get; set; }

        [Required]
        public DateTime FailedPasswordAttemptWindowStart { get; set; }

        [Required]
        public int FailedPasswordAnswerAttemptCount { get; set; }

        [Required]
        public DateTime FailedPasswordAnswerAttemptWindowStart { get; set; }

        public string Comment { get; set; }

        public DateTime? DeleteDate { get; set; }

        [Required]
        public Guid CreatedByUserID { get; set; }

        //[Required]
        //public DateTime CreatedOnDate { get; set; }

        [Required]
        public DateTime LastModifiedOnDate { get; set; }

        [StringLength(512)]
        public string FullName { get; set; }
    }
}
