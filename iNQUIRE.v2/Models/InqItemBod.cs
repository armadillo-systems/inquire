using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Text;
using System.Xml.Linq;
using SolrNet.Attributes;
using iNQUIRE.Helper;
using iNQUIRE.Helpers;

namespace iNQUIRE.Models
{
    public class InqItemBod : InqItemBase
    {
        #region IInqItem properties implementation
        public override ImageMetadata ImageMetadata { get; set; }

        public override string ID { get { return id; } set { } }

        public override string Title
        {
            get
            {
                if (_dcterms_title_display == null)
                    _dcterms_title_display = Concatenate(dcterms_title);
                return _dcterms_title_display;
            }
            set { }
        }

        public override string Author
        {
            get
            {
                if (_dcterms_creator_display == null)
                    _dcterms_creator_display = Concatenate(dcterms_creator);
                return _dcterms_creator_display;
            }

            set { }
        }

        public override string Description
        {
            get
            {
                if (_dcterms_description_display == null)
                    _dcterms_description_display = Concatenate(dcterms_description);
                return _dcterms_description_display;
            }
            set { }
        }

        public override string File
        {
            get { return ox_mediaId; } // id
            set { ox_mediaId = value; }
        }
        #endregion

        #region data fields from solr mapped to .net properties
        [SolrUniqueKey("id")]
        public string id { get; set; }

        [SolrField("dcterms:title")]
        public ICollection<string> dcterms_title { get; set; }
        private string _dcterms_title_display;

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
                            Console.WriteLine("Error creating Date from DateStr");
                        }
                    }
                }
                return _date;
            }
            set { _date = value; }
        }

        [SolrField("dcterms:date")] // Date
        public string dcterms_date { get; set; }
        public override string DateStr { get { return dcterms_date; } set { } }

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

        [SolrField("ox:mediaId")] // MediaFilename
        public string ox_mediaId { get; set; }

        [SolrField("dcterms:description")]
        public ICollection<string> dcterms_description { get; set; }
        private string _dcterms_description_display;

        [SolrField("dcterms:creator")]
        public ICollection<string> dcterms_creator { get; set; }
        private string _dcterms_creator_display;

        [SolrField("dcterms:isPartOf")]
        public override ICollection<string> ParentNodes { get; set; }

        [SolrField("dcterms:hasPart")]
        public override ICollection<string> ChildNodes { get; set; }

        [SolrField("dcterms:type")]
        public ICollection<string> dcterms_type { get; set; }

        [SolrField("dcterms:coverage")]
        public ICollection<string> dcterms_coverage { get; set; }

        [SolrField("dcterms:subject")]
        public ICollection<string> dcterms_subject { get; set; }

        [SolrField("dcterms:identifier")]
        public ICollection<string> dcterms_identifier { get; set; }

        [SolrField("dcterms:relation")]
        public ICollection<string> dcterms_relation { get; set; }

        [SolrField("dcterms:format")]
        public ICollection<string> dcterms_format { get; set; }

        [SolrField("dcterms:rights")]
        public string dcterms_rights { get; set; }

        [SolrField("dcterms:source")]
        public string dcterms_source { get; set; }

        [SolrField("dcterms:isReferencedBy")]
        public ICollection<string> dcterms_isReferencedBy { get; set; }

        [SolrField("dcterms:available")]
        public string dcterms_available { get; set; }

        [SolrField("dcterms:accessRights")]
        public string dcterms_accessRights { get; set; }

        [SolrField("ox:collection")]
        public ICollection<string> ox_collection { get; set; }

        [SolrField("ox:shelfmark")]
        public string ox_shelfmark { get; set; }

        [SolrField("ox:risType")]
        public string ox_risType { get; set; }

        [SolrField("ox:uuid")]
        public string ox_uuid { get; set; }

        [SolrField("ox:folio")]
        public string ox_folio { get; set; }

        [SolrField("ox:sort")]
        public int ox_sort { get; set; }

        [SolrField("ox:displayLanguage")]
        public ICollection<string> ox_displayLanguage { get; set; }

        [SolrField("ox:incipit")]
        public string ox_incipit { get; set; }

        [SolrField("dcterms:abstract")]
        public string dcterms_abstract { get; set; }

        [SolrField("dcterms:contributor")]
        public ICollection<string> dcterms_contributor { get; set; }

        [SolrField("dcterms:alternative")]
        public ICollection<string> dcterms_alternative { get; set; }

        [SolrField("dcterms:language")]
        public ICollection<string> dcterms_language { get; set; }

        [SolrField("dcterms:publisher")]
        public ICollection<string> dcterms_publisher { get; set; }
        #endregion


        #region IInqItem implementation of methods

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
            var ris_type = ox_risType;
            if (String.IsNullOrEmpty(ox_risType))
                ris_type = "GEN";
            sb.AppendLine(String.Format("TY  - {0}", ris_type));
            sb.AppendLine(String.Format("ID  - {0}", id));
            sb.AppendLine(String.Format("CN  - {0}", ox_shelfmark));
            sb.AppendLine(String.Format("TI  - {0}", Concatenate(dcterms_title)));
            sb.AppendLine(String.Format("T2  - {0}", Concatenate(dcterms_alternative)));

            if (dcterms_creator != null)
            {
                foreach (var c in dcterms_creator)
                    sb.AppendLine(String.Format("AU  - {0}", c));
            }

            sb.AppendLine(String.Format("DA  - {0}", dcterms_date));
            sb.AppendLine(String.Format("M3  - {0}", Concatenate(dcterms_type)));

            if (dcterms_subject != null)
            {
                foreach (var kw in dcterms_subject)
                    sb.AppendLine(String.Format("KW  - {0}", kw));
            }

            if (dcterms_contributor != null)
            {
                foreach (var sa in dcterms_contributor)
                    sb.AppendLine(String.Format("A2  - {0}", sa));
            }

            sb.AppendLine(String.Format("AB  - {0}", dcterms_abstract));
            sb.AppendLine(String.Format("LA  - {0}", Concatenate(dcterms_language)));
            sb.AppendLine(String.Format("N1  - {0}", Concatenate(dcterms_description)));
            sb.AppendLine(String.Format("PB  - {0}", Concatenate(dcterms_publisher)));
            sb.AppendLine(String.Format("UR  - {0}{1}", ViewItemBaseUri, id));

            return sb.Append("ER  - ").ToString();
        }

        public override string ExportHtmlFields(string content_id)
        {
            var html = new StringBuilder();

            if (!String.IsNullOrEmpty(Description))
                html.Append(String.Format("<div class=\"desc\">{0}</div>", Description));

            if (!String.IsNullOrEmpty(Author))
                html.Append(String.Format("<div><span class=\"label\">Author:</span> {0}</div>", Author));

            if (!String.IsNullOrEmpty(Date.ToString()))
                html.Append(String.Format("<div><span class=\"label\">Date:</span> {0}</div>", DateStr));

            if (!String.IsNullOrEmpty(ViewItemUrl))
                html.Append(String.Format("<div><span class=\"label\">URL:</span> <a href=\"{0}\" target=\"_blank\">{0}</a></div>", ViewItemUrl));

            return html.ToString();
        }
        #endregion
    }
}