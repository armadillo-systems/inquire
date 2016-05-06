using System;

namespace iNQUIRE.Helper
{
    public class ImageMetadata
    {
        public String Identifier { get; set; }
        public String Imagefile { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public int DwtLevels { get; set; }
        public int Levels { get; set; }
        public int CompositingLayerCount { get; set; }
    }
}