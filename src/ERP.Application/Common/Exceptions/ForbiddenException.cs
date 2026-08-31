namespace ERP.Application.Common.Exceptions;

/// <summary>
/// Exceção lançada quando o usuário não tem permissão para a ação.
/// </summary>
public class ForbiddenException : Exception
{
    public ForbiddenException() : base("Você não tem permissão para realizar esta ação.") { }

    public ForbiddenException(string message) : base(message) { }

    public ForbiddenException(string permissao, string acao)
        : base($"Permissão '{permissao}' necessária para '{acao}'.") { }
}
