using System.Text.RegularExpressions;
using ERP.Application.Common.Interfaces;
using ERP.Application.Compras.DTOs;
using ERP.Domain.Core.Entities.Cadastros;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ERP.Application.Compras.Commands;

public record CadastrarFornecedorXmlCommand(NfeXmlFornecedorDto Dto) : IRequest<int>;

public class CadastrarFornecedorXmlCommandHandler : IRequestHandler<CadastrarFornecedorXmlCommand, int>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentEmpresaService _empresaService;

    public CadastrarFornecedorXmlCommandHandler(IApplicationDbContext context, ICurrentEmpresaService empresaService)
    {
        _context = context;
        _empresaService = empresaService;
    }

    public async Task<int> Handle(CadastrarFornecedorXmlCommand request, CancellationToken cancellationToken)
    {
        var dto = request.Dto;
        var empresaId = _empresaService.EmpresaId ?? 1;

        string cnpjLimpo = Regex.Replace(dto.Cnpj, @"[^\d]", "");
        if (string.IsNullOrWhiteSpace(cnpjLimpo))
            throw new InvalidOperationException("CNPJ do fornecedor inválido.");

        var fornecedoresExistentes = await _context.Fornecedores
            .Where(f => f.EmpresaId == empresaId && f.Ativo)
            .ToListAsync(cancellationToken);

        var existente = fornecedoresExistentes.FirstOrDefault(f => Regex.Replace(f.Cnpj, @"[^\d]", "") == cnpjLimpo);
        if (existente != null)
            return existente.Id;

        var fornecedor = new Fornecedor
        {
            EmpresaId = empresaId,
            RazaoSocial = dto.RazaoSocial,
            NomeFantasia = dto.NomeFantasia,
            Cnpj = dto.Cnpj,
            InscricaoEstadual = dto.InscricaoEstadual,
            Telefone = dto.Telefone,
            Email = dto.Email,
            Observacoes = "Cadastrado automaticamente via importação de XML da NF-e",
            CriadoEm = DateTime.UtcNow,
            Ativo = true
        };

        if (!string.IsNullOrWhiteSpace(dto.Logradouro) || !string.IsNullOrWhiteSpace(dto.Cidade))
        {
            var endereco = new Endereco
            {
                Logradouro = dto.Logradouro ?? string.Empty,
                Numero = dto.Numero ?? "S/N",
                Complemento = dto.Complemento,
                Bairro = dto.Bairro ?? string.Empty,
                Cidade = dto.Cidade ?? string.Empty,
                Uf = dto.Uf ?? "SP",
                Cep = dto.Cep ?? string.Empty,
                Principal = true,
                CriadoEm = DateTime.UtcNow,
                Ativo = true
            };
            fornecedor.Enderecos.Add(endereco);
        }

        _context.Fornecedores.Add(fornecedor);
        await _context.SaveChangesAsync(cancellationToken);

        return fornecedor.Id;
    }
}
