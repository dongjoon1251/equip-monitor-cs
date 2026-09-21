using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

// equip-monitor 를 Copilot Agent 에 노출하는 읽기 전용 MCP 서버 (stdio).
// 실행 중인 API(기본 http://localhost:8000, 환경변수 EQUIP_API) 를 HTTP 로 조회한다. VS Code: .vscode/mcp.json 의 "equip".
var builder = Host.CreateApplicationBuilder(args);
builder.Logging.ClearProviders(); // stdout 은 MCP 전용 — 로그로 더럽히지 않는다
builder.Services.AddSingleton(new HttpClient
{
    BaseAddress = new Uri(Environment.GetEnvironmentVariable("EQUIP_API") ?? "http://localhost:8000"),
    Timeout = TimeSpan.FromSeconds(5),
});
builder.Services.AddMcpServer().WithStdioServerTransport().WithToolsFromAssembly();
await builder.Build().RunAsync();
