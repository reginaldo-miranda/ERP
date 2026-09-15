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

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
