namespace WF.MsSql
{
    using System;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    [Table("DM_WorkflowCommand")]
    public partial class DM_WorkflowCommand
    {
        [Key]
        [Column("CommandID")]
        public Guid CommandID { get; set; }
        [StringLength(256)]
        [Column("CommandCode")]
        public string CommandCode { get; set; }
        [StringLength(1024)]
        [Column("CommandName")]
        public string CommandName { get; set; }
    }
}
