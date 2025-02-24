using DinkToPdf;

namespace Core.Report
{
    public static class ReportGenerator
    {
        public static byte[] GeneratePDF_from_HTML(string html)
        {
            var converter = new BasicConverter(new PdfTools());

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