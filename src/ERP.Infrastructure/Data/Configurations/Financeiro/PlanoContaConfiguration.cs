using ERP.Domain.Core.Entities.Financeiro;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ERP.Infrastructure.Data.Configurations.Financeiro;

public class PlanoContaConfiguration : IEntityTypeConfiguration<PlanoConta>
{
    public void Configure(EntityTypeBuilder<PlanoConta> builder)
    {
        builder.ToTable("PlanosContas");
        builder.HasKey(p => p.Id);
        builder.Property(p => p.Codigo).HasMaxLength(30).IsRequired();
        builder.Property(p => p.Descricao).HasMaxLength(150).IsRequired();

        builder.HasIndex(p => new { p.EmpresaId, p.Codigo }).IsUnique();

        builder.HasOne(p => p.PlanoContaPai)
            .WithMany(p => p.SubContas)
            .HasForeignKey(p => p.PlanoContaPaiId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
