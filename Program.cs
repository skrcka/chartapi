using RDotNet;
using Microsoft.AspNetCore.HttpLogging;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
var devCorsPolicy = "devCorsPolicy";
builder.Services.AddCors(options =>
{
    options.AddPolicy(devCorsPolicy, builder => {
        //builder.WithOrigins("http://localhost:800").AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader();
        builder.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader();
        //builder.SetIsOriginAllowed(origin => new Uri(origin).Host == "localhost");
        //builder.SetIsOriginAllowed(origin => true);
    });
});

builder.Services.AddHttpLogging(logging =>
{
    logging.LoggingFields = HttpLoggingFields.All;
    logging.RequestHeaders.Add("sec-ch-ua");
    logging.RequestBodyLogLimit = 4096;
    logging.ResponseBodyLogLimit = 4096;

});

builder.WebHost.UseUrls("http://0.0.0.0:5337");

var app = builder.Build();
app.UseHttpLogging();
app.Use(async (context, next) =>
{
    await next();
});

REngine.SetEnvironmentVariables();
REngine engine = REngine.GetInstance();
object _lock = new Object();

Data data = new Data();
if(File.Exists("./upload/file"))
    data.File = true;

app.MapGet("/", () => {
    if (Monitor.TryEnter(_lock, 10))
    {
        try
        {
            string[] result = engine.Evaluate("'Hi there .NET, from the R engine'").AsCharacter().ToArray();
            data.Test = result[0];
        }
        finally
        {
            Monitor.Exit(_lock);
        }
    }
    return Results.Json(data);
});

app.MapPost("/upload", async (HttpRequest request) =>
{
    var filePath = Path.Combine("./upload/", "file");

    await using var writeStream = File.Create(filePath);
    await request.BodyReader.CopyToAsync(writeStream);
    data.File = true;
});

app.UseCors(devCorsPolicy);
app.MapControllers();
app.Run();

engine.Dispose();
