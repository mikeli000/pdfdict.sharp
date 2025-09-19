using System.Data.SqlTypes;
using System.Drawing;
using System.Text;

namespace PDFDict.SDK.Sharp.Core.Contents
{
    public class TextElement : PageElement
    {

        public static IDictionary<char, char> CharToSup = new Dictionary<char, char>
        {
            { '0', '⁰' }, { '1', '¹' }, { '2', '²' }, { '3', '³' }, { '4', '⁴' },
            { '5', '⁵' }, { '6', '⁶' }, { '7', '⁷' }, { '8', '⁸' }, { '9', '⁹' },
            //{ '+', '⁺' }, { '-', '⁻' }, { '=', '⁼' }, { '(', '⁽' }, { ')', '⁾' },
            //{ 'n', 'ⁿ' }, { 'i', 'ⁱ' }
        };

        public static IDictionary<char, char> CharToSub = new Dictionary<char, char>
        {
            { '0', '₀' }, { '1', '₁' }, { '2', '₂' }, { '3', '₃' }, { '4', '₄' },
            { '5', '₅' }, { '6', '₆' }, { '7', '₇' }, { '8', '₈' }, { '9', '₉' },
            //{ '+', '₊' }, { '-', '₋' }, { '=', '₌' }, { '(', '₍' }, { ')', '₎' },
            //{ 'a', 'ₐ' }, { 'e', 'ₑ' }, { 'o', 'ₒ' }, { 'x', 'ₓ' }, { 'h', 'ₕ' },
            //{ 'k', 'ₖ' }, { 'l', 'ₗ' }, { 'm', 'ₘ' }, { 'n', 'ₙ' }, { 'p', 'ₚ' },
            //{ 's', 'ₛ' }, { 't', 'ₜ' }
        };

        private double _deltaBaseline = 0.1;
        private double _deltaDistanceRatio = 1.3;

        private double _baselineX = double.MinValue;
        private double _baselineY = double.MinValue;

        private GraphicsState _gState;
        private StringBuilder _text;
        private Dictionary<int, string> _markedContentDict = new Dictionary<int, string>();
        private string _tooltip = null;
        private string _linkUrl = null;
        private bool _hasLink = false;

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

        public string GetText(bool outputMarkdownLink = false)
        {
            string text = _text.ToString();
            if (outputMarkdownLink && HasLink())
            {
                if (string.IsNullOrEmpty(_tooltip) || string.Equals(text, _tooltip))
                {
                    return $"[{text}]({_linkUrl})";
                }
                else
                {
                    return $"{text} " + $"[{_tooltip}]({_linkUrl})";
                }
            }

            return text;
        }

        public Dictionary<int, string> GetMarkedContentDict()
        {
            return _markedContentDict;
        }

        public void SetLink(string url, string tooltip = null)
        {
            if (!string.IsNullOrEmpty(url))
            {
                _hasLink = true;
                _linkUrl = url;
                _tooltip = tooltip?? string.Empty;
            }
        }

        public bool HasLink()
        {
            return _hasLink;
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

        public bool TryMatchSupSub(string text, double originX, double originY, RectangleF bbox, GraphicsState gState, out string scriptChar)
        {
            scriptChar = text;
            if (text.Trim().Length == 0 && text.Length < 5)
            {
                return false;
            }
            // check if SUB or SUP
            bool charMatch = text.All(c => c == ' ' || (CharToSup.ContainsKey(c) || CharToSub.ContainsKey(c)));
            if (!charMatch) 
            {
                return false;
            }

            bool fontSizeMatch = false;
            if (Math.Abs(originX - BBox.Right) < _gState.TextState?.SpaceWidth / 2)
            {
                var preFontSize = _gState.TextState.FontSize;
                var currFontSize = gState.TextState.FontSize;
                if (preFontSize * 3 / 4 >= currFontSize)
                {
                    fontSizeMatch = true;
                }
            }
            if (!fontSizeMatch)
            {
                return false;
            }

            bool isSup = false;
            bool isSub = false;
            var preMidY = (BBox.Top + BBox.Bottom) / 2;
            var currMidY = (bbox.Top + bbox.Bottom) / 2;
            if (preMidY < currMidY)
            {
                isSup = true;
            }
            else if (preMidY > currMidY)
            {
                isSub = true;
            }

            if (isSup)
            {
                scriptChar = string.Concat(text.Select(c => c == ' ' ? " " : (CharToSup.ContainsKey(c) ? CharToSup[c].ToString() : c.ToString())));
            }
            else if (isSub)
            {
                scriptChar = string.Concat(text.Select(c => c == ' ' ? " " : (CharToSub.ContainsKey(c) ? CharToSub[c].ToString() : c.ToString())));
            }

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

            if (TryMatchSupSub(text, originX, originY, bbox, gState, out var scriptChar) && !string.Equals(text, scriptChar))
            {
                // TODO simple union currently, but <sup> <sub> should be seperated for post processing
                _text.Append(scriptChar);
                BBox = RectangleF.Union(BBox, bbox);

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
            if (space <= 1)
            {
                space = AverageCharWidth();
            }
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

        public static bool IsListParagraphBegin(string text, out bool ordered, out string bullet)
        {
            ordered = false;
            bullet = string.Empty;
            if (string.IsNullOrEmpty(text.Trim()))
            {
                return false;
            }
            text = text.Trim();
            char c = text[0];

            if (ListBullets.Contains(c))
            {
                if (text.Length == 1)
                {
                    bullet = c + "";
                    return true;
                }
                else if (text.Length > 1 && Char.IsWhiteSpace(text[1]))
                {
                    bullet = c + " ";
                    return true;
                }
                else
                {
                    return false;
                }
            }

            foreach (var tag in ListItemBeginTag)
            {
                if (text.StartsWith(tag))
                {
                    if (text.Length == tag.Length)
                    {
                        bullet = tag;
                        ordered = true;
                        return true;
                    }
                    else if (text.Length > tag.Length && Char.IsWhiteSpace(text[tag.Length]))
                    {
                        bullet = tag + " ";
                        ordered = true;
                        return true;
                    }
                    else
                    {
                        return false;
                    }
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

        public override bool TryBuildHTMLPiece(out string html, string altText = null)
        {
            html = null;

            if (GetGState() != null)
            {
                string css = GraphicsStateToCssConverter.Convert(GetGState());
                if (string.IsNullOrWhiteSpace(css))
                {
                    return false;
                }

                string text = altText == null ? GetText() : altText;
                html = $"<span style=\"{css}\">{text}</span>";

                if (HasLink())
                {
                    html = $"<a href=\"{_linkUrl}\">{html}</a>";
                }
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
                    var color = ConvertColor(g.NonStrokingColor, true);
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

            private static string ConvertColor(ColorState color, bool whiteToBlack = false)
            {
                if (color?.Components != null && color?.Components.Length == 4)
                {
                    int r = (int)color.Components[0];
                    int g = (int)color.Components[1];
                    int b = (int)color.Components[2];

                    if (r > 222 && g > 222 && b > 222)
                    {
                        r = 0;
                        g = 0;
                        b = 0;
                    }
                    return $"rgb({r},{g},{b})";
                }

                return null;
            }
        }
    }
}
