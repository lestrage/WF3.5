using System;
using System.Collections.Generic;

namespace WF.Business.Model
{
    public class Document
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public int? Number { get; set; }
        public string Comment { get; set; }
        public Guid AuthorId { get; set; }
        public Guid? ManagerId { get; set; }
        public decimal Sum { get; set; }
        public string State { get; set; }
        public string StateName { get; set; }
        public aspnet_Users Author { get; set;}
        public aspnet_Users Manager { get; set; }
        public string SchemeName { get; set; }
        public bool? IsFinished { get; set; }
    }    
}
