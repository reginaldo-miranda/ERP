namespace ERP.Application.Common.Interfaces;

/// <summary>
/// Serviço para obter e definir a empresa ativa na sessão/requisição do usuário.
/// </summary>
public interface ICurrentEmpresaService
{
    int? EmpresaId { get; }
    void SetEmpresaId(int empresaId);
}
