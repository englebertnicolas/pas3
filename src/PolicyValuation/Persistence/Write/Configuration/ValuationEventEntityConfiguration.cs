using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PAS.PolicyValuation.Domain.PolicyAggregate;

namespace PAS.PolicyValuation.Persistence.Write.Configuration;

internal class ValuationEventEntityConfiguration : IEntityTypeConfiguration<ValuationEvent> {

    public void Configure(EntityTypeBuilder<ValuationEvent> builder) {
        builder.ToTable("ValuationEvents");
        builder.Ignore(e => e.DomainEvents);

        builder.HasKey(e => e.Id);

        builder.Property(p => p.TotalReservesInPolicyCurrency).HasPrecision(18, 4);
        builder.Property(p => p.TotalReservesInEur).HasPrecision(18, 4);

        builder.HasOne<Policy>()
           .WithMany(p => p.Events)
           .HasForeignKey(e => e.PolicyValuationId)
           .OnDelete(DeleteBehavior.Cascade);

        builder.OwnsMany(e => e.Movements, mvtsBuilder => {
            mvtsBuilder.ToTable("ValuationMovements");
            mvtsBuilder.HasKey("Id");
            mvtsBuilder.Property<long>("Id").HasColumnOrder(0);
            mvtsBuilder.Property<ValuationEventId>("ValuationEventId").HasColumnOrder(1);
            mvtsBuilder.Property(e => e.Type).HasConversion<string>();

            // ComplexProperty not supported on owned types, see open issue: https://github.com/dotnet/efcore/issues/33170
            // -> Using OwnsOne instead of ComplexProperty for now
            //    + adding .WithOwner() to avoid exception 'Unable to determine the owner for the relationship'.
            //resBuilder.ComplexProperty(e => e.Valuation, valuationBuilder => {
            mvtsBuilder.OwnsOne(e => e.Valuation, valuationBuilder => {
                valuationBuilder.WithOwner();
                valuationBuilder.Property(p => p.Units).HasPrecision(28, 10).HasColumnName("Units");
                valuationBuilder.Property(p => p.AmountInFundCurrency).HasPrecision(18, 4).HasColumnName("AmountInFundCurrency");
                valuationBuilder.Property(p => p.AmountInPolicyCurrency).HasPrecision(18, 4).HasColumnName("AmountInPolicyCurrency");
                valuationBuilder.Property(p => p.AmountInEur).HasPrecision(18, 4).HasColumnName("AmountInEur");
                valuationBuilder.Property(p => p.RawAmountInFundCurrency).HasPrecision(28, 10).HasColumnName("RawAmountInFundCurrency");
                valuationBuilder.Property(p => p.RawAmountInPolicyCurrency).HasPrecision(28, 10).HasColumnName("RawAmountInPolicyCurrency");
                valuationBuilder.Property(p => p.RawAmountInEur).HasPrecision(28, 10).HasColumnName("RawAmountInEur");

                //pricingBuilder.ComplexProperty(p => p.Nav, navBuilder => {
                valuationBuilder.OwnsOne(p => p.Nav, navBuilder => {
                    navBuilder.WithOwner();
                    navBuilder.Property(n => n.Value).HasPrecision(28, 10).HasColumnName("NavValue");
                    navBuilder.Property(n => n.Date).HasColumnName("NavDate");
                });

                valuationBuilder.Property(p => p.NavValuationMode)
                    .HasConversion<string>()
                    .HasMaxLength(128)
                    .HasColumnName("NavValuationMode");

                //pricingBuilder.ComplexProperty(p => p.FundToPolicyFxRate, fxBuilder =>
                valuationBuilder.OwnsOne(p => p.FundToPolicyFxRate, fxBuilder => {
                    fxBuilder.WithOwner();
                    fxBuilder.Property(f => f.Value).HasPrecision(28, 10).HasColumnName("FundToPolicyFxRateValue");
                    fxBuilder.Property(f => f.Date).HasColumnName("FundToPolicyFxRateDate");
                });

                //pricingBuilder.ComplexProperty(p => p.PolicyToFundFxRate, fxBuilder =>
                valuationBuilder.OwnsOne(p => p.PolicyToFundFxRate, fxBuilder => {
                    fxBuilder.WithOwner();
                    fxBuilder.Property(f => f.Value).HasPrecision(28, 10).HasColumnName("PolicyToFundFxRateValue");
                    fxBuilder.Property(f => f.Date).HasColumnName("PolicyToFundFxRateDate");
                });

                //pricingBuilder.ComplexProperty(p => p.PolicyToEurFxRate, fxBuilder =>
                valuationBuilder.OwnsOne(p => p.PolicyToEurFxRate, fxBuilder => {
                    fxBuilder.WithOwner();
                    fxBuilder.Property(f => f.Value).HasPrecision(28, 10).HasColumnName("PolicyToEurFxRateValue");
                    fxBuilder.Property(f => f.Date).HasColumnName("PolicyToEurFxRateDate");
                });
            });
        });

        builder.OwnsMany(e => e.Reserves, resBuilder => {
            resBuilder.ToTable("ValuationReserves");
            resBuilder.HasKey("Id");
            resBuilder.Property<long>("Id").HasColumnOrder(0);
            resBuilder.Property<ValuationEventId>("ValuationEventId").HasColumnOrder(1);

            // ComplexProperty not supported on owned types, see open issue: https://github.com/dotnet/efcore/issues/33170
            // -> Using OwnsOne instead of ComplexProperty for now
            //    + adding .WithOwner() to avoid exception 'Unable to determine the owner for the relationship'.
            //resBuilder.ComplexProperty(e => e.Valuation, valuationBuilder => {
            resBuilder.OwnsOne(e => e.Valuation, valuationBuilder => {
                valuationBuilder.WithOwner();
                valuationBuilder.Property(p => p.Units).HasPrecision(28, 10).HasColumnName("Units");
                valuationBuilder.Property(p => p.AmountInFundCurrency).HasPrecision(18, 4).HasColumnName("AmountInFundCurrency");
                valuationBuilder.Property(p => p.AmountInPolicyCurrency).HasPrecision(18, 4).HasColumnName("AmountInPolicyCurrency");
                valuationBuilder.Property(p => p.AmountInEur).HasPrecision(18, 4).HasColumnName("AmountInEur");
                valuationBuilder.Property(p => p.RawAmountInFundCurrency).HasPrecision(28, 10).HasColumnName("RawAmountInFundCurrency");
                valuationBuilder.Property(p => p.RawAmountInPolicyCurrency).HasPrecision(28, 10).HasColumnName("RawAmountInPolicyCurrency");
                valuationBuilder.Property(p => p.RawAmountInEur).HasPrecision(28, 10).HasColumnName("RawAmountInEur");

                //pricingBuilder.ComplexProperty(p => p.Nav, navBuilder => {
                valuationBuilder.OwnsOne(p => p.Nav, navBuilder => {
                    navBuilder.WithOwner();
                    navBuilder.Property(n => n.Value).HasPrecision(28, 10).HasColumnName("NavValue");
                    navBuilder.Property(n => n.Date).HasColumnName("NavDate");
                });

                valuationBuilder.Ignore(e => e.NavValuationMode);

                //pricingBuilder.ComplexProperty(p => p.FundToPolicyFxRate, fxBuilder =>
                valuationBuilder.OwnsOne(p => p.FundToPolicyFxRate, fxBuilder => {
                    fxBuilder.WithOwner();
                    fxBuilder.Property(f => f.Value).HasPrecision(28, 10).HasColumnName("FundToPolicyFxRateValue");
                    fxBuilder.Property(f => f.Date).HasColumnName("FundToPolicyFxRateDate");
                });

                //pricingBuilder.ComplexProperty(p => p.PolicyToFundFxRate, fxBuilder =>
                valuationBuilder.OwnsOne(p => p.PolicyToFundFxRate, fxBuilder => {
                    fxBuilder.WithOwner();
                    fxBuilder.Property(f => f.Value).HasPrecision(28, 10).HasColumnName("PolicyToFundFxRateValue");
                    fxBuilder.Property(f => f.Date).HasColumnName("PolicyToFundFxRateDate");
                });

                //pricingBuilder.ComplexProperty(p => p.PolicyToEurFxRate, fxBuilder =>
                valuationBuilder.OwnsOne(p => p.PolicyToEurFxRate, fxBuilder => {
                    fxBuilder.WithOwner();
                    fxBuilder.Property(f => f.Value).HasPrecision(28, 10).HasColumnName("PolicyToEurFxRateValue");
                    fxBuilder.Property(f => f.Date).HasColumnName("PolicyToEurFxRateDate");
                });
            });
        });

        builder.HasIndex(x => new { x.PolicyValuationId, x.Date, x.OperationId }).IsUnique();
    }
}
