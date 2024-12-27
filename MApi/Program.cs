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
    builder.Host.ConfigureAppSettings();

    builder.Services.AddControllers().AddJsonOptions(options =>
    {
        // avoid circular references when returning JSON in the API
        options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
    });
    // Add services to the container.

    services.Configure<ApiBehaviorOptions>(options => options.SuppressModelStateInvalidFilter = true);
    services.AddMvc();
    services.AddCors(o =>
    {
        o.AddPolicy("AllowAll", build =>
        {
            build.AllowAnyOrigin()
                .AllowAnyMethod()
                .AllowAnyHeader();
        });
    });
    services.AddSwaggerGen(c =>
    {
        c.SwaggerDoc("v1", new OpenApiInfo()
        {
            Title = "My new job API",
            Version = "v1",
        });
    });

    // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddControllers().AddJsonOptions(options =>
    {
        // avoid circular references when returning JSON in the API
        options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
    });

    var config = new AppConfiguration();
    builder.Configuration.Bind(config);
    services.AddSingleton(config);
    services.AddSingleton<DuplicateFinder.Service.DuplicateFinder>();
    services.AddSingleton<MainClass>();
    //   var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
    //var svc = services.Configure<AppConfiguration>(builder.Configuration.GetRequiredSection("ApplicationConfiguration"));
    
}
// Created to support this class to be used for integration tests purpose

void ConfigureMiddleware(WebApplication app)
{
    builder.Host.ConfigureAppSettings();


    if (app.Environment.IsDevelopment())
    {

        app.UseDeveloperExceptionPage();
        app.UseSwagger();
        app.UseSwaggerUI(c =>
        {
            c.SwaggerEndpoint("/swagger/v1/swagger.json", "My new job API");
            c.RoutePrefix = String.Empty;
        });

    }
    app.UseHttpsRedirection();
    app.UseCors("AllowAll");
    app.UseRouting();
    app.UseAuthentication();
    app.UseEndpoints(endpoints =>
    {
        endpoints.MapControllers();
    });
    // Register your services/dependencies 
}