using ERP.Domain.Common;

namespace ERP.Domain.Core.Entities;

/// <summary>
/// Notificação interna do sistema.
/// Filtrada por permissão do usuário.
/// </summary>
public class Notificacao : BaseEntity
{
    public int? EmpresaId { get; set; }
    public string UsuarioId { get; set; } = string.Empty;
    public string Titulo { get; set; } = string.Empty;
    public string Mensagem { get; set; } = string.Empty;
    public string Tipo { get; set; } = string.Empty; // Info, Warning, Error, Success
    public string? Modulo { get; set; } // Módulo de origem
    public string? Link { get; set; } // URL para navegar ao clicar
    public bool Lida { get; set; } = false;
    public DateTime? LidaEm { get; set; }
    public DateTime CriadoEm { get; set; } = DateTime.UtcNow;
}
