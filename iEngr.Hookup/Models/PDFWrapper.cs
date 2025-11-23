using PdfiumViewer;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace iEngr.Hookup.Models
{
    public class PDFWrapper
    {
        // 获取 PDF 页数
        public static int GetPageCount(string filePath)
        {
            using (var document = PdfDocument.Load(filePath))
            {
                return document.PageCount;
            }
        }

        // 获取指定页面的图像
        public static Bitmap GetImage(string filePath, int pageIndex, int dpi)
        {
            using (var document = PdfDocument.Load(filePath))
            {
                // Render the PDF page as a System.Drawing.Image
                var image = document.Render(pageIndex, dpi, dpi, true);

                // Convert the System.Drawing.Image to a System.Drawing.Bitmap
                var bitmap = new Bitmap(image);
                return bitmap;
            }
        }

    }

}
