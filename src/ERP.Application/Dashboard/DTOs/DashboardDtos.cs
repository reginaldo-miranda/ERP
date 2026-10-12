namespace ERP.Application.Dashboard.DTOs;

public class DashboardKpisDto
{
    public decimal VendasHoje { get; set; }
    public decimal VendasMes { get; set; }
    public decimal VendasMesAnterior { get; set; }
    public double PercentualVariacaoVendas { get; set; }

    public decimal ContasReceberHoje { get; set; }
    public decimal ContasReceberVencidas { get; set; }
    public int QuantidadeReceberVencidas { get; set; }

    public decimal ContasPagarHoje { get; set; }
    public decimal ContasPagarVencidas { get; set; }
    public int QuantidadePagarVencidas { get; set; }

    public int ProdutosEstoqueCritico { get; set; }
    public int TotalProdutosCadastrados { get; set; }

    public int TotalNotasFiscaisMes { get; set; }
    public decimal FaturamentoFiscalMes { get; set; }
}

public class DashboardGraficoEvolucaoDto
{
    public string[] Meses { get; set; } = Array.Empty<string>();
    public double[] SerieVendas { get; set; } = Array.Empty<double>();
    public double[] SerieCompras { get; set; } = Array.Empty<double>();
}

public class DashboardGraficoCategoriaDto
{
    public string[] Categorias { get; set; } = Array.Empty<string>();
    public double[] Valores { get; set; } = Array.Empty<double>();
}

public class DashboardPedidoRecenteDto
{
    public int Id { get; set; }
    public string Numero { get; set; } = string.Empty;
    public DateTime Data { get; set; }
    public string ClienteNome { get; set; } = string.Empty;
    public decimal ValorTotal { get; set; }
    public string Status { get; set; } = string.Empty;
}

public class DashboardAlertaDto
{
    public string Titulo { get; set; } = string.Empty;
    public string Mensagem { get; set; } = string.Empty;
    public string Tipo { get; set; } = "info"; // warning, error, info, success
    public string? Link { get; set; }
    public string? Icone { get; set; }
}

public class DashboardResumoCompletoDto
{
    public DashboardKpisDto Kpis { get; set; } = new();
    public DashboardGraficoEvolucaoDto Evolucao { get; set; } = new();
    public DashboardGraficoCategoriaDto FaturamentoPorCategoria { get; set; } = new();
    public List<DashboardPedidoRecenteDto> PedidosRecentes { get; set; } = new();
    public List<DashboardAlertaDto> Alertas { get; set; } = new();
}
