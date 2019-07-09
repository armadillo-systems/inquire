﻿using System;
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

        [SolrField("Width")]
        public Int32 Width { get; set; }

        [SolrField("Height")]
        public Int32 Height { get; set; }

        public double AspectRatio
        {
            get
            {
                return (double)Width / Height;
            }
        }

        public virtual string GetImageUri(int max_w, int max_h)
        {
            if (this.Width > this.Height)
                return string.Format("{0}/full/{1},/0/default.jpg", this.IIIFImageRoot, max_w);
            else
                return string.Format("{0}/full/,{1}/0/default.jpg", this.IIIFImageRoot, max_h);
        }
    }
}