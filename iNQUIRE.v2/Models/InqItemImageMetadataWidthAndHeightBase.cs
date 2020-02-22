using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace iNQUIRE.Models
{
    public abstract class InqItemImageMetadataWidthAndHeightBase : InqItemBase
    {
        public override int Width
        {
            get { return ImageMetadata.Width; }
            set { ImageMetadata.Width = value; }
        }

        public override int Height
        {
            get { return ImageMetadata.Height; }
            set { ImageMetadata.Height = value; }
        }
    }
}