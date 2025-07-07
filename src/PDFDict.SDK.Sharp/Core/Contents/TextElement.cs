using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PDFDict.SDK.Sharp.Core.Contents
{
    public class TextElement : PageElement
    {
        private double _deltaBaseline = 0.1;
        private double _deltaDistanceRatio = 1.5;

        private double _baselineX = double.MinValue;
        private double _baselineY = double.MinValue;

        private GraphicsState _gState;
        private StringBuilder _text;
        private Dictionary<int, string> _markedContentDict = new Dictionary<int, string>();

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

        public Dictionary<int, string> GetMarkedContentDict()
        {
            return _markedContentDict;
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

        public bool TryAppendText(string text, double originX, double originY, RectangleF bbox, GraphicsState gState, int markedContentID = -1)
        {
            if (_text.Length == 0)
            {
                BBox = bbox;
                _text.Append(text);
                _baselineX = originX;
                _baselineY = originY;
                _gState = gState;

                if (markedContentID != -1)
                {
                    _markedContentDict[markedContentID] = text;
                }
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

            if (markedContentID != -1)
            {
                _markedContentDict[markedContentID] = _text.ToString();
            }

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

        public static bool IsSpaceBetween(TextElement t1, TextElement t2)
        {
            if (t1 == null || t2 == null || t1.GetText().Length == 0 || t2.GetText().Length == 0)
            {
                return false;
            }

            if (Math.Abs(t1.GetBaselineY() - t2.GetBaselineY()) > t1._deltaBaseline)
            {
                return false;
            }

            double distance = Math.Abs(t1.GetBaselineX() + t1.BBox.Width - t2.GetBaselineX());
            double avgCharWidth = t1.AverageCharWidth() / 2;
            if (distance > avgCharWidth * t1._deltaDistanceRatio)
            {
                return true;
            }

            return false;
        }

        public static char[] ListBullets =  {
                '•', '‣', '∙', '●',
                '▪', '▫', '■', '□', '▣',
                '▤', '▥', '▦', '▧', '▨', '▩',
                '→', '⇒', '➤', '▶', '➔', '➢',
                '♦', '◆', '◇', '○',
                '✓', '✔',
                '✦', '★', '✧', '❖', '❑'
            };

        public static string[] ListItemBeginTag = {
                "i.", "ii.", "iii.", "iv.", "v.", "vi.", "vii.", "viii.", "ix.", "x.",
                "1.", "2.", "3.", "4.", "5.", "6.", "7.", "8.", "9.", "10.",
                "1)", "2)", "3)", "4)", "5)", "6)", "7)", "8)", "9)", "10)",
                "1-", "2-", "3-", "4-", "5-",
                "a.", "b.", "c.", "d.", "e.",
                "A.", "B.", "C.", "D.", "E.",
                "a)", "b)", "c)", "d)", "e)",
                "A)", "B)", "C)", "D)", "E)",
            };

        public static bool IsListParagraphBegin(string text)
        {
            if (string.IsNullOrEmpty(text))
            {
                return false;
            }
            text = text.Trim();
            char c = text[0];

            if (ListBullets.Contains(c))
            {
                return true;
            }

            foreach (var tag in ListItemBeginTag)
            {
                if (text.StartsWith(tag))
                {
                    return true;
                }
            }

            return false;
        }

        public bool IsWingdingFont()
        {
            var fontName = _gState?.TextState?.FontFamilyName;
            if (fontName != null && fontName.ToLower().Contains("wingding"))
            {
                return true;
            }

            return false;
        }

        public override bool TryBuildHTMLPiece(out string html)
        {
            html = null;

            if (GetGState() != null)
            {
                string css = GraphicsStateToCssConverter.Convert(GetGState());
                if (string.IsNullOrWhiteSpace(css))
                {
                    return false;
                }

                html = $"<span style=\"{css}\">{GetText()}</span>";
                return true;
            }

            return false;
        }

        public static class GraphicsStateToCssConverter
        {
            public static string Convert(GraphicsState g)
            {
                if (g == null || g.TextState == null)
                {
                    return string.Empty;
                }

                var sb = new StringBuilder();
                if (g.NonStrokingColor != null)
                {
                    var color = ConvertColor(g.NonStrokingColor);
                    if (color != null)
                    {
                        sb.Append($"color: {color};");
                    }
                }

                //if (g.TextState.FontSize > 0)
                //{
                //    sb.Append($"font-size: {g.TextState.FontSize}pt;");
                //}

                //if (g.TextState.FontWeight >= 600)
                //{
                //    sb.Append("font-weight: bold;");
                //}

                //if (Math.Abs(g.TextState.FontItalicAngle) > 0.1)
                //{
                //    sb.Append("font-style: italic;");
                //}

                return sb.ToString();
            }

            private static string ConvertColor(ColorState color)
            {
                if (color?.Components != null && color?.Components.Length == 4)
                {
                    int r = (int)color.Components[0];
                    int g = (int)color.Components[1];
                    int b = (int)color.Components[2];
                    return $"rgb({r},{g},{b})";
                }

                return null;
            }
        }
    }
}
