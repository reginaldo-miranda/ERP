namespace ERP.Application.Common.Exceptions;

/// <summary>
/// Exceção lançada quando uma entidade não é encontrada.
/// </summary>
public class NotFoundException : Exception
{
    public NotFoundException() : base() { }

    public NotFoundException(string message) : base(message) { }

    public NotFoundException(string name, object key) : base($"Entidade \"{name}\" ({key}) não foi encontrada.") { }
}
