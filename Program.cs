using LogMate.Application.Interfaces;
using LogMate.Application.Services;
using LogMate.Infrastructure.Data;
using LogMate.Infrastructure.Data.Interfaces;
using LogMate.Infrastructure.Data.Repositories;
using LogMate.Middleware;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

var builder = FunctionsApplication.CreateBuilder(args);

builder.ConfigureFunctionsWebApplication();
builder.UseMiddleware<ExceptionHandlingMiddleware>();
builder.UseMiddleware<SqlInjectionMiddleware>();

// 1. Register the services first
builder.Services.AddApplicationInsightsTelemetryWorkerService();
builder.Services.ConfigureFunctionsApplicationInsights();

// 2. NOW modify or explicitly set the filter rules
builder.Services.Configure<LoggerFilterOptions>(options =>
{
    // Remove the hidden internal telemetry rules that force Warning level
    var rulesToRemove = options.Rules.Where(rule =>
        rule.ProviderName == "Microsoft.Extensions.Logging.ApplicationInsights.ApplicationInsightsLoggerProvider").ToList();
    
    foreach (var rule in rulesToRemove)
    {
        options.Rules.Remove(rule);
    }

    // Forcefully add an explicit rule allowing Information level for App Insights
    options.Rules.Add(new LoggerFilterRule(
        "Microsoft.Extensions.Logging.ApplicationInsights.ApplicationInsightsLoggerProvider", 
        null, 
        LogLevel.Information, 
        null));
});


builder.Services.AddSingleton<IServiceOptionService, ServiceOptionService>();
builder.Services.AddSingleton<INfcTagService, NfcTagService>();
builder.Services.AddSingleton<IServiceRecordService, ServiceRecordService>();
builder.Services.AddSingleton<IEmailService, EmailService>();
builder.Services.AddSingleton<IErrorHandlerService, ErrorHandlerService>();

builder.Services.AddSingleton<IServiceOptionRepository, ServiceOptionRepository>();
builder.Services.AddSingleton<INfcTagRepository, NfcTagRepository>();
builder.Services.AddSingleton<IServiceRecordRepository, ServiceRecordRepository>();

builder.Services.AddSingleton<SqlConnectionFactory>();
builder.Services.AddSingleton<CosmosConnectionFactory>();
builder.Services.AddSingleton<BlobConnectionFactory>();

builder.Build().Run();
