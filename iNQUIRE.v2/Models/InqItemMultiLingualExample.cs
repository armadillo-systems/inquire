using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using SolrNet.Attributes;


namespace iNQUIRE.Models
{
    public class InqItemMultiLingualExample : InqItemImageMetadataWidthAndHeightBase, IInqItemMultiLingual
    {
        #region IInqItem properties implementation
        public override string Title
        {
            get
            { return _title; }
            set
            { _title = value; }
        }

        public override string File
        {
            get { return _file; }
            set { _file = value; }
        }

        public override string Collection
        {
            get
            {
                return (CollectionHistory != null && CollectionHistory.Count() > 0) ? CollectionHistory.ToList()[0] : null;
            }
            set { } }

        public override string Author
        {
            get
            {
                return (_attribution != null && _attribution.Count > 0) ?  _attribution.ToList()[0] :  "";
            }
            set { _attribution.ToList().Insert(0, value); }
        }

        #endregion

        #region Properties specific to your Solr data
        public ICollection<string> Attribution
        {
            get
            {
                return _attribution;
            }
            set { }
        }

        private Dictionary<string, List<string>> _mlCategory
        {
            get
            {
                if (_dcategory == null)
                    _dcategory = MakeMultilingualStringListDictionary("Category");
                return _dcategory;
            }
            set { }
        }

        public List<string> Category
        {
            get;
            set;
        }

        private Dictionary<string, List<string>> _mlKeywords
        {
            get
            {
                if (_dkeywords == null)
                    _dkeywords = MakeMultilingualStringListDictionary("Keywords");

                return _dkeywords;
            }
            set { }
        }

        public List<string> Keywords
        {
            get;
            set;
        }

        private Dictionary<string, List<string>> _mlMedium
        {
            get
            {
                if (_dmedium == null)
                    _dmedium = ParseMultilingualStringListToDictionary(_medium);

                return _dmedium;
            }
            set { }
        }

        public List<string> Medium
        {
            get;
            set;
        }

        private Dictionary<string, List<string>> _mlSupport
        {
            get
            {
                if (_dsupport == null)
                    _dsupport = ParseMultilingualStringListToDictionary(_support);

                return _dsupport;
            }
            set { }
        }

        public List<string> Support
        {
            get;
            set;
        }

        public int Year
        {
            get { return _year; }
            set { }
        }

        private Dictionary<string, List<string>> _mlDateStrList
        {
            get
            {
                if (_ddate_str == null)
                    _ddate_str = ParseMultilingualStringListToDictionary(_date_str);
                return _ddate_str;
            }
            set { }
        }

        public List<string> DateStrList
        {
            get; set;
        }

        public override string DateStr
        {
            get { return (DateStrList != null && DateStrList.Count() > 0) ? DateStrList[0] : null; }
            set { }
        }
        #endregion

        #region data fields from solr mapped to .NET properties, any field that is backed by a Dictionary is multi lingual
        [SolrUniqueKey("id")]
        public override string ID { get; set; }

        [SolrField("title")]
        public string _title { internal get; set; }

        [SolrField("collection_history")]
        public ICollection<string> CollectionHistory { get; set; }

        [SolrField("attribution")]
        public ICollection<string> _attribution { internal get; set; }

        // we will use the multiple fields which have each language to build a dictionary, just like the  normal multilingual fields
        private Dictionary<string, List<string>> _dcategory;

        [SolrField("category_nl")]
        public ICollection<string> _category_nl { internal get; set; }

        [SolrField("category_en")]
        public ICollection<string> _category_en { internal get; set; }

        [SolrField("keywords_nl")]
        public ICollection<string> _keywords_nl { internal get; set; }

        [SolrField("keywords_en")]
        public ICollection<string> _keywords_en { internal get; set; }

        private Dictionary<string, List<string>> _dkeywords;

        [SolrField("medium")]
        public ICollection<string> _medium { internal get; set; }
        private Dictionary<string, List<string>> _dmedium;

        [SolrField("support")]
        public ICollection<string> _support { internal get; set; }
        private Dictionary<string, List<string>> _dsupport;

        [SolrField("date_str")]
        public ICollection<string> _date_str { internal get; set; }
        private Dictionary<string, List<string>> _ddate_str;

        [SolrField("year")]
        public int _year { internal get; set; }

        [SolrField("file")]
        public string _file { internal get; set; }

        // if no obvious SolrField mapping to a property eg Description
        // then return an empty string as this will break less than returning null
        public override string Description { get { return ""; } set { } }


        [SolrField("parent_node")]
        public string ParentNode { get; set; }

        // use this to maintain ParentNodes override as a collection, just make the single result in to a collection
        private ICollection<string> _privateNodes;
        public override ICollection<string> ParentNodes
        {
            get
            {
                if (!String.IsNullOrEmpty(ParentNode))
                {
                    _privateNodes = new List<string>();
                    _privateNodes.Add(ParentNode);
                }
                return _privateNodes;
            }
            set { _privateNodes = value; }
        }

        [SolrField("child_nodes")]
        public override ICollection<string> ChildNodes { get; set; }


        private DateTime _date;
        public DateTime Date
        {
            get
            {
                if (_date == new DateTime())
                {
                    try
                    {
                        _date = new DateTime(_year, 1, 1);
                    }
                    catch (Exception)
                    {
                        Console.WriteLine("Error creating Date from _year");
                    }
                }
                return _date;
            }
            set { _date = value; }
        }
        #endregion


        #region IInqItem implementation of methods

        public override XElement ExportXml(string lang_id)
        {
            return new XElement("item",
                                new XElement("Title", Title),
                                new XElement("Image", String.Format("{0}.jpg", ID)),
                                new XElement("Author", Author),
                                new XElement("Attribution", Concatenate(Attribution)),
                                new XElement("Date", Year),
                                new XElement("Keywords", Concatenate(_mlKeywords[lang_id]))
                                );
        }

        public override string ExportRis(string lang_id)
        {
            var sb = new StringBuilder(System.Environment.NewLine);
            var ris_type = "";
            if (String.IsNullOrEmpty(ris_type))
                ris_type = "GEN";
            sb.AppendLine(String.Format("TY  - {0}", ris_type));
            sb.AppendLine(String.Format("ID  - {0}", ID));
            sb.AppendLine(String.Format("CN  - {0}", ID));
            sb.AppendLine(String.Format("TI  - {0}", Title));
            sb.AppendLine(String.Format("T2  - {0}", Concatenate(_mlKeywords[lang_id])));
            sb.AppendLine(String.Format("UR  - {0}{1}", ViewItemBaseUri, ID));

            return sb.Append("ER  - ").ToString();
        }

        public override string ExportHtmlFields(string lang_id)
        {
            var html = new StringBuilder();

            //if (!String.IsNullOrEmpty(Description))
            //    html.Append(String.Format("<div class=\"desc\">{0}</div>", Description));

            if (!String.IsNullOrEmpty(Author))
                html.Append(String.Format("<div><span class=\"label\">Author:</span> {0}</div>", Author));

            if (!string.IsNullOrEmpty(DateStr))
                html.Append(String.Format("<div><span class=\"label\">Date:</span> {0}</div>", DateStr));

            if (Year > 0)
                html.Append(String.Format("<div><span class=\"label\">Year:</span> {0}</div>", Year));

            if (Category != null && Category.Count() > 0)
                html.Append(String.Format("<div class=\"label\">{0}</div>", Concatenate(Category)));

            if (!string.IsNullOrEmpty(lang_id) && _mlKeywords != null && _mlKeywords[lang_id].Count() > 0)
                html.Append(String.Format("<div><span class=\"label\">Keywords:</span> {0}</div>", Concatenate(_mlKeywords[lang_id])));

            if (!String.IsNullOrEmpty(ViewItemUrl))
                html.Append(String.Format("<div><span class=\"label\">URL:</span> <a href=\"{0}\" target=\"_blank\">{0}</a></div>", ViewItemUrl));

            return html.ToString();
        }
        #endregion
    }
}