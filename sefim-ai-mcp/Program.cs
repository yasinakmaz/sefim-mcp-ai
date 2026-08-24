var builder = Host.CreateApplicationBuilder(new HostApplicationBuilderSettings
{
    Args = args,
    ContentRootPath = AppContext.BaseDirectory
});

builder.Logging.AddConsole(o => o.LogToStandardErrorThreshold = LogLevel.Trace);

builder.Services.AddSqlService(builder.Configuration);

builder.Services.AddSingleton<IStock, Stock>();

builder.Services.AddSingleton<Stock>();

var jsonSerializerOptions = new System.Text.Json.JsonSerializerOptions(
    SefimMcp.Json.AppJsonSerializerContext.Default.Options);

builder.Services
    .AddMcpServer()
    .WithStdioServerTransport()
    .WithTools<Stock>(jsonSerializerOptions);

await builder.Build().RunAsync();
