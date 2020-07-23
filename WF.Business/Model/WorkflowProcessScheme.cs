using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WF.Business.Model
{
    public partial class WorkflowProcessScheme
    {
        public Guid Id { get; set; }
        public string Scheme { get; set; }
        public string DefiningParameters { get; set; }
        public string DefiningParametersHash { get; set; }
        public string SchemeCode{get;set;}
        public bool IsObsolete{get;set;}
        public string RootSchemeCode{get;set;}
        public Guid? RootSchemeId{get;set;}
        public string AllowedActivities{get;set;}
        public string StartingTransition{get;set;}
    }
}
