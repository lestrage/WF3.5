using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace WF.Business
{
    public class DocumentByFinish
    {
        public List<Guid> DataDocumentId { get; set; }
        public bool? IsFinish { get; set; }
        public Guid? UserId { get; set; }
    }

    public class DocumentByStateName
    {
        public List<Guid> DataDocumentId { get; set; }
        public List<string> DataStateName { get; set; }
        public bool IsContain { get; set; }
    }

    public class DocumentState
    {
        public Guid DocumentId { get; set; }
        public string StateName { get; set; }
    }

    public class HistoryAllForBaoCao
    {
        public List<Guid> dataHoSoId { get; set; }
    }
}
