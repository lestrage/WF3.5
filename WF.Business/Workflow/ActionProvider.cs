using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using OptimaJet.Workflow.Core.Model;
using OptimaJet.Workflow.Core.Runtime;
using WF.Business.DataAccess;

namespace WF.Business.Workflow
{
    public class ActionProvider : IWorkflowActionProvider
    {
        private readonly Dictionary<string, Action<ProcessInstance, WorkflowRuntime, string>> _actions = new Dictionary<string, Action<ProcessInstance, WorkflowRuntime, string>>();

        private readonly Dictionary<string, Func<ProcessInstance, WorkflowRuntime, string, CancellationToken, Task>> _asyncActions =
            new Dictionary<string, Func<ProcessInstance, WorkflowRuntime, string, CancellationToken, Task>>();

        private readonly Dictionary<string, Func<ProcessInstance, WorkflowRuntime, string, bool>> _conditions =
            new Dictionary<string, Func<ProcessInstance, WorkflowRuntime, string, bool>>();

        private readonly Dictionary<string, Func<ProcessInstance, WorkflowRuntime, string, CancellationToken, Task<bool>>> _asyncConditions =
            new Dictionary<string, Func<ProcessInstance, WorkflowRuntime, string, CancellationToken, Task<bool>>>();

        private readonly IDataServiceProvider _dataServiceProvider;

        public ActionProvider(IDataServiceProvider dataServiceProvider)
        {
            _dataServiceProvider = dataServiceProvider;
            //Register your actions in _actions and _asyncActions dictionaries
            _actions.Add("UpdateTransitionHistory", UpdateTransitionHistory); //sync
            _actions.Add("WriteTransitionHistory", WriteTransitionHistory); //sync
            // _actions.Add("CalculateEndTime", CalculateEndTime); //sync
            // _actions.Add("PushNotificationForAll", PushNotificationForAll); //sync
            //_asyncActions.Add("MyAsyncAction", MyAsyncAction); //async

            //Register your conditions in _conditions and _asyncConditions dictionaries
            //            _conditions.Add("MyCondition", MyCondition); //sync
            //            _asyncConditions.Add("MyAsyncCondition", MyAsyncCondition); //async
        }

        private void UpdateTransitionHistory(ProcessInstance processInstance, WorkflowRuntime runtime, string actionParameter)
        {
            string nextState = runtime.GetLocalizedStateName(processInstance.ProcessId, processInstance.ExecutedActivityState);
            Guid? identity = null;
            if (processInstance.IdentityId != null)
                identity = new Guid(processInstance.IdentityId);
            var _documentRepository = _dataServiceProvider.Get<IDocumentRepository>();
            _documentRepository.UpdateTransitionHistory(processInstance.ProcessId, processInstance.CurrentState, nextState, processInstance.CurrentCommand, identity);            
        }

        private void WriteTransitionHistory(ProcessInstance processInstance, WorkflowRuntime runtime, string actionParameter)
        {
            string nextState = runtime.GetLocalizedStateName(processInstance.ProcessId, processInstance.ExecutedActivityState);
            var _documentRepository = _dataServiceProvider.Get<IDocumentRepository>();
            _documentRepository.WriteTransitionHistory(processInstance.ProcessId, processInstance.CurrentState, nextState, processInstance.CurrentCommand, processInstance.IdentityIds);
        }

        private void CalculateEndTime(ProcessInstance processInstance, WorkflowRuntime runtime, string actionParameter)
        {
            var _documentRepository = _dataServiceProvider.Get<IDocumentRepository>();
            string nextState = runtime.GetLocalizedStateName(processInstance.ProcessId, processInstance.ExecutedActivityState);
            _documentRepository.CalculateEndTime(processInstance.ProcessId, nextState, actionParameter);
        }

        private void PushNotificationForAll(ProcessInstance processInstance, WorkflowRuntime runtime, string actionParameter)
        {
            var _documentRepository = _dataServiceProvider.Get<IDocumentRepository>();
            string nextState = runtime.GetLocalizedStateName(processInstance.ProcessId, processInstance.ExecutedActivityState);
            _documentRepository.PushNotificationForAll(processInstance.ProcessId, processInstance.ImpersonatedIdentityId, actionParameter);
        }

        private async Task MyAsyncAction(ProcessInstance processInstance, WorkflowRuntime runtime, string actionParameter, CancellationToken token)
        {
            //Execute your asynchronous code here. You can use await in your code.
        }

        private bool MyCondition(ProcessInstance processInstance, WorkflowRuntime runtime, string actionParameter)
        {
            //Execute your code here
            return false;
        }

        private async Task<bool> MyAsyncCondition(ProcessInstance processInstance, WorkflowRuntime runtime, string actionParameter, CancellationToken token)
        {
            //Execute your asynchronous code here. You can use await in your code.
            return false;
        }

        #region Implementation of IWorkflowActionProvider

        public void ExecuteAction(string name, ProcessInstance processInstance, WorkflowRuntime runtime,
            string actionParameter)
        {
            if (_actions.ContainsKey(name))
                _actions[name].Invoke(processInstance, runtime, actionParameter);
            else
                throw new NotImplementedException($"Action with name {name} isn't implemented");
        }

        public async Task ExecuteActionAsync(string name, ProcessInstance processInstance, WorkflowRuntime runtime, string actionParameter, CancellationToken token)
        {
            //token.ThrowIfCancellationRequested(); // You can use the transferred token at your discretion
            if (_asyncActions.ContainsKey(name))
                await _asyncActions[name].Invoke(processInstance, runtime, actionParameter, token);
            else
                throw new NotImplementedException($"Async Action with name {name} isn't implemented");
        }

        public bool ExecuteCondition(string name, ProcessInstance processInstance, WorkflowRuntime runtime,
            string actionParameter)
        {
            if (_conditions.ContainsKey(name))
                return _conditions[name].Invoke(processInstance, runtime, actionParameter);

            throw new NotImplementedException($"Condition with name {name} isn't implemented");
        }

        public async Task<bool> ExecuteConditionAsync(string name, ProcessInstance processInstance, WorkflowRuntime runtime, string actionParameter, CancellationToken token)
        {
            //token.ThrowIfCancellationRequested(); // You can use the transferred token at your discretion
            if (_asyncConditions.ContainsKey(name))
                return await _asyncConditions[name].Invoke(processInstance, runtime, actionParameter, token);

            throw new NotImplementedException($"Async Condition with name {name} isn't implemented");
        }

        public bool IsActionAsync(string name)
        {
            return _asyncActions.ContainsKey(name);
        }

        public bool IsConditionAsync(string name)
        {
            return _asyncConditions.ContainsKey(name);
        }

        public List<string> GetActions()
        {
            return _actions.Keys.Union(_asyncActions.Keys).ToList();
        }

        public List<string> GetConditions()
        {
            return _conditions.Keys.Union(_asyncConditions.Keys).ToList();
        }

        #endregion
    }
}

