using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WF.Business.DataAccess;
using Microsoft.Extensions.Configuration;
using WF.Business.Model;
using System.Net.Http;
using Newtonsoft.Json;
using WF.Business;
using WF.Business.Workflow;
using System.Globalization;

namespace WF.MsSql.Implementation
{
    public class DocumentRepository : IDocumentRepository
    {
        private SampleContext _sampleContext;
        private MICHRContext _michrContext;

        public DocumentRepository(SampleContext sampleContext, MICHRContext michrContext)
        {
            _sampleContext = sampleContext;
            _michrContext = michrContext;
        }

        public void ChangeState(Guid id, string nextState, string nextStateName)
        {
            var document = GetDocument(id);
            if (document == null)
                return;

            document.State = nextState;
            document.StateName = nextStateName;

            _sampleContext.SaveChanges();
        }

        public void Delete(Guid[] ids)
        {
            var objs = _sampleContext.Documents.Where(x => ids.Contains(x.Id));

            _sampleContext.Documents.RemoveRange(objs);

            var objInbox = _sampleContext.WorkflowInboxes.Where(x => ids.Contains(x.ProcessId));

            _sampleContext.WorkflowInboxes.RemoveRange(objInbox);

            var objwfPInstance = _sampleContext.WorkflowProcessInstances.Where(x => ids.Contains(x.RootProcessId) || ids.Contains(x.Id));

            _sampleContext.WorkflowProcessInstances.RemoveRange(objwfPInstance);

            var objwfPInstancePer = _sampleContext.WorkflowProcessInstancePersistences.Where(x => ids.Contains(x.ProcessId));

            _sampleContext.WorkflowProcessInstancePersistences.RemoveRange(objwfPInstancePer);

            var objwfproTransitionHis = _sampleContext.WorkflowProcessTransitionHistorys.Where(x => ids.Contains(x.ProcessId));

            _sampleContext.WorkflowProcessTransitionHistorys.RemoveRange(objwfproTransitionHis);

            _sampleContext.SaveChanges();
        }

        public void DeleteEmptyPreHistory(Guid processId)
        {
            var existingNotUsedItems =
                   _sampleContext.DocumentTransitionHistories.Where(
                       dth =>
                       dth.DocumentId == processId && !dth.TransitionTime.HasValue);

            _sampleContext.DocumentTransitionHistories.RemoveRange(existingNotUsedItems);

            _sampleContext.SaveChanges();
        }

        public List<Business.Model.Document> Get(out int count, int page = 0, int pageSize = 128)
        {
            int actual = page * pageSize;
            var query = _sampleContext.Documents.OrderByDescending(c => c.Number);
            count = query.Count();
            return query.Skip(actual)
                        .Take(pageSize)
                        .ToList()
                        .Select(d => Mappings.Mapper.Map<Business.Model.Document>(d)).ToList();
        }

        public IEnumerable<string> GetAuthorsBoss(Guid documentId)
        {
            var document = _sampleContext.Documents.Find(documentId);
            if (document == null)
                return new List<string> { };

            return
                _sampleContext.VHeads.Where(h => h.Id == document.AuthorId)
                    .Select(h => h.HeadId)
                    .ToList()
                    .Select(c => c.ToString());
        }

        public List<Business.Model.DocumentTransitionHistory> GetHistory(Guid id)
        {
            DateTime orderTime = new DateTime(9999, 12, 31);

            return _sampleContext.DocumentTransitionHistories
                 .Where(h => h.DocumentId == id)
                 .OrderBy(h => h.TransitionTime == null ? orderTime : h.TransitionTime.Value)
                 .ThenBy(h => h.Order)
                 .ToList()
                 .Select(x => Mappings.Mapper.Map<Business.Model.DocumentTransitionHistory>(x)).ToList();
        }

        public List<Business.Model.Document> GetInbox(Guid identityId, out int count, int page = 0, int pageSize = 128)
        {
            var strGuid = identityId.ToString();
            int actual = page * pageSize;
            var subQuery = _sampleContext.WorkflowInboxes.Where(c => c.IdentityId == strGuid);

            var query = _sampleContext.Documents.Where(c => subQuery.Any(i => i.ProcessId == c.Id));
            count = query.Count();
            return query.OrderByDescending(c => c.Number).Skip(actual).Take(pageSize)
                        .ToList()
                        .Select(d => Mappings.Mapper.Map<Business.Model.Document>(d)).ToList();
        }

        public List<Business.Model.Document> GetOutbox(Guid identityId, out int count, int page = 0, int pageSize = 128)
        {
            int actual = page * pageSize;
            var subQuery = _sampleContext.DocumentTransitionHistories.Where(c => c.EmployeeId == identityId);
            var query = _sampleContext.Documents.Where(c => subQuery.Any(i => i.DocumentId == c.Id));
            count = query.Count();
            return query.OrderByDescending(c => c.Number).Skip(actual).Take(pageSize)
                .ToList()
                .Select(d => Mappings.Mapper.Map<Business.Model.Document>(d)).ToList();
        }

        public Business.Model.Document InsertOrUpdate(Business.Model.Document doc)
        {
            Document target = null;
            target = new Document
            {
                Id = doc.Id,
                AuthorId = doc.AuthorId,
                StateName = doc.StateName,
                SchemeName = doc.SchemeName,
                State = doc.StateName
            };
            _sampleContext.Documents.Add(target);

            target.Name = doc.Name;
            target.ManagerId = doc.ManagerId;
            target.Comment = doc.Comment;
            target.Sum = doc.Sum;

            _sampleContext.SaveChanges();

            doc.Id = target.Id;
            doc.Number = target.Number;

            return doc;
        }

        public bool IsAuthorsBoss(Guid documentId, Guid identityId)
        {
            var document = _sampleContext.Documents.Find(documentId);
            if (document == null)
                return false;
            return _sampleContext.VHeads.Count(h => h.Id == document.AuthorId && h.HeadId == identityId) > 0;
        }

        public void UpdateTransitionHistory(Guid id, string currentState, string nextState, string command, Guid? employeeId)
        {
            if (command != "Timer")
            {

                var docs = _sampleContext.Documents.FirstOrDefault(x => x.Id == id);
                var intState = WorkflowInit.Runtime.GetInitialState(docs.SchemeName);
                if (currentState == intState.Name)
                {

                }

                var sComment = (from Doc in _sampleContext.Documents where Doc.Id == id select Doc.Comment).FirstOrDefault();

                var historyItem =
                    _sampleContext.DocumentTransitionHistories.FirstOrDefault(
                        h => h.DocumentId == id && !h.TransitionTime.HasValue &&
                             h.InitialState == currentState && h.DestinationState == nextState);

                if (historyItem == null)
                {
                    var s = (from time in _sampleContext.DocumentTransitionHistories
                             where (time.DestinationState == currentState)
                             && (time.DocumentId == id)
                             && time.TransitionTime != null
                             orderby time.Order descending
                             select time.TransitionTime).FirstOrDefault();

                    historyItem = new DocumentTransitionHistory
                    {
                        Id = Guid.NewGuid(),
                        AllowedToEmployeeNames = string.Empty,
                        DestinationState = nextState,
                        DocumentId = id,
                        InitialState = currentState,
                        InitialTime = s
                    };

                    _sampleContext.DocumentTransitionHistories.Add(historyItem);
                }

                historyItem.Command = (!string.IsNullOrWhiteSpace(command)) ? command : "";
                historyItem.TransitionTime = DateTime.Now;

                if (string.IsNullOrWhiteSpace(employeeId.ToString()))
                    historyItem.EmployeeId = null;
                else
                    historyItem.EmployeeId = employeeId;
                historyItem.Comment = sComment;
                historyItem.EmployeeId = employeeId;
                historyItem.FromUser = employeeId;

                _sampleContext.SaveChanges();
            }
        }

        public void WriteTransitionHistory(Guid id, string currentState, string nextState, string command, IEnumerable<string> identities)
        {
            if (command != "Timer")
            {
                var doc = _sampleContext.Documents.FirstOrDefault(x => x.Id == id);
                var intState = WorkflowInit.Runtime.GetInitialState(doc.SchemeName);

                if (identities == null)
                    return;

                if (currentState == intState.Name)
                {
                    var historyItem = new DocumentTransitionHistory
                    {
                        Id = Guid.NewGuid(),
                        AllowedToEmployeeNames = GetEmployeesString(identities),
                        DestinationState = nextState,
                        DocumentId = id,
                        InitialState = currentState,
                        Command = command,
                        InitialTime = DateTime.Now
                    };
                    _sampleContext.DocumentTransitionHistories.Add(historyItem);
                    _sampleContext.SaveChanges();
                }
                else
                {
                    var s = (from time in _sampleContext.DocumentTransitionHistories
                             where (time.DestinationState == currentState)
                             && (time.DocumentId == id)
                             orderby time.Order descending
                             select time.TransitionTime).FirstOrDefault();

                    var historyItem = new DocumentTransitionHistory
                    {
                        Id = Guid.NewGuid(),
                        AllowedToEmployeeNames = GetEmployeesString(identities),
                        DestinationState = nextState,
                        DocumentId = id,
                        InitialState = currentState,
                        Command = command,
                        InitialTime = s
                    };
                    _sampleContext.DocumentTransitionHistories.Add(historyItem);
                    _sampleContext.SaveChanges();
                }
            }
        }

        public Business.Model.Document Get(Guid id, bool loadChildEntities = true)
        {
            Document document = GetDocument(id, loadChildEntities);
            if (document == null) return null;
            return Mappings.Mapper.Map<Business.Model.Document>(document);
        }

        private Document GetDocument(Guid id, bool loadChildEntities = true)
        {
            Document document = null;

            if (!loadChildEntities)
            {
                document = _sampleContext.Documents.Find(id);
            }
            else
            {
                document = _sampleContext.Documents.FirstOrDefault(x => x.Id == id);
            }

            return document;

        }

        private string GetEmployeesString(IEnumerable<string> identities)
        {
            var identitiesGuid = identities.Select(c => new Guid(c));

            var employees = _michrContext.aspnet_Users.Where(e => identitiesGuid.Contains(e.UserId)).ToList();

            var sb = new StringBuilder();
            bool isFirst = true;
            foreach (var employee in employees)
            {
                if (!isFirst)
                    sb.Append(",");
                isFirst = false;

                sb.Append(employee.UserName);
            }

            return sb.ToString();
        }

        public string DeleteScheme(string[] ids)
        {
            var checkDoc = _sampleContext.Documents.FirstOrDefault(x => ids.Contains(x.SchemeName));
            if (checkDoc != null)
            {
                return "Xóa thất bại do Workflow đã gắn Document";
            }
            else
            {
                var objs = _sampleContext.WorkflowSchemes.Where(x => ids.Contains(x.Code));

                _sampleContext.WorkflowSchemes.RemoveRange(objs);

                var objsProcessScheme = _sampleContext.WorkflowProcessSchemes.Where(x => ids.Contains(x.SchemeCode));

                _sampleContext.WorkflowProcessSchemes.RemoveRange(objsProcessScheme);

                _sampleContext.SaveChanges();
                return "Xóa thành công";
            }
        }

        public void UpdateOtherInfo(Guid historyId, Guid documentId, Guid CurrentUserId, Guid PreUserId, string Status, string Comment)
        {
            try
            {
                var doc = _sampleContext.Documents.FirstOrDefault(x => x.Id == documentId);
                //var hs = _lcgContext.lcg_HoSos.FirstOrDefault(x => x.HoSoId == documentId);
                doc.Comment = Comment;

                var his = _sampleContext.DocumentTransitionHistories.FirstOrDefault(x => x.Id == historyId);
                his.Comment = Comment;
                his.FromUser = PreUserId;
                his.ToUser = CurrentUserId;

                _sampleContext.SaveChanges();

                CheckAndInsertHistoryUyQuyen(CurrentUserId, documentId, his.InitialState, his.DestinationState);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public void CheckAndInsertHistoryUyQuyen(Guid CurrentUserId, Guid documentId, string currentState, string nextState)
        {

            try
            {
                //#region Check ủy quyền
                //DateTime now = DateTime.Now;
                //var uyquyen = _lcgContext.lcg_UyQuyen.FirstOrDefault(x => x.NguoiUyQuyen == CurrentUserId
                //&& x.TrangThai == true && x.TuNgay <= now && x.DenNgay >= now);
                //if (uyquyen != null)
                //{
                //    var doc = _sampleContext.Documents.FirstOrDefault(x => x.Id == documentId);
                //    doc.Comment = "Ủy quyền";
                //    var historyItem = new DocumentTransitionHistory
                //    {
                //        Id = Guid.NewGuid(),
                //        AllowedToEmployeeNames = string.Empty,
                //        DestinationState = nextState,
                //        DocumentId = documentId,
                //        InitialState = currentState,
                //        InitialTime = DateTime.Now,
                //        FromUser = CurrentUserId,
                //        ToUser = uyquyen.NguoiDuocUyQuyen,
                //        Command = "Ủy quyền",
                //        TransitionTime = DateTime.Now,
                //        EmployeeId = CurrentUserId,
                //        Comment = "Ủy quyền xử lý"
                //    };
                //    _sampleContext.DocumentTransitionHistories.Add(historyItem);
                //    _sampleContext.SaveChanges();
                //}
                //#endregion
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public void UpdateFileAttach(Guid historyId, List<FileAttach> dataFile)
        {
            try
            {
                if (dataFile != null && dataFile.Count > 0)
                {
                    foreach (var item in dataFile)
                    {
                        Ext_DocumentHitory_FileAttach f = new Ext_DocumentHitory_FileAttach();
                        f.DocumentHistoryId = historyId;
                        f.FileName = item.FileName;
                        f.FilePath = item.FilePath;
                        f.Id = Guid.NewGuid();
                        _sampleContext.Ext_DocumentHitory_FileAttachs.Add(f);
                    }
                    _sampleContext.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public Guid GetHistoryIdExcuteCommand(Guid documentId)
        {
            try
            {
                var his = _sampleContext.DocumentTransitionHistories.Where(x => x.DocumentId == documentId
                && x.DestinationState == x.Document.StateName && x.TransitionTime != null).OrderByDescending(x => x.Order).FirstOrDefault();
                if (his != null)
                    return his.Id;
                else
                    return Guid.Empty;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public void CalculateEndTime(Guid documentId, string nextState, string para)
        {
            try
            {
                //var hs = _lcgContext.lcg_HoSos.FirstOrDefault(x => x.HoSoId == documentId);
                //var dataNgayNghi = _lcgContext.lcg_NgayNghi.Where(x => x.TrangThai == true && x.Ngay.Date >= DateTime.Now.Date);
                //var sang1 = _lcgContext.SystemParameter.FirstOrDefault(x => x.ParameterCode == GIO_LAM_VIEC.DAU_GIO_SANG);
                //var sang2 = _lcgContext.SystemParameter.FirstOrDefault(x => x.ParameterCode == GIO_LAM_VIEC.CUOI_GIO_SANG);
                //var chieu1 = _lcgContext.SystemParameter.FirstOrDefault(x => x.ParameterCode == GIO_LAM_VIEC.DAU_GIO_CHIEU);
                //var chieu2 = _lcgContext.SystemParameter.FirstOrDefault(x => x.ParameterCode == GIO_LAM_VIEC.CUOI_GIO_CHIEU);

                //DateTime dateEndState = DateTime.Now;
                //DateTime beginDateState = DateTime.Now;
                //var histrans = _sampleContext.DocumentTransitionHistories.Where(x => x.TransitionTime != null && x.DocumentId == documentId).OrderByDescending(x => x.TransitionTime).ThenByDescending(x => x.Order).FirstOrDefault();
                //if (histrans != null)
                //{
                //    histrans.EndTime = hs.EndDateState;
                //    _sampleContext.SaveChanges();
                //    beginDateState = histrans.TransitionTime.Value;
                //}
                //dateEndState = beginDateState;
                //if (!string.IsNullOrEmpty(para))
                //{
                //    int add = Convert.ToInt32(para);
                //    int d = add / 8 / 60;
                //    int h = add / 60 % 8;
                //    int m = add % 60;
                //    int totalMinute = (h * 60) + m;
                //    #region Tính ngày
                //    while (d > 0)
                //    {
                //        dateEndState = dateEndState.AddDays(1);
                //        if (dateEndState.DayOfWeek != DayOfWeek.Saturday
                //            && dateEndState.DayOfWeek != DayOfWeek.Sunday
                //            && !dataNgayNghi.Any(x => x.Ngay.Date == dateEndState.Date))
                //        {
                //            d--;
                //        }
                //        else
                //        {
                //            dateEndState = dateEndState.AddDays(1);
                //        }
                //    };
                //    #endregion
                //    #region Tính giờ
                //    //Set lai gio
                //    if (sang1 != null && sang2 != null && chieu1 != null && chieu2 != null)
                //    {
                //        while (h > 0)
                //        {
                //            dateEndState = dateEndState.AddHours(1);
                //            if (!dataNgayNghi.Any(x => x.Ngay.Date == dateEndState.Date)
                //                && dateEndState.DayOfWeek != DayOfWeek.Saturday
                //                && dateEndState.DayOfWeek != DayOfWeek.Sunday)
                //            {
                //                if (CheckRealDateTime(dateEndState, sang1.ParameterValue, sang2.ParameterValue, chieu1.ParameterValue, chieu2.ParameterValue))
                //                {
                //                    h--;
                //                }
                //                else
                //                {
                //                    dateEndState = dateEndState.AddHours(1);
                //                }
                //            }
                //            else
                //            {
                //                dateEndState = dateEndState.AddDays(1);
                //            }
                //        };
                //    }
                //    #endregion
                //    #region Tính phút
                //    //Set lai gio
                //    if (sang1 != null && sang2 != null && chieu1 != null && chieu2 != null)
                //    {
                //        while (m > 0)
                //        {
                //            dateEndState = dateEndState.AddMinutes(1);
                //            if (!dataNgayNghi.Any(x => x.Ngay.Date == dateEndState.Date)
                //                && dateEndState.DayOfWeek != DayOfWeek.Saturday
                //                && dateEndState.DayOfWeek != DayOfWeek.Sunday)
                //            {
                //                if (CheckRealDateTime(dateEndState, sang1.ParameterValue, sang2.ParameterValue, chieu1.ParameterValue, chieu2.ParameterValue))
                //                {
                //                    m--;
                //                }
                //                else
                //                {
                //                    dateEndState = dateEndState.AddMinutes(1);
                //                }
                //            }
                //            else
                //            {
                //                dateEndState = dateEndState.AddDays(1);
                //            }
                //        };
                //    }
                //    #endregion
                //    if (hs != null)
                //    {
                //        hs.BeginDateState = beginDateState;
                //        hs.EndDateState = dateEndState;
                //        _lcgContext.SaveChanges();
                //    }
                //}
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public bool CheckRealDateTime(DateTime endDate, string s1, string s2, string c1, string c2)
        {
            DateTime ts1 = DateTime.ParseExact(s1, "HH:mm", CultureInfo.InvariantCulture);
            DateTime ts2 = DateTime.ParseExact(s2, "HH:mm", CultureInfo.InvariantCulture);
            DateTime tc1 = DateTime.ParseExact(c1, "HH:mm", CultureInfo.InvariantCulture);
            DateTime tc2 = DateTime.ParseExact(c2, "HH:mm", CultureInfo.InvariantCulture);
            //-1 if <; 0 if =; 1 if >
            int compare_ts1 = TimeSpan.Compare(endDate.TimeOfDay, ts1.TimeOfDay);
            int compare_ts2 = TimeSpan.Compare(endDate.TimeOfDay, ts2.TimeOfDay);
            int compare_tc1 = TimeSpan.Compare(endDate.TimeOfDay, tc1.TimeOfDay);
            int compare_tc2 = TimeSpan.Compare(endDate.TimeOfDay, tc2.TimeOfDay);
            if ((compare_ts1 > -1 && compare_ts2 < 0) || (compare_tc1 > -1 && compare_tc2 < 0))
                return true;
            else
                return false;
        }

        public List<DocumentHistoryViewModel> GetDocumentHistory(Guid documentId)
        {
            try
            {
                List<DocumentHistoryViewModel> data = new List<DocumentHistoryViewModel>();
                //&& x.FromUser != x.ToUser
                var his = _sampleContext.DocumentTransitionHistories.Where(x => x.DocumentId == documentId
                && x.TransitionTime != null && x.Command != "" && x.Command != null).OrderBy(x => x.Order);
                List<Guid> dataHisId = his.Select(x => x.Id).ToList();
                var dataTep = _sampleContext.Ext_DocumentHitory_FileAttachs.Where(x => dataHisId.Contains(x.DocumentHistoryId)).ToList();

                List<Guid?> dataUserFrom = his.Select(x => x.FromUser).ToList();
                List<Guid?> dataUserTo = his.Select(x => x.ToUser).ToList();
                dataUserFrom.AddRange(dataUserTo);
                var dataMem = _michrContext.aspnet_Memberships.Where(x => dataUserFrom.Contains(x.UserId)).ToList();
                foreach (var h in his)
                {
                    DocumentHistoryViewModel item = new DocumentHistoryViewModel();
                    List<FileAttach> dataTepDinhKem = new List<FileAttach>();
                    item.GhiChu = h.Comment;
                    item.HanhDong = h.Command;
                    item.ThoiGian = h.InitialTime;
                    item.ThoiGianDuKien = h.EndTime;
                    item.ThoiGianKetThuc = h.TransitionTime;
                    item.UserIdNguoiXuLy = h.FromUser;
                    item.UserIdNguoiXuLyTiepTheo = h.ToUser;
                    if (h.TransitionTime > h.EndTime)
                        item.TrangThai = 0;//Qua han
                    else
                        item.TrangThai = 1;//Dung han, truoc han
                    var lstTep = dataTep.Where(x => x.DocumentHistoryId == h.Id);
                    foreach (var t in lstTep)
                    {
                        dataTepDinhKem.Add(new FileAttach() { FileName = t.FileName, FilePath = t.FilePath });
                    }
                    item.TepDinhKem = dataTepDinhKem;
                    var nxl = dataMem.FirstOrDefault(x => x.UserId == h.FromUser);
                    if (nxl != null)
                        item.NguoiXuLy = nxl.FullName;
                    var nxltiep = dataMem.FirstOrDefault(x => x.UserId == h.ToUser);
                    if (nxltiep != null)
                        item.NguoiXuLyTiepTheo = nxltiep.FullName;
                    data.Add(item);
                }
                return data;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public void UpdateFinishHoSo(Guid documentId)
        {
            try
            {
                var doc = _sampleContext.Documents.FirstOrDefault(x => x.Id == documentId);
                doc.IsFinished = true;
                _sampleContext.SaveChanges();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public void UpdateFinishHoSoNV(Guid documentId)
        {
            try
            {
                //var doc = _michrContext.lcg_HoSos.FirstOrDefault(x => x.HoSoId == documentId);
                //if (doc != null)
                //{
                //    doc.IsFinished = 1;
                //    _michrContext.SaveChanges();
                //}
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public List<string> GetDocumentAuthor(Guid documentId)
        {
            try
            {
                List<string> dataReturn = new List<string>();
                var data = _sampleContext.Documents.FirstOrDefault(x => x.Id == documentId);
                if (data != null)
                {
                    dataReturn.Add(data.AuthorId.ToString());
                }
                return dataReturn;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public bool CheckDocumentAuthor(Guid documentId, Guid currentUserId)
        {
            try
            {
                var data = _sampleContext.Documents.FirstOrDefault(x => x.Id == documentId && x.AuthorId == currentUserId);
                if (data != null)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public List<Guid> GetDocumentByFinish(List<Guid> documentId, bool? isFinished)
        {
            try
            {
                return _sampleContext.Documents.Where(x => documentId.Contains(x.Id) && x.IsFinished == isFinished).Select(x => x.Id).ToList();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public List<DocumentState> GetDocumentByStateName(List<Guid> documentId, List<string> stateName, bool isContain)
        {
            try
            {
                if (isContain)
                {
                    return _sampleContext.Documents.Where(x => documentId.Contains(x.Id) && stateName.Contains(x.StateName)).Select(x => new DocumentState { DocumentId = x.Id, StateName = x.StateName }).ToList();
                }
                else
                {
                    return _sampleContext.Documents.Where(x => documentId.Contains(x.Id) && !stateName.Contains(x.StateName)).Select(x => new DocumentState { DocumentId = x.Id, StateName = x.StateName }).ToList();
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public List<Guid> GetDocumentInHistoryByUser(List<Guid> documentId, Guid userId)
        {
            try
            {
                return _sampleContext.DocumentTransitionHistories.Where(x => documentId.Contains(x.DocumentId) && x.FromUser == userId).Select(x => x.DocumentId).Distinct().ToList();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public void CheckAndInsertHistoryTimer(Guid CurrentUserId, Guid ToUserId, string nameTimer, Guid documentId, string currentState, string nextState)
        {

            try
            {
                var doc = _sampleContext.Documents.FirstOrDefault(x => x.Id == documentId);
                doc.Comment = "Timer tự động chuyển";
                var historyItem = new DocumentTransitionHistory
                {
                    Id = Guid.NewGuid(),
                    AllowedToEmployeeNames = string.Empty,
                    DestinationState = nextState,
                    DocumentId = documentId,
                    InitialState = currentState,
                    InitialTime = DateTime.Now,
                    FromUser = CurrentUserId,
                    ToUser = ToUserId,
                    Command = "Timer",
                    TransitionTime = DateTime.Now,
                    EmployeeId = CurrentUserId,
                    Comment = "Quy trình tự động chuyển"
                };
                _sampleContext.DocumentTransitionHistories.Add(historyItem);
                _sampleContext.SaveChanges();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public void UpdateCurrentUserWithTimer(string userNext, string nameTimer, Guid documentId, string currentState, string nextState)
        {
            try
            {
                ///*Update hồ sơ nghiệp vụ*/
                //var hs = _lcgContext.lcg_HoSos.FirstOrDefault(x => x.HoSoId == documentId);
                //if (hs != null)
                //{
                //    Guid curUserId = hs.CurrentUserId;
                //    hs.CurrentUserId = new Guid(userNext);
                //    hs.PreUserId = curUserId;
                //    _lcgContext.lcg_HoSos.Update(hs);
                //    _lcgContext.SaveChanges();

                //    /*Update document*/
                //    var doc = _sampleContext.Documents.FirstOrDefault(x => x.Id == documentId);
                //    doc.Comment = "Timer " + nameTimer;
                //    _sampleContext.SaveChanges();

                //    /*Insert history*/
                //    CheckAndInsertHistoryTimer(curUserId, new Guid(userNext), nameTimer, documentId, currentState, nextState);
                //}
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public void PushNotificationForAll(Guid documentId, string userId, string exclude)
        {
            try
            {
                //string API_NV_URL = Common.GetConfig("Appsetting:API_NV_URL");
                //if (!string.IsNullOrEmpty(userId))
                //{
                //    var uInfo = _michrContext.aspnet_Memberships.FirstOrDefault(x => x.UserId == new Guid(userId));
                //    if (uInfo != null)
                //    {
                //        List<Guid> dataUserExclude = new List<Guid>();
                //        if (!string.IsNullOrEmpty(exclude))//Exclude role
                //        {
                //            var roleExclude = _michrContext.aspnet_Roles.Where(x => exclude.Contains(x.RoleCode));
                //            if (roleExclude != null)
                //            {
                //                List<Guid> dataRoleIdExclude = new List<Guid>();
                //                dataRoleIdExclude = roleExclude.Select(x => x.RoleId).ToList();
                //                dataUserExclude = _michrContext.aspnet_UsersInRoles.Where(x => dataRoleIdExclude.Contains(x.RoleId)).Select(x => x.UserId).ToList();
                //            }
                //        }
                //        var hs = _michrContext.lcg_HoSos.FirstOrDefault(x => x.HoSoId == documentId);
                //        List<Guid?> dataUseInScheme = _sampleContext.DocumentTransitionHistories.Where(x => x.DocumentId == documentId).Select(x => x.FromUser).ToList();
                //        using (HttpClient _client = new HttpClient())
                //        {
                //            string url = API_NV_URL + "/api/notifications/pushmess";
                //            var resUri = new Uri(url);
                //            PushNotificationModelForUser data = new PushNotificationModelForUser();
                //            data.HoSoId = documentId;
                //            string mess = "";
                //            string messHtml = mess;
                //            if (string.IsNullOrEmpty(mess))
                //            {
                //                mess = uInfo.FullName + " đã duyệt hồ sơ " + hs.Name;
                //                messHtml = uInfo.FullName + " đã duyệt hồ sơ <b>" + hs.Name + "</b>";
                //            }
                //            data.ContentMess = messHtml;
                //            data.ContentMessMobile = mess;
                //            data.DataUserReviceMess = new List<Guid>();
                //            foreach (var id in dataUseInScheme)
                //            {
                //                if (id != null && !dataUserExclude.Any(x => x == id))
                //                    data.DataUserReviceMess.Add(id.Value);
                //            }
                //            data.TenNguoiGui = uInfo.FullName;
                //            data.Type = "WF";
                //            var jsonString = JsonConvert.SerializeObject(data);
                //            var content = new StringContent(jsonString, Encoding.UTF8);
                //            content.Headers.ContentType.MediaType = "application/json";
                //            content.Headers.ContentType.CharSet = "UTF-8";
                //            var response = _client.PostAsync(resUri, content).Result;
                //            if (response.IsSuccessStatusCode)
                //            {
                //                var messageContent = response.Content.ReadAsStringAsync().Result;
                //                var statusCode = response.StatusCode;
                //                var result = JsonConvert.DeserializeObject<Response<bool>>(messageContent);
                //            }
                //        }
                //    }
                //}
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public List<DocumentTransitionHistoryModel> GetAllHistoryForBaoCao(List<Guid> dataHoSoId)
        {
            try
            {
                List<DocumentTransitionHistoryModel> data = new List<DocumentTransitionHistoryModel>();
                if (dataHoSoId != null)
                {
                    data = (from x in _sampleContext.DocumentTransitionHistories
                            where x.FromUser != null && x.TransitionTime != null && dataHoSoId.Contains(x.DocumentId)
                            select new DocumentTransitionHistoryModel()
                            {
                                AllowedToEmployeeNames = x.AllowedToEmployeeNames,
                                Command = x.Command,
                                Comment = x.Comment,
                                DestinationState = x.DestinationState,
                                DocumentId = x.DocumentId,
                                EmployeeId = x.EmployeeId,
                                EndTime = x.EndTime,
                                FromUser = x.FromUser,
                                Id = x.Id,
                                InitialState = x.InitialState,
                                InitialTime = x.InitialTime,
                                Order = x.Order,
                                ToUser = x.ToUser,
                                TransitionTime = x.TransitionTime
                            }).ToList();
                }
                return data;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public DocumentStateModel GetStateNameOfDocument(Guid documentId)
        {
            try
            {
                var data = _sampleContext.Documents.FirstOrDefault(x => x.Id == documentId);
                if (data != null)
                {
                    return new DocumentStateModel()
                    {
                        StateName = data.StateName,
                        IsFinished = data.IsFinished
                    };
                }
                return new DocumentStateModel()
                {
                    StateName = string.Empty,
                    IsFinished = null
                };
            }
            catch (Exception)
            {
                return new DocumentStateModel()
                {
                    StateName = string.Empty,
                    IsFinished = null
                };
            }
        }
    }
}
