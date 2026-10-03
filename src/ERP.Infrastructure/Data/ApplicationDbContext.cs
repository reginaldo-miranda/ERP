using System.Linq.Expressions;
using ERP.Application.Common.Interfaces;
using ERP.Domain.Common;
using ERP.Domain.Core.Entities;
using ERP.Domain.Core.Entities.Cadastros;
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

    // Fase 1 - Base
    public DbSet<Empresa> Empresas => Set<Empresa>();
    public DbSet<UsuarioEmpresa> UsuarioEmpresas => Set<UsuarioEmpresa>();
    public DbSet<Permissao> Permissoes => Set<Permissao>();
    public DbSet<PapelPermissao> PapelPermissoes => Set<PapelPermissao>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();
    public DbSet<Notificacao> Notificacoes => Set<Notificacao>();
    public DbSet<Moeda> Moedas => Set<Moeda>();

    // Fase 2 - Cadastros
    public DbSet<Cliente> Clientes => Set<Cliente>();
    public DbSet<Fornecedor> Fornecedores => Set<Fornecedor>();
    public DbSet<Produto> Produtos => Set<Produto>();
    public DbSet<Servico> Servicos => Set<Servico>();
    public DbSet<Categoria> Categorias => Set<Categoria>();
    public DbSet<UnidadeMedida> UnidadesMedida => Set<UnidadeMedida>();
    public DbSet<Endereco> Enderecos => Set<Endereco>();

    // Fase 3 - Financeiro
    public DbSet<ERP.Domain.Core.Entities.Financeiro.Banco> Bancos => Set<ERP.Domain.Core.Entities.Financeiro.Banco>();
    public DbSet<ERP.Domain.Core.Entities.Financeiro.ContaBancaria> ContasBancarias => Set<ERP.Domain.Core.Entities.Financeiro.ContaBancaria>();
    public DbSet<ERP.Domain.Core.Entities.Financeiro.FormaPagamento> FormasPagamento => Set<ERP.Domain.Core.Entities.Financeiro.FormaPagamento>();
    public DbSet<ERP.Domain.Core.Entities.Financeiro.PlanoConta> PlanosContas => Set<ERP.Domain.Core.Entities.Financeiro.PlanoConta>();
    public DbSet<ERP.Domain.Core.Entities.Financeiro.CentroCusto> CentrosCusto => Set<ERP.Domain.Core.Entities.Financeiro.CentroCusto>();
    public DbSet<ERP.Domain.Core.Entities.Financeiro.ContaPagar> ContasPagar => Set<ERP.Domain.Core.Entities.Financeiro.ContaPagar>();
    public DbSet<ERP.Domain.Core.Entities.Financeiro.ContaReceber> ContasReceber => Set<ERP.Domain.Core.Entities.Financeiro.ContaReceber>();
    public DbSet<ERP.Domain.Core.Entities.Financeiro.BaixaContaPagar> BaixasContasPagar => Set<ERP.Domain.Core.Entities.Financeiro.BaixaContaPagar>();
    public DbSet<ERP.Domain.Core.Entities.Financeiro.BaixaContaReceber> BaixasContasReceber => Set<ERP.Domain.Core.Entities.Financeiro.BaixaContaReceber>();
    public DbSet<ERP.Domain.Core.Entities.Financeiro.MovimentacaoFinanceira> MovimentacoesFinanceiras => Set<ERP.Domain.Core.Entities.Financeiro.MovimentacaoFinanceira>();

    // Fase 3B - Conciliação Bancária
    public DbSet<ERP.Domain.Core.Entities.Financeiro.ExtratoImportado> ExtratosImportados => Set<ERP.Domain.Core.Entities.Financeiro.ExtratoImportado>();
    public DbSet<ERP.Domain.Core.Entities.Financeiro.ExtratoImportadoItem> ExtratosImportadosItens => Set<ERP.Domain.Core.Entities.Financeiro.ExtratoImportadoItem>();

    // Fase 4 - Estoque
    public DbSet<ERP.Domain.Core.Entities.Estoque.Deposito> Depositos => Set<ERP.Domain.Core.Entities.Estoque.Deposito>();
    public DbSet<ERP.Domain.Core.Entities.Estoque.EstoqueProduto> EstoqueProdutos => Set<ERP.Domain.Core.Entities.Estoque.EstoqueProduto>();
    public DbSet<ERP.Domain.Core.Entities.Estoque.MovimentacaoEstoque> MovimentacoesEstoque => Set<ERP.Domain.Core.Entities.Estoque.MovimentacaoEstoque>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // Mapeamentos e Nomes de Tabelas - Fase 1
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

        // Aplicar todas as configurações Fluent API via IEntityTypeConfiguration (inclui Cadastros)
        builder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);

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
        builder.Entity<TEntity>().HasQueryFilter(e => _currentEmpresaService.EmpresaId == null || e.EmpresaId == _currentEmpresaService.EmpresaId);
    }
}
