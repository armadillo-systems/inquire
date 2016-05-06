using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace iNQUIRE.Models
{
    public class ExportItem
    {
        public IInqItem Item { get; set; }
        public string ImageUri { get; set; }

        public ExportItem(IInqItem item, string image_uri)
        {
            Item = item;
            ImageUri = image_uri;
        }
    }
}