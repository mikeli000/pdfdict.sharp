using PDFiumCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PDFDict.SDK.Sharp.Core
{
    public class PDFStructureTree: IDisposable
    {
        private PDFPage _pdfPage;
        private FpdfStructtreeT _pdfStructtreeT;

        public PDFStructureTree(PDFPage page)
        {
            _pdfPage = page;
            _pdfStructtreeT = fpdf_structtree.FPDF_StructTreeGetForPage(_pdfPage.GetHandle());
        }

        public int GetChildCount()
        {
            return fpdf_structtree.FPDF_StructTreeCountChildren(_pdfStructtreeT);
        }

        public void Dispose()
        {
            if (_pdfStructtreeT != null)
            {
                fpdf_structtree.FPDF_StructTreeClose(_pdfStructtreeT);
            }
        }
    }
}
