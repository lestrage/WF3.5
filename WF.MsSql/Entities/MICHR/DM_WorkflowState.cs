namespace WF.MsSql
{
    using System;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    [Table("DM_WorkflowState")]
    public partial class DM_WorkflowState
    {
        [Key]
        [Column("StateID")]
        public Guid StateId { get; set; }
        [StringLength(256)]
        [Column("StateCode")]
        public string StateCode { get; set; }
        [StringLength(1024)]
        [Column("StateName")]
        public string StateName { get; set; }
    }
}
