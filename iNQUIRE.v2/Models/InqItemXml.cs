using System;
using System.Collections.Generic;
using System.Xml.Linq;
using SolrNet.Attributes;
using iNQUIRE.Helper;

namespace iNQUIRE.Models
{
    public class InqItemXml : IInqItem
    {
        [SolrUniqueKey("id")]
        public string ID { get; set; }

        [SolrField("title")]
        public string Title { get; set; }

        public ICollection<string> ParentNodes { get; set; }
        public ICollection<string> ChildNodes { get; set; }

        private DateTime _date;
        public DateTime Date
        {
            get
            {
                if (_date == new DateTime())
                {
                    if (Year != 0)
                    {
                        try
                        {
                            _date = new DateTime(Year, 1, 1);
                        }
                        catch (Exception)
                        {
                            Console.WriteLine("Error creating Date from InqItemXml.Year");
                        }
                    }
                }
                return _date;
            }
            set { _date = value; }
        }

        public string DateStr { get { return Date.ToString(); } set { } }

        public string ViewItemUrl
        {
            get { throw new NotImplementedException(); }
        }

        public int YearStart
        {
            get { return Year; }
            set { throw new NotImplementedException(); }
        }

        public int YearEnd
        {
            get { return Year; }
            set { throw new NotImplementedException(); }
        }

        [SolrField("year")]
        public int Year { get; set; }

        [SolrField("file")]
        public string File { get; set; }

        [SolrField("description")]
        public string Description { get; set; }

        [SolrField("author")]
        public string Author { get; set; }

        [SolrField("type")]
        public string Type { get; set; }

        [SolrField("subject")]
        public string Subject { get; set; }

        [SolrField("source")]
        public string Source { get; set; }

        [SolrField("collection")]
        public string Collection { get; set; }

        [SolrField("categories")]
        public ICollection<string> Categories { get; set; }

        [SolrField("timestamp")]
        public DateTime Timestamp { get; set; }

        [SolrField("width")]
        public int Width { get; set; }

        [SolrField("height")]
        public int Height { get; set; }

        public ImageMetadata ImageMetadata { get; set; }

        public XElement ExportXml()
        {
            throw new Exception("Not implemented yet");
        }

        public string ExportRis()
        {
            throw new Exception("Not implemented yet");
        }

        public string ExportHtml(string content_id)
        {
            throw new Exception("Not implemented yet");
        }

        public string ExportHtmlFields(string content_id)
        {
            throw new Exception("Not implemented yet");
        }

        public string ExportHtmlImage(string content_id)
        {
            throw new Exception("Not implemented yet");
        }
    }
}