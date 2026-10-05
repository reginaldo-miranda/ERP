using ERP.Domain.Core.Entities.Estoque;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ERP.Infrastructure.Data.Configurations.Estoque;

public class InventarioItemConfiguration : IEntityTypeConfiguration<InventarioItem>
{
    public void Configure(EntityTypeBuilder<InventarioItem> builder)
    {
        builder.ToTable("InventarioItens");
        builder.HasKey(it => it.Id);

        builder.Property(it => it.QuantidadeSistema)
            .HasPrecision(18, 4)
            .IsRequired();

        builder.Property(it => it.QuantidadeContada)
            .HasPrecision(18, 4)
            .IsRequired();

        builder.Property(it => it.CustoUnitario)
            .HasPrecision(18, 4)
            .HasDefaultValue(0);

        builder.Property(it => it.Observacao)
            .HasMaxLength(250);

        builder.HasOne(it => it.Produto)
            .WithMany()
            .HasForeignKey(it => it.ProdutoId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(it => it.Lote)
            .WithMany()
            .HasForeignKey(it => it.LoteId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasIndex(it => new { it.InventarioId, it.ProdutoId, it.LoteId });
    }
}
