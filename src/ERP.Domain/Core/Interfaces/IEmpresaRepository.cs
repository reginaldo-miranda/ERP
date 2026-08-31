using ERP.Domain.Core.Entities;

namespace ERP.Domain.Core.Interfaces;

/// <summary>
/// Interface de repositório para Empresas.
/// </summary>
public interface IEmpresaRepository : Common.IRepository<Empresa>
{
    Task<IReadOnlyList<Empresa>> ObterEmpresasPorUsuarioAsync(string usuarioId, CancellationToken cancellationToken = default);
    Task<Empresa?> ObterPorCnpjAsync(string cnpj, CancellationToken cancellationToken = default);
}
