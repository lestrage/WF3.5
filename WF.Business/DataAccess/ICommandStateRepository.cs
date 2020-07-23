using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WF.Business.DataAccess
{
    public interface ICommandStateRepository
    {
        List<Model.DM_WorkflowCommand> GetAllCommand();
        List<Model.DM_WorkflowState> GetAllState();
    }
}
