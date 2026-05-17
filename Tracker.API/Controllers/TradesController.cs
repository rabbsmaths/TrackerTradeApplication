using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.EntityFrameworkCore;

using System.Data.Common;

using Tracker.Api.Dtos;
using Tracker.Core.Entities;
using Tracker.Core.Interfaces;
using Tracker.Infrastructure.Data;

namespace Tracker.Api.Controllers;

/// <summary>
/// Exposes HTTP API endpoints for upstream trade ingestion and back-office reporting.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class TradesController(AppDbContext db, IEnrichmentService enrichmentService, IConfiguration configuration) : ControllerBase
{
    /// <summary>
    /// Ingests a single trade from an upstream source, enriches it via legacy SOAP reference data, 
    /// and persists it securely inside SQL Server with strict concurrency safeguards.
    /// </summary>
    /// <param name="dto">The trade ingestion data payload submitted from the upstream system.</param>
    /// <returns>An IActionResult indicating the outcome of the transaction execution.</returns>
    [HttpPost("capture")]
    public async Task<IActionResult> CaptureTrade([FromBody] CreateTradeDto dto)
    {
        // Concurrency handling and duplication check
        bool exists = await db.Trades.AnyAsync(t => t.ExternalId == dto.external_id);
        if (exists)
        {
            return Ok(new
            {
                status = "DuplicateIgnored",
                message = $"Trade with External ID '{dto.external_id}' has already been sequentially captured and processed. Request safely ignored to ensure consistent, duplicate-free outcomes."
            });
        }

        decimal conversionRate = await enrichmentService.GetExchangeRateToDocBaseAsync(dto.currency);
        decimal computedNotionalBase = dto.quantity * dto.price * conversionRate;

        var trade = new Trade
        {
            ExternalId = dto.external_id,
            Account = dto.account,
            Symbol = dto.symbol,
            Side = dto.side,
            Quantity = dto.quantity,
            Price = dto.price,
            Currency = dto.currency,
            BaseCurrencyRate = conversionRate,
            NotionalBaseValue = computedNotionalBase,
            BaseCurrency = configuration["WcfSettings:TargetBaseCurrency"]??"USD"
        };

        try
        {
            db.Trades.Add(trade);
            await db.SaveChangesAsync();
            return Ok(trade);
        }
        //Catch SPECIFIC unique index constraints (simultaneous double-submissions)
        catch (DbUpdateException ex)
            when (ex.InnerException is Microsoft.Data.SqlClient.SqlException sqlEx
                  && (sqlEx.Number == 2601 || sqlEx.Number == 2627))
        {
            return Ok(new
            {
                status = "ConcurrencyInterception",
                message = $"Simultaneous duplicate injection for Trade '{dto.external_id}' was successfully intercepted at the database relational level. State integrity and system consistency maintained."
            });
        }
        //Catch ANY OTHER unexpected database failure 
        catch (DbUpdateException generalDbEx)
        {
            return BadRequest(new
            {
                status = "DatabaseError",
                message = "An unexpected database state error occurred while committing the entity timeline.",
                details = generalDbEx.Message
            });
        }
        //Global Catch-All
        catch (Exception criticalEx)
        {
            return BadRequest(new
            {
                status = "CriticalSystemError",
                message = "An unhandled execution routine collapse occurred within the tracking kernel engine core.",
                details = criticalEx.Message
            });
        }

    }

    /// <summary>
    /// Exposes a reporting HTTP API endpoint that retrieves trade data aggregated wholly on the database server.
    /// </summary>
    /// <param name="from">The inclusive start window constraint date for filtering historical data.</param>
    /// <param name="to">The inclusive end window constraint date for filtering historical data.</param>
    /// <returns>An object containing the structured summary matrices grouped cleanly by account and symbol.</returns>
    [HttpGet("report")]
    public async Task<ActionResult<TradeReportResponseDto>> GetReport(
    [FromQuery] DateTime from,
    [FromQuery] DateTime to)
    {
        if (to < from)
        {
            return BadRequest(new
            {
                error = "Invalid Range Parameters",
                message = $"The specified 'to' date parameters ({to:yyyy-MM-dd}) cannot be historically earlier than the requested 'from' date parameters ({from:yyyy-MM-dd})."
            });
        }

        try
        {
            // SQL Server set-based calculations (Executes wholly within Database Engine)
            var databaseQuery = db.Trades
                .Where(t => t.TradeTime >= from && t.TradeTime <= to)
                .GroupBy(t => new { t.Account, t.Symbol, t.BaseCurrency })
                .Select(g => new TradeReportRowDto(
                    g.Key.Account,
                    g.Key.Symbol,
                    g.Sum(x => x.Quantity),
                    g.Average(x => x.Price),
                    g.Sum(x => x.NotionalBaseValue),
                    g.Key.BaseCurrency
                ));


            var rows = await databaseQuery.ToListAsync();

            return Ok(new TradeReportResponseDto(from, to, rows));
        }
        //Catch specific data access errors (e.g., deadlocks, connection timeouts)
        catch (DbException dbEx)
        {
            
            return BadRequest(new
            {
                status = "DatabaseQueryError",
                message = "An error occurred while compiling or executing the analytical calculation matrix within the database server.",
                details = dbEx.Message
            });
        }
        //Catch-all
        catch (Exception criticalEx)
        {
            return BadRequest(new
            {
                status = "CriticalSystemError",
                message = "An unhandled pipeline crash occurred while preparing the trade report payload.",
                details = criticalEx.Message
            });
        }
    }
}