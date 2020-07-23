using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WF.Business.Model
{
    public class WorkflowProcessInstancePersistence
    {
        public Guid Id { get; set; }
        public Guid ProcessId { get; set; }
        public string ParameterName { get; set; }
        public string Value { get; set; }
    }
}
