using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WF.Business;
using WF.Business.DataAccess;
using WF.Business.Model;

namespace WF.MsSql.Implementation
{
    public class CommandStateRepository : ICommandStateRepository
    {
        private readonly MICHRContext _michrContext;

        public CommandStateRepository(MICHRContext michrContext)
        {
            _michrContext = michrContext;
        }

        public List<Business.Model.DM_WorkflowCommand> GetAllCommand()
        {
            try
            {
                var data = _michrContext.DM_WorkflowCommands.Select(e => Mappings.Mapper.Map<Business.Model.DM_WorkflowCommand>(e)).ToList();
                return data;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public List<Business.Model.DM_WorkflowState> GetAllState()
        {
            try
            {
                var data = _michrContext.DM_WorkflowStates.Select(e => Mappings.Mapper.Map<Business.Model.DM_WorkflowState>(e)).ToList();
                return data;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}
