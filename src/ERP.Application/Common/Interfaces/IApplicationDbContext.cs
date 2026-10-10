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
    DbSet<ERP.Domain.Core.Entities.Estoque.Lote> Lotes { get; }
    DbSet<ERP.Domain.Core.Entities.Estoque.NumeroSerie> NumerosSerie { get; }
    DbSet<ERP.Domain.Core.Entities.Estoque.Inventario> Inventarios { get; }
    DbSet<ERP.Domain.Core.Entities.Estoque.InventarioItem> InventarioItens { get; }

    // Fase 5 - Vendas
    DbSet<ERP.Domain.Core.Entities.Vendas.Orcamento> Orcamentos { get; }
    DbSet<ERP.Domain.Core.Entities.Vendas.OrcamentoItem> OrcamentoItens { get; }
    DbSet<ERP.Domain.Core.Entities.Vendas.PedidoVenda> PedidosVenda { get; }
    DbSet<ERP.Domain.Core.Entities.Vendas.PedidoVendaItem> PedidoVendaItens { get; }
    DbSet<ERP.Domain.Core.Entities.Vendas.Faturamento> Faturamentos { get; }

    // Fase 6 - Compras
    DbSet<ERP.Domain.Core.Entities.Compras.SolicitacaoCompra> SolicitacoesCompra { get; }
    DbSet<ERP.Domain.Core.Entities.Compras.SolicitacaoCompraItem> SolicitacaoCompraItens { get; }
    DbSet<ERP.Domain.Core.Entities.Compras.PedidoCompra> PedidosCompra { get; }
    DbSet<ERP.Domain.Core.Entities.Compras.PedidoCompraItem> PedidoCompraItens { get; }
    DbSet<ERP.Domain.Core.Entities.Compras.EntradaMercadoria> EntradasMercadoria { get; }
    DbSet<ERP.Domain.Core.Entities.Compras.EntradaMercadoriaItem> EntradaMercadoriaItens { get; }

    // Fase 7 - Fiscal
    DbSet<ERP.Domain.Core.Entities.Fiscal.Ncm> Ncms { get; }
    DbSet<ERP.Domain.Core.Entities.Fiscal.Cfop> Cfops { get; }
    DbSet<ERP.Domain.Core.Entities.Fiscal.RegimeTributario> RegimesTributarios { get; }
    DbSet<ERP.Domain.Core.Entities.Fiscal.RegraTributaria> RegrasTributarias { get; }
    DbSet<ERP.Domain.Core.Entities.Fiscal.CertificadoDigital> CertificadosDigitais { get; }
    DbSet<ERP.Domain.Core.Entities.Fiscal.NotaFiscal> NotasFiscais { get; }
    DbSet<ERP.Domain.Core.Entities.Fiscal.NotaFiscalItem> NotaFiscalItens { get; }
    DbSet<ERP.Domain.Core.Entities.Fiscal.XmlNotaFiscal> XmlNotasFiscais { get; }
    DbSet<ERP.Domain.Core.Entities.Fiscal.CartaCorrecao> CartasCorrecao { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
