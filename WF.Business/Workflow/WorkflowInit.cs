using System;
using System.Reflection;
using System.Xml.Linq;
using OptimaJet.Workflow.Core.Builder;
using OptimaJet.Workflow.Core.Bus;
using OptimaJet.Workflow.Core.Parser;
using OptimaJet.Workflow.Core.Runtime;
using OptimaJet.Workflow.Core.Persistence;
using WF.Business.DataAccess;
using System.Threading.Tasks;

namespace WF.Business.Workflow
{
    public static class WorkflowInit
    {
        private static volatile WorkflowRuntime _runtime;

        private static readonly object _sync = new object();

        public static IDataServiceProvider DataServiceProvider { get; private set; }

        public static WorkflowRuntime Create(IDataServiceProvider dataServiceProvider)
        {
            DataServiceProvider = dataServiceProvider;
            WorkflowRuntime.RegisterLicense("Nexdata-TmV4ZGF0YTowMS4yOS4yMDIwOmV5Sk5ZWGhPZFcxaVpYSlBaa0ZqZEdsMmFYUnBaWE1pT2kweExDSk5ZWGhPZFcxaVpYSlBabFJ5WVc1emFYUnBiMjV6SWpvdE1Td2lUV0Y0VG5WdFltVnlUMlpUWTJobGJXVnpJam90TVN3aVRXRjRUblZ0WW1WeVQyWlVhSEpsWVdSeklqb3RNU3dpVFdGNFRuVnRZbVZ5VDJaRGIyMXRZVzVrY3lJNkxURjk6bG5BY0VvYnpTMmlzYzRBeDllRGtkVnNBbzJqdUduNU81aWYrQmZhNE1NeDlkNUM4U1MyTmM5RnFYaGpRdVlYQ0Y1ZUtuZmp6anA1cTgvQ3M0Qk5vWC9ZQlc3NTh3ZzNWa1o0cy9nQ240aGpPL3J5eEsreENEcnZPQXFsVUwvY2s3QzhRWSthSFNnbnhFY2IrQjhManNtMnBqRXNiV2gzdmdFVHpmZ1k0WHdvPQ==");
            CreateRuntime();
            return Runtime;
        }

        private static void CreateRuntime()
        {
            if (_runtime == null)
            {
                lock (_sync)
                {
                    if (_runtime == null)
                    {
                        var provider = DataServiceProvider.Get<IPersistenceProviderContainer>();

                        var builder = new WorkflowBuilder<XElement>(provider.AsWorkflowGenerator,
                            new XmlWorkflowParser(),
                            provider.AsSchemePersistenceProvider
                            ).WithDefaultCache();

                        _runtime = new WorkflowRuntime()
                            .WithBuilder(builder)
                            .WithActionProvider(new ActionProvider(DataServiceProvider))
                            .WithRuleProvider(new RuleProvider(DataServiceProvider))
                            .WithPersistenceProvider(provider.AsPersistenceProvider)
                            .WithTimerManager(new TimerManager())
                            .WithBus(new NullBus())
                            .SwitchAutoUpdateSchemeBeforeGetAvailableCommandsOn()
                            .RegisterAssemblyForCodeActions(Assembly.GetExecutingAssembly())
                            .Start();

                        _runtime.ProcessStatusChanged += _runtime_ProcessStatusChanged;
                    }
                }
            }
        }

        public static WorkflowRuntime Runtime => _runtime;

        static void _runtime_ProcessStatusChanged(object sender, ProcessStatusChangedEventArgs e)
        {
            if (e.NewStatus != ProcessStatus.Idled && e.NewStatus != ProcessStatus.Finalized)
                return;

            if (string.IsNullOrEmpty(e.SchemeCode))
                return;

            DataServiceProvider.Get<IDocumentRepository>().DeleteEmptyPreHistory(e.ProcessId);
            _runtime.PreExecuteFromCurrentActivity(e.ProcessId);

            //Inbox
            var ir = DataServiceProvider.Get<IInboxRepository>();
            ir.DropWorkflowInbox(e.ProcessId);

            if (e.NewStatus != ProcessStatus.Finalized)
            {
                Task.Run(() => PreExecuteAndFillInbox(e));
            }

            //Change state name
            if (!e.IsSubprocess)
            {
                var nextState = e.ProcessInstance.CurrentState;
                var nextStateName = Runtime.GetLocalizedStateName(e.ProcessId, e.ProcessInstance.CurrentState);

                var docRepository = DataServiceProvider.Get<IDocumentRepository>();

                docRepository.ChangeState(e.ProcessId, nextState, nextStateName);
            }
            if (e.ProcessInstance.CurrentActivity.IsFinal == true)
            {
                var docRepository = DataServiceProvider.Get<IDocumentRepository>();
                docRepository.UpdateFinishHoSo(e.ProcessId);
            }
            //If run timer
            if (e.ProcessInstance.ExecutedTimer != null && e.ProcessInstance.CurrentCommand == "Timer")
            {
                var newActors = WorkflowInit.Runtime.GetAllActorsForAllCommandTransitions(e.ProcessId, true, e.ProcessInstance.CurrentActivityName);
                var nextActorFirst = "";
                foreach (var act in newActors)
                {
                    nextActorFirst = act;
                }
                if (!string.IsNullOrEmpty(nextActorFirst))
                {
                    var docRepository = DataServiceProvider.Get<IDocumentRepository>();
                    docRepository.UpdateCurrentUserWithTimer(nextActorFirst, e.ProcessInstance.ExecutedTimer, e.ProcessId, e.ProcessInstance.PreviousState, e.ProcessInstance.CurrentState);
                }
            }
        }

        #region Inbox
        private static void PreExecuteAndFillInbox(ProcessStatusChangedEventArgs e)
        {
            DataServiceProvider.Get<IInboxRepository>().FillInbox(e.ProcessId, Runtime);
        }

        public static void RecalcInbox()
        {
            DataServiceProvider.Get<IInboxRepository>().RecalcInbox(Runtime);
        }
        #endregion
    }
}
