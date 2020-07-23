namespace WF.MsSql
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;


    [Table("WorkflowProcessTimer")]
    public partial class WorkflowProcessTimer
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        public Guid ProcessId { get; set; }
        [Required]
        public string Name { get; set; }
        [Required]
        public DateTime NextExecutionDateTime { get; set; }
        [Required]
        public bool Ignore { get; set; }
    }
}
