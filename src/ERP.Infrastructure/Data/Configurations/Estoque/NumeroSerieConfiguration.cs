using ERP.Domain.Core.Entities.Estoque;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ERP.Infrastructure.Data.Configurations.Estoque;

public class NumeroSerieConfiguration : IEntityTypeConfiguration<NumeroSerie>
{
    public void Configure(EntityTypeBuilder<NumeroSerie> builder)
    {
        builder.ToTable("NumerosSerie");
        builder.HasKey(n => n.Id);

        builder.Property(n => n.Numero)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(n => n.Observacoes)
            .HasMaxLength(500);

        builder.HasOne(n => n.Produto)
            .WithMany()
            .HasForeignKey(n => n.ProdutoId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(n => n.Deposito)
            .WithMany()
            .HasForeignKey(n => n.DepositoId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(n => n.Lote)
            .WithMany()
            .HasForeignKey(n => n.LoteId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasIndex(n => new { n.EmpresaId, n.ProdutoId, n.Numero }).IsUnique();
        builder.HasIndex(n => new { n.EmpresaId, n.Status });
    }
}
