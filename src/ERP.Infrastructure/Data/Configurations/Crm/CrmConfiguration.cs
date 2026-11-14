using ERP.Domain.Core.Entities.Crm;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ERP.Infrastructure.Data.Configurations.Crm;

public class LeadConfiguration : IEntityTypeConfiguration<Lead>
{
    public void Configure(EntityTypeBuilder<Lead> builder)
    {
        builder.ToTable("CrmLeads");
        builder.HasKey(l => l.Id);

        builder.Property(l => l.Nome).HasMaxLength(200).IsRequired();
        builder.Property(l => l.EmpresaContato).HasMaxLength(200);
        builder.Property(l => l.CpfCnpj).HasMaxLength(20);
        builder.Property(l => l.Email).HasMaxLength(200);
        builder.Property(l => l.Telefone).HasMaxLength(30);
        builder.Property(l => l.Celular).HasMaxLength(30);
        builder.Property(l => l.Cargo).HasMaxLength(100);
        builder.Property(l => l.Observacoes).HasMaxLength(2000);
        builder.Property(l => l.ResponsavelId).HasMaxLength(100);
        builder.Property(l => l.ResponsavelNome).HasMaxLength(150);

        builder.HasOne(l => l.ClienteConvertido)
            .WithMany()
            .HasForeignKey(l => l.ClienteConvertidoId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(l => new { l.EmpresaId, l.Status });
        builder.HasIndex(l => l.ResponsavelId);
        builder.HasIndex(l => l.Email);
    }
}

public class OportunidadeConfiguration : IEntityTypeConfiguration<Oportunidade>
{
    public void Configure(EntityTypeBuilder<Oportunidade> builder)
    {
        builder.ToTable("CrmOportunidades");
        builder.HasKey(o => o.Id);

        builder.Property(o => o.Codigo).HasMaxLength(30).IsRequired();
        builder.Property(o => o.Titulo).HasMaxLength(200).IsRequired();
        builder.Property(o => o.Descricao).HasMaxLength(2000);
        builder.Property(o => o.ValorEstimado).HasPrecision(18, 2);
        builder.Property(o => o.ResponsavelId).HasMaxLength(100);
        builder.Property(o => o.ResponsavelNome).HasMaxLength(150);
        builder.Property(o => o.MotivoPerda).HasMaxLength(500);

        builder.HasOne(o => o.Lead)
            .WithMany(l => l.Oportunidades)
            .HasForeignKey(o => o.LeadId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(o => o.Cliente)
            .WithMany()
            .HasForeignKey(o => o.ClienteId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(o => o.Orcamento)
            .WithMany()
            .HasForeignKey(o => o.OrcamentoId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(o => o.PedidoVenda)
            .WithMany()
            .HasForeignKey(o => o.PedidoVendaId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(o => new { o.EmpresaId, o.Codigo }).IsUnique();
        builder.HasIndex(o => new { o.EmpresaId, o.Etapa });
        builder.HasIndex(o => o.ResponsavelId);
        builder.HasIndex(o => o.LeadId);
        builder.HasIndex(o => o.ClienteId);
    }
}

public class AtividadeCrmConfiguration : IEntityTypeConfiguration<AtividadeCrm>
{
    public void Configure(EntityTypeBuilder<AtividadeCrm> builder)
    {
        builder.ToTable("CrmAtividades");
        builder.HasKey(a => a.Id);

        builder.Property(a => a.Assunto).HasMaxLength(200).IsRequired();
        builder.Property(a => a.Descricao).HasMaxLength(2000);
        builder.Property(a => a.ResponsavelId).HasMaxLength(100);
        builder.Property(a => a.ResponsavelNome).HasMaxLength(150);

        builder.HasOne(a => a.Lead)
            .WithMany(l => l.Atividades)
            .HasForeignKey(a => a.LeadId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(a => a.Oportunidade)
            .WithMany(o => o.Atividades)
            .HasForeignKey(a => a.OportunidadeId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(a => new { a.EmpresaId, a.Concluida, a.DataPrevista });
        builder.HasIndex(a => a.LeadId);
        builder.HasIndex(a => a.OportunidadeId);
    }
}
