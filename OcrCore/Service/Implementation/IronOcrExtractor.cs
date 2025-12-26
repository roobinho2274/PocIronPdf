using System.Text;
using System.Text.RegularExpressions;

using Grpc.Core.Logging;

using IronOcr;

using IronPdf.Imaging; // Add this using directive at the top of the file

using Microsoft.Extensions.Logging;

using OcrCore.Iron.Service.Interfaces;

namespace OcrCore.Iron.Service.Implementation;

public sealed class IronOcrExtractor(ILogger<IronOcrExtractor> logger) : IIronOcrExtractor
{
    private readonly ILogger<IronOcrExtractor> _logger = logger;

    public async Task<string> ExtractCleanTextAsync(string pdfPath, OcrOptions? options = null, CancellationToken ct = default)
    {
        options ??= new OcrOptions();
        if (!File.Exists(pdfPath))
        {
            _logger.LogError("PDF file not found: {PdfPath}", pdfPath);
            throw new FileNotFoundException("PDF file not found.", pdfPath);
        }
        var dpi = options.Dpi is >= 120 and <= 600 ? options.Dpi : 300;

        var tempDir = Path.Combine(Path.GetTempPath(), "ocr-iron", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(tempDir);

        try
        {
            _logger.LogInformation("Starting OCR extraction for PDF: {PdfPath} with DPI: {Dpi}", pdfPath, dpi);

            var pdf = PdfDocument.FromFile(pdfPath);

            var imagePaths = pdf.RasterizeToImageFiles(
                Path.Combine(tempDir, "page_{0}.png"),
                IronPdf.Imaging.ImageType.Png,
                dpi,
                true);

            if (imagePaths is null || imagePaths.Length == 0)
            {
                _logger.LogWarning("No images were rasterized from the PDF: {PdfPath}", pdfPath);
                return string.Empty;
            }

            _logger.LogInformation("Rasterized {PageCount} pages from PDF: {PdfPath}", imagePaths.Length, pdfPath);

            var ocr = new IronOcr.IronTesseract();
            ConfigureIronLanguages(ocr, options.Langs);

            var sb = new StringBuilder(capacity: 1024 * 1024);

            foreach (var imagePath in imagePaths)
            {
                ct.ThrowIfCancellationRequested();

                try
                {
                    _logger.LogInformation("Performing OCR on image: {ImagePath}", imagePath);
                    using var input = new OcrInput();
                    input.LoadImage(imagePath);

                    var result = await Task.Run(() => ocr.Read(input), ct);
                    sb.AppendLine(result.Text);

                    sb.AppendLine();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error during OCR processing of image: {ImagePath}", imagePath);
                }
            }

            var raw = sb.ToString();
            var cleaned = CleanText(raw);

            return cleaned;
        }
        finally
        {
            try
            {
                if (Directory.Exists(tempDir))
                {
                    Directory.Delete(tempDir, true);
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to delete temporary directory: {TempDir}", tempDir);
            }
        }
    }

    private static string CleanText(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
            return string.Empty;
        var t = input;

        t = t.Normalize(NormalizationForm.FormKC);
        t = t.Replace("ﬁ", "fi").Replace("ﬂ", "fl");
        t = Regex.Replace(t, @"[^\P{C}\t\r\n]", "");
        t = Regex.Replace(t, @"(\p{L})-\r?\n(\p{L})", "$1$2");
        t = t.Replace("\r\n", "\n").Replace("\r", "\n");
        t = Regex.Replace(t, @"[ \t]+", " ");
        t = Regex.Replace(t, @"\n{3,}", "\n\n");
        t = Regex.Replace(t, @"[^\p{L}\p{N}\p{P}\p{Z}\n]", "");
        t = Regex.Replace(t, @"(^|\n)[\p{P}\p{S}]{4,}(\n|$)", "\n");
        t = Regex.Replace(t, @"\s+([,.;:!?])", "$1");
        t = Regex.Replace(t, @"([,.;:!?]){3,}", "$1$1");
        return t.Trim();
    }

    /// <summary>
    /// Mapeia "por+eng" para o modelo de idiomas do IronOcr.
    /// Se o pacote não tiver esses idiomas instalados, ele vai cair no default.
    /// </summary>
    private static void ConfigureIronLanguages(IronTesseract ocr, string? langs)
    {
        // Para POC: comportamento previsível
        // Se você tiver language packs instalados, você pode ajustar aqui.

        var s = (langs ?? "").Trim().ToLowerInvariant();

        // Default seguro
        ocr.Language = OcrLanguage.English;
        // Se pedir português, troca pra pt.
        // (Algumas versões suportam combinar idiomas; outras não.)
        if (s.Contains("por") || s.Contains("pt"))
        {
            ocr.Language = OcrLanguage.Portuguese;
        }

        // Se pedir ambos, em muitas versões dá pra combinar:
        // ocr.Language = OcrLanguage.Portuguese + OcrLanguage.English;
        // Se sua versão suportar, você pode habilitar:
        if ((s.Contains("por") || s.Contains("pt")) && (s.Contains("eng") || s.Contains("en")))
        {
            try
            {
                ocr.Language = OcrLanguage.Portuguese | OcrLanguage.English;
            }
            catch
            {
                // se a versão não
            }
        }
    }
}
