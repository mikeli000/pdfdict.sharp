using System.Drawing;

namespace PDFDict.SDK.Sharp.Core.Contents
{
    public abstract class PageElement
    {
        public enum ElementType
        {
            Text,
            Image,
            Graphics,
            Annotation,
            XObject
        };

        public ElementType Type { get; protected set; }
        public RectangleF BBox { get; protected set; }

        public PageElement(ElementType type, RectangleF bbox)
        {
            Type = type;
            BBox = bbox;
        }
    }

    public class GraphicsState : IEquatable<GraphicsState>
    {
        public TextState TextState;
        public Matrix Matrix;
        public ColorState NonStrokingColor;
        public ColorState StrokingColor;

        public float LineWidth;
        public int LineCap;
        public int LineJoin;
        public float MiterLimit;

        public bool Equals(GraphicsState? that)
        {
            if (ReferenceEquals(that, null))
            {
                return false;
            }

            if (ReferenceEquals(this, that))
            {
                return true;
            }

            if (!TextState.Equals(that.TextState))
            {
                return false;
            }

            if (NonStrokingColor != null)
            {
                if (!NonStrokingColor.Equals(that.NonStrokingColor))
                {
                    return false;
                }
            }
            else
            {
                if (that.NonStrokingColor != null)
                {
                    return false;
                }
            }

            if (StrokingColor != null)
            {
                if (!StrokingColor.Equals(that.StrokingColor))
                {
                    return false;
                }
            }
            else
            {
                if (that.StrokingColor != null)
                {
                    return false;
                }
            }
            
            if (LineWidth != that.LineWidth)
            {
                return false;
            }
            if (LineCap != that.LineCap)
            {
                return false;
            }
            if (LineJoin != that.LineJoin)
            {
                return false;
            }
            if (MiterLimit != that.MiterLimit)
            {
                return false;
            }

            return true;
        }
    }

    public class TextState: IEquatable<TextState>
    {
        public int FontWeight = 0;
        public float FontItalicAngle = 0; 
        public float CharacterSpacing = 0;
        public float WordSpacing = 0;
        public float HorizontalScaling = 100;
        public float Leading = 0;
        public float FontSize = 0;
        public float Rise = 0;
        public bool Knockout = true;
        public string FontFamilyName = string.Empty;
        public int RenderingMode = TextRenderingMode.UNKNOWN;
        public float SpaceWidth = 0;

        public bool Equals(TextState? that)
        {
            if (ReferenceEquals(that, null))
            {
                return false;
            }

            if (ReferenceEquals(this, that))
            { 
                return true; 
            }
            
            if (FontWeight != that.FontWeight)
            {
                return false;
            }
            if (FontItalicAngle != that.FontItalicAngle)
            {
                return false;
            }
            if (CharacterSpacing != that.CharacterSpacing)
            {
                return false;
            }
            if (WordSpacing != that.WordSpacing)
            {
                return false;
            }
            if (HorizontalScaling != that.HorizontalScaling)
            {
                return false;
            }
            if (Leading != that.Leading)
            {
                return false;
            }
            if (FontSize != that.FontSize)
            {
                return false;
            }
            if (Rise != that.Rise)
            {
                return false;
            }
            if (Knockout != that.Knockout)
            {
                return false;
            }
            if (FontFamilyName != that.FontFamilyName)
            {
                return false;
            }
            if (RenderingMode != that.RenderingMode)
            {
                return false;
            }

            return true;
        }
    }

    public class ColorState : IEquatable<ColorState>
    {
        public float[] Components;
        public string PatternName;
        public string ColorSpace;
        public uint IntValue = 0;

        public static ColorState From(uint R, uint G, uint B, uint A)
        {
            ColorState colorState = new ColorState();
            colorState.Components = new float[] { R, G, B, A };
            colorState.PatternName = null;
            colorState.ColorSpace = "RGBA";
            colorState.IntValue = R << 24 | G << 16 | B << 8 | A;

            return colorState;
        }

        public bool Equals(ColorState? that)
        {
            if (ReferenceEquals(that, null))
            {
                return false;
            }

            if (ReferenceEquals(this, that))
            {
                return true;
            }

            if (!ArrayEquals(Components, that.Components))
            {
                return false;
            }
            if (!string.Equals(PatternName, that.PatternName, StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }
            if (!string.Equals(ColorSpace, that.ColorSpace, StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }
            if (IntValue != that.IntValue)
            {
                return false;
            }

            return true;
        }

        private static bool ArrayEquals(float[] arr1, float[] arr2)
        {
            if (arr1 == null &&  arr2 == null)
            {
                return true;
            }
            if (arr1 == null || arr2 == null) 
            { 
                return false; 
            }

            if (arr1.Length != arr2.Length)
            {
                return false;
            }

            for (int i = 0; i < arr1.Length; i++) 
            {
                if (arr1[i] != arr2[i])
                {
                    return false;
                }
            }

            return true;
        }
    }

    public class TextRenderingMode
    {
        public static readonly int UNKNOWN = -1;
        public static readonly int FILL = 0;
        public static readonly int STROKE = 1;
        public static readonly int FILL_STROKE = 2;
        public static readonly int INVISIBLE = 3;
        public static readonly int FILL_CLIP = 4;
        public static readonly int STROKE_CLIP = 5;
        public static readonly int FILL_STROKE_CLIP = 6;
        public static readonly int CLIP = 7;
        public static readonly int LAST = 7;
    }
}
