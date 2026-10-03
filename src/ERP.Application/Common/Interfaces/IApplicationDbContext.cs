using ERP.Domain.Core.Entities;
using ERP.Domain.Core.Entities.Cadastros;
using Microsoft.EntityFrameworkCore;

namespace ERP.Application.Common.Interfaces;

/// <summary>
/// Interface do DbContext principal para ser consumida pela camada Application sem depender de EF Core diretamente na infraestrutura.
/// </summary>
public interface IApplicationDbContext
{
    // Fase 1 - Base
    DbSet<Empresa> Empresas { get; }
    DbSet<UsuarioEmpresa> UsuarioEmpresas { get; }
    DbSet<Permissao> Permissoes { get; }
    DbSet<PapelPermissao> PapelPermissoes { get; }
    DbSet<AuditLog> AuditLogs { get; }
    DbSet<Notificacao> Notificacoes { get; }
    DbSet<Moeda> Moedas { get; }

    // Fase 2 - Cadastros
    DbSet<Cliente> Clientes { get; }
    DbSet<Fornecedor> Fornecedores { get; }
    DbSet<Produto> Produtos { get; }
    DbSet<Servico> Servicos { get; }
    DbSet<Categoria> Categorias { get; }
    DbSet<UnidadeMedida> UnidadesMedida { get; }
    DbSet<Endereco> Enderecos { get; }

    // Fase 3 - Financeiro
    DbSet<ERP.Domain.Core.Entities.Financeiro.Banco> Bancos { get; }
    DbSet<ERP.Domain.Core.Entities.Financeiro.ContaBancaria> ContasBancarias { get; }
    DbSet<ERP.Domain.Core.Entities.Financeiro.FormaPagamento> FormasPagamento { get; }
    DbSet<ERP.Domain.Core.Entities.Financeiro.PlanoConta> PlanosContas { get; }
    DbSet<ERP.Domain.Core.Entities.Financeiro.CentroCusto> CentrosCusto { get; }
    DbSet<ERP.Domain.Core.Entities.Financeiro.ContaPagar> ContasPagar { get; }
    DbSet<ERP.Domain.Core.Entities.Financeiro.ContaReceber> ContasReceber { get; }
    DbSet<ERP.Domain.Core.Entities.Financeiro.BaixaContaPagar> BaixasContasPagar { get; }
    DbSet<ERP.Domain.Core.Entities.Financeiro.BaixaContaReceber> BaixasContasReceber { get; }
    DbSet<ERP.Domain.Core.Entities.Financeiro.MovimentacaoFinanceira> MovimentacoesFinanceiras { get; }

    // Fase 3B - Conciliação Bancária
    DbSet<ERP.Domain.Core.Entities.Financeiro.ExtratoImportado> ExtratosImportados { get; }
    DbSet<ERP.Domain.Core.Entities.Financeiro.ExtratoImportadoItem> ExtratosImportadosItens { get; }

    // Fase 4 - Estoque
    DbSet<ERP.Domain.Core.Entities.Estoque.Deposito> Depositos { get; }
    DbSet<ERP.Domain.Core.Entities.Estoque.EstoqueProduto> EstoqueProdutos { get; }
    DbSet<ERP.Domain.Core.Entities.Estoque.MovimentacaoEstoque> MovimentacoesEstoque { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
