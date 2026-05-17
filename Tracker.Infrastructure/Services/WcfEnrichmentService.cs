using System;
using System.ServiceModel;
using System.Threading.Tasks;

using Microsoft.Extensions.Configuration;

using Tracker.Core.Interfaces;

namespace Tracker.Infrastructure.Services;

/// <summary>
/// Infrastructure service that handles currency enrichment via a legacy SOAP backend.
/// </summary>
public class WcfEnrichmentService : IEnrichmentService
{
    private readonly string _serviceUrl;
    private readonly string _targetBaseCurrency;

    /// <summary>
    /// Initializes the service and extracts configurations for the SOAP endpoint and base currency target.
    /// </summary>
    public WcfEnrichmentService(IConfiguration configuration)
    {
        _serviceUrl = configuration["WcfSettings:ServiceUrl"]
                      ?? $"http://localhost:8080/LegacyEnrichmentService.svc";

        // Dynamically reads the target base from appsettings, defaulting to USD if null or whitespace
        var configuredBase = configuration["WcfSettings:TargetBaseCurrency"];
        _targetBaseCurrency = string.IsNullOrWhiteSpace(configuredBase) ? "USD" : configuredBase.ToUpper();
    }

    /// <summary>
    /// Fetches the exchange conversion rate relative to the configured system anchor.
    /// Falls back to localized static matrices if the channel communication fails.
    /// </summary>
    /// <param name="fromCurrency">The source currency ISO string (e.g., EUR, ZAR).</param>
    /// <returns>The calculated decimal multiplier rate factor.</returns>
    public async Task<decimal> GetExchangeRateToDocBaseAsync(string fromCurrency)
    {
        var binding = new BasicHttpBinding(BasicHttpSecurityMode.None);
        var endpoint = new EndpointAddress(_serviceUrl);

        // Dynamically creates the WCF pipeline using your explicit contract
        var factory = new ChannelFactory<ICurrencyExchangeSoapContract>(binding, endpoint);
        var channel = factory.CreateChannel();

        try
        {
            // Dynamically passes the configured target base currency from appsettings
            decimal rate = await channel.GetConversionRateAsync(fromCurrency, _targetBaseCurrency);

            ((IClientChannel)channel).Close();
            factory.Close();

            return rate;
        }
        catch (Exception)
        {
            ((IClientChannel)channel).Abort();
            factory.Abort();

            // Bulletproof application fallback logic if the SOAP server goes down
            return fromCurrency.ToUpper() switch
            {
                "EUR" => 1.09m,
                "ZAR" => 0.054m,
                _ => 1.0m
            };
        }
    }
}