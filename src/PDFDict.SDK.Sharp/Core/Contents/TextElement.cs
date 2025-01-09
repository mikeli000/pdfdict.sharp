using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PDFDict.SDK.Sharp.Core.Contents
{
    public class TextElement: PageElement
    {
        private double _deltaBaseline = 0.01;
        private double _deltaDistanceRatio = 1.5;

        private double _baselineX = double.MinValue;
        private double _baselineY = double.MinValue;
        
        private GraphicsState _gState;
        private StringBuilder _text;

        public TextElement() : base(ElementType.Text, RectangleF.Empty)
        {
            _text = new StringBuilder();
        }

        public double GetBaselineX()
        {
            return _baselineX;
        }

        public double GetBaselineY()
        {
            return _baselineY;
        }

        public GraphicsState GetGState()
        {
            return _gState;
        }

        public string GetText()
        {
            return _text.ToString();
        }

        public bool TryAppendChar(char c, double originX, double originY, RectangleF bbox, GraphicsState gState)
        {
            if (_text.Length == 0)
            {
                BBox = bbox;
                _text.Append(c);
                _baselineX = originX;
                _baselineY = originY;
                _gState = gState;

                return true;
            }

            if (!OnBaseline(originX, originY))
            {
                return false;
            }

            if (!_gState.Equals(gState))
            {
                return false;
            }

            double aw = AverageCharWidth();
            if (Distance(originX, originY) > aw * _deltaDistanceRatio)
            {
                return false;
            }

            _text.Append(c);
            BBox = RectangleF.Union(BBox, bbox);

            return true;
        }

        public bool TryAppendText(string text, double originX, double originY, RectangleF bbox, GraphicsState gState)
        {
            if (_text.Length == 0)
            {
                BBox = bbox;
                _text.Append(text);
                _baselineX = originX;
                _baselineY = originY;
                _gState = gState;

                return true;
            }

            if (!OnBaseline(originX, originY))
            {
                return false;
            }

            if (!_gState.Equals(gState))
            {
                return false;
            }

            double space = gState.TextState.SpaceWidth;
            if (Distance(originX, originY) > space * _deltaDistanceRatio)
            {
                return false;
            }

            _text.Append(text);
            BBox = RectangleF.Union(BBox, bbox);

            return true;
        }

        private bool OnBaseline(double ox, double oy, bool horizontal = true)
        {
            if (horizontal)
            {
                if (Math.Abs(oy - _baselineY) < _deltaBaseline)
                {
                    return true;
                }
            }
            else
            {
                if (Math.Abs(ox - _baselineX) < _deltaBaseline)
                {
                    return true;
                }
            }
            
            return false;
        }

        private double Distance(double ox, double oy, bool horizontal = true)
        {
            if (horizontal)
            {
                return Math.Abs(ox - (_baselineX + BBox.Width));
            }
            else
            {
                return Math.Abs(oy - (_baselineY + BBox.Height));
            }
        }

        private double AverageCharWidth(bool horizontal = true)
        {
            if (horizontal)
            {
                return BBox.Width / _text.Length;
            }
            else
            {
                return BBox.Height / _text.Length;
            }
        }

        public override string ToString()
        {
            return $"Text: {GetText()}, BBox: {BBox}";
        }
    }
}
