var builder = Host.CreateApplicationBuilder(new HostApplicationBuilderSettings
{
    Args = args,
    ContentRootPath = AppContext.BaseDirectory
});

builder.Logging.AddConsole(o => o.LogToStandardErrorThreshold = LogLevel.Trace);

builder.Services.AddSqlService(builder.Configuration);

builder.Services.AddSingleton<IStock, Stock>();

builder.Services.AddSingleton<Stock>();

builder.Services.AddSingleton<ICustomer, Customers>();

builder.Services.AddSingleton<Customers>();

builder.Services.AddSingleton<IUser, Users>();

builder.Services.AddSingleton<Users>();

builder.Services.AddSingleton<ITransactions, Transactions>();

builder.Services.AddSingleton<Transactions>();

builder.Services.AddSingleton<ITableGroup, TableGroups>();

builder.Services.AddSingleton<TableGroups>();

builder.Services.AddSingleton<ICampaign, Campaigns>();

builder.Services.AddSingleton<Campaigns>();

var jsonSerializerOptions = new System.Text.Json.JsonSerializerOptions(
    SefimMcp.Json.AppJsonSerializerContext.Default.Options);

builder.Services
    .AddMcpServer()
    .WithStdioServerTransport()
    .WithTools<Stock>(jsonSerializerOptions)
    .WithTools<Customers>(jsonSerializerOptions)
    .WithTools<Users>(jsonSerializerOptions)
    .WithTools<Transactions>(jsonSerializerOptions)
    .WithTools<TableGroups>(jsonSerializerOptions)
    .WithTools<Campaigns>(jsonSerializerOptions);

await builder.Build().RunAsync();
