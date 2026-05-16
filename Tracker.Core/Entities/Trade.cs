using System;

namespace Tracker.Core.Entities
{
    public class Trade
    {
        public int Id { get; set; }
        public string ExternalId { get; set; } = string.Empty;
        public string Account { get; set; } = string.Empty;
        public string Symbol { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public string Side { get; set; } = string.Empty; 
        public decimal Price { get; set; }
        public DateTime TradeTime { get; set; }
        public string Currency { get; set; } = string.Empty; 
        public string BaseCurrency { get; set; } = string.Empty;
        public decimal BaseCurrencyRate { get; set; } 
        public decimal NotionalBaseValue { get; set; } 
    }
}