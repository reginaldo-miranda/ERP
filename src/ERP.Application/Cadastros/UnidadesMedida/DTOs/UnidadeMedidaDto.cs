namespace ERP.Application.Cadastros.UnidadesMedida.DTOs;

public class UnidadeMedidaDto
{
    public int Id { get; set; }
    public string Sigla { get; set; } = string.Empty;
    public string Descricao { get; set; } = string.Empty;
    public bool Ativo { get; set; }
}
