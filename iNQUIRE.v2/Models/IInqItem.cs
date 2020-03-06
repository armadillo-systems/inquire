using System;
using System.Collections.Generic;
using System.Xml.Linq;
using iNQUIRE.Helper;

namespace iNQUIRE.Models
{
    public interface IInqItem
    {
        #region properties
        // fields here MUST match \\armserv\solr\multicore\core1\conf\schema.xml,
        // else when updating a Solr record (when creating a jp2) an error will be thrown, causing jp2 creation to fail
        string ID { get; set; }
        string Title { get; set; }
        string Author { get; set; }
        string Description { get; set; }
        string File { get; set; } // has a set accessor as solr record needs to be updated if jpeg converted to jpeg2000
        string DateStr { get; set; }
        ICollection<string> ParentNodes { get; set; }
        ICollection<string> ChildNodes { get; set; }
        ImageMetadata ImageMetadata { get; set; }
        #endregion

        string ViewItemUrl { get; }

        #region methods
        string ExportRis(string lang_id);
        XElement ExportXml(string lang_id);
        string ExportHtml(string content_id, string lang_id);
        string ExportHtmlImage(string content_id);
        string ExportHtmlFields(string lang_id);
        #endregion

        //string Title { get; set; }
        //DateTime Date { get; set; }
        //int Year { get; set; }
        //int YearStart { get; set; }
        //int YearEnd { get; set; }

        ////int Width { get; set; }
        ////int Height { get; set; }
        //string Description { get; set; }
        //string Author { get; set; }
        //string Type { get; set; }
        //string Subject { get; set; }
        //string Source { get; set; }
        //string Collection { get; set; }
        //ICollection<string> Categories { get; set; }
        //DateTime Timestamp { get; set; }
    }
}