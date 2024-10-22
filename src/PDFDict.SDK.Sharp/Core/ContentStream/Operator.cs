﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PDFDict.SDK.Sharp.Core.ContentStream
{
    public class Operator
    {
        // non stroking color
        public const string NON_STROKING_COLOR = "sc";
        public const string NON_STROKING_COLOR_N = "scn";
        public const string NON_STROKING_RGB = "rg";
        public const string NON_STROKING_GRAY = "g";
        public const string NON_STROKING_CMYK = "k";
        public const string NON_STROKING_COLORSPACE = "cs";

        // stroking color
        public const string STROKING_COLOR = "SC";
        public const string STROKING_COLOR_N = "SCN";
        public const string STROKING_COLOR_RGB = "RG";
        public const string STROKING_COLOR_GRAY = "G";
        public const string STROKING_COLOR_CMYK = "K";
        public const string STROKING_COLORSPACE = "CS";

        // marked content
        public const string BEGIN_MARKED_CONTENT_SEQ = "BDC";
        public const string BEGIN_MARKED_CONTENT = "BMC";
        public const string END_MARKED_CONTENT = "EMC";
        public const string MARKED_CONTENT_POINT_WITH_PROPS = "DP";
        public const string MARKED_CONTENT_POINT = "MP";
        public const string DRAW_OBJECT = "Do";

        // state
        public const string CONCAT = "cm";
        public const string RESTORE = "Q";
        public const string SAVE = "q";
        public const string SET_FLATNESS = "i";
        public const string SET_GRAPHICS_STATE_PARAMS = "gs";
        public const string SET_LINE_CAPSTYLE = "J";
        public const string SET_LINE_DASHPATTERN = "d";
        public const string SET_LINE_JOINSTYLE = "j";
        public const string SET_LINE_MITERLIMIT = "M";
        public const string SET_LINE_WIDTH = "w";
        public const string SET_MATRIX = "Tm";
        public const string SET_RENDERINGINTENT = "ri";

        // graphics
        public const string APPEND_RECT = "re";
        public const string BEGIN_INLINE_IMAGE = "BI";
        public const string BEGIN_INLINE_IMAGE_DATA = "ID";
        public const string END_INLINE_IMAGE = "EI";
        public const string CLIP_EVEN_ODD = "W*";
        public const string CLIP_NON_ZERO = "W";
        public const string CLOSE_AND_STROKE = "s";
        public const string CLOSE_FILL_EVEN_ODD_AND_STROKE = "b*";
        public const string CLOSE_FILL_NON_ZERO_AND_STROKE = "b";
        public const string CLOSE_PATH = "h";
        public const string CURVE_TO = "c";
        public const string CURVE_TO_REPLICATE_FINAL_POINT = "y";
        public const string CURVE_TO_REPLICATE_INITIAL_POINT = "v";
        public const string ENDPATH = "n";
        public const string FILL_EVEN_ODD_AND_STROKE = "B*";
        public const string FILL_EVEN_ODD = "f*";
        public const string FILL_NON_ZERO_AND_STROKE = "B";
        public const string FILL_NON_ZERO = "f";
        public const string LEGACY_FILL_NON_ZERO = "F";
        public const string LINE_TO = "l";
        public const string MOVE_TO = "m";
        public const string SHADING_FILL = "sh";
        public const string STROKE_PATH = "S";

        // text
        public const string BEGIN_TEXT = "BT";
        public const string END_TEXT = "ET";
        public const string MOVE_TEXT = "Td";
        public const string MOVE_TEXT_SET_LEADING = "TD";
        public const string NEXT_LINE = "T*";
        public const string SET_CHAR_SPACING = "Tc";
        public const string SET_FONT_AND_SIZE = "Tf";
        public const string SET_TEXT_HORIZONTAL_SCALING = "Tz";
        public const string SET_TEXT_LEADING = "TL";
        public const string SET_TEXT_RENDERINGMODE = "Tr";
        public const string SET_TEXT_RISE = "Ts";
        public const string SET_WORD_SPACING = "Tw";
        public const string SHOW_TEXT = "Tj";
        public const string SHOW_TEXT_ADJUSTED = "TJ";
        public const string SHOW_TEXT_LINE = "'";
        public const string SHOW_TEXT_LINE_AND_SPACE = "\"";

        // type3 font
        public const string TYPE3_D0 = "d0";
        public const string TYPE3_D1 = "d1";

        // compatibility section
        public const string BEGIN_COMPATIBILITY_SECTION = "BX";
        public const string END_COMPATIBILITY_SECTION = "EX";
    }
}
