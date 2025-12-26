using System.Text;

using Microsoft.AspNetCore.Mvc;

using OcrCore.Iron.Service.Implementation;
using OcrCore.Iron.Service.Interfaces;

using OcrServer.Iron.Contracts;

namespace OcrServer.Iron.Controllers;

[ApiController]
[Route("api/ocr")]
public class OcrController(IIronOcrExtractor ocr, ILogger<OcrController> logger) : ControllerBase
{
    private readonly IIronOcrExtractor OCR = ocr;
    private readonly ILogger<OcrController> LOGGER = logger;

    [HttpPost("pdf")]
    [Consumes("multipart/form-data")]
    [RequestSizeLimit(200_000_000)]
    public async Task<IActionResult> Pdf([FromForm] OcrPdfRequest request)
    {
        if (request.File is null || request.File.Length == 0)
            return BadRequest("Arquivo não enviado.");

        var sw = System.Diagnostics.Stopwatch.StartNew();

        var tempPdf = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.pdf");
        await using (var fs = System.IO.File.Create(tempPdf))
            await request.File.CopyToAsync(fs);

        try
        {
            var opts = new OcrOptions
            {
                Dpi = request.Dpi.GetValueOrDefault(300),
                Langs = request.Langs
            };

            var text = await OCR.ExtractCleanTextAsync(tempPdf, opts);

            LOGGER.LogInformation("IRON OCR OK in {ElapsedMs}ms file={File} size={Size}",
                sw.ElapsedMilliseconds, request.File.FileName, request.File.Length);

            var bytes = Encoding.UTF8.GetBytes(text);
            var outName = Path.GetFileNameWithoutExtension(request.File.FileName) + ".txt";
            return File(bytes, "text/plain; charset=utf-8", outName);
        }
        catch (Exception ex)
        {
            LOGGER.LogError(ex, "IRON OCR FAILED after {ElapsedMs}ms", sw.ElapsedMilliseconds);
            return Problem("OCR failed. Check server logs.", statusCode: 500);
        }
        finally
        {
            System.IO.File.Delete(tempPdf);
        }
    }
}
