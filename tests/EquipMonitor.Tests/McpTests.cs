using System.Reflection;
using ModelContextProtocol.Server;

namespace EquipMonitor.Tests;

public class McpTests
{
    [Fact]
    public void Mcp_ExposesReadOnlyTools()
    {
        var names = typeof(EquipMonitor.Mcp.EquipTools).GetMethods(BindingFlags.Public | BindingFlags.Static)
            .Select(m => m.GetCustomAttribute<McpServerToolAttribute>())
            .Where(a => a is not null).Select(a => a!.Name!).Order().ToList();
        Assert.Equal(["get_alarms", "get_recent_telemetry", "get_uptime", "list_devices"], names);
    }
}
