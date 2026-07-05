using LogMate.Application.Interfaces;
using LogMate.Application.Services;
using LogMate.Infrastructure.Data;
using LogMate.Infrastructure.Data.Interfaces;
using LogMate.Infrastructure.Data.Repositories;
using LogMate.Middleware;
using Microsoft.Azure.Functions.Worker.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = FunctionsApplication.CreateBuilder(args);

builder.ConfigureFunctionsWebApplication();
builder.UseMiddleware<ExceptionHandlingMiddleware>();
builder.UseMiddleware<SqlInjectionMiddleware>();

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
