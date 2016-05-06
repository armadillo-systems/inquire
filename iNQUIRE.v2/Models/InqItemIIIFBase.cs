using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using SolrNet.Attributes;

namespace iNQUIRE.Models
{
    public abstract class InqItemIIIFBase : InqItemBase
    {
        // can use following to check for derived IIIF classes: if (r.GetType().IsSubclassOf(typeof(InqItemIIIFBase)))

        [SolrField("IIIF-Manifest")]
        public string IIIFManifest { get; set; }

        [SolrField("IIIFSource")]
        public string IIIFSource { get; set; }

        [SolrField("IIIFImageRoot")]
        public string IIIFImageRoot { get; set; }
    }
}