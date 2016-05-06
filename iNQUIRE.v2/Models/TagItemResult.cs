using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace iNQUIRE.Models
{
    public class TagItemResult
    {
        public bool Result { get; set; }
        public Guid TagID { get; set; }
        public string Title { get; set; }
        public string Message { get; set; }

        public TagItemResult(bool result, Guid id, string title, string message)
        {
            Result = result;
            TagID = id;
            Title = title;
            Message = message;
        }
    }
}