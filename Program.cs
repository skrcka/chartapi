var builder = WebApplication.CreateBuilder(args);
builder.WebHost.UseUrls("http://0.0.0.0:5337");
var app = builder.Build();

app.MapGet("/", () => "Hello World!");

app.Run();
