using ERP.Application.Common.Interfaces;
using ERP.Application.Crm.DTOs;
using ERP.Domain.Core.Entities.Cadastros;
using ERP.Domain.Core.Entities.Crm;
using ERP.Domain.Core.Entities.Vendas;
using ERP.Domain.Core.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ERP.Application.Crm.Commands;

public record CriarLeadCommand(CriarLeadDto Dto) : IRequest<int>;
public record AtualizarLeadCommand(int Id, AtualizarLeadDto Dto) : IRequest<bool>;
public record DescartarLeadCommand(int Id) : IRequest<bool>;
public record ConverterLeadEmClienteCommand(int LeadId) : IRequest<int>;

public record CriarOportunidadeCommand(CriarOportunidadeDto Dto) : IRequest<int>;
public record AtualizarOportunidadeCommand(int Id, AtualizarOportunidadeDto Dto) : IRequest<bool>;
public record MoverEtapaOportunidadeCommand(int Id, EtapaFunil NovaEtapa, string? MotivoPerda = null) : IRequest<bool>;
public record GerarOrcamentoDeOportunidadeCommand(int OportunidadeId) : IRequest<int>;
public record GerarPedidoDeOportunidadeCommand(int OportunidadeId) : IRequest<int>;

public record CriarAtividadeCrmCommand(CriarAtividadeCrmDto Dto) : IRequest<int>;
public record ConcluirAtividadeCrmCommand(int Id) : IRequest<bool>;
public record ExcluirAtividadeCrmCommand(int Id) : IRequest<bool>;

public class CrmCommandsHandler :
    IRequestHandler<CriarLeadCommand, int>,
    IRequestHandler<AtualizarLeadCommand, bool>,
    IRequestHandler<DescartarLeadCommand, bool>,
    IRequestHandler<ConverterLeadEmClienteCommand, int>,
    IRequestHandler<CriarOportunidadeCommand, int>,
    IRequestHandler<AtualizarOportunidadeCommand, bool>,
    IRequestHandler<MoverEtapaOportunidadeCommand, bool>,
    IRequestHandler<GerarOrcamentoDeOportunidadeCommand, int>,
    IRequestHandler<GerarPedidoDeOportunidadeCommand, int>,
    IRequestHandler<CriarAtividadeCrmCommand, int>,
    IRequestHandler<ConcluirAtividadeCrmCommand, bool>,
    IRequestHandler<ExcluirAtividadeCrmCommand, bool>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentEmpresaService _currentEmpresaService;

    public CrmCommandsHandler(IApplicationDbContext context, ICurrentEmpresaService currentEmpresaService)
    {
        _context = context;
        _currentEmpresaService = currentEmpresaService;
    }

    public async Task<int> Handle(CriarLeadCommand request, CancellationToken cancellationToken)
    {
        int empresaId = _currentEmpresaService.EmpresaId ?? 1;
        var dto = request.Dto;

        var lead = new Lead
        {
            EmpresaId = empresaId,
            Nome = dto.Nome.Trim(),
            EmpresaContato = dto.EmpresaContato?.Trim(),
            TipoPessoa = dto.TipoPessoa,
            CpfCnpj = dto.CpfCnpj?.Trim(),
            Email = dto.Email?.Trim(),
            Telefone = dto.Telefone?.Trim(),
            Celular = dto.Celular?.Trim(),
            Cargo = dto.Cargo?.Trim(),
            Origem = dto.Origem,
            Status = dto.Status,
            Observacoes = dto.Observacoes?.Trim(),
            ResponsavelId = dto.ResponsavelId,
            ResponsavelNome = dto.ResponsavelNome?.Trim()
        };

        _context.Leads.Add(lead);
        await _context.SaveChangesAsync(cancellationToken);
        return lead.Id;
    }

    public async Task<bool> Handle(AtualizarLeadCommand request, CancellationToken cancellationToken)
    {
        int empresaId = _currentEmpresaService.EmpresaId ?? 1;
        var lead = await _context.Leads
            .FirstOrDefaultAsync(l => l.Id == request.Id && l.EmpresaId == empresaId, cancellationToken);

        if (lead == null) return false;

        var dto = request.Dto;
        lead.Nome = dto.Nome.Trim();
        lead.EmpresaContato = dto.EmpresaContato?.Trim();
        lead.TipoPessoa = dto.TipoPessoa;
        lead.CpfCnpj = dto.CpfCnpj?.Trim();
        lead.Email = dto.Email?.Trim();
        lead.Telefone = dto.Telefone?.Trim();
        lead.Celular = dto.Celular?.Trim();
        lead.Cargo = dto.Cargo?.Trim();
        lead.Origem = dto.Origem;
        if (lead.Status != StatusLead.Convertido)
            lead.Status = dto.Status;
        lead.Observacoes = dto.Observacoes?.Trim();
        lead.ResponsavelId = dto.ResponsavelId;
        lead.ResponsavelNome = dto.ResponsavelNome?.Trim();

        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<bool> Handle(DescartarLeadCommand request, CancellationToken cancellationToken)
    {
        int empresaId = _currentEmpresaService.EmpresaId ?? 1;
        var lead = await _context.Leads
            .FirstOrDefaultAsync(l => l.Id == request.Id && l.EmpresaId == empresaId, cancellationToken);

        if (lead == null) return false;
        lead.Status = StatusLead.Descartado;
        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<int> Handle(ConverterLeadEmClienteCommand request, CancellationToken cancellationToken)
    {
        int empresaId = _currentEmpresaService.EmpresaId ?? 1;
        var lead = await _context.Leads
            .FirstOrDefaultAsync(l => l.Id == request.LeadId && l.EmpresaId == empresaId, cancellationToken)
            ?? throw new KeyNotFoundException("Lead não encontrado.");

        int clienteId = await GarantirClienteDoLeadAsync(lead, empresaId, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
        return clienteId;
    }

    public async Task<int> Handle(CriarOportunidadeCommand request, CancellationToken cancellationToken)
    {
        int empresaId = _currentEmpresaService.EmpresaId ?? 1;
        var dto = request.Dto;

        int ano = DateTime.UtcNow.Year;
        int totalAno = await _context.Oportunidades
            .CountAsync(o => o.EmpresaId == empresaId && o.CriadoEm.Year == ano, cancellationToken);
        string codigo = $"CRM-{ano}-{(totalAno + 1):D4}";

        var oportunidade = new Oportunidade
        {
            EmpresaId = empresaId,
            Codigo = codigo,
            Titulo = dto.Titulo.Trim(),
            Descricao = dto.Descricao?.Trim(),
            LeadId = dto.LeadId,
            ClienteId = dto.ClienteId,
            Etapa = dto.Etapa,
            ValorEstimado = dto.ValorEstimado,
            Probabilidade = dto.Probabilidade > 0 ? dto.Probabilidade : ProbabilidadePadrao(dto.Etapa),
            DataFechamentoPrevista = dto.DataFechamentoPrevista?.ToUniversalTime(),
            DataFechamentoReal = dto.Etapa is EtapaFunil.Ganha or EtapaFunil.Perdida ? DateTime.UtcNow : null,
            ResponsavelId = dto.ResponsavelId,
            ResponsavelNome = dto.ResponsavelNome?.Trim()
        };

        // Se selecionou um Lead que já foi convertido, propaga o ClienteId automaticamente
        if (oportunidade.LeadId.HasValue && !oportunidade.ClienteId.HasValue)
        {
            var lead = await _context.Leads.FirstOrDefaultAsync(l => l.Id == oportunidade.LeadId.Value && l.EmpresaId == empresaId, cancellationToken);
            if (lead?.ClienteConvertidoId != null)
                oportunidade.ClienteId = lead.ClienteConvertidoId;
        }

        _context.Oportunidades.Add(oportunidade);
        await _context.SaveChangesAsync(cancellationToken);
        return oportunidade.Id;
    }

    public async Task<bool> Handle(AtualizarOportunidadeCommand request, CancellationToken cancellationToken)
    {
        int empresaId = _currentEmpresaService.EmpresaId ?? 1;
        var op = await _context.Oportunidades
            .FirstOrDefaultAsync(o => o.Id == request.Id && o.EmpresaId == empresaId, cancellationToken);

        if (op == null) return false;

        var dto = request.Dto;
        op.Titulo = dto.Titulo.Trim();
        op.Descricao = dto.Descricao?.Trim();
        op.LeadId = dto.LeadId;
        op.ClienteId = dto.ClienteId;
        op.Etapa = dto.Etapa;
        op.ValorEstimado = dto.ValorEstimado;
        op.Probabilidade = dto.Probabilidade;
        op.DataFechamentoPrevista = dto.DataFechamentoPrevista?.ToUniversalTime();
        op.ResponsavelId = dto.ResponsavelId;
        op.ResponsavelNome = dto.ResponsavelNome?.Trim();
        op.MotivoPerda = dto.MotivoPerda?.Trim();

        if (op.Etapa is EtapaFunil.Ganha or EtapaFunil.Perdida)
            op.DataFechamentoReal ??= DateTime.UtcNow;
        else
            op.DataFechamentoReal = null;

        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<bool> Handle(MoverEtapaOportunidadeCommand request, CancellationToken cancellationToken)
    {
        int empresaId = _currentEmpresaService.EmpresaId ?? 1;
        var op = await _context.Oportunidades
            .Include(o => o.Lead)
            .FirstOrDefaultAsync(o => o.Id == request.Id && o.EmpresaId == empresaId, cancellationToken);

        if (op == null) return false;

        op.Etapa = request.NovaEtapa;
        op.Probabilidade = ProbabilidadePadrao(request.NovaEtapa);

        if (request.NovaEtapa == EtapaFunil.Perdida)
        {
            op.MotivoPerda = request.MotivoPerda?.Trim();
            op.DataFechamentoReal = DateTime.UtcNow;
        }
        else if (request.NovaEtapa == EtapaFunil.Ganha)
        {
            op.MotivoPerda = null;
            op.DataFechamentoReal = DateTime.UtcNow;

            if (!op.ClienteId.HasValue && op.Lead != null)
            {
                op.ClienteId = await GarantirClienteDoLeadAsync(op.Lead, empresaId, cancellationToken);
            }
        }
        else
        {
            op.DataFechamentoReal = null;
        }

        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<int> Handle(GerarOrcamentoDeOportunidadeCommand request, CancellationToken cancellationToken)
    {
        int empresaId = _currentEmpresaService.EmpresaId ?? 1;
        var op = await _context.Oportunidades
            .Include(o => o.Lead)
            .FirstOrDefaultAsync(o => o.Id == request.OportunidadeId && o.EmpresaId == empresaId, cancellationToken)
            ?? throw new KeyNotFoundException("Oportunidade não encontrada.");

        if (op.OrcamentoId.HasValue)
            return op.OrcamentoId.Value;

        int clienteId = await GarantirClienteDaOportunidadeAsync(op, empresaId, cancellationToken);

        int ano = DateTime.UtcNow.Year;
        int totalAno = await _context.Orcamentos
            .CountAsync(o => o.EmpresaId == empresaId && o.DataOrcamento.Year == ano, cancellationToken);
        string numero = $"ORC-{ano}-{(totalAno + 1):D4}";

        var orcamento = new Orcamento
        {
            EmpresaId = empresaId,
            Numero = numero,
            ClienteId = clienteId,
            VendedorId = op.ResponsavelId,
            VendedorNome = op.ResponsavelNome,
            DataOrcamento = DateTime.UtcNow,
            DataValidade = DateTime.UtcNow.AddDays(15),
            Status = StatusOrcamento.Aberto,
            ValorBruto = op.ValorEstimado,
            DescontoTipo = TipoDescontoVenda.Valor,
            DescontoGeral = 0,
            ValorDesconto = 0,
            ValorTotal = op.ValorEstimado,
            Observacao = $"Gerado a partir da Oportunidade {op.Codigo} - {op.Titulo}. {op.Descricao}".Trim()
        };

        _context.Orcamentos.Add(orcamento);
        await _context.SaveChangesAsync(cancellationToken);

        op.OrcamentoId = orcamento.Id;
        if (op.Etapa < EtapaFunil.Proposta)
        {
            op.Etapa = EtapaFunil.Proposta;
            op.Probabilidade = ProbabilidadePadrao(EtapaFunil.Proposta);
        }

        await _context.SaveChangesAsync(cancellationToken);
        return orcamento.Id;
    }

    public async Task<int> Handle(GerarPedidoDeOportunidadeCommand request, CancellationToken cancellationToken)
    {
        int empresaId = _currentEmpresaService.EmpresaId ?? 1;
        var op = await _context.Oportunidades
            .Include(o => o.Lead)
            .FirstOrDefaultAsync(o => o.Id == request.OportunidadeId && o.EmpresaId == empresaId, cancellationToken)
            ?? throw new KeyNotFoundException("Oportunidade não encontrada.");

        if (op.PedidoVendaId.HasValue)
            return op.PedidoVendaId.Value;

        int clienteId = await GarantirClienteDaOportunidadeAsync(op, empresaId, cancellationToken);

        int ano = DateTime.UtcNow.Year;
        int totalAno = await _context.PedidosVenda
            .CountAsync(p => p.EmpresaId == empresaId && p.DataPedido.Year == ano, cancellationToken);
        string numero = $"PED-{ano}-{(totalAno + 1):D4}";

        var pedido = new PedidoVenda
        {
            EmpresaId = empresaId,
            Numero = numero,
            ClienteId = clienteId,
            OrcamentoId = op.OrcamentoId,
            VendedorId = op.ResponsavelId,
            VendedorNome = op.ResponsavelNome,
            DataPedido = DateTime.UtcNow,
            Status = StatusPedidoVenda.Pendente,
            NumeroParcelas = 1,
            IntervaloDias = 30,
            DiasPrimeiroVencimento = 0,
            ValorBruto = op.ValorEstimado,
            DescontoTipo = TipoDescontoVenda.Valor,
            DescontoGeral = 0,
            ValorDesconto = 0,
            ValorTotal = op.ValorEstimado,
            Observacao = $"Gerado a partir da Oportunidade {op.Codigo} - {op.Titulo}. {op.Descricao}".Trim()
        };

        _context.PedidosVenda.Add(pedido);
        await _context.SaveChangesAsync(cancellationToken);

        op.PedidoVendaId = pedido.Id;
        op.Etapa = EtapaFunil.Ganha;
        op.Probabilidade = 100;
        op.DataFechamentoReal ??= DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);
        return pedido.Id;
    }

    public async Task<int> Handle(CriarAtividadeCrmCommand request, CancellationToken cancellationToken)
    {
        int empresaId = _currentEmpresaService.EmpresaId ?? 1;
        var dto = request.Dto;

        var atividade = new AtividadeCrm
        {
            EmpresaId = empresaId,
            Tipo = dto.Tipo,
            Assunto = dto.Assunto.Trim(),
            Descricao = dto.Descricao?.Trim(),
            DataPrevista = dto.DataPrevista.ToUniversalTime(),
            Concluida = dto.Tipo == TipoAtividadeCrm.Anotacao,
            DataConclusao = dto.Tipo == TipoAtividadeCrm.Anotacao ? DateTime.UtcNow : null,
            LeadId = dto.LeadId,
            OportunidadeId = dto.OportunidadeId,
            ResponsavelId = dto.ResponsavelId,
            ResponsavelNome = dto.ResponsavelNome?.Trim()
        };

        _context.AtividadesCrm.Add(atividade);
        await _context.SaveChangesAsync(cancellationToken);
        return atividade.Id;
    }

    public async Task<bool> Handle(ConcluirAtividadeCrmCommand request, CancellationToken cancellationToken)
    {
        int empresaId = _currentEmpresaService.EmpresaId ?? 1;
        var atividade = await _context.AtividadesCrm
            .FirstOrDefaultAsync(a => a.Id == request.Id && a.EmpresaId == empresaId, cancellationToken);

        if (atividade == null) return false;

        atividade.Concluida = true;
        atividade.DataConclusao = DateTime.UtcNow;
        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<bool> Handle(ExcluirAtividadeCrmCommand request, CancellationToken cancellationToken)
    {
        int empresaId = _currentEmpresaService.EmpresaId ?? 1;
        var atividade = await _context.AtividadesCrm
            .FirstOrDefaultAsync(a => a.Id == request.Id && a.EmpresaId == empresaId, cancellationToken);

        if (atividade == null) return false;

        _context.AtividadesCrm.Remove(atividade);
        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }

    private async Task<int> GarantirClienteDaOportunidadeAsync(Oportunidade op, int empresaId, CancellationToken cancellationToken)
    {
        if (op.ClienteId.HasValue)
            return op.ClienteId.Value;

        if (op.Lead != null)
        {
            op.ClienteId = await GarantirClienteDoLeadAsync(op.Lead, empresaId, cancellationToken);
            return op.ClienteId.Value;
        }

        throw new InvalidOperationException("A oportunidade precisa estar vinculada a um Cliente ou Lead antes de gerar documento comercial.");
    }

    private async Task<int> GarantirClienteDoLeadAsync(Lead lead, int empresaId, CancellationToken cancellationToken)
    {
        if (lead.ClienteConvertidoId.HasValue)
            return lead.ClienteConvertidoId.Value;

        string cpfCnpjLimpo = !string.IsNullOrWhiteSpace(lead.CpfCnpj)
            ? lead.CpfCnpj.Trim()
            : $"LEAD-{lead.Id:D10}";

        if (cpfCnpjLimpo.Length > 18)
            cpfCnpjLimpo = cpfCnpjLimpo[..18];

        var clienteExistente = await _context.Clientes
            .FirstOrDefaultAsync(c => c.EmpresaId == empresaId && c.CpfCnpj == cpfCnpjLimpo, cancellationToken);

        int clienteId;
        if (clienteExistente != null)
        {
            clienteId = clienteExistente.Id;
        }
        else
        {
            var novoCliente = new Cliente
            {
                EmpresaId = empresaId,
                Nome = string.IsNullOrWhiteSpace(lead.EmpresaContato) ? lead.Nome : $"{lead.EmpresaContato} ({lead.Nome})",
                NomeFantasia = lead.EmpresaContato,
                TipoPessoa = lead.TipoPessoa,
                CpfCnpj = cpfCnpjLimpo,
                Email = lead.Email,
                Telefone = lead.Telefone,
                Celular = lead.Celular,
                Observacoes = $"Convertido do Lead #{lead.Id}. {lead.Observacoes}".Trim()
            };

            _context.Clientes.Add(novoCliente);
            await _context.SaveChangesAsync(cancellationToken);
            clienteId = novoCliente.Id;
        }

        lead.ClienteConvertidoId = clienteId;
        lead.Status = StatusLead.Convertido;
        lead.DataConversao = DateTime.UtcNow;

        // Propaga o cliente para oportunidades abertas do mesmo lead
        var opsDoLead = await _context.Oportunidades
            .Where(o => o.EmpresaId == empresaId && o.LeadId == lead.Id && o.ClienteId == null)
            .ToListAsync(cancellationToken);
        foreach (var op in opsDoLead)
        {
            op.ClienteId = clienteId;
        }

        return clienteId;
    }

    private static int ProbabilidadePadrao(EtapaFunil etapa) => etapa switch
    {
        EtapaFunil.Prospeccao => 20,
        EtapaFunil.Qualificacao => 40,
        EtapaFunil.Proposta => 60,
        EtapaFunil.Negociacao => 80,
        EtapaFunil.Ganha => 100,
        EtapaFunil.Perdida => 0,
        _ => 20
    };
}
