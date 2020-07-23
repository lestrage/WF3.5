namespace WF.MsSql
{
    using System;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;


    [Table("Ext_DocumentHitory_FileAttach")]
    public partial class Ext_DocumentHitory_FileAttach
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        public Guid DocumentHistoryId { get; set; }

        [Required]
        public string FileName { get; set; }

        [Required]
        public string FilePath { get; set; }
    }
}
