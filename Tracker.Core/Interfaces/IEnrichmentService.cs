namespace Tracker.Core.Interfaces;

/// <summary>
/// The internal domain abstraction used by the API controller. 
/// It isolates your core app logic from needing to know about underlying SOAP protocols.
/// </summary>
public interface IEnrichmentService
{
    Task<decimal> GetExchangeRateToDocBaseAsync(string fromCurrency);
}