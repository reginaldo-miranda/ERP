using ERP.Domain.Core.Entities;
using Microsoft.EntityFrameworkCore;

namespace ERP.Application.Common.Interfaces;

/// <summary>
/// Interface do DbContext principal para ser consumida pela camada Application sem depender de EF Core diretamente na infraestrutura.
/// </summary>
public interface IApplicationDbContext
{
    DbSet<Empresa> Empresas { get; }
    DbSet<UsuarioEmpresa> UsuarioEmpresas { get; }
    DbSet<Permissao> Permissoes { get; }
    DbSet<PapelPermissao> PapelPermissoes { get; }
    DbSet<AuditLog> AuditLogs { get; }
    DbSet<Notificacao> Notificacoes { get; }
    DbSet<Moeda> Moedas { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
