namespace ERP.Domain.Core.Enums;

/// <summary>Periodicidade de cobrança do contrato (valor numérico = meses por ciclo).</summary>
public enum PeriodicidadeContrato
{
    Mensal = 1,
    Bimestral = 2,
    Trimestral = 3,
    Semestral = 6,
    Anual = 12
}

/// <summary>Status do ciclo de vida de um contrato recorrente.</summary>
public enum StatusContrato
{
    Rascunho = 1,
    Ativo = 2,
    Suspenso = 3,
    Encerrado = 4,
    Cancelado = 5
}
