using System;
using System.Collections.Generic;
using System.Linq;
using OptimaJet.Workflow.Core.Runtime;
using OptimaJet.Workflow.Core.Model;
using WF.Business.DataAccess;

namespace WF.Business.Workflow
{
    public class RuleProvider : IWorkflowRuleProvider
    {
        private class RuleFunction
        {
            public Func<ProcessInstance, WorkflowRuntime, string, IEnumerable<string>> GetFunction { get; set; }

            public Func<ProcessInstance, WorkflowRuntime, string, string, bool> CheckFunction { get; set; }
        }

        private readonly Dictionary<string, RuleFunction> _rules = new Dictionary<string, RuleFunction>();

        private readonly IDataServiceProvider _dataServiceProvider;

        public RuleProvider(IDataServiceProvider dataServiceProvider)
        {
            _dataServiceProvider = dataServiceProvider;
            //Register your rules in the _rules Dictionary
            _rules.Add("IsDocumentAuthor", new RuleFunction() { CheckFunction = IsDocumentAuthor, GetFunction = GetDocumentAuthor });
            _rules.Add("CheckRole", new RuleFunction { CheckFunction = CheckRole, GetFunction = GetRole });
            _rules.Add("CheckUser", new RuleFunction { CheckFunction = ChecUser, GetFunction = GetUser });
        }

        private IEnumerable<string> GetDocumentAuthor(ProcessInstance processInstance, WorkflowRuntime runtime, string parameter)
        {
            var _documentServiceProvider = _dataServiceProvider.Get<IDocumentRepository>();
            return _documentServiceProvider.GetDocumentAuthor(processInstance.ProcessId);
        }

        private bool IsDocumentAuthor(ProcessInstance processInstance, WorkflowRuntime runtime, string identityId, string parameter)
        {
            var _documentServiceProvider = _dataServiceProvider.Get<IDocumentRepository>();
            return _documentServiceProvider.CheckDocumentAuthor(processInstance.ProcessId, new Guid(identityId));
        }


        public IEnumerable<string> GetRole(ProcessInstance processInstance, WorkflowRuntime runtime, string parameter)
        {
            var _roleServiceProvider = _dataServiceProvider.Get<IRoleRepository>();
            return _roleServiceProvider.GetAllUserByRole(parameter).Select(x => x.UserId.ToString()).ToList();
        }

        public bool CheckRole(ProcessInstance processInstance, WorkflowRuntime runtime, string identityId,
            string parameter)
        {
            var _roleServiceProvider = _dataServiceProvider.Get<IRoleRepository>();
            return _roleServiceProvider.CheckRole(parameter, new Guid(identityId));
        }

        public IEnumerable<string> GetUser(ProcessInstance processInstance, WorkflowRuntime runtime, string parameter)
        {
            var _employeeServiceProvider = _dataServiceProvider.Get<IEmployeeRepository>();
            return _employeeServiceProvider.GetAllUser().Select(x => x.UserName).ToList();
        }

        public bool ChecUser(ProcessInstance processInstance, WorkflowRuntime runtime, string identityId,
            string parameter)
        {
            var _employeeServiceProvider = _dataServiceProvider.Get<IEmployeeRepository>();
            return _employeeServiceProvider.CheckUser(parameter);
        }

        #region Implementation of IWorkflowRuleProvider

        public List<string> GetRules()
        {
            return _rules.Keys.ToList();
        }

        public bool Check(ProcessInstance processInstance, WorkflowRuntime runtime, string identityId, string ruleName,
            string parameter)
        {
            if (_rules.ContainsKey(ruleName))
                return _rules[ruleName].CheckFunction(processInstance, runtime, identityId, parameter);
            throw new NotImplementedException();
        }

        public IEnumerable<string> GetIdentities(ProcessInstance processInstance, WorkflowRuntime runtime,
            string ruleName, string parameter)
        {
            if (_rules.ContainsKey(ruleName))
                return _rules[ruleName].GetFunction(processInstance, runtime, parameter);
            throw new NotImplementedException();
        }

        #endregion
    }
}
