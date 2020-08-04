using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using WF.Business;

namespace WF.Web.Models
{
    public class WorkflowListModel
    {
        public int Count { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
        public int TotalPage { get; set; }
        public List<WorkflowSchemeDetails> WorkflowSchemeData { get; set; }
    }

    public class WorkflowSchemeDetails
    {
        public int STT { get; set; }
        public string WorkflowCode { get; set; }
    }
}