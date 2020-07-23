using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WF.Business.Model;

namespace WF.Business.DataAccess
{
    public interface IDocumentRepository
    {
        Model.Document InsertOrUpdate(Model.Document doc);
        void DeleteEmptyPreHistory(Guid processId);
        List<Model.Document> Get(out int count, int page = 0, int pageSize = 128);
        List<Model.Document> GetInbox(Guid identityId, out int count, int page = 0, int pageSize = 128);
        List<Model.Document> GetOutbox(Guid identityId, out int count, int page = 0, int pageSize = 128);
        List<Model.DocumentTransitionHistory> GetHistory(Guid id);
        Model.Document Get(Guid id, bool loadChildEntities = true);
        void Delete(Guid[] ids);
        string DeleteScheme(string[] ids);
        void ChangeState(Guid id, string nextState,  string nextStateName);
        bool IsAuthorsBoss(Guid documentId, Guid identityId);
        IEnumerable<string> GetAuthorsBoss(Guid documentId);
        void WriteTransitionHistory(Guid id, string currentState, string nextState, string command, IEnumerable<string> identities);
        void UpdateTransitionHistory(Guid id, string currentState, string nextState, string command, Guid? employeeId);
        Guid GetHistoryIdExcuteCommand(Guid documentId);
        void UpdateOtherInfo(Guid historyId, Guid documentId, Guid CurrentUserId, Guid PreUserId, string Status, string Comment);
        void UpdateFileAttach(Guid historyId, List<FileAttach> dataFile);
        void CalculateEndTime(Guid documentId, string nextState, string para);
        List<DocumentHistoryViewModel> GetDocumentHistory(Guid documentId);
        void UpdateFinishHoSo(Guid documentId);
        void UpdateFinishHoSoNV(Guid documentId);
        void UpdateCurrentUserWithTimer(string userNext, string nameTimer, Guid documentId, string currentState, string nextState);
        List<string> GetDocumentAuthor(Guid documentId);
        bool CheckDocumentAuthor(Guid documentId, Guid currentUserId);
        List<Guid> GetDocumentByFinish(List<Guid> documentId, bool? isFinished);
        List<DocumentState> GetDocumentByStateName(List<Guid> documentId, List<string> stateName, bool isContain);
        List<Guid> GetDocumentInHistoryByUser(List<Guid> documentId, Guid userId);
        void PushNotificationForAll(Guid documentId, string userId, string mess);
        List<DocumentTransitionHistoryModel> GetAllHistoryForBaoCao(List<Guid> dataHoSoId);
    }
}
