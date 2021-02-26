using PdfSharp.Pdf;
using PdfSharp.Pdf.Advanced;
using PdfSharp.Pdf.IO;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace ConsoleApp6
{
    class Program
    {
        static void Main(string[] args)
        {
            DirectoryInfo d = new DirectoryInfo(AppDomain.CurrentDomain.BaseDirectory);
            var files = d.GetFiles("*.pdf");
            foreach (var file in files)
            {
                var dirPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, file.Name.Replace(file.Extension, ""));
                if (!Directory.Exists(dirPath))
                    Directory.CreateDirectory(dirPath);
                PdfDocument document = PdfReader.Open(file.FullName);
                int imageCount = 0;
                foreach (PdfPage page in document.Pages)
                {
                    PdfDictionary resources = page.Elements.GetDictionary("/Resources");
                    if (resources != null)
                    {
                        PdfDictionary xObjects = resources.Elements.GetDictionary("/XObject");
                        if (xObjects != null)
                        {
                            ICollection<PdfItem> items = xObjects.Elements.Values;
                            foreach (PdfItem item in items)
                            {
                                PdfReference reference = item as PdfReference;
                                if (reference != null)
                                {
                                    PdfDictionary xObject = reference.Value as PdfDictionary;
                                    if (xObject != null && xObject.Elements.GetString("/Subtype") == "/Image")
                                    {
                                        ExportImage(xObject, dirPath, ref imageCount);
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }
        static void ExportImage(PdfDictionary image, string dirPath, ref int count)
        {
            string filter = image.Elements.GetName("/Filter");
            switch (filter)
            {
                case "/DCTDecode":
                    ExportJpegImage(image, dirPath, ref count);
                    break;

                case "/FlateDecode":
                    ExportAsPngImage(image, dirPath, ref count);
                    break;
            }
        }
        static void ExportJpegImage(PdfDictionary image, string dirPath, ref int count)
        {
            count++;
            byte[] stream = image.Stream.Value;
            using (FileStream fs = new FileStream(Path.Combine(dirPath, $"{count}.jpg"), FileMode.Create, FileAccess.Write))
            using (BinaryWriter bw = new BinaryWriter(fs))
            {
                bw.Write(stream);
                bw.Close();
            }
        }
        static void ExportAsPngImage(PdfDictionary image, string dirPath, ref int count)
        {
            count++;
            int width = image.Elements.GetInteger(PdfImage.Keys.Width);
            int height = image.Elements.GetInteger(PdfImage.Keys.Height);
            int bitsPerComponent = image.Elements.GetInteger(PdfImage.Keys.BitsPerComponent);

            // TODO: You can put the code here that converts vom PDF internal image format to a Windows bitmap
            // and use GDI+ to save it in PNG format.
            // It is the work of a day or two for the most important formats. Take a look at the file
            // PdfSharp.Pdf.Advanced/PdfImage.cs to see how we create the PDF image formats.
            // We don't need that feature at the moment and therefore will not implement it.
            // If you write the code for exporting images I would be pleased to publish it in a future release
            // of PDFsharp.
        }
    }
}
