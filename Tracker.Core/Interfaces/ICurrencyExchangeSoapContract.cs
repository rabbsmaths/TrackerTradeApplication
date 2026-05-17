using System.ServiceModel;
using System.Threading.Tasks;

namespace Tracker.Core.Interfaces;

/// <summary>
/// Defines the explicit service contract boundary for the legacy back-office SOAP API.
/// This interface is shared between the infrastructure WCF client and the mock CoreWCF host.
/// </summary>
[ServiceContract(Namespace = "http://tracker.connect.services/legacy/enrichment")]
public interface ICurrencyExchangeSoapContract
{
    /// <summary>
    /// Communicates with the SOAP service engine to fetch the exchange conversion factor
    /// required to calculate the Notional Base Value in the target base currency.
    /// </summary>
    [OperationContract(
        Action = "http://tracker.connect.services/legacy/enrichment/ICurrencyExchangeSoapContract/GetConversionRate",
        ReplyAction = "http://tracker.connect.services/legacy/enrichment/ICurrencyExchangeSoapContract/GetConversionRateResponse")]
    Task<decimal> GetConversionRateAsync(string fromCurrency, string toCurrency);
}