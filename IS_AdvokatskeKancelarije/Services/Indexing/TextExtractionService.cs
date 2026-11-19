using System.Text;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using UglyToad.PdfPig;

namespace IS_AdvokatskeKancelarije.Services
{
   
    public class TextExtractionService
    {
        public List<PageText> Extract(Stream stream, string extension)
        {
            return extension.ToLowerInvariant() switch
            {
                ".pdf" => ExtractPdf(stream),
                ".docx" => ExtractDocx(stream),
                ".txt" => ExtractTxt(stream),
                _ => throw new NotSupportedException($"Unsupported file type: {extension}")
            };
        }

        private static List<PageText> ExtractPdf(Stream stream)
        {
            var pages = new List<PageText>();

            using var pdf = PdfDocument.Open(stream);
            foreach (var page in pdf.GetPages())
            {
                var text = string.Join(" ", page.GetWords().Select(w => w.Text));
                pages.Add(new PageText(page.Number, text));
            }

            return pages;
        }

        private static List<PageText> ExtractDocx(Stream stream)
        {
            using var doc = WordprocessingDocument.Open(stream, false);
            var body = doc.MainDocumentPart?.Document?.Body;
            if (body == null)
                return new List<PageText>();

            var sb = new StringBuilder();
            foreach (var paragraph in body.Descendants<Paragraph>())
            {
                sb.Append(paragraph.InnerText).Append(' ');
            }

            return new List<PageText> { new PageText(null, sb.ToString()) };
        }

        private static List<PageText> ExtractTxt(Stream stream)
        {
            using var reader = new StreamReader(stream, Encoding.UTF8, detectEncodingFromByteOrderMarks: true);
            return new List<PageText> { new PageText(null, reader.ReadToEnd()) };
        }
    }
}
