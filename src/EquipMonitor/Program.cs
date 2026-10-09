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
var apiKey = app.Configuration["EQUIP_API_KEY"]; // 환경변수 EQUIP_API_KEY 도 여기로 들어온다
if (!string.IsNullOrEmpty(apiKey))
{
    // 조회(GET)만 키 검사 — 게이트웨이/시뮬레이터 수집(POST)은 그대로
    var expected = System.Text.Encoding.UTF8.GetBytes(apiKey);
    app.Use(async (ctx, next) =>
    {
        var given = System.Text.Encoding.UTF8.GetBytes(ctx.Request.Headers["X-API-Key"].ToString());
        if (HttpMethods.IsGet(ctx.Request.Method) && ctx.Request.Path != "/health"
            && !System.Security.Cryptography.CryptographicOperations.FixedTimeEquals(given, expected))
        {
            ctx.Response.StatusCode = 401;
            await ctx.Response.WriteAsJsonAsync(new { detail = "invalid or missing X-API-Key" });
            return;
        }
        await next();
    });
}
app.MapGet("/health", () => new { status = "ok" });
app.MapDevices();
app.MapTelemetry();
app.MapAlarms();
app.MapReport();
app.Run();

public partial class Program { }
