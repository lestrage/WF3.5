namespace WF.MsSql
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;


    [Table("WorkflowProcessInstancePersistence")]
    public partial class WorkflowProcessInstancePersistence
    {
        [Key]
        public Guid Id { get; set; }
        [Required]
        public Guid ProcessId { get; set; }
        [Required]
        public string ParameterName { get; set; }
        [Required]
        public string Value { get; set; }
    }
}
