using System.ComponentModel;
using System.Text;

namespace PDFDict.SDK.Sharp.Core
{
    public enum PDFAnnotTypes
    {
        FPDF_ANNOT_UNKNOWN = 0,
        FPDF_ANNOT_TEXT = 1,
        FPDF_ANNOT_LINK = 2,
        FPDF_ANNOT_FREETEXT = 3,
        FPDF_ANNOT_LINE = 4,
        FPDF_ANNOT_SQUARE = 5,
        FPDF_ANNOT_CIRCLE = 6,
        FPDF_ANNOT_POLYGON = 7,
        FPDF_ANNOT_POLYLINE = 8,
        FPDF_ANNOT_HIGHLIGHT = 9,
        FPDF_ANNOT_UNDERLINE = 10,
        FPDF_ANNOT_SQUIGGLY = 11,
        FPDF_ANNOT_STRIKEOUT = 12,
        FPDF_ANNOT_STAMP = 13,
        FPDF_ANNOT_CARET = 14,
        FPDF_ANNOT_INK = 15,
        FPDF_ANNOT_POPUP = 16,
        FPDF_ANNOT_FILEATTACHMENT = 17,
        FPDF_ANNOT_SOUND = 18,
        FPDF_ANNOT_MOVIE = 19,
        FPDF_ANNOT_WIDGET = 20,
        FPDF_ANNOT_SCREEN = 21,
        FPDF_ANNOT_PRINTERMARK = 22,
        FPDF_ANNOT_TRAPNET = 23,
        FPDF_ANNOT_WATERMARK = 24,
        FPDF_ANNOT_THREED = 25,
        FPDF_ANNOT_RICHMEDIA = 26,
        FPDF_ANNOT_XFAWIDGET = 27,
        FPDF_ANNOT_REDACT = 28
    }

    public enum FPDFFormFieldTypes
    {
        FPDF_FORMFIELD_UNKNOWN = 0,
        FPDF_FORMFIELD_PUSHBUTTON = 1,
        FPDF_FORMFIELD_CHECKBOX = 2,
        FPDF_FORMFIELD_RADIOBUTTON = 3,
        FPDF_FORMFIELD_COMBOBOX = 4,
        FPDF_FORMFIELD_LISTBOX = 5,
        FPDF_FORMFIELD_TEXTFIELD = 6,
        FPDF_FORMFIELD_SIGNATURE = 7,
#if PDF_ENABLE_XFA
        FPDF_FORMFIELD_XFA = 8,             
        FPDF_FORMFIELD_XFA_CHECKBOX = 9,    
        FPDF_FORMFIELD_XFA_COMBOBOX = 10,   
        FPDF_FORMFIELD_XFA_IMAGEFIELD = 11, 
        FPDF_FORMFIELD_XFA_LISTBOX = 12,    
        FPDF_FORMFIELD_XFA_PUSHBUTTON = 13, 
        FPDF_FORMFIELD_XFA_SIGNATURE = 14,  
        FPDF_FORMFIELD_XFA_TEXTFIELD = 15,  
#endif // PDF_ENABLE_XFA

#if PDF_ENABLE_XFA
        FPDF_FORMFIELD_COUNT = 16
#else // PDF_ENABLE_XFA
        FPDF_FORMFIELD_COUNT = 8
#endif // PDF_ENABLE_XFA
    }

    public static class PDFAnnotFlags
    {
        // Refer to PDF Reference (6th edition) table 8.16 for all annotation flags.
        public const int FPDF_ANNOT_FLAG_NONE = 0;
        public const int FPDF_ANNOT_FLAG_INVISIBLE = 1 << 0;
        public const int FPDF_ANNOT_FLAG_HIDDEN = 1 << 1;
        public const int FPDF_ANNOT_FLAG_PRINT = 1 << 2;
        public const int FPDF_ANNOT_FLAG_NOZOOM = 1 << 3;
        public const int FPDF_ANNOT_FLAG_NOROTATE = 1 << 4;
        public const int FPDF_ANNOT_FLAG_NOVIEW = 1 << 5;
        public const int FPDF_ANNOT_FLAG_READONLY = 1 << 6;
        public const int FPDF_ANNOT_FLAG_LOCKED = 1 << 7;
        public const int FPDF_ANNOT_FLAG_TOGGLENOVIEW = 1 << 8;

        public const int FPDF_ANNOT_APPEARANCEMODE_NORMAL = 0;
        public const int FPDF_ANNOT_APPEARANCEMODE_ROLLOVER = 1;
        public const int FPDF_ANNOT_APPEARANCEMODE_DOWN = 2;
        public const int FPDF_ANNOT_APPEARANCEMODE_COUNT = 3;

        // Refer to PDF Reference version 1.7 table 8.70 for field flags common to all interactive form field types.
        public const int FPDF_FORMFLAG_NONE = 0;
        public const int FPDF_FORMFLAG_READONLY = 1 << 0;
        public const int FPDF_FORMFLAG_REQUIRED = 1 << 1;
        public const int FPDF_FORMFLAG_NOEXPORT = 1 << 2;

        // Refer to PDF Reference version 1.7 table 8.77 for field flags specific to interactive form text fields.
        public const int FPDF_FORMFLAG_TEXT_MULTILINE = 1 << 12;
        public const int FPDF_FORMFLAG_TEXT_PASSWORD = 1 << 13;

        // Refer to PDF Reference version 1.7 table 8.79 for field flags specific to interactive form choice fields.
        public const int FPDF_FORMFLAG_CHOICE_COMBO = 1 << 17;
        public const int FPDF_FORMFLAG_CHOICE_EDIT = 1 << 18;
        public const int FPDF_FORMFLAG_CHOICE_MULTI_SELECT = 1 << 21;

        // Additional actions type of form field:
        //   K, on key stroke, JavaScript action.
        //   F, on format, JavaScript action.
        //   V, on validate, JavaScript action.
        //   C, on calculate, JavaScript action.
        public const int FPDF_ANNOT_AACTION_KEY_STROKE = 12;
        public const int FPDF_ANNOT_AACTION_FORMAT = 13;
        public const int FPDF_ANNOT_AACTION_VALIDATE = 14;
        public const int FPDF_ANNOT_AACTION_CALCULATE = 15;

        public enum FPDFANNOT_COLORTYPE
        {
            Color = 0,
            InteriorColor
        }

        public static string AnnotFlagsToString(int flags)
        {
            var sb = new StringBuilder();
            if ((flags & FPDF_ANNOT_FLAG_INVISIBLE) != 0)
            {
                sb.Append("invisible ");
            }
            if ((flags & FPDF_ANNOT_FLAG_HIDDEN) != 0)
            {
                sb.Append("hidden ");
            }
            if ((flags & FPDF_ANNOT_FLAG_PRINT) != 0)
            {
                sb.Append("print ");
            }
            if ((flags & FPDF_ANNOT_FLAG_NOZOOM) != 0)
            {
                sb.Append("nozoom ");
            }
            if ((flags & FPDF_ANNOT_FLAG_NOROTATE) != 0)
            {
                sb.Append("norotate ");
            }
            if ((flags & FPDF_ANNOT_FLAG_NOVIEW) != 0)
            {
                sb.Append("noview ");
            }
            if ((flags & FPDF_ANNOT_FLAG_READONLY) != 0)
            {
                sb.Append("readonly ");
            }
            if ((flags & FPDF_ANNOT_FLAG_LOCKED) != 0)
            {
                sb.Append("locked ");
            }
            if ((flags & FPDF_ANNOT_FLAG_TOGGLENOVIEW) != 0)
            {
                sb.Append("togglenoview ");
            }
            return sb.ToString();
        }
    }
}