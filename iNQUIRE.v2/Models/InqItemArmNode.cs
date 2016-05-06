using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Xml.Linq;
using System.Text;
using SolrNet.Attributes;
using iNQUIRE.Helper;
using iNQUIRE.Helpers;

namespace iNQUIRE.Models
{
    public class InqItemArmNode : InqItemBase
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
                            _date = new DateTime(Year, Convert.ToInt32(DateStr.Substring(4, 2)), Convert.ToInt32(DateStr.Substring(6, 2)));
                        }
                        catch (Exception)
                        {
                            Console.WriteLine("Error creating Date from InqItemArmNode.DateStr");
                        }
                    }
                }
                return _date;
            }
            set { _date = value; }
        }

        [SolrField("Date")]
        public override string DateStr { get; set; }

        [SolrField("DateStartYear")]
        public int YearStart { get; set; }

        [SolrField("DateStartMonth")]
        public int MonthStart { get; set; }

        [SolrField("DateStartDay")]
        public int DayStart { get; set; }

        [SolrField("DateEndYear")]
        public int YearEnd { get; set; }

        public int Year
        {
            get
            {
                try
                {
                    if (String.IsNullOrEmpty(DateStr) == false)
                    {
                        if (DateStr.Length >= 4)
                            return Convert.ToInt32(DateStr.Substring(0, 4));

                        return Convert.ToInt32(DateStr);
                    }

                    return 0;
                }
                catch (Exception)
                {
                    return 0;
                }
            }
            set { throw new NotImplementedException(); }
        }

        [SolrField("MediaFilename")]
        public override string File { get; set; }

        [SolrField("Description")]
        public override string Description { get; set; }

        [SolrField("Author")]
        public override string Author { get; set; }

        [SolrField("Type")]
        public string Type { get; set; }

        [SolrField("Subject")]
        public string Subject { get; set; }

        [SolrField("Source")]
        public string Source { get; set; }

        [SolrField("Collection")]
        public string Collection { get; set; }

        [SolrField("Genre")]
        public string Genre { get; set; }

        [SolrField("Artist")]
        public string Artist { get; set; }

        [SolrField("AssetID")]
        public string AssetID { get; set; }

        [SolrField("AssetTitle")]
        public string AssetTitle { get; set; }

        [SolrField("MediaID")]
        public string MediaID { get; set; }

        [SolrField("MediaTitle")]
        public string MediaTitle { get; set; }

        [SolrField("Notice")]
        public string Notice { get; set; }

        [SolrField("ShelfMark")]
        public string ShelfMark { get; set; }

        [SolrField("ShortTitle")]
        public string ShortTitle { get; set; }

        [SolrField("Position")]
        public int Position { get; set; }

        [SolrField("Categories")]
        public ICollection<string> Categories { get; set; }

        [SolrField("ChildNodes")]
        public override ICollection<string> ChildNodes { get; set; }

        [SolrField("ParentNodes")]
        public override ICollection<string> ParentNodes { get; set; }

        [SolrField("Timestamp")]
        public DateTime Timestamp { get; set; }

        private ICollection<string> _multiValueTest;
        public ICollection<string> MultiValueTest
        {
            get
            {
                if (_multiValueTest == null)
                {
                    _multiValueTest = new List<string>();
                    _multiValueTest.Add("String 1");
                    _multiValueTest.Add("String 2");
                    _multiValueTest.Add("String 3");
                }
                return _multiValueTest;
            }
        }

        /*[SolrField("Width")]
        public int Width { get; set; }

        [SolrField("Height")]
        public int Height { get; set; }*/

        public override ImageMetadata ImageMetadata { get; set; }

        public override XElement ExportXml()
        {
            return new XElement("item",
                                new XElement("Title", Title),
                                new XElement("Image", String.Format("{0}.jpg", ID)),
                                new XElement("Description", Description),
                                new XElement("Author", Author),
                                new XElement("Collection", Collection),
                                new XElement("Date", Date),
                                new XElement("Source", Source),
                                new XElement("Subject", Subject),
                                new XElement("Type", Type),
                                new XElement("Year", Year)
                                );
        }

        public override string ExportRis()
        {
            var sb = new StringBuilder(System.Environment.NewLine);
            sb.AppendLine("TY  - BOOK");
            sb.AppendLine(String.Format("AU  - {0}", Author));
            sb.AppendLine(String.Format("TI  - {0}", Title));
            sb.AppendLine(String.Format("N1  - {0}", Concatenate(MultiValueTest)));
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

            if (!String.IsNullOrEmpty(Date.ToString()))
                html.Append(String.Format("<div><span class=\"label\">Date:</span> {0}</div>", Date));

            if (!String.IsNullOrEmpty(Source))
                html.Append(String.Format("<div><span class=\"label\">Source:</span> {0}</div>", Source));

            if (!String.IsNullOrEmpty(Subject))
                html.Append(String.Format("<div><span class=\"label\">Subject:</span> {0}</div>", Subject));

            if (!String.IsNullOrEmpty(Type))
                html.Append(String.Format("<div><span class=\"label\">Type:</span> {0}</div>", Type));

            if (!String.IsNullOrEmpty(Year.ToString()))
                html.Append(String.Format("<div><span class=\"label\">Year:</span> {0}</div>", Year));

            if (!String.IsNullOrEmpty(ViewItemUrl))
                html.Append(String.Format("<div><span class=\"label\">URL:</span> <a href=\"{0}\" target=\"_blank\">{0}</a></div>", ViewItemUrl));

            return html.ToString();
        }
    }
}