// using Microsoft.Azure.Functions.Worker.Builder;
// using Microsoft.Extensions.DependencyInjection;
// using Microsoft.Extensions.Hosting;

// var builder = FunctionsApplication.CreateBuilder(args);

// builder.ConfigureFunctionsWebApplication();


// // ==============================
// // Register your application services
// // ==============================
// builder.Services.AddSingleton<IServiceOptionService, ServiceOptionService>();
// builder.Services.AddSingleton<INfcTagService, NfcTagService>();
// builder.Services.AddSingleton<IServiceRecordService, ServiceRecordService>();

// builder.Services.AddSingleton<IServiceOptionRepository, ServiceOptionRepository>();
// builder.Services.AddSingleton<INfcTagRepository, NfcTagRepository>();
// builder.Services.AddSingleton<IServiceRecordRepository, ServiceRecordRepository>();

// builder.Services.AddSingleton<SqlConnectionFactory>();
// builder.Services.AddSingleton<CosmosConnectionFactory>();
// builder.Services.AddSingleton<BlobConnectionFactory>();

// // Application Insights isn't enabled by default. See https://aka.ms/AAt8mw4.
// // builder.Services
// //     .AddApplicationInsightsTelemetryWorkerService()
// //     .ConfigureFunctionsApplicationInsights();

// builder.Build().Run();


using Microsoft.Azure.Functions.Worker.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = FunctionsApplication.CreateBuilder(args);

builder.ConfigureFunctionsWebApplication();

builder.Services.AddSingleton<IServiceOptionService, ServiceOptionService>();
builder.Services.AddSingleton<INfcTagService, NfcTagService>();
builder.Services.AddSingleton<IServiceRecordService, ServiceRecordService>();

builder.Services.AddSingleton<IServiceOptionRepository, ServiceOptionRepository>();
builder.Services.AddSingleton<INfcTagRepository, NfcTagRepository>();
builder.Services.AddSingleton<IServiceRecordRepository, ServiceRecordRepository>();

builder.Services.AddSingleton<SqlConnectionFactory>();
builder.Services.AddSingleton<CosmosConnectionFactory>();
builder.Services.AddSingleton<BlobConnectionFactory>();

builder.Build().Run();
