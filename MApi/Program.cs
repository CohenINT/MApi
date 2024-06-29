using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;
using DuplicateIndexer;
using MApi;


var builder = WebApplication.CreateBuilder(args);
ConfigureServices(builder.Services);
var app = builder.Build();
ConfigureMiddleware(app);
app.Run();

void ConfigureServices(IServiceCollection services)
{
    builder.Host.ConfigureAppSettings();

    builder.Services.AddControllers().AddJsonOptions(options =>
    {
        // avoid circular references when returning JSON in the API
        options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
    });
    // Add services to the container.

    services.Configure<ApiBehaviorOptions>(options => options.SuppressModelStateInvalidFilter = true);

    // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
    builder.Services.AddEndpointsApiExplorer();

    services.AddSingleton<DuplicateFinder.Service.DuplicateFinder>();
    services.AddSingleton<MainClass>();
    var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
}
// Created to support this class to be used for integration tests purpose

void ConfigureMiddleware(WebApplication app)
{
    builder.Host.ConfigureAppSettings();
    builder.Services.AddControllers().AddJsonOptions(options =>
    {
        // avoid circular references when returning JSON in the API
        options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
    });

    // Register your services/dependencies 
}