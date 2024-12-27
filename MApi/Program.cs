using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;
using DuplicateIndexer;
using MApi;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);
ConfigureServices(builder.Services);
var app = builder.Build();
 
ConfigureMiddleware(app);
app.Run();

void ConfigureServices(IServiceCollection services)
{
    // Configuration should be first
    builder.Host.ConfigureAppSettings();

    // Add core services
    services.AddMvc();
    services.AddControllers().AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
    });

    // API behavior and documentation
    services.Configure<ApiBehaviorOptions>(options =>
        options.SuppressModelStateInvalidFilter = true);
    services.AddEndpointsApiExplorer();
    services.AddSwaggerGen(c =>
    {
        c.SwaggerDoc("v1", new OpenApiInfo()
        {
            Title = "My new job API",
            Version = "v1",
        });
    });

    // CORS configuration
    services.AddCors(o =>
    {
        o.AddPolicy("AllowAll", build =>
        {
            build.AllowAnyOrigin()
                .AllowAnyMethod()
                .AllowAnyHeader();
        });
    });

    // Application configuration
    var config = new AppConfiguration();
    builder.Configuration.Bind(config);
    services.AddSingleton(config);

    // Application services
    services.AddSingleton<DuplicateFinder.Service.DuplicateFinder>();
    services.AddSingleton<MainClass>();
}

void ConfigureMiddleware(WebApplication app)
{
    if (app.Environment.IsDevelopment())
    {
        app.UseDeveloperExceptionPage();
        app.UseSwagger();
        app.UseSwaggerUI(c =>
        {
            c.SwaggerEndpoint("/swagger/v1/swagger.json", "My new job API");
            c.RoutePrefix = string.Empty;
        });
    }

    // Security and routing middleware
    app.UseHttpsRedirection();
    app.UseCors("AllowAll");
    app.UseRouting();
    app.UseAuthentication();
    app.MapControllers();


}