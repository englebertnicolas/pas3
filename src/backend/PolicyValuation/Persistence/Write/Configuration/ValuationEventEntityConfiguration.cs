using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PAS.PolicyValuation.Domain.ValuationLedgerAggregate;

namespace PAS.PolicyValuation.Persistence.Write.Configuration;

internal class ValuationEventEntityConfiguration : IEntityTypeConfiguration<ValuationEvent>
{
    public void Configure(EntityTypeBuilder<ValuationEvent> builder)
    {
        builder.ToTable("ValuationEvents");
        builder.Ignore(e => e.DomainEvents);

        builder.HasKey(e => e.Id);

        builder.Property(p => p.TotalReservesInPolicyCurrency).HasPrecision(18, 4);
        builder.Property(p => p.TotalReservesInEur).HasPrecision(18, 4);

        builder.HasOne<ValuationLedger>()
           .WithMany(p => p.Events)
           .HasForeignKey(e => e.LedgerId)
           .OnDelete(DeleteBehavior.Cascade);

        builder.OwnsMany(e => e.Movements, mvtsBuilder =>
        {
            mvtsBuilder.ToTable("Movements");
            mvtsBuilder.WithOwner().HasForeignKey("EventId");
            mvtsBuilder.HasKey("Id");
            mvtsBuilder.Property<long>("Id").HasColumnOrder(0);
            mvtsBuilder.Property<ValuationEventId>("EventId").HasColumnOrder(1);
            mvtsBuilder.Property(e => e.Type).HasConversion<string>();
            mvtsBuilder.Property(p => p.Units).HasPrecision(28, 10).HasColumnName("Units");

            // ComplexProperty not supported on owned types, see open issue: https://github.com/dotnet/efcore/issues/33170
            // -> Using OwnsOne instead of ComplexProperty for now
            //    + adding .WithOwner() to avoid exception 'Unable to determine the owner for the relationship'.
            //resBuilder.ComplexProperty(e => e.Amount, valuationBuilder => {
            mvtsBuilder.OwnsOne(e => e.Amount, valuationBuilder =>
            {
                valuationBuilder.WithOwner();
                valuationBuilder.Property(p => p.InFundCurrency).HasPrecision(14, 4).HasColumnName("Amount");
                valuationBuilder.Property(p => p.InPolicyCurrency).HasPrecision(14, 4).HasColumnName("AmountInPolicyCurrency");
                valuationBuilder.Property(p => p.InEur).HasPrecision(14, 4).HasColumnName("AmountInEur");

            });

            mvtsBuilder.Property(e => e.Details)
                .HasConversion(
                    v => JsonSerializer.Serialize(v, JsonOptions.DatabaseDefault),
                    v => JsonSerializer.Deserialize<MovementDetails>(v, JsonOptions.DatabaseDefault)!
                )
                .HasColumnType("nvarchar(max)");
        });

        builder.OwnsMany(e => e.Reserves, resBuilder =>
        {
            resBuilder.ToTable("Reserves");
            resBuilder.WithOwner().HasForeignKey("EventId");
            resBuilder.HasKey("Id");
            resBuilder.Property<long>("Id").HasColumnOrder(0);
            resBuilder.Property<ValuationEventId>("EventId").HasColumnOrder(1);
            resBuilder.Property(p => p.Units).HasPrecision(28, 10).HasColumnName("Units");

            // ComplexProperty not supported on owned types, see open issue: https://github.com/dotnet/efcore/issues/33170
            // -> Using OwnsOne instead of ComplexProperty for now
            //    + adding .WithOwner() to avoid exception 'Unable to determine the owner for the relationship'.
            //resBuilder.ComplexProperty(e => e.Amount, valuationBuilder => {
            resBuilder.OwnsOne(e => e.Amount, valuationBuilder =>
            {
                valuationBuilder.WithOwner();
                valuationBuilder.Property(p => p.InFundCurrency).HasPrecision(14, 4).HasColumnName("Amount");
                valuationBuilder.Property(p => p.InPolicyCurrency).HasPrecision(14, 4).HasColumnName("AmountInPolicyCurrency");
                valuationBuilder.Property(p => p.InEur).HasPrecision(14, 4).HasColumnName("AmountInEur");
            });

            resBuilder.Property(e => e.Details)
                .HasConversion(
                    v => JsonSerializer.Serialize(v, JsonOptions.DatabaseDefault),
                    v => JsonSerializer.Deserialize<ReserveDetails>(v, JsonOptions.DatabaseDefault)!
                )
                .HasColumnType("nvarchar(max)");
        });

        builder.HasIndex(x => new { x.LedgerId, x.Date, x.OperationId }).IsUnique();
    }
}
