namespace Tracker.Api.Dtos;

/// <summary>
/// An immutable data packet carrying raw trade ingestion parameters sent by external clients.
/// </summary>
public record CreateTradeDto(
    string external_id,
    string account,
    string symbol,
    string side,
    int quantity,
    decimal price,
    DateTime trade_time,
    string currency
);

/// <summary>
/// An immutable reporting row containing normalized, aggregated position metrics.
/// </summary>
public record TradeReportRowDto(
    string account,
    string symbol,
    long total_qty,
    decimal avg_price,
    decimal notional_base,
    string base_ccy
);

/// <summary>
/// A structured, read-only wrapper enclosing the complete time-series trade report payload.
/// </summary>
public record TradeReportResponseDto(
    DateTime from,
    DateTime to,
    IEnumerable<TradeReportRowDto> rows
);