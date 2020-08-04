using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WF.Business.DataAccess
{
    public interface IWorkflowSchemeRepository
    {
        List<Model.WorkflowScheme> Get(out int count, int page = 0, int pageSize = 10, string textSearch = "");
    }
}
