using System.Text.Json;
using System.Text.Json.Serialization;
using EquipMonitor.Alarms;
using EquipMonitor.Api;
using EquipMonitor.Store;

// 실행: dotnet run --project src/EquipMonitor   (http://localhost:8000)
var builder = WebApplication.CreateBuilder(args);
builder.WebHost.UseUrls("http://localhost:8000");
builder.Services.AddSingleton<InMemoryStore>();
builder.Services.AddSingleton<AlarmEngine>(sp => new AlarmEngine(sp.GetRequiredService<InMemoryStore>()));
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.ConfigureHttpJsonOptions(o =>
{
    o.SerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower;  // Python 판과 와이어 호환
    o.SerializerOptions.DictionaryKeyPolicy = null;
    o.SerializerOptions.Converters.Add(new JsonStringEnumConverter());
});

var app = builder.Build();
app.UseSwagger();
app.UseSwaggerUI(o =>  // Python 판과 같은 http://localhost:8000/docs
{
    o.RoutePrefix = "docs";
    o.SwaggerEndpoint("/swagger/v1/swagger.json", "EquipMonitor v1");
});
app.MapGet("/health", () => new { status = "ok" });
app.MapDevices();
app.MapTelemetry();
app.MapAlarms();
app.Run();

public partial class Program { }
