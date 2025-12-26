
using OcrCore.Iron.Service.Implementation;
using OcrCore.Iron.Service.Interfaces;

namespace OcrServer.Iron;

public class Program
{
    public static void Main(string[] args)
    {
        var license = Environment.GetEnvironmentVariable("IRON_LICENSE_KEY");
        if (!string.IsNullOrWhiteSpace(license))
        {
            IronPdf.License.LicenseKey = license;
        }

        var builder = WebApplication.CreateBuilder(args);

        // Add services to the container.

        builder.Services.AddControllers();
        // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();

        builder.Services.AddSingleton<IIronOcrExtractor, IronOcrExtractor>();

        var app = builder.Build();

        // Configure the HTTP request pipeline.
        //if (app.Environment.IsDevelopment())
        //{
        //    app.UseSwagger();
        //    app.UseSwaggerUI();
        //}
        app.UseSwagger();
        app.UseSwaggerUI(c =>
        {
            c.SwaggerEndpoint("/swagger/v1/swagger.json", "OCR API v1");
            c.RoutePrefix = "swagger"; // http://host:port/swagger
        });
        app.UseAuthorization();


        app.MapControllers();

        app.Run();
    }
}
