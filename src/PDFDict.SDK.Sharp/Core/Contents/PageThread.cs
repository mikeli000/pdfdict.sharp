using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PDFDict.SDK.Sharp.Core.Contents
{
    public class PageThread
    {
        private TextThread _textThread;
        private List<PageElement> _contentList = new List<PageElement>();

        public PageThread()
        {
            _textThread = new TextThread();
        }

        public void AddPageElement(PageElement element)
        {
            if (element is TextElement)
            {
                _textThread.AddTextElement((TextElement)element);
            }

            _contentList.Add(element);
        }

        public List<PageElement> GetContentList()
        {
            return _contentList;
        }

        public TextThread GetTextThread()
        {
            return _textThread;
        }

        public void GroupText()
        {
            var textElements = new List<TextElement>();
            for (int i = 0; i < _contentList.Count; i++)
            {
                if (_contentList[i].Type == PageElement.ElementType.Text)
                {
                    textElements.Add((TextElement)_contentList[i]);
                }
            }


        }
    }

    public class TextThread
    {
        private List<TextElement> _textElements;

        public TextThread()
        {
            _textElements = new List<TextElement>();
        }

        public void AddTextElement(TextElement textElement)
        {
            _textElements.Add(textElement);
        }

        public List<TextElement> GetTextElements()
        {
            return _textElements;
        }

        private List<TextElement> SortTextElements()
        {
            return _textElements.OrderByDescending(x => x.GetBaselineY()).ThenBy(x => x.BBox.X).ToList();
        }

        public List<TextLine> LinearTextElements()
        {
            var textLines = new List<TextLine>();

            _textElements = SortTextElements();
            foreach (var ele in _textElements)
            {
                bool inline = false;
                for (int i = 0; i < textLines.Count; i++)
                {
                    if (textLines[i].TryAddElement(ele))
                    {
                        inline = true;
                        break;
                    }
                }

                if (!inline)
                {
                    TextLine textLine = new TextLine(ele);
                    textLines.Add(textLine);
                }
            }

            var groups = Columnarize(textLines);

            foreach (var group in groups)
            {
                Console.WriteLine("--------------------------");
                for (int i = group.Item1; i <= group.Item2; i++)
                {
                    Console.WriteLine(textLines[i]);
                }
                Console.WriteLine("--------------------------");
            }

            return textLines;
        }

        public List<Tuple<int, int>> Columnarize(List<TextLine> textLines)
        {
            int[] colCountArr = new int[textLines.Count];
            for (int i = 0; i < textLines.Count(); i++)
            {
                textLines[i].Columnar();
                colCountArr[i] = textLines[i].ColCount();
            }

            List<Tuple<int, int>> groups = new List<Tuple<int, int>>();
            for (int i = 0; i < colCountArr.Length; i++)
            {
                if (colCountArr[i] < 2)
                {
                    continue;
                }

                int start = i;
                while (i < colCountArr.Length - 1 && colCountArr[i] == colCountArr[i + 1])
                {
                    i++;
                }
                int end = i;
                if (end - start >= 1)
                {
                    groups.Add(new Tuple<int, int>(start, end));
                }
            }

            return groups;
        }

        private static bool LeftAligned(TextElement prev, TextElement curr)
        {
            return Math.Abs(prev.BBox.X - curr.BBox.X) <= 1;
        }

        private static bool RightAligned(TextElement prev, TextElement curr)
        {
            return Math.Abs(prev.BBox.Right - curr.BBox.Right) <= 1;
        }

        private static bool CenterAligned(TextElement prev, TextElement curr)
        {
            return Math.Abs((prev.BBox.X + prev.BBox.Width) - (curr.BBox.X + curr.BBox.Width)) <= 1;
        }

        public void FindGridTableElements(List<ColumnInfo> cols)
        {

        }
    }

    public class TextLine
    {
        public int Baseline { get; private set; }

        private List<TextElement> _textElements;
        
        private List<ColumnInfo> _columns;

        public TextLine(TextElement firstElement) 
        { 
            _textElements = new List<TextElement>() { firstElement };
            Baseline = (int)Math.Round(firstElement.GetBaselineY());
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            foreach (var ele in _textElements)
            {
                sb.Append(ele.GetText());
                sb.Append(" ");
            }

            return sb.ToString();
        }

        public bool TryAddElement(TextElement element)
        {
            if (Math.Abs(Baseline - element.GetBaselineY()) <= 1)
            {
                _textElements.Add(element);
                return true;
            }

            return false;
        }

        public int ElementCount()
        {
            return _textElements.Count;
        }

        public int ColCount()
        {
            return _columns == null ? 0 : _columns.Count;
        }

        public TextElement ElementAt(int index)
        {
            if (index < 0 || index >= _textElements.Count)
            {
                throw new IndexOutOfRangeException($"Index out of range(0 - {_textElements.Count}) {index}");
            }

            return _textElements[index];
        }

        public void Columnar()
        {
            if (_textElements.Count == 0)
            {
                return;
            }

            _textElements.Sort((x, y) => x.BBox.X.CompareTo(y.BBox.X));

            _columns = new List<ColumnInfo>();
            for (int i = 0; i < _textElements.Count - 1; i++)
            {
                var col = new ColumnInfo();
                col.Start = (int)_textElements[i].BBox.Left;
                col.Middle = col.Start + (int)_textElements[i].BBox.Right;
                col.End = (int)_textElements[i + 1].BBox.Right;
                col.TextElement = _textElements[i];
                _columns.Add(col);
            }
        }
    }

    public class ColumnStyle
    {
        public enum Alignment
        {
            Left = 0,
            Right = 1,
            Center = 2,
            Justify = 3,
            Unknown = -1
        }

        private List<Column> _columns;

        public ColumnStyle()
        {
            _columns = new List<Column>();
        }

        public int ColumnCount()
        {
            return _columns.Count;
        }
        
        public void AddColumn(int start, int width, Alignment alignment = Alignment.Unknown)
        {
            if (_columns.Count > 0)
            {
                var last = _columns[_columns.Count - 1];
                if (start < last.Start + last.Width)
                {
                    throw new Exception("Column start position is less than the end of the last column");
                }
            }

            Column col = new Column();
            col.Start = start;
            col.Width = width;
            col.Alignment = alignment;

            _columns.Add(col);
        }

        public class Column
        {
            public int Start { get; set; }
            public int Width { get; set; }
            public Alignment Alignment { get; set; }
        }
    }

    public class ColumnInfo
    {
        public int Start { get; set; }
        public int Middle { get; set; }
        public int End { get; set; }
        public TextElement TextElement { get; set; }
    }

}
