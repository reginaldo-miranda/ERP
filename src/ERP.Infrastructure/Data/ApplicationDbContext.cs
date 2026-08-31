using System.Linq.Expressions;
using ERP.Application.Common.Interfaces;
using ERP.Domain.Common;
using ERP.Domain.Core.Entities;
using ERP.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace ERP.Infrastructure.Data;

/// <summary>
/// DbContext principal do sistema ERP.
/// Herda de IdentityDbContext para gerenciamento de usuários/papéis e aplica filtro de multi-tenancy global.
/// </summary>
public class ApplicationDbContext : IdentityDbContext<ApplicationUser>, IApplicationDbContext
{
    private readonly ICurrentEmpresaService _currentEmpresaService;

    public ApplicationDbContext(
        DbContextOptions<ApplicationDbContext> options,
        ICurrentEmpresaService currentEmpresaService)
        : base(options)
    {
        _currentEmpresaService = currentEmpresaService;
    }

    public DbSet<Empresa> Empresas => Set<Empresa>();
    public DbSet<UsuarioEmpresa> UsuarioEmpresas => Set<UsuarioEmpresa>();
    public DbSet<Permissao> Permissoes => Set<Permissao>();
    public DbSet<PapelPermissao> PapelPermissoes => Set<PapelPermissao>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();
    public DbSet<Notificacao> Notificacoes => Set<Notificacao>();
    public DbSet<Moeda> Moedas => Set<Moeda>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // Mapeamentos e Nomes de Tabelas
        builder.Entity<Empresa>(entity =>
        {
            entity.ToTable("Empresas");
            entity.HasIndex(e => e.Cnpj).IsUnique();
        });

        builder.Entity<UsuarioEmpresa>(entity =>
        {
            entity.ToTable("UsuarioEmpresas");
            entity.HasIndex(ue => new { ue.UsuarioId, ue.EmpresaId }).IsUnique();
        });

        builder.Entity<Permissao>(entity =>
        {
            entity.ToTable("Permissoes");
            entity.HasIndex(p => p.Codigo).IsUnique();
        });

        builder.Entity<PapelPermissao>(entity =>
        {
            entity.ToTable("PapelPermissoes");
            entity.HasIndex(pp => new { pp.PapelId, pp.PermissaoId }).IsUnique();
        });

        builder.Entity<AuditLog>(entity =>
        {
            entity.ToTable("AuditLogs");
            entity.HasIndex(a => a.EmpresaId);
            entity.HasIndex(a => a.TableName);
            entity.HasIndex(a => a.Timestamp);
        });

        builder.Entity<Notificacao>(entity =>
        {
            entity.ToTable("Notificacoes");
            entity.HasIndex(n => n.UsuarioId);
            entity.HasIndex(n => n.EmpresaId);
        });

        builder.Entity<Moeda>(entity =>
        {
            entity.ToTable("Moedas");
            entity.HasIndex(m => m.Codigo).IsUnique();
        });

        // Configuração do Filtro Global de Multi-Tenancy (EmpresaId)
        foreach (var entityType in builder.Model.GetEntityTypes())
        {
            if (typeof(BaseEmpresaEntity).IsAssignableFrom(entityType.ClrType))
            {
                var method = typeof(ApplicationDbContext)
                    .GetMethod(nameof(ConfigureMultiTenancyFilter), System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                    ?.MakeGenericMethod(entityType.ClrType);

                method?.Invoke(this, new object[] { builder });
            }
        }
    }

    private void ConfigureMultiTenancyFilter<TEntity>(ModelBuilder builder) where TEntity : BaseEmpresaEntity
    {
        builder.Entity<TEntity>().HasQueryFilter(e => !_currentEmpresaService.EmpresaId.HasValue || e.EmpresaId == _currentEmpresaService.EmpresaId.Value);
    }
}
