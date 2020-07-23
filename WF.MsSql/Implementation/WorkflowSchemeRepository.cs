using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WF.Business.DataAccess;


namespace WF.MsSql.Implementation
{
    public class WorkflowSchemeRepository : IWorkflowSchemeRepository
    {
        private readonly SampleContext _sampleContext;

        public WorkflowSchemeRepository(SampleContext sampleContext)
        {
            _sampleContext = sampleContext;
        }

        public List<Business.Model.WorkflowScheme> Get(out int count, int page = 0, int pageSize = 128)
        {
            int actual = page * pageSize;
            var query = _sampleContext.WorkflowSchemes.OrderByDescending(c => c.Code);
            count = query.Count();
            return query.Skip(actual)
                        .Take(pageSize)
                        .ToList()
                        .Select(d => Mappings.Mapper.Map<Business.Model.WorkflowScheme>(d)).ToList();
        }
    }
}
