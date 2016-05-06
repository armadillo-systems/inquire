using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace iNQUIRE.Models
{
    public class EmailExport
    {
        public string EmailTo { get; set; }
        public List<ExportItem> Items { get; set; }
        public DateTime Created { get; set; }
        public string ImageNotFoundUri { get; set; }
        public string Message { get; set; }

        public EmailExport(string to, string message, List<ExportItem> items, string image_not_found_uri)
        {
            EmailTo = to;
            Items = items;
            Created = DateTime.Now;
            ImageNotFoundUri = image_not_found_uri;
            Message = message;
        }
    }
}