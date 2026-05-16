using Microsoft.EntityFrameworkCore;

using System;

using Tracker.Core.Entities;

namespace Tracker.Infrastructure.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<Trade> Trades { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Trade>(entity =>
            {
                // Table configuration
                entity.ToTable("Trades");
                entity.HasKey(t => t.Id);

                // ExternalId: Deduplication
                entity.Property(t => t.ExternalId)
                      .HasMaxLength(100)
                      .IsRequired();

                entity.HasIndex(t => t.ExternalId)
                      .IsUnique(); // Strict relational guard against concurrent duplicates

                // Core fields
                entity.Property(t => t.Account)
                      .HasMaxLength(50)
                      .IsRequired();

                entity.Property(t => t.Symbol)
                      .HasMaxLength(20)
                      .IsRequired();

                // Side field (BUY or SELL)
                entity.Property(t => t.Side)
                      .HasMaxLength(10)
                      .IsRequired();

                entity.Property(t => t.Quantity)
                      .IsRequired();

                entity.Property(t => t.Price)
                      .HasPrecision(18, 4);

                entity.Property(t => t.TradeTime)
                      .HasColumnType("datetime2");

                // Currency strings (e.g., "EUR", "USD")
                entity.Property(t => t.Currency)
                      .HasMaxLength(10)
                      .IsRequired();

                entity.Property(t => t.BaseCurrency)
                      .HasMaxLength(10)
                      .IsRequired();

                // WCF Enrichment Fields
                entity.Property(t => t.BaseCurrencyRate)
                      .HasPrecision(18, 8); // Extracted conversion decimal factor from SOAP service

                entity.Property(t => t.NotionalBaseValue)
                      .HasPrecision(18, 4);

                // Index optimized for the database computed reporting task
                entity.HasIndex(t => new { t.TradeTime, t.Account, t.Symbol })
                      .HasDatabaseName("IX_Trades_ReportFiltering");
            });
        }
    }
}