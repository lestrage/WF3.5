using System;

namespace WF.Business.Model
{
    public class DM_WorkflowCommand
    {
        public Guid CommandId { get; set; }
        public string CommandCode { get; set; }
        public string CommandName { get; set; }
        public int Order { get; set;}
    }
}
