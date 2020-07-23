using System;
using System.Collections.Generic;

namespace WF.Business.Model
{
    public class Ext_DocumentHitory_FileAttach
    {
        public Guid Id { get; set; }
        public Guid DocumentHistoryId { get; set; }
        public string FileName { get; set; }
        public string FilePath { get; set; }
    }
}