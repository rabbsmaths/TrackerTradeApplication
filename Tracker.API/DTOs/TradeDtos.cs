using System;
using System.Collections.Generic;

namespace Tracker.Api.Dtos;

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

public record TradeReportRowDto(
    string account,
    string symbol,
    long total_qty,
    decimal avg_price,
    decimal notional_base,
    string base_ccy
);

public record TradeReportResponseDto(
    DateTime from,
    DateTime to,
    IEnumerable<TradeReportRowDto> rows
);