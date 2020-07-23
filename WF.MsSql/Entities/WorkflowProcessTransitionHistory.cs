namespace WF.MsSql
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
 

    [Table("WorkflowProcessTransitionHistory")]
    public partial class WorkflowProcessTransitionHistory
    {
        [Key]
        public Guid Id { get; set; }
        [Required]
        public Guid ProcessId { get; set; }
        public string ExecutorIdentityId { get; set; }
        public string ActorIdentityId { get; set; }
        [Required]
        public string FromActivityName { get; set; }
        [Required]
        public string ToActivityName { get; set; }
        public string ToStateName { get; set; }
        [Required]
        public DateTime TransitionTime { get; set; }
        [Required]
        public string TransitionClassifier { get; set; }
        [Required]
        public bool IsFinalised { get; set; }
        public string FromStateName { get; set; }
        public string TriggerName { get; set; }
    }
}
