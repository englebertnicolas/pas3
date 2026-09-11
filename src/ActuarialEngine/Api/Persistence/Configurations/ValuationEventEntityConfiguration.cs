using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PAS.ActuarialEngine.Domain.PolicyAggregate;

namespace PAS.ActuarialEngine.Persistence.Configurations;

internal class ValuationEventEntityConfiguration : IEntityTypeConfiguration<ValuationEvent> {

    public void Configure(EntityTypeBuilder<ValuationEvent> builder) {
        builder.ToTable("ValuationEvents");
        builder.Ignore(e => e.DomainEvents);

        builder.HasKey(e => e.Id);

        builder.HasOne<Policy>()
           .WithMany(p => p.Events)
           .HasForeignKey(e => e.PolicyId)
           .OnDelete(DeleteBehavior.Cascade);

        builder.OwnsMany(e => e.Movements, mvtsBuilder => {
            mvtsBuilder.ToTable("ValuationMovements");
            mvtsBuilder.HasKey("Id");
            mvtsBuilder.Property<long>("Id").HasColumnOrder(0);
            mvtsBuilder.Property<ValuationEventId>("ValuationEventId").HasColumnOrder(1);
            mvtsBuilder.Property(e => e.Units).HasPrecision(28, 10);

            // ComplexProperty not supported on owned types, see open issue: https://github.com/dotnet/efcore/issues/33170
            // -> Using OwnsOne instead of ComplexProperty for now
            //    + adding .WithOwner() to avoid exception 'Unable to determine the owner for the relationship'.
            //resBuilder.ComplexProperty(e => e.Pricing, pricingBuilder => {
            mvtsBuilder.OwnsOne(e => e.Pricing, pricingBuilder => {
                pricingBuilder.WithOwner();
                pricingBuilder.Property(p => p.AmountInFundCurrency).HasPrecision(28, 10).HasColumnName("AmountInFundCurrency");
                pricingBuilder.Property(p => p.AmountInPolicyCurrency).HasPrecision(28, 10).HasColumnName("AmountInPolicyCurrency");
                pricingBuilder.Property(p => p.AmountInEur).HasPrecision(28, 10).HasColumnName("AmountInEur");

                //pricingBuilder.ComplexProperty(p => p.Nav, navBuilder => {
                pricingBuilder.OwnsOne(p => p.Nav, navBuilder => {
                    navBuilder.WithOwner();
                    navBuilder.Property(n => n.Value).HasPrecision(28, 10).HasColumnName("NavValue");
                    navBuilder.Property(n => n.Date).HasColumnName("NavDate");
                });

                pricingBuilder.Property(p => p.NavValuationMode)
                    .HasConversion<string>()
                    .HasMaxLength(128)
                    .HasColumnName("NavValuationMode");

                //pricingBuilder.ComplexProperty(p => p.FundToPolicyFxRate, fxBuilder =>
                pricingBuilder.OwnsOne(p => p.FundToPolicyFxRate, fxBuilder => {
                    fxBuilder.WithOwner();
                    fxBuilder.Property(f => f.Value).HasPrecision(28, 10).HasColumnName("FundToPolicyFxRateValue");
                    fxBuilder.Property(f => f.Date).HasColumnName("FundToPolicyFxRateDate");
                });

                //pricingBuilder.ComplexProperty(p => p.PolicyToFundFxRate, fxBuilder =>
                pricingBuilder.OwnsOne(p => p.PolicyToFundFxRate, fxBuilder => {
                    fxBuilder.WithOwner();
                    fxBuilder.Property(f => f.Value).HasPrecision(28, 10).HasColumnName("PolicyToFundFxRateValue");
                    fxBuilder.Property(f => f.Date).HasColumnName("PolicyToFundFxRateDate");
                });

                //pricingBuilder.ComplexProperty(p => p.PolicyToEurFxRate, fxBuilder =>
                pricingBuilder.OwnsOne(p => p.PolicyToEurFxRate, fxBuilder => {
                    fxBuilder.WithOwner();
                    fxBuilder.Property(f => f.Value).HasPrecision(28, 10).HasColumnName("PolicyToEurFxRateValue");
                    fxBuilder.Property(f => f.Date).HasColumnName("PolicyToEurFxRateDate");
                });
            });
        });

        builder.OwnsMany(e => e.MathReserves, resBuilder => {
            resBuilder.ToTable("MathReserves");
            resBuilder.HasKey("Id");
            resBuilder.Property<long>("Id").HasColumnOrder(0);
            resBuilder.Property<ValuationEventId>("ValuationEventId").HasColumnOrder(1);
            resBuilder.Property(e => e.Units).HasPrecision(28, 10);

            // ComplexProperty not supported on owned types, see open issue: https://github.com/dotnet/efcore/issues/33170
            // -> Using OwnsOne instead of ComplexProperty for now
            //    + adding .WithOwner() to avoid exception 'Unable to determine the owner for the relationship'.
            //resBuilder.ComplexProperty(e => e.Pricing, pricingBuilder => {
            resBuilder.OwnsOne(e => e.Pricing, pricingBuilder => {
                pricingBuilder.WithOwner();
                pricingBuilder.Property(p => p.AmountInFundCurrency).HasPrecision(28, 10).HasColumnName("AmountInFundCurrency");
                pricingBuilder.Property(p => p.AmountInPolicyCurrency).HasPrecision(28, 10).HasColumnName("AmountInPolicyCurrency");
                pricingBuilder.Property(p => p.AmountInEur).HasPrecision(28, 10).HasColumnName("AmountInEur");

                //pricingBuilder.ComplexProperty(p => p.Nav, navBuilder => {
                pricingBuilder.OwnsOne(p => p.Nav, navBuilder => {
                    navBuilder.WithOwner();
                    navBuilder.Property(n => n.Value).HasPrecision(28, 10).HasColumnName("NavValue");
                    navBuilder.Property(n => n.Date).HasColumnName("NavDate");
                });

                pricingBuilder.Ignore(e => e.NavValuationMode);

                //pricingBuilder.ComplexProperty(p => p.FundToPolicyFxRate, fxBuilder =>
                pricingBuilder.OwnsOne(p => p.FundToPolicyFxRate, fxBuilder => {
                    fxBuilder.WithOwner();
                    fxBuilder.Property(f => f.Value).HasPrecision(28, 10).HasColumnName("FundToPolicyFxRateValue");
                    fxBuilder.Property(f => f.Date).HasColumnName("FundToPolicyFxRateDate");
                });

                //pricingBuilder.ComplexProperty(p => p.PolicyToFundFxRate, fxBuilder =>
                pricingBuilder.OwnsOne(p => p.PolicyToFundFxRate, fxBuilder => {
                    fxBuilder.WithOwner();
                    fxBuilder.Property(f => f.Value).HasPrecision(28, 10).HasColumnName("PolicyToFundFxRateValue");
                    fxBuilder.Property(f => f.Date).HasColumnName("PolicyToFundFxRateDate");
                });

                //pricingBuilder.ComplexProperty(p => p.PolicyToEurFxRate, fxBuilder =>
                pricingBuilder.OwnsOne(p => p.PolicyToEurFxRate, fxBuilder => {
                    fxBuilder.WithOwner();
                    fxBuilder.Property(f => f.Value).HasPrecision(28, 10).HasColumnName("PolicyToEurFxRateValue");
                    fxBuilder.Property(f => f.Date).HasColumnName("PolicyToEurFxRateDate");
                });
            });
        });

        builder.HasIndex(x => new { x.PolicyId, x.Date, x.OperationId }).IsUnique();
    }
}
