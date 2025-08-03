using Ambev.DeveloperEvaluation.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Ambev.DeveloperEvaluation.ORM.Mapping;

public class SaleItemConfiguration : IEntityTypeConfiguration<SaleItem>
{
    public void Configure(EntityTypeBuilder<SaleItem> builder)
    {
        builder.ToTable("SaleItems");

        builder.HasKey("Id");
        builder.Property<Guid>("Id")
            .HasColumnType("uuid")
            .HasDefaultValueSql("gen_random_uuid()");

        builder.Property(si => si.SaleId)
            .IsRequired()
            .HasColumnType("uuid");

        builder.Property(si => si.ProductId)
            .IsRequired()
            .HasColumnType("uuid");

        builder.Property(si => si.ProductName)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(si => si.Quantity)
            .IsRequired()
            .HasColumnType("integer");

        builder.Property(si => si.UnitPrice)
            .IsRequired()
            .HasColumnType("decimal(18,2)");

        builder.Property(si => si.Discount)
            .IsRequired()
            .HasColumnType("decimal(5,2)")
            .HasDefaultValue(0);

        builder.Property(si => si.Total)
            .IsRequired()
            .HasColumnType("decimal(18,2)");

        builder.Property(si => si.IsCancelled)
            .IsRequired()
            .HasDefaultValue(false);

        // Indexes for performance
        builder.HasIndex(si => si.ProductId);
        builder.HasIndex(si => si.SaleId);
        builder.HasIndex(si => si.IsCancelled);

        // Constraints
        builder.HasCheckConstraint("CK_SaleItems_Quantity", "\"Quantity\" > 0 AND \"Quantity\" <= 20");
        builder.HasCheckConstraint("CK_SaleItems_UnitPrice", "\"UnitPrice\" > 0");
        builder.HasCheckConstraint("CK_SaleItems_Discount", "\"Discount\" >= 0 AND \"Discount\" <= 100");
        builder.HasCheckConstraint("CK_SaleItems_Total", "\"Total\" >= 0");
    }
}