using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using WF.Business;
using WF.Business.DataAccess;
using WF.Business.Model;
using WF.Business.Workflow;
using WF.Web.Helpers;
using WF.Web.Models;

namespace WF.Web.Controllers
{
    public class WFController : Controller
    {
        private readonly IDocumentRepository _documentRepository;
        private readonly IWorkflowSchemeRepository _workflowSchemeRepository;
        private readonly IEmployeeRepository _employeeRepository;
        private readonly IMapper _mapper;

        public WFController(IDocumentRepository documentRepository, IWorkflowSchemeRepository workflowSchemeRepository, IEmployeeRepository employeeRepository, IMapper mapper)
        {
            _documentRepository = documentRepository;
            _workflowSchemeRepository = workflowSchemeRepository;
            _employeeRepository = employeeRepository;
            _mapper = mapper;
        }

        #region Code API workflow by ThienND 19219
        [HttpPost]
        [Route("api/documents")]
        public Response<Document> AddNewDocument([FromBody] Document data)
        {
            try
            {
                var intState = WorkflowInit.Runtime.GetInitialState(data.SchemeName);
                if (!string.IsNullOrWhiteSpace(intState.VisibleName))
                {
                    data.State = intState.VisibleName;
                    data.StateName = intState.VisibleName;
                    var dataReturn = _documentRepository.InsertOrUpdate(data);

                    var d = _documentRepository.Get(dataReturn.Id);
                    if (d != null)
                    {
                        CreateWorkflowIfNotExists(d.SchemeName, d.Id);

                        var h = _documentRepository.GetHistory(d.Id);
                        if (h != null && h.Count > 0)
                        {
                            DocumentModel model = new DocumentModel()
                            {
                                Id = d.Id,
                                AuthorId = d.AuthorId,
                                AuthorName = string.Empty,
                                Comment = d.Comment,
                                ManagerId = d.ManagerId,
                                ManagerName = string.Empty,
                                Name = d.Name,
                                Number = d.Number,
                                StateName = d.StateName,
                                Sum = d.Sum,
                                Commands = GetCommands(d.Id),
                                AvailiableStates = GetStates(d.Id),
                                HistoryModel = new DocumentHistoryModel { Items = h }
                            };
                        }
                    }

                    return new Response<Document>(1, "Success", dataReturn);
                }
                else
                {
                    return new Response<Document>(-1, "Lỗi không tìm thấy trạng thái khởi tạo. Vui lòng xem lại WF: "+ data.SchemeName, null);
                }
            }
            catch (Exception ex)
            {
                return new Response<Document>(-1, ex.Message, null);
            }
        }

        [HttpPost]
        [Route("api/deletedocument")]
        public Response<bool> DeleteDocument([FromBody] Document data)
        {
            try
            {
                List<Guid> dataDocId = new List<Guid>();
                dataDocId.Add(data.Id);
                _documentRepository.Delete(dataDocId.ToArray());
                return new Response<bool>(1, "Success", true);
            }
            catch (Exception ex)
            {
                return new Response<bool>(-1, ex.Message, false);
            }
        }

        [HttpPost]
        [Route("api/commands")]
        public Response<List<string>> GetAvaliableCommand([FromBody] InputCommand data)
        {
            try
            {
                var dataReturn = new List<string>();
                var availableProcessCommands = WorkflowInit.Runtime.GetAvailableCommands(data.DocumentId, data.UserId);
                foreach (var workflowCommand in availableProcessCommands)
                {
                    if (!dataReturn.Any(x => x == workflowCommand.CommandName))
                        dataReturn.Add(workflowCommand.CommandName);
                }
                return new Response<List<string>>(1, "Success", dataReturn);
            }
            catch (Exception ex)
            {
                return new Response<List<string>>(-1, ex.Message, null);
            }
        }

        [HttpPost]
        [Route("api/excutecommand")]
        public Response<bool> ExcuteCommand([FromBody] InputCommand data)
        {
            ExecuteCommand(data);
            return new Response<bool>(1, "Success", true);
        }

        [HttpPost]
        [Route("api/nextuser")]
        public Response<List<UserInfo>> GetNextUserProcess([FromBody] InputCommand data)
        {
            try
            {
                List<UserInfo> dataReturn = new List<UserInfo>();
                var pi = WorkflowInit.Runtime.GetProcessInstanceAndFillProcessParameters(data.DocumentId);
                var allCommandTransitions = pi.ProcessScheme.GetCommandTransitions(pi.CurrentActivity);
                var yourTransition = allCommandTransitions.FirstOrDefault(t => t.Trigger.Command.Name == data.CommandName);
                if (yourTransition == null)
                    return new Response<List<UserInfo>>(0, "Data null", null);
                var yourActivityName = yourTransition.To.Name;
                bool nextIsFinal = yourTransition.To.IsFinal;

                var Nextactor = WorkflowInit.Runtime.GetAllActorsForAllCommandTransitions(data.DocumentId, false, yourActivityName);
                var dataList = Nextactor.ToList();
                if (dataList != null && dataList.Count > 0)
                {
                    dataReturn = _employeeRepository.GetAllUserById(dataList);
                }
                return new Response<List<UserInfo>>(1, "Success", dataReturn);
            }
            catch (Exception ex)
            {
                return new Response<List<UserInfo>>(-1, ex.Message, null);
            }
        }

        [HttpPost]
        [Route("api/history/{id}")]
        public Response<List<DocumentHistoryViewModel>> GetDocumentHistory(Guid id)
        {
            try
            {
                var data = _documentRepository.GetDocumentHistory(id);
                return new Response<List<DocumentHistoryViewModel>>(1, "Success", data);
            }
            catch (Exception ex)
            {
                return new Response<List<DocumentHistoryViewModel>>(-1, ex.Message, null);
            }
        }

        [HttpPost]
        [Route("api/documentbyfinish")]
        public Response<List<Guid>> GetDocumentByFinish([FromBody] DocumentByFinish data)
        {
            try
            {
                var dataRS = _documentRepository.GetDocumentByFinish(data.DataDocumentId, data.IsFinish);
                return new Response<List<Guid>>(1, "Success", dataRS);
            }
            catch (Exception ex)
            {
                return new Response<List<Guid>>(-1, ex.Message, null);
            }
        }

        [HttpPost]
        [Route("api/statenameofdocument/{id}")]
        public Response<DocumentStateModel> GetStateNameOfDocument(Guid id)
        {
            try
            {
                var data = _documentRepository.GetStateNameOfDocument(id);
                return new Response<DocumentStateModel>(1, "Success", data);
            }
            catch (Exception ex)
            {
                return new Response<DocumentStateModel>(-1, ex.Message, new DocumentStateModel() { StateName = string.Empty, IsFinished = null });
            }
        }
        
        [HttpPost]
        [Route("api/documentbystatename")]
        public Response<List<DocumentState>> GetDocumentByStateName([FromBody] DocumentByStateName data)
        {
            try
            {
                var dataRS = _documentRepository.GetDocumentByStateName(data.DataDocumentId, data.DataStateName, data.IsContain);
                return new Response<List<DocumentState>>(1, "Success", dataRS);
            }
            catch (Exception ex)
            {
                return new Response<List<DocumentState>>(-1, ex.Message, null);
            }
        }

        [HttpPost]
        [Route("api/documentbyuserjoin")]
        public Response<List<Guid>> GetDocumentInHistoryByUser([FromBody] DocumentByFinish data)
        {
            try
            {
                var dataRS = _documentRepository.GetDocumentInHistoryByUser(data.DataDocumentId, data.UserId.Value);
                return new Response<List<Guid>>(1, "Success", dataRS);
            }
            catch (Exception ex)
            {
                return new Response<List<Guid>>(-1, ex.Message, null);
            }
        }

        [HttpPost]
        [Route("api/allhistoryforbaocao")]
        public Response<List<DocumentTransitionHistoryModel>> GetAllHistoryForBaoCao([FromBody] HistoryAllForBaoCao data)
        {
            try
            {
                var dataRS = _documentRepository.GetAllHistoryForBaoCao(data.dataHoSoId);
                return new Response<List<DocumentTransitionHistoryModel>>(1, "Success", dataRS);
            }
            catch (Exception ex)
            {
                return new Response<List<DocumentTransitionHistoryModel>>(-1, ex.Message, null);
            }
        }

        #region Hàm sử dụng cho các API
        private void ExecuteCommand(InputCommand data)
        {
            var currentUser = data.UserId.ToString();

            if (data.CommandName.Equals("SetState", StringComparison.InvariantCultureIgnoreCase))
            {
                if (string.IsNullOrEmpty(data.StateNameToSet))
                    return;

                WorkflowInit.Runtime.SetState(data.DocumentId, currentUser, currentUser, data.StateNameToSet, new Dictionary<string, object> { { "Comment", data.Comment } });
                return;
            }

            var commands = WorkflowInit.Runtime.GetAvailableCommands(data.DocumentId, currentUser);

            var command =
                commands.FirstOrDefault(
                    c => c.CommandName.Equals(data.CommandName, StringComparison.CurrentCultureIgnoreCase));

            if (command == null)
                return;

            WorkflowInit.Runtime.ExecuteCommand(command, currentUser, currentUser);

            Guid hisId = _documentRepository.GetHistoryIdExcuteCommand(data.DocumentId);

            _documentRepository.UpdateOtherInfo(hisId, data.DocumentId, new Guid(data.NextUserId), new Guid(currentUser), "", data.Comment);

            if (data.DataFileAttach != null && data.DataFileAttach.Count > 0)
                _documentRepository.UpdateFileAttach(hisId, data.DataFileAttach);
        }

        private void CreateWorkflowIfNotExists(string schemeName, Guid id)
        {
            if (WorkflowInit.Runtime.IsProcessExists(id))
                return;

            WorkflowInit.Runtime.CreateInstance(schemeName, id);
        }

        private DocumentCommandModel[] GetCommands(Guid id)
        {
            var result = new List<DocumentCommandModel>();
            var commands = WorkflowInit.Runtime.GetAvailableCommands(id, CurrentUserSettings.GetCurrentUser(HttpContext).ToString());
            foreach (var workflowCommand in commands)
            {
                if (result.Count(c => c.key == workflowCommand.CommandName) == 0)
                    result.Add(new DocumentCommandModel() { key = workflowCommand.CommandName, value = workflowCommand.LocalizedName, Classifier = workflowCommand.Classifier });
            }
            return result.ToArray();
        }

        private Dictionary<string, string> GetStates(Guid id)
        {
            var result = new Dictionary<string, string>();
            var states = WorkflowInit.Runtime.GetAvailableStateToSet(id);
            foreach (var state in states)
            {
                if (!result.ContainsKey(state.Name))
                    result.Add(state.Name, state.VisibleName);
            }
            return result;
        }
        #endregion
        #endregion
    }
}
