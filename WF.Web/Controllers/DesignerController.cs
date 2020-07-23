using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using OptimaJet.Workflow;
using WF.Business.DataAccess;
using WF.Business.Workflow;
using WF.Business;

namespace WF.Web.Controllers
{
    public class DesignerController : Controller
    {
        private readonly IRoleRepository _roleRepository;
        private readonly IEmployeeRepository _employeeRepository;
        private readonly ICommandStateRepository _commandStateRepository;

        public DesignerController(IRoleRepository roleRepository, IEmployeeRepository employeeRepository, ICommandStateRepository commandStateRepository)
        {
            _roleRepository = roleRepository;
            _employeeRepository = employeeRepository;
            _commandStateRepository = commandStateRepository;
        }

        public ActionResult Index(string schemeName)
        {
            return View();
        }

        public IActionResult API()
        {
            Stream filestream = null;
            var isPost = Request.Method.Equals("POST", StringComparison.OrdinalIgnoreCase);
            if (isPost && Request.Form.Files != null && Request.Form.Files.Count > 0)
                filestream = Request.Form.Files[0].OpenReadStream();

            var pars = new NameValueCollection();
            foreach (var q in Request.Query)
            {
                pars.Add(q.Key, q.Value.First());
            }


            if (isPost)
            {
                var parsKeys = pars.AllKeys;
                //foreach (var key in Request.Form.AllKeys)
                foreach (var key in Request.Form.Keys)
                {
                    if (!parsKeys.Contains(key))
                    {
                        pars.Add(key, Request.Form[key]);
                    }
                }
            }

            var res = WorkflowInit.Runtime.DesignerAPI(pars, filestream);

            if (pars["operation"].ToLower() == "downloadscheme")
                return File(Encoding.UTF8.GetBytes(res), "text/xml", "scheme.xml");
            if (pars["operation"].ToLower() == "downloadschemebpmn")
                return File(Encoding.UTF8.GetBytes(res), "text/xml", "scheme.bpmn");

            return Content(res);

        }

        #region Code API for designer by ThienND 19219
        [HttpGet]
        [Route("api/getvalueactorlist")]
        public Response<List<string>> GetAllActor()
        {
            try
            {
                List<string> dataRole = new List<string>();
                dataRole = _roleRepository.GetAllRole().OrderBy(x => x.RoleName).Select(x => x.RoleName).ToList();
                //List<string> dataEmployee = new List<string>();
                //dataEmployee = _employeeRepository.GetAllUser().OrderBy(x => x.UserName).Select(x => x.UserName).ToList();
                //dataRole.AddRange(dataEmployee);
                return new Response<List<string>>(1, "", dataRole);
            }
            catch (Exception ex)
            {
                return new Response<List<string>>(-1, ex.Message, null);
            }
        }
        [HttpGet]
        [Route("api/getcommandlist")]
        public Response<List<string>> GetAllCommand()
        {
            try
            {
                List<string> dataCommand = new List<string>();
                dataCommand = _commandStateRepository.GetAllCommand().OrderBy(x => x.CommandName).Select(x => x.CommandName).ToList();
                return new Response<List<string>>(1, "", dataCommand);
            }
            catch (Exception ex)
            {
                return new Response<List<string>>(-1, ex.Message, null);
            }
        }

        [HttpGet]
        [Route("api/getstatelist")]
        public Response<List<string>> GetAllState()
        {
            try
            {
                List<string> dataState = new List<string>();
                dataState = _commandStateRepository.GetAllState().OrderBy(x => x.StateName).Select(x => x.StateName).ToList();
                return new Response<List<string>>(1, "", dataState);
            }
            catch (Exception ex)
            {
                return new Response<List<string>>(-1, ex.Message, null);
            }
        }
        #endregion 
    }
}
