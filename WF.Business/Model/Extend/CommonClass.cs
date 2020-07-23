using System;
using System.Collections.Generic;
using System.Text;

namespace WF.Business.Model
{
    public class InputCommand
    {
        public Guid DocumentId { get; set; }
        public string UserId { get; set; }
        public string NextUserId { get; set; }
        public string CommandName { get; set; }
        public string StateNameToSet { get; set; }
        public string Comment { get; set; }
        public List<FileAttach> DataFileAttach { get; set; }
    }

    public class FileAttach
    {
        public string FileName { get; set; }
        public string FilePath { get; set; }
    }

    public class DocumentHistoryViewModel
    {
        public string NguoiXuLy { get; set; }
        public Guid? UserIdNguoiXuLy { get; set; }
        public string NguoiXuLyTiepTheo { get; set; }
        public Guid? UserIdNguoiXuLyTiepTheo { get; set; }
        public string HanhDong { get; set; }
        public DateTime? ThoiGian { get; set; }
        public DateTime? ThoiGianDuKien { get; set; }
        public DateTime? ThoiGianKetThuc { get; set; }
        public string GhiChu { get; set; }
        public int TrangThai { get; set; }
        public List<FileAttach> TepDinhKem { get; set; }
    }

    public class PushNotificationModelForUser
    {
        public Guid HoSoId { get; set; }
        public string TenNguoiGui { get; set; }
        public string ContentMessMobile { get; set; }
        public string ContentMess { get; set; }
        public List<Guid> DataUserReviceMess { get; set; }
        public string Type { get; set; }
    }
}
