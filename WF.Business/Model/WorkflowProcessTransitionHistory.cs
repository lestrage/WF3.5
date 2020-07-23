using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WF.Business.Model
{ 
    public partial class WorkflowProcessTransitionHistory
    {
        public Guid Id { get; set; }
        public Guid ProcessId { get; set; }
        public string ExecutorIdentityId { get; set; }
        public string ActorIdentityId { get; set; }
        public string FromActivityName { get; set; }
        public string ToActivityName { get; set; }
        public string ToStateName { get; set; }
        public DateTime TransitionTime { get; set; }
        public string TransitionClassifier { get; set; }
        public bool IsFinalised { get; set; }
        public string FromStateName { get; set; }
        public string TriggerName { get; set; }
    }
}
