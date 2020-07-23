using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WF.Business.Model;
using WF.Business.Workflow;
using WF.Web.Helpers;
using WF.Web.Models;
using System.Threading;
using AutoMapper;
using WF.Business.DataAccess;
using Microsoft.AspNetCore.Mvc;

namespace WF.Web.Controllers
{
    public class DocumentController : Controller
    {
        private readonly IDocumentRepository _documentRepository;
        private readonly IWorkflowSchemeRepository _workflowSchemeRepository;
        private readonly IEmployeeRepository _employeeRepository;
        private readonly IMapper _mapper;

        public DocumentController(IDocumentRepository documentRepository, IWorkflowSchemeRepository workflowSchemeRepository, IEmployeeRepository employeeRepository, IMapper mapper)
        {
            _documentRepository = documentRepository;
            _workflowSchemeRepository = workflowSchemeRepository;
            _employeeRepository = employeeRepository;
            _mapper = mapper;
        }

        #region Index
        public ActionResult Index(int page = 0)
        {
            int count = 0;
            const int pageSize = 15;

            return View(new DocumentListModel()
            {
                Page = page,
                PageSize = pageSize,
                Docs = _documentRepository.Get(out count, page, pageSize).Select(c => GetDocumentModel(c)).ToList(),
                Count = count,
            });
        }

        public ActionResult WorkflowScheme(int page = 0)
        {
            int count = 0;
            const int pageSize = 15;

            List<WorkflowSchemeDetails> dataWorkflowScheme = _workflowSchemeRepository.Get(out count, page, pageSize).Select(c => GetWorkflowSchemeModel(c)).ToList();
            int i = 1;
            foreach (var item in dataWorkflowScheme)
            {
                item.STT = i;
                i++;
            }
            return View(new WorkflowListModel()
            {
                Page = page,
                PageSize = pageSize,
                WorkflowSchemeData = dataWorkflowScheme,
                Count = count,
            });
        }

        public ActionResult Inbox(int page = 0)
        {
            int count = 0;
            const int pageSize = 15;

            return View("Index", new DocumentListModel()
            {
                Page = page,
                PageSize = pageSize,
                Docs = _documentRepository.GetInbox(CurrentUserSettings.GetCurrentUser(HttpContext), out count, page, pageSize)
                        .Select(c => GetDocumentModel(c)).ToList(),
                Count = count,
            });
        }

        public ActionResult Outbox(int page = 0)
        {
            int count = 0;
            const int pageSize = 15;

            return View("Index", new DocumentListModel()
            {
                Page = page,
                PageSize = pageSize,
                Docs = _documentRepository.GetOutbox(CurrentUserSettings.GetCurrentUser(HttpContext), out count, page, pageSize)
                        .Select(c => GetDocumentModel(c)).ToList(),
                Count = count,
            });
        }
        #endregion

        #region Edit
        public ActionResult Edit(Guid? Id, string SchemeName)
        {
            DocumentModel model = null;

            if (Id.HasValue)
            {
                var d = _documentRepository.Get(Id.Value);
                if (d != null)
                {
                    CreateWorkflowIfNotExists(d.SchemeName, Id.Value);

                    var h = _documentRepository.GetHistory(Id.Value);
                    model = new DocumentModel()
                    {
                        Id = d.Id,
                        AuthorId = d.AuthorId,
                        AuthorName = d.Author.UserName,
                        Comment = d.Comment,
                        ManagerId = d.ManagerId,
                        ManagerName = d.ManagerId.HasValue ? d.Manager.UserName : string.Empty,
                        Name = d.Name,
                        Number = d.Number,
                        StateName = d.StateName,
                        Sum = d.Sum,
                        Commands = GetCommands(Id.Value),
                        AvailiableStates = GetStates(Id.Value),
                        HistoryModel = new DocumentHistoryModel { Items = h }
                    };
                }

            }
            else
            {
                Guid userId = CurrentUserSettings.GetCurrentUser(HttpContext);
                var intState = WorkflowInit.Runtime.GetInitialState(SchemeName);
                model = new DocumentModel()
                {
                    AuthorId = userId,
                    AuthorName = _employeeRepository.GetNameById(userId),
                    StateName = "Vacation request created"
                };
            }

            return View(model);
        }

        [HttpPost]
        public ActionResult Edit(Guid? Id, DocumentModel model, string button)
        {

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            Document doc = _mapper.Map<Document>(model);

            try
            {
                doc = _documentRepository.InsertOrUpdate(doc);

                if (doc == null)
                {
                    ModelState.AddModelError("", "Row not found!");
                    return View(model);
                }
            }
            catch (Exception ex)
            {
                var sb = new StringBuilder("Ошибка сохранения. " + ex.Message);
                if (ex.InnerException != null)
                    sb.AppendLine(ex.InnerException.Message);
                ModelState.AddModelError("", sb.ToString());
                return View(model);
            }

            if (button == "SaveAndExit")
                return RedirectToAction("Index");
            if (button != "Save")
            {
                ExecuteCommand(doc.Id, button, model);
            }
            return RedirectToAction("Edit", new { doc.Id });

        }
        #endregion

        #region Delete
        public ActionResult DeleteRows(Guid[] ids)
        {
            if (ids == null || ids.Length == 0)
                return Content("Items not selected");

            try
            {
                _documentRepository.Delete(ids);
            }
            catch (Exception ex)
            {
                return Content(ex.Message);
            }

            return Content("Rows deleted");
        }

        public ActionResult DeleteScheme(string[] ids)
        {
            if (ids == null || ids.Length == 0)
                return Content("Items not selected");

            try
            {
                string rs = _documentRepository.DeleteScheme(ids);
                return Content(rs);
            }
            catch (Exception ex)
            {
                return Content(ex.Message);
            }            
        }
        #endregion

        #region Workflow
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

        private void ExecuteCommand(Guid id, string commandName, DocumentModel document)
        {
            var currentUser = CurrentUserSettings.GetCurrentUser(HttpContext).ToString();

            if (commandName.Equals("SetState", StringComparison.InvariantCultureIgnoreCase))
            {
                if (string.IsNullOrEmpty(document.StateNameToSet))
                    return;

                WorkflowInit.Runtime.SetState(id, currentUser, currentUser, document.StateNameToSet, new Dictionary<string, object> { { "Comment", document.Comment } });
                return;
            }

            var commands = WorkflowInit.Runtime.GetAvailableCommands(id, currentUser);

            var command =
                commands.FirstOrDefault(
                    c => c.CommandName.Equals(commandName, StringComparison.CurrentCultureIgnoreCase));

            if (command == null)
                return;

            if (command.Parameters.Count(p => p.ParameterName == "Comment") == 1)
                command.Parameters.Single(p => p.ParameterName == "Comment").Value = document.Comment ?? string.Empty;

            WorkflowInit.Runtime.ExecuteCommand(command, currentUser, currentUser);
        }

        private void CreateWorkflowIfNotExists(string schemeName, Guid id)
        {
            if (WorkflowInit.Runtime.IsProcessExists(id))
                return;

            WorkflowInit.Runtime.CreateInstance(schemeName, id);
        }

        #endregion

        public ActionResult RecalcInbox()
        {
            var newThread = new Thread(WorkflowInit.RecalcInbox);
            newThread.Start();
            return Content("Calculating inbox started!");
        }

        private DocumentModel GetDocumentModel(Document d)
        {
            return new DocumentModel()
            {
                Id = d.Id,
                AuthorId = d.AuthorId,
                AuthorName = d.Author.UserName,
                Comment = d.Comment,
                ManagerId = d.ManagerId,
                ManagerName = d.ManagerId.HasValue ? d.Manager.UserName : string.Empty,
                Name = d.Name,
                Number = d.Number,
                StateName = d.StateName,
                Sum = d.Sum
            };
        }

        private WorkflowSchemeDetails GetWorkflowSchemeModel(WorkflowScheme d)
        {
            return new WorkflowSchemeDetails()
            {
                WorkflowCode = d.Code
            };
        }
    }
}
