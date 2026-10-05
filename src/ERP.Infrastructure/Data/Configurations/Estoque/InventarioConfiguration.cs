using ERP.Domain.Core.Entities.Estoque;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ERP.Infrastructure.Data.Configurations.Estoque;

public class InventarioConfiguration : IEntityTypeConfiguration<Inventario>
{
    public void Configure(EntityTypeBuilder<Inventario> builder)
    {
        builder.ToTable("Inventarios");
        builder.HasKey(i => i.Id);

        builder.Property(i => i.Protocolo)
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(i => i.Responsavel)
            .HasMaxLength(150)
            .IsRequired();

        builder.Property(i => i.Observacao)
            .HasMaxLength(500);

        builder.HasOne(i => i.Deposito)
            .WithMany()
            .HasForeignKey(i => i.DepositoId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(i => i.Categoria)
            .WithMany()
            .HasForeignKey(i => i.CategoriaId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasMany(i => i.Itens)
            .WithOne(it => it.Inventario)
            .HasForeignKey(it => it.InventarioId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(i => new { i.EmpresaId, i.Protocolo }).IsUnique();
        builder.HasIndex(i => new { i.EmpresaId, i.Status });
    }
}
