using System;
using System.IO;

using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

using Tracker.Core.Interfaces;
using Tracker.Infrastructure.Data;
using Tracker.Infrastructure.Services;

namespace Tracker.API
{
    public class Program
    {
        public static void Main(string[] args)
        {
            // ============================================================================
            // LEAD-LEVEL SOLUTION: DYNAMIC SOURCE PROJECT ROOT DIRECTION FOR LOCALDB
            // ============================================================================
            // Navigates backward from the active 'bin\Debug\net8.0\' directory to anchor 
            // the |DataDirectory| macro straight to your source-controlled 'App_Data' folder.
            string baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
            string dataDirectoryPath;

            if (baseDirectory.Contains($"{Path.DirectorySeparatorChar}bin{Path.DirectorySeparatorChar}"))
            {
                // Isolate the project root folder prior to the execution binaries path block
                string projectRoot = baseDirectory.Substring(0, baseDirectory.IndexOf($"{Path.DirectorySeparatorChar}bin{Path.DirectorySeparatorChar}"));
                dataDirectoryPath = Path.Combine(projectRoot, "App_Data");
            }
            else
            {
                // Fallback architecture safety check if deployed via publish artifacts direct
                dataDirectoryPath = Path.Combine(baseDirectory, "App_Data");
            }

            // Self-healing check: Ensure the localized schema directory exists securely
            if (!Directory.Exists(dataDirectoryPath))
            {
                Directory.CreateDirectory(dataDirectoryPath);
            }

            // Bind the corrected system path variable directly to the relational string macro
            AppDomain.CurrentDomain.SetData("DataDirectory", dataDirectoryPath);
            // ============================================================================

            // Initialize the web application host builder
            var builder = WebApplication.CreateBuilder(args);

            // Register SQL Server Infrastructure with your AppDbContext
            builder.Services.AddDbContext<AppDbContext>(options =>
                options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

            // Register your SOAP/WCF Enrichment Client Service
            builder.Services.AddScoped<IEnrichmentService, WcfEnrichmentService>();

            // Configure core Web API services and OpenAPI/Swagger documentation
            builder.Services.AddControllers();
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            // Build the application pipeline
            var app = builder.Build();

            // Configure development-only features like Swagger UI
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger(options =>
                {
                    options.OpenApiVersion = Microsoft.OpenApi.OpenApiSpecVersion.OpenApi2_0;
                });
                app.UseSwaggerUI();
            }

            // Configure HTTP middleware routing, transport security, and endpoints
            app.UseHttpsRedirection();
            app.UseAuthorization();
            app.MapControllers();

            // Run the application listening for incoming requests
            app.Run();
        }
    }
}