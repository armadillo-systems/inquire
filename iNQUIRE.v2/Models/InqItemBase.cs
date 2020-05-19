using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Text;
using System.Xml.Linq;
using iNQUIRE.Helper;

namespace iNQUIRE.Models
{
    public abstract class InqItemBase : IInqItem
    {
        public static string ViewItemBaseUri { get; set; }

        public abstract string ID { get; set; }
        public abstract string Title { get; set; }
        public abstract string Author { get; set; }
        public abstract string Description { get; set; }
        public abstract string Collection { get; set; }
        public abstract string File { get; set; } // has a set accessor as solr record needs to be updated if jpeg converted to jpeg2000
        public abstract string DateStr { get; set; }
        public abstract ICollection<string> ParentNodes { get; set; }
        public abstract ICollection<string> ChildNodes { get; set; }

        public ImageMetadata ImageMetadata { get; set; }

        public abstract int Width { get; set; }
        public abstract int Height { get; set; }

        public abstract string ExportRis(string lang_id = null);
        public abstract XElement ExportXml(string lang_id = null);
        public abstract string ExportHtmlFields(string lang_id = null);

        public virtual double AspectRatio
        {
            get
            {
                return (double)Width / Height;
            }
        }

        public virtual string ExportHtml(string content_id, string lang_id = null)
        {
            var html = new StringBuilder();

            if (!string.IsNullOrEmpty(content_id))
                html.Append(ExportHtmlImage(content_id));

            html.Append(ExportHtmlFields(lang_id));
            return html.ToString();
        }

        public virtual string ExportHtmlImage(string content_id)
        {
            var html = new StringBuilder(String.Format("<td><a href=\"{2}\" target=\"_blank\"><img style=\"max-width: 600px;\" alt=\"{1}\" src=\"cid:{0}\" /></a></td>", content_id, Title, ViewItemUrl));
            html.Append(String.Format("<td valign=\"top\" align=\"left\"><h2>{0}</h2>", Title));
            return html.ToString();
        }

        public string ViewItemUrl
        {
            get { return string.Format("{0}{1}", ViewItemBaseUri, ID); }
        }

        public static string Concatenate(ICollection<string> strings)
        {
            if (strings == null)
                return null;

            var sb = new StringBuilder();
            var x = 0;

            foreach (string s in strings)
            {
                if (x > 0)
                    sb.Append(Helpers.SolrHelper.FieldConcatenationString);
                sb.Append(s.Trim());
                x++;
            }

            return sb.ToString();
        }
    }
}