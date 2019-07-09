using System;
using System.Collections.Generic;
using System.Xml.Linq;
using System.Text;
using SolrNet.Attributes;
using iNQUIRE.Helper;

namespace iNQUIRE.Models
{
    public class InqItemBodIIIF : InqItemIIIFBase
    {
        [SolrUniqueKey("ObjectLocaleID")]
        public override string ID { get; set; }

        [SolrField("Title")]
        public override string Title { get; set; }

        private DateTime _date;
        public DateTime Date
        {
            get
            {
                if (_date == new DateTime())
                {
                    if (!String.IsNullOrEmpty(DateStr))
                    {
                        try
                        {
                            _date = new DateTime(Convert.ToInt32(DateStr), 1, 1);
                        }
                        catch (Exception)
                        {
                            Console.WriteLine("Error creating Date from IntItemBodIIIF.DateStr");
                        }
                    }
                }
                return _date;
            }
            set { _date = value; }
        }

        [SolrField("DateStr")]
        public override string DateStr { get; set; }

        [SolrField("Date")]
        public Int32 DateInt { get; set; }

        public override ImageMetadata ImageMetadata { get; set; }

        [SolrField("ParentNodes")]
        public string ParentNode { get; set; }

        private ICollection<string> _privateNodes;
        public override ICollection<string> ParentNodes {
            get
            {
                if(!String.IsNullOrEmpty(ParentNode))
                {
                    _privateNodes = new List<string>();
                    _privateNodes.Add(ParentNode);
                }
                return _privateNodes;
            }
            set { _privateNodes = value; }
        }

        [SolrField("ChildNodes")]
        public override ICollection<string> ChildNodes { get; set; }

        public override string File { get; set; }
        public override string Author { get; set; }
        //public override string Description { get; set; }

        public override XElement ExportXml()
        {
            return new XElement("item",
                                new XElement("Title", Title),
                                new XElement("Image", String.Format("{0}.jpg", ID)),
                                new XElement("Description", Description),
                                new XElement("Creator", Author),
                                new XElement("Date", DateStr)
                                );
        }

        public override string ExportRis()
        {
            var sb = new StringBuilder(System.Environment.NewLine);
            sb.AppendLine("TY  - BOOK");
            sb.AppendLine(String.Format("AU  - {0}", Author));
            sb.AppendLine(String.Format("TI  - {0}", Title));
            // sb.AppendLine(String.Format("N1  - {0}", Concatenate(MultiValueTest)));
            sb.AppendLine(String.Format("PY  - {0}", DateStr));
            sb.AppendLine(String.Format("UR  - {0}", ViewItemUrl));
            return sb.Append("ER  - ").ToString();
        }

        public override string ExportHtmlFields(string content_id)
        {
            var html = new StringBuilder();

            if (!String.IsNullOrEmpty(Description))
                html.Append(String.Format("<div class=\"desc\">{0}</div>", Description));

            if (!String.IsNullOrEmpty(Author))
                html.Append(String.Format("<div><span class=\"label\">Author:</span> {0}</div>", Author));

            if (!String.IsNullOrEmpty(Collection))
                html.Append(String.Format("<div><span class=\"label\">Collection:</span> {0}</div>", Collection));

            if (!String.IsNullOrEmpty(DateStr.ToString()))
                html.Append(String.Format("<div><span class=\"label\">Date:</span> {0}</div>", DateStr));

            if (!String.IsNullOrEmpty(Source))
                html.Append(String.Format("<div><span class=\"label\">Source:</span> {0}</div>", Source));


            if (!String.IsNullOrEmpty(Type))
                html.Append(String.Format("<div><span class=\"label\">Type:</span> {0}</div>", Type));

            if (!String.IsNullOrEmpty(ViewItemUrl))
                html.Append(String.Format("<div><span class=\"label\">URL:</span> <a href=\"{0}\" target=\"_blank\">{0}</a></div>", ViewItemUrl));

            return html.ToString();
        }

        [SolrField("Attribution")]
        public string Attribution { get; set; }

        [SolrField("Description")]
        public override string Description { get; set; }

        [SolrField("See-Also")]
        public string SeeAlso { get; set; }

        [SolrField("Source")]
        public string Source { get; set; }

        [SolrField("Shelfmark")]
        public string Shelfmark { get; set; }

        [SolrField("Type")]
        public string Type { get; set; }

        [SolrField("Accessrights")]
        public string Accessrights { get; set; }

        [SolrField("Collection")]
        public string Collection { get; set; }

        [SolrField("Identifier")]
        public string Identifier { get; set; }

        [SolrField("Page")]
        public string Page { get; set; }
    }
}