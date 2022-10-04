using RDotNet;
using Microsoft.AspNetCore.HttpLogging;
using Microsoft.AspNetCore.Http.Json;
using System.Text.Json;

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

builder.Services.Configure<JsonOptions>(options =>
{
    //options.SerializerOptions.ReferenceHandler = ReferenceHandler.Preserve;
    options.SerializerOptions.PropertyNamingPolicy = null;
    options.SerializerOptions.MaxDepth = 64;
    options.SerializerOptions.IncludeFields = true;
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
if(File.Exists("./upload/file.png"))
    data.File = true;

app.MapGet("/", () => {
    if (Monitor.TryEnter(_lock, 10))
    {
        GenericVector res;
        try
        {
            using (FileStream fs = File.OpenRead("./scripts/Histogram.R"))
            {
                res = engine.Evaluate(fs).AsList();
            }
        }
        finally
        {
            Monitor.Exit(_lock);
        }
        data.Rdata.Names = res.Names.ToList();
        data.Rdata.Vectors.Clear();
        for (int i = 0; i < res.Length; i++)
        {
            data.Rdata.Vectors.Add(res[i].AsVector().ToList<dynamic>());
        }
        data.TestData = "test";
    }
    //return JsonSerializer.Serialize(data);
   return Results.Json(data, null, "application/json", 200);
});

app.MapPost("/upload", async (HttpRequest request) =>
{
    var filePath = Path.Combine("./upload/", "file.png");

    await using var writeStream = File.Create(filePath);
    await request.BodyReader.CopyToAsync(writeStream);
    data.File = true;
});

app.UseCors(devCorsPolicy);
app.MapControllers();
app.Run();

engine.Dispose();
