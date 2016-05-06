using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace iNQUIRE.Models
{
    public class NoteInfo
    {
        public Guid NoteID { get; set; }
        public string UserID { get; set; }
        public string UserName { get; set; }
        public string Text { get; set; }
        public string ObjectID { get; set; }
        // public string ObjectTitle { get; set; }
        public DateTime? LastEdited { get; set; }
        public bool PublicNote { get; set; }
    }
}