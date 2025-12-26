using OcrCore.Iron.Service.Implementation;

namespace OcrCore.Iron.Service.Interfaces;

public interface IIronOcrExtractor
{
    Task<string> ExtractCleanTextAsync(string pdfPath, OcrOptions? options = null, CancellationToken ct = default);
}
