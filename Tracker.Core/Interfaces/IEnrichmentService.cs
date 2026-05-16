using System.Threading.Tasks;

namespace Tracker.Core.Interfaces;

public interface IEnrichmentService
{
    Task<decimal> GetExchangeRateToDocBaseAsync(string fromCurrency);
}