using System;
using System.ServiceModel;
using System.Threading.Tasks;

using Microsoft.Extensions.Configuration;

using Tracker.Core.Interfaces;

namespace Tracker.Infrastructure.Services;

public class WcfEnrichmentService : IEnrichmentService
{
    private readonly ChannelFactory<ICurrencyExchangeSoapContract> _channelFactory;

    public WcfEnrichmentService(IConfiguration configuration)
    {
        string endpointUrl = configuration["WcfSettings:ServiceUrl"]??"";

        var binding = new BasicHttpBinding(BasicHttpSecurityMode.None);
        var endpoint = new EndpointAddress(endpointUrl);
        _channelFactory = new ChannelFactory<ICurrencyExchangeSoapContract>(binding, endpoint);
    }

    public async Task<decimal> GetExchangeRateToDocBaseAsync(string fromCurrency)
    {
        if (string.Equals(fromCurrency, "USD", StringComparison.OrdinalIgnoreCase)) return 1.0m;

        ICurrencyExchangeSoapContract client = null!;
        try
        {
            client = _channelFactory.CreateChannel();
            return await client.GetConversionRateAsync(fromCurrency, "USD");
        }
        catch (CommunicationException)
        {
            return fromCurrency.ToUpper() switch
            {
                "EUR" => 1.09m,
                "ZAR" => 0.054m,
                _ => 1.0m
            };
        }
        finally
        {
            (client as IDisposable)?.Dispose();
        }
    }
}

[ServiceContract]
public interface ICurrencyExchangeSoapContract
{
    [OperationContract]
    Task<decimal> GetConversionRateAsync(string fromCurrency, string toCurrency);
}