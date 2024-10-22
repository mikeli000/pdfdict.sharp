using PDFiumCore;

namespace PDFDict.SDK.Sharp.Core
{
    public sealed class PDFSharpLib
    {
        public static void Initialize(PDFSharpConfig config = null)
        {
            fpdfview.FPDF_InitLibrary();
        }
    }

    public class PDFSharpConfig
    {
        public string FontDirectory { get; set; }
    }
}
