using Microsoft.AspNetCore.Mvc;

namespace OcrServer.Iron.Contracts;

public sealed class OcrPdfRequest
{
    [FromForm(Name = "file")]
    public IFormFile File { get; set; } = default!;

    [FromForm(Name = "dpi")]
    public int? Dpi { get; set; }

    [FromForm(Name = "langs")]
    public string? Langs { get; set; }
}
