using AutoMapper;
using System;

namespace WF.MsSql
{
    internal static class Mappings
    {
        public static IMapper Mapper { get { return _mapper.Value; } }

        private static Lazy<IMapper> _mapper = new Lazy<IMapper>(GetMapper);

        private static IMapper GetMapper()
        {
            var config = new MapperConfiguration(cfg => {

                /*Core Workflow Engine*/
                cfg.CreateMap<StructDivision, Business.Model.StructDivision>();
                cfg.CreateMap<Employee, Business.Model.Employee>();
                cfg.CreateMap<Role, Business.Model.Role>();
                cfg.CreateMap<EmployeeRole, Business.Model.EmployeeRole>();
                cfg.CreateMap<DocumentTransitionHistory, Business.Model.DocumentTransitionHistory>();
                cfg.CreateMap<Document, Business.Model.Document>();
                cfg.CreateMap<WorkflowScheme, Business.Model.WorkflowScheme>();
                cfg.CreateMap<WorkflowProcessInstance, Business.Model.WorkflowProcessInstance>();
                cfg.CreateMap<WorkflowProcessInstancePersistence, Business.Model.WorkflowProcessInstancePersistence>();
                cfg.CreateMap<WorkflowProcessScheme, Business.Model.WorkflowProcessScheme>();
                cfg.CreateMap<WorkflowProcessTimer, Business.Model.WorkflowProcessTimer>();
                cfg.CreateMap<WorkflowProcessTransitionHistory, Business.Model.WorkflowProcessTransitionHistory>();

                /*Extend for WFE*/
                cfg.CreateMap<Ext_DocumentHitory_FileAttach, Business.Model.Ext_DocumentHitory_FileAttach>();

                /*Nghiệp vụ*/
                cfg.CreateMap<aspnet_Membership, Business.Model.aspnet_Membership>();
                cfg.CreateMap<aspnet_Roles, Business.Model.aspnet_Roles>();
                cfg.CreateMap<aspnet_Users, Business.Model.aspnet_Users>();
                cfg.CreateMap<aspnet_UsersInRoles, Business.Model.aspnet_UsersInRoles>();
                //cfg.CreateMap<Taxonomy_Vocabulary, Business.Model.Taxonomy_Vocabulary>();
                //cfg.CreateMap<Taxonomy_Term, Business.Model.Taxonomy_Term>();
                //cfg.CreateMap<lcg_HoSo, Business.Model.lcg_HoSo>();
                //cfg.CreateMap<lcg_UyQuyen, Business.Model.lcg_UyQuyen>();
                //cfg.CreateMap<lcg_NgayNghi, Business.Model.lcg_NgayNghi>();
                // cfg.CreateMap<SystemParameter, Business.Model.SystemParameter>();
            });

            var mapper = config.CreateMapper();

            return mapper;
        }
    }
}
