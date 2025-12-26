namespace OcrCore.Iron.Service.Implementation;

public sealed class OcrOptions
{
    public int Dpi { get; set; } = 300;
    public string? Langs { get; set; } // opcional (Iron trabalha diferente, mas mantenha p/ compat)
}