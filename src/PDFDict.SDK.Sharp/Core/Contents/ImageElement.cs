using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PDFDict.SDK.Sharp.Core.Contents
{
    public class ImageElement: PageElement
    {
        public string ImagePath { get; private set; }

        public ImageElement(string imagePath, RectangleF bbox): base(ElementType.Image, bbox)
        {
            ImagePath = imagePath;
        }

        public override string ToString()
        {
            return $"Image: {ImagePath}, BBox: {BBox}";
        }
    }
}
