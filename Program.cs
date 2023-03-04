using RDotNet;
using Microsoft.AspNetCore.HttpLogging;
using Microsoft.AspNetCore.Http.Json;

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

var filePath = Path.Combine("./upload/", "file.png");
string histScript = File.ReadAllText(@"./scripts/Histogram.R");

Data data = new Data();
if(File.Exists("./upload/file.png"))
    data.File = true;

app.MapGet("/", () => {
    return Results.Json(data, null, "application/json", 200);
});

app.MapGet("/histogram", () => {
    if (Monitor.TryEnter(_lock, 10))
    {
        GenericVector res;
        try
        {
            res = engine.Evaluate(histScript).AsList();
        }
        finally
        {
            Monitor.Exit(_lock);
        }

        data.Func = "histogram";
        data.RData.Names = res.Names.ToList();
        data.RData.Vectors.Clear();
        for (int i = 0; i < res.Length; i++)
        {
            data.RData.Vectors.Add(res[i].AsVector().ToList<dynamic>());
        }
        return Results.Ok();
    }
    return Results.Problem("REngine not ready!");
});

app.MapPost("/upload", async (HttpRequest request) =>
{
    if (!request.HasFormContentType)
        return Results.BadRequest();

    var form = await request.ReadFormAsync();
    var formFile = form.Files["file"];

    if (formFile is null || formFile.Length == 0)
        return Results.BadRequest();

    using(Stream stream = formFile.OpenReadStream()){

        var reader = new StreamContent(stream);
        
        using (FileStream fs = File.Create(filePath))
        {
            await reader.CopyToAsync(fs);
        }
    }

    data.File = true;
    return Results.Ok();
});

app.MapGet("/reset", () =>
{
    File.Delete(filePath);
    data.File = false;
    data.Func = "";
    data.RData.Names.Clear();
    data.RData.Vectors.Clear();
    Results.Ok();
});

app.MapGet("/resetfn", () =>
{
    data.Func = "";
    data.RData.Names.Clear();
    data.RData.Vectors.Clear();
    Results.Ok();
});

app.UseCors(devCorsPolicy);
app.MapControllers();
app.Run();

engine.Dispose();
