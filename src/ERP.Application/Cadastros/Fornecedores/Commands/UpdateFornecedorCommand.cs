using ERP.Application.Common.Interfaces;
using ERP.Application.Common.Models;
using FluentValidation;
using MediatR;

namespace ERP.Application.Cadastros.Fornecedores.Commands;

public record UpdateFornecedorCommand(int Id, string RazaoSocial, string? NomeFantasia, string? InscricaoEstadual, string? Email, string? Telefone, string? Celular, string? Contato, string? Site, string? Observacoes, bool Ativo) : IRequest<Result>;

public class UpdateFornecedorCommandValidator : AbstractValidator<UpdateFornecedorCommand>
{
    public UpdateFornecedorCommandValidator() { RuleFor(x => x.RazaoSocial).NotEmpty().MaximumLength(200); }
}

public class UpdateFornecedorCommandHandler : IRequestHandler<UpdateFornecedorCommand, Result>
{
    private readonly IApplicationDbContext _context;
    public UpdateFornecedorCommandHandler(IApplicationDbContext context) => _context = context;

    public async Task<Result> Handle(UpdateFornecedorCommand request, CancellationToken cancellationToken)
    {
        var f = await _context.Fornecedores.FindAsync(new object[] { request.Id }, cancellationToken);
        if (f == null) return Result.Falha("Fornecedor não encontrado.");
        f.RazaoSocial = request.RazaoSocial; f.NomeFantasia = request.NomeFantasia; f.InscricaoEstadual = request.InscricaoEstadual;
        f.Email = request.Email; f.Telefone = request.Telefone; f.Celular = request.Celular;
        f.Contato = request.Contato; f.Site = request.Site; f.Observacoes = request.Observacoes; f.Ativo = request.Ativo;
        await _context.SaveChangesAsync(cancellationToken);
        return Result.Ok();
    }
}
