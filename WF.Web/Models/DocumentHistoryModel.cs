using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WF.Business.Model;

namespace WF.Web.Models
{
    public class DocumentHistoryModel
    {
        public List<DocumentTransitionHistory> Items { get; set; }
    }
}
