namespace WF.MsSql
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;


    [Table("WorkflowProcessScheme")]
    public partial class WorkflowProcessScheme
    {
        [Key]
        public Guid Id { get; set; }
        [Required]
        public string Scheme { get; set; }
        [Required]
        public string DefiningParameters { get; set; }
        [Required]
        public string DefiningParametersHash { get; set; }
        [Required]
        public string SchemeCode{get;set;}
        [Required]
        public bool IsObsolete{get;set;}
        public string RootSchemeCode{get;set;}
        public Guid? RootSchemeId{get;set;}
        public string AllowedActivities{get;set;}
        public string StartingTransition{get;set;}
    }
}
