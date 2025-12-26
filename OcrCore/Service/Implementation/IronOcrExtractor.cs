using OcrCore.Iron.Service.Interfaces;

namespace OcrCore.Iron.Service.Implementation;

public sealed class IronOcrExtractor : IIronOcrExtractor
{
    public Task<string> ExtractCleanTextAsync(string pdfPath, OcrOptions? options = null)
    {
        options ??= new OcrOptions();

        // TODO: implementar com IronPdf + IronOcr
        // - carregar PDF
        // - renderizar páginas em imagens (dpi)
        // - rodar IronTesseract em cada imagem
        // - juntar texto
        // - (opcional) limpar texto

        throw new NotImplementedException();
    }
}
