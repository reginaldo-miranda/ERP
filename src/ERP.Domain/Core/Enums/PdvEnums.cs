namespace ERP.Domain.Core.Enums;

/// <summary>Perfil operacional do terminal PDV.</summary>
public enum TipoTerminalPdv
{
    Caixa = 1,
    PreVenda = 2,
    Hibrido = 3
}

/// <summary>Estado da sessão (turno) de um terminal de caixa.</summary>
public enum StatusSessaoCaixa
{
    Aberta = 1,
    Fechada = 2
}

/// <summary>Movimentações avulsas no caixa durante o turno.</summary>
public enum TipoMovimentoCaixa
{
    Sangria = 1,
    Suprimento = 2
}

/// <summary>Status de uma venda ou pré-venda do PDV.</summary>
public enum StatusVendaPdv
{
    PreVenda = 1,
    Finalizada = 2,
    Cancelada = 3
}
