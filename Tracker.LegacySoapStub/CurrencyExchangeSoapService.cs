using System.Threading.Tasks;

using Tracker.Core.Interfaces;

namespace Tracker.LegacySoapStub;

// This class implements the contract shared from your Core project
public class CurrencyExchangeSoapService : ICurrencyExchangeSoapContract
{
    public Task<decimal> GetConversionRateAsync(string fromCurrency, string toCurrency)
    {
   
        decimal rate = fromCurrency.ToUpper() switch
        {
            "EUR" => 1.09m,
            "ZAR" => 0.054m, 
            _ => 1.0m
        };

        return Task.FromResult(rate);
    }
}