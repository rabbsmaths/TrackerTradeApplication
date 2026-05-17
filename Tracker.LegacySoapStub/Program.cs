using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using CoreWCF;
using CoreWCF.Configuration;
using Tracker.Core.Interfaces;
using CoreWCF.Channels;

namespace Tracker.LegacySoapStub
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // ============================================================================
            // KESTREL NETWORK CONFIGURATION
            // ============================================================================
            // Explicitly locks Kestrel down to port 8080 to match the infrastructure URL
            builder.WebHost.ConfigureKestrel(options =>
            {
                options.ListenLocalhost(8080);
            });

            // ============================================================================
            // COREWCF DEPENDENCY REGISTRATION
            // ============================================================================
            // Registers the underlying CoreWCF internal messaging pipelines and services
            builder.Services.AddServiceModelServices();

            var app = builder.Build();

            // Root diagnostics landing check for simple browser verification
            app.MapGet("/", () => "Legacy Exchange Rate SOAP Service Stub is active on Port 8080.");

            // ============================================================================
            // SOAP ENDPOINT MAPPING & VIRUTAL SVCS ROUTING
            // ============================================================================
            // Binds the shared core interface directly to a virtual '.svc' endpoint
            app.UseServiceModel(serviceBuilder =>
            {
                serviceBuilder.AddService<CurrencyExchangeSoapService>();
                serviceBuilder.AddServiceEndpoint<CurrencyExchangeSoapService, ICurrencyExchangeSoapContract>(
                    new BasicHttpBinding(BasicHttpSecurityMode.None),
                    "/LegacyEnrichmentService.svc"
                );
            });

            app.Run();
        }
    }
}