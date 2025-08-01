using DinkToPdf;
using DinkToPdf.Contracts;

namespace Core.Report
{
    public static class ReportGenerator
    {
        static readonly IConverter converter;

        static ReportGenerator()
        {
            converter = new SynchronizedConverter(new PdfTools());
        }

        public static byte[] GeneratePDF_from_HTML(string html)
        {
            var doc = new HtmlToPdfDocument
            {
                GlobalSettings =
                {
                    ColorMode = ColorMode.Color,
                    Orientation = Orientation.Portrait,
                    PaperSize = PaperKind.A4
                },
                Objects =
                {
                    new ObjectSettings
                    {
                        HtmlContent = html,
                        WebSettings = { DefaultEncoding = "utf-8" }
                    }
                }
            };

            return converter.Convert(doc);
        }
    }
}