using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

// equip-monitor 를 Copilot Agent 에 노출하는 읽기 전용 MCP 서버 (stdio, 또는 --http).
// 실행 중인 API(기본 http://localhost:8000, 환경변수 EQUIP_API) 를 HTTP 로 조회한다. VS Code: .vscode/mcp.json 의 "equip".
var apiKey = Environment.GetEnvironmentVariable("EQUIP_API_KEY");
if (string.IsNullOrEmpty(apiKey))
{
    Console.Error.WriteLine("EQUIP_API_KEY 가 없습니다 — .vscode/mcp.json 의 inputs 로 입력하세요.");
    return 1;
}
var http = new HttpClient(new ApiErrorHandler { InnerHandler = new HttpClientHandler() })
{
    BaseAddress = new Uri(Environment.GetEnvironmentVariable("EQUIP_API") ?? "http://localhost:8000"),
    Timeout = TimeSpan.FromSeconds(5),
};
http.DefaultRequestHeaders.Add("X-API-Key", apiKey);

if (args.Contains("--http"))
{
    // 원격(http) 방식: 먼저 띄워 두고 VS Code 는 URL + X-API-Key 헤더로 접속한다
    var web = WebApplication.CreateBuilder(args);
    web.WebHost.UseUrls($"http://127.0.0.1:{Environment.GetEnvironmentVariable("EQUIP_MCP_PORT") ?? "8001"}");
    web.Services.AddSingleton(http);
    web.Services.AddMcpServer().WithHttpTransport().WithToolsFromAssembly();
    var app = web.Build();
    var expected = System.Text.Encoding.UTF8.GetBytes(apiKey);
    app.Use(async (ctx, next) =>
    {
        var given = System.Text.Encoding.UTF8.GetBytes(ctx.Request.Headers["X-API-Key"].ToString());
        if (!System.Security.Cryptography.CryptographicOperations.FixedTimeEquals(given, expected))
        {
            ctx.Response.StatusCode = 401;
            await ctx.Response.WriteAsJsonAsync(new { detail = "invalid or missing X-API-Key" });
            return;
        }
        await next();
    });
    app.MapMcp("/mcp");
    await app.RunAsync();
    return 0;
}

// stdio: VS Code 가 직접 이 프로세스를 실행
var builder = Host.CreateApplicationBuilder(args);
builder.Logging.ClearProviders(); // stdout 은 MCP 전용 — 로그로 더럽히지 않는다
builder.Services.AddSingleton(http);
builder.Services.AddMcpServer().WithStdioServerTransport().WithToolsFromAssembly();
await builder.Build().RunAsync();
return 0;

// 일반 예외는 모델에 "An error occurred" 류만 보인다 → 원인을 McpException 으로 알려 준다
sealed class ApiErrorHandler : DelegatingHandler
{
    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken ct)
    {
        HttpResponseMessage r;
        try { r = await base.SendAsync(request, ct); }
        catch (HttpRequestException) { throw new ModelContextProtocol.McpException($"equip-monitor API({request.RequestUri?.GetLeftPart(UriPartial.Authority)})에 연결할 수 없다 — 서버가 꺼져 있다."); }
        if (r.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            throw new ModelContextProtocol.McpException("API 키가 틀렸다(401) — mcp.json inputs 의 Edit 로 다시 입력해야 한다.");
        return r;
    }
}

// .NET 10 웹 SDK 가 Program 을 public 으로 생성하면 테스트에서 EquipMonitor 의 Program 과 충돌하므로 internal 로 고정.
internal partial class Program { }
