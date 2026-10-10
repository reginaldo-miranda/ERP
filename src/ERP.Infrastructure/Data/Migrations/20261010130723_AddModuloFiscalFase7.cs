using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace ERP.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddModuloFiscalFase7 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CertificadosDigitais",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    NomeArquivo = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    ArquivoBytes = table.Column<byte[]>(type: "bytea", nullable: false),
                    SenhaCriptografada = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    ValidadeInicio = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ValidadeFim = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CnpjOuCpfTitular = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    RazaoSocialTitular = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    Thumbprint = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Emissor = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    CriadoEm = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CriadoPor = table.Column<string>(type: "text", nullable: true),
                    AlteradoEm = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    AlteradoPor = table.Column<string>(type: "text", nullable: true),
                    Ativo = table.Column<bool>(type: "boolean", nullable: false),
                    EmpresaId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CertificadosDigitais", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CertificadosDigitais_Empresas_EmpresaId",
                        column: x => x.EmpresaId,
                        principalTable: "Empresas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Cfops",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Codigo = table.Column<string>(type: "character varying(4)", maxLength: 4, nullable: false),
                    Descricao = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    TipoOperacao = table.Column<int>(type: "integer", nullable: false),
                    Aplicacao = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    GeraFinanceiro = table.Column<bool>(type: "boolean", nullable: false),
                    MovimentaEstoque = table.Column<bool>(type: "boolean", nullable: false),
                    CriadoEm = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CriadoPor = table.Column<string>(type: "text", nullable: true),
                    AlteradoEm = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    AlteradoPor = table.Column<string>(type: "text", nullable: true),
                    Ativo = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Cfops", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Ncms",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Codigo = table.Column<string>(type: "character varying(8)", maxLength: 8, nullable: false),
                    Descricao = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    AliquotaIpi = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    AliquotaII = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    AliquotaNacional = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    AliquotaImportado = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    AliquotaEstadual = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    AliquotaMunicipal = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    CriadoEm = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CriadoPor = table.Column<string>(type: "text", nullable: true),
                    AlteradoEm = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    AlteradoPor = table.Column<string>(type: "text", nullable: true),
                    Ativo = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Ncms", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "NotasFiscais",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Modelo = table.Column<int>(type: "integer", nullable: false),
                    Serie = table.Column<int>(type: "integer", nullable: false),
                    Numero = table.Column<int>(type: "integer", nullable: false),
                    ChaveAcesso = table.Column<string>(type: "character varying(44)", maxLength: 44, nullable: false),
                    NaturezaOperacao = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    TipoOperacao = table.Column<int>(type: "integer", nullable: false),
                    TipoEmissao = table.Column<int>(type: "integer", nullable: false),
                    Ambiente = table.Column<int>(type: "integer", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    DataEmissao = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DataSaidaEntrada = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ClienteId = table.Column<int>(type: "integer", nullable: true),
                    FornecedorId = table.Column<int>(type: "integer", nullable: true),
                    DestinatarioCpfCnpj = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    DestinatarioRazaoSocial = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    DestinatarioInscricaoEstadual = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    DestinatarioEmail = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: true),
                    DestinatarioTelefone = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    DestinatarioLogradouro = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    DestinatarioNumero = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    DestinatarioComplemento = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    DestinatarioBairro = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    DestinatarioMunicipio = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    DestinatarioCodigoIbgeMunicipio = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
                    DestinatarioUf = table.Column<string>(type: "character varying(2)", maxLength: 2, nullable: false),
                    DestinatarioCep = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
                    PedidoVendaId = table.Column<int>(type: "integer", nullable: true),
                    FaturamentoId = table.Column<int>(type: "integer", nullable: true),
                    EntradaMercadoriaId = table.Column<int>(type: "integer", nullable: true),
                    ValorProdutos = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    ValorFrete = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    ValorSeguro = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    ValorDesconto = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    ValorOutrasDespesas = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    ValorTotalNota = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    BaseCalculoIcms = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    ValorIcms = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    BaseCalculoIcmsSt = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    ValorIcmsSt = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    ValorPis = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    ValorCofins = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    ValorIpi = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    ValorIbs = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    ValorCbs = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    ValorAproximadoTributos = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    ProtocoloAutorizacao = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    DataAutorizacao = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CodigoStatusSefaz = table.Column<int>(type: "integer", nullable: true),
                    MotivoStatusSefaz = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    DataCancelamento = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    MotivoCancelamento = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    ProtocoloCancelamento = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    InformacoesComplementares = table.Column<string>(type: "character varying(5000)", maxLength: 5000, nullable: true),
                    ObservacoesFisco = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    CriadoEm = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CriadoPor = table.Column<string>(type: "text", nullable: true),
                    AlteradoEm = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    AlteradoPor = table.Column<string>(type: "text", nullable: true),
                    Ativo = table.Column<bool>(type: "boolean", nullable: false),
                    EmpresaId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NotasFiscais", x => x.Id);
                    table.ForeignKey(
                        name: "FK_NotasFiscais_Clientes_ClienteId",
                        column: x => x.ClienteId,
                        principalTable: "Clientes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_NotasFiscais_Empresas_EmpresaId",
                        column: x => x.EmpresaId,
                        principalTable: "Empresas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_NotasFiscais_EntradasMercadoria_EntradaMercadoriaId",
                        column: x => x.EntradaMercadoriaId,
                        principalTable: "EntradasMercadoria",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_NotasFiscais_Fornecedores_FornecedorId",
                        column: x => x.FornecedorId,
                        principalTable: "Fornecedores",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_NotasFiscais_PedidosVenda_PedidoVendaId",
                        column: x => x.PedidoVendaId,
                        principalTable: "PedidosVenda",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "RegimesTributarios",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Regime = table.Column<int>(type: "integer", nullable: false),
                    Ambiente = table.Column<int>(type: "integer", nullable: false),
                    SerieNFePadrao = table.Column<int>(type: "integer", nullable: false),
                    UltimoNumeroNFe = table.Column<int>(type: "integer", nullable: false),
                    SerieNFCePadrao = table.Column<int>(type: "integer", nullable: false),
                    UltimoNumeroNFCe = table.Column<int>(type: "integer", nullable: false),
                    IdCscToken = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
                    CodigoCsc = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    AliquotaCreditoSimples = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    IncentivadorCultural = table.Column<bool>(type: "boolean", nullable: false),
                    PermiteCreditoIcms = table.Column<bool>(type: "boolean", nullable: false),
                    MensagemPadraoNotas = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    CriadoEm = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CriadoPor = table.Column<string>(type: "text", nullable: true),
                    AlteradoEm = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    AlteradoPor = table.Column<string>(type: "text", nullable: true),
                    Ativo = table.Column<bool>(type: "boolean", nullable: false),
                    EmpresaId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RegimesTributarios", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RegimesTributarios_Empresas_EmpresaId",
                        column: x => x.EmpresaId,
                        principalTable: "Empresas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "RegrasTributarias",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Descricao = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    TipoOperacao = table.Column<int>(type: "integer", nullable: false),
                    NcmCodigo = table.Column<string>(type: "character varying(8)", maxLength: 8, nullable: true),
                    CfopCodigo = table.Column<string>(type: "character varying(4)", maxLength: 4, nullable: false),
                    UfOrigem = table.Column<string>(type: "character varying(2)", maxLength: 2, nullable: false),
                    UfDestino = table.Column<string>(type: "character varying(2)", maxLength: 2, nullable: false),
                    CstIcms = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: true),
                    Csosn = table.Column<string>(type: "character varying(4)", maxLength: 4, nullable: true),
                    AliquotaIcms = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    ReducaoBaseIcms = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    AliquotaIcmsSt = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    MvaIcmsSt = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    ReducaoBaseIcmsSt = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    CstPis = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: true),
                    AliquotaPis = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    CstCofins = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: true),
                    AliquotaCofins = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    CstIpi = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: true),
                    AliquotaIpi = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    AliquotaIbs = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    AliquotaCbs = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    ObservacoesFiscais = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    CriadoEm = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CriadoPor = table.Column<string>(type: "text", nullable: true),
                    AlteradoEm = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    AlteradoPor = table.Column<string>(type: "text", nullable: true),
                    Ativo = table.Column<bool>(type: "boolean", nullable: false),
                    EmpresaId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RegrasTributarias", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RegrasTributarias_Empresas_EmpresaId",
                        column: x => x.EmpresaId,
                        principalTable: "Empresas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CartasCorrecao",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    NotaFiscalId = table.Column<int>(type: "integer", nullable: false),
                    SequenciaEvento = table.Column<int>(type: "integer", nullable: false),
                    CorrecaoTexto = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    DataEvento = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ProtocoloHomologacao = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    CodigoStatusSefaz = table.Column<int>(type: "integer", nullable: true),
                    MotivoStatusSefaz = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    XmlEnvio = table.Column<string>(type: "text", nullable: true),
                    XmlRetorno = table.Column<string>(type: "text", nullable: true),
                    CriadoEm = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CriadoPor = table.Column<string>(type: "text", nullable: true),
                    AlteradoEm = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    AlteradoPor = table.Column<string>(type: "text", nullable: true),
                    Ativo = table.Column<bool>(type: "boolean", nullable: false),
                    EmpresaId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CartasCorrecao", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CartasCorrecao_Empresas_EmpresaId",
                        column: x => x.EmpresaId,
                        principalTable: "Empresas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CartasCorrecao_NotasFiscais_NotaFiscalId",
                        column: x => x.NotaFiscalId,
                        principalTable: "NotasFiscais",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "NotaFiscalItens",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    NotaFiscalId = table.Column<int>(type: "integer", nullable: false),
                    NumeroItem = table.Column<int>(type: "integer", nullable: false),
                    ProdutoId = table.Column<int>(type: "integer", nullable: true),
                    CodigoProduto = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: false),
                    DescricaoProduto = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Ncm = table.Column<string>(type: "character varying(8)", maxLength: 8, nullable: false),
                    Cfop = table.Column<string>(type: "character varying(4)", maxLength: 4, nullable: false),
                    UnidadeMedida = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    Quantidade = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    ValorUnitario = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    ValorTotalBruto = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    ValorDesconto = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    ValorFrete = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    ValorSeguro = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    ValorOutrasDespesas = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    ValorTotalLiquido = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    OrigemMercadoria = table.Column<int>(type: "integer", nullable: false),
                    CstIcms = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: true),
                    Csosn = table.Column<string>(type: "character varying(4)", maxLength: 4, nullable: true),
                    BaseCalculoIcms = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    AliquotaIcms = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    ValorIcms = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    PercentualReducaoBaseIcms = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    BaseCalculoIcmsSt = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    AliquotaIcmsSt = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    ValorIcmsSt = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    PercentualMvaSt = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    CstPis = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: true),
                    BaseCalculoPis = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    AliquotaPis = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    ValorPis = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    CstCofins = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: true),
                    BaseCalculoCofins = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    AliquotaCofins = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    ValorCofins = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    CstIpi = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: true),
                    BaseCalculoIpi = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    AliquotaIpi = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    ValorIpi = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    AliquotaIbs = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    ValorIbs = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    AliquotaCbs = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    ValorCbs = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    ValorAproximadoTributos = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    CriadoEm = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CriadoPor = table.Column<string>(type: "text", nullable: true),
                    AlteradoEm = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    AlteradoPor = table.Column<string>(type: "text", nullable: true),
                    Ativo = table.Column<bool>(type: "boolean", nullable: false),
                    EmpresaId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NotaFiscalItens", x => x.Id);
                    table.ForeignKey(
                        name: "FK_NotaFiscalItens_Empresas_EmpresaId",
                        column: x => x.EmpresaId,
                        principalTable: "Empresas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_NotaFiscalItens_NotasFiscais_NotaFiscalId",
                        column: x => x.NotaFiscalId,
                        principalTable: "NotasFiscais",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_NotaFiscalItens_Produtos_ProdutoId",
                        column: x => x.ProdutoId,
                        principalTable: "Produtos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "XmlNotasFiscais",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    NotaFiscalId = table.Column<int>(type: "integer", nullable: false),
                    TipoXml = table.Column<int>(type: "integer", nullable: false),
                    XmlConteudo = table.Column<string>(type: "text", nullable: false),
                    ChaveAcesso = table.Column<string>(type: "character varying(44)", maxLength: 44, nullable: true),
                    DataGeracao = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CriadoEm = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CriadoPor = table.Column<string>(type: "text", nullable: true),
                    AlteradoEm = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    AlteradoPor = table.Column<string>(type: "text", nullable: true),
                    Ativo = table.Column<bool>(type: "boolean", nullable: false),
                    EmpresaId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_XmlNotasFiscais", x => x.Id);
                    table.ForeignKey(
                        name: "FK_XmlNotasFiscais_Empresas_EmpresaId",
                        column: x => x.EmpresaId,
                        principalTable: "Empresas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_XmlNotasFiscais_NotasFiscais_NotaFiscalId",
                        column: x => x.NotaFiscalId,
                        principalTable: "NotasFiscais",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CartasCorrecao_EmpresaId",
                table: "CartasCorrecao",
                column: "EmpresaId");

            migrationBuilder.CreateIndex(
                name: "IX_CartasCorrecao_NotaFiscalId_SequenciaEvento",
                table: "CartasCorrecao",
                columns: new[] { "NotaFiscalId", "SequenciaEvento" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CertificadosDigitais_EmpresaId_Thumbprint",
                table: "CertificadosDigitais",
                columns: new[] { "EmpresaId", "Thumbprint" });

            migrationBuilder.CreateIndex(
                name: "IX_CertificadosDigitais_EmpresaId_ValidadeFim",
                table: "CertificadosDigitais",
                columns: new[] { "EmpresaId", "ValidadeFim" });

            migrationBuilder.CreateIndex(
                name: "IX_Cfops_Codigo",
                table: "Cfops",
                column: "Codigo",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Cfops_TipoOperacao",
                table: "Cfops",
                column: "TipoOperacao");

            migrationBuilder.CreateIndex(
                name: "IX_Ncms_Codigo",
                table: "Ncms",
                column: "Codigo",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Ncms_Descricao",
                table: "Ncms",
                column: "Descricao");

            migrationBuilder.CreateIndex(
                name: "IX_NotaFiscalItens_EmpresaId",
                table: "NotaFiscalItens",
                column: "EmpresaId");

            migrationBuilder.CreateIndex(
                name: "IX_NotaFiscalItens_NotaFiscalId",
                table: "NotaFiscalItens",
                column: "NotaFiscalId");

            migrationBuilder.CreateIndex(
                name: "IX_NotaFiscalItens_ProdutoId",
                table: "NotaFiscalItens",
                column: "ProdutoId");

            migrationBuilder.CreateIndex(
                name: "IX_NotasFiscais_ClienteId",
                table: "NotasFiscais",
                column: "ClienteId");

            migrationBuilder.CreateIndex(
                name: "IX_NotasFiscais_EmpresaId_ChaveAcesso",
                table: "NotasFiscais",
                columns: new[] { "EmpresaId", "ChaveAcesso" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_NotasFiscais_EmpresaId_DataEmissao",
                table: "NotasFiscais",
                columns: new[] { "EmpresaId", "DataEmissao" });

            migrationBuilder.CreateIndex(
                name: "IX_NotasFiscais_EmpresaId_Modelo_Serie_Numero",
                table: "NotasFiscais",
                columns: new[] { "EmpresaId", "Modelo", "Serie", "Numero" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_NotasFiscais_EmpresaId_Status",
                table: "NotasFiscais",
                columns: new[] { "EmpresaId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_NotasFiscais_EntradaMercadoriaId",
                table: "NotasFiscais",
                column: "EntradaMercadoriaId");

            migrationBuilder.CreateIndex(
                name: "IX_NotasFiscais_FaturamentoId",
                table: "NotasFiscais",
                column: "FaturamentoId");

            migrationBuilder.CreateIndex(
                name: "IX_NotasFiscais_FornecedorId",
                table: "NotasFiscais",
                column: "FornecedorId");

            migrationBuilder.CreateIndex(
                name: "IX_NotasFiscais_PedidoVendaId",
                table: "NotasFiscais",
                column: "PedidoVendaId");

            migrationBuilder.CreateIndex(
                name: "IX_RegimesTributarios_EmpresaId",
                table: "RegimesTributarios",
                column: "EmpresaId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RegrasTributarias_EmpresaId_NcmCodigo",
                table: "RegrasTributarias",
                columns: new[] { "EmpresaId", "NcmCodigo" });

            migrationBuilder.CreateIndex(
                name: "IX_RegrasTributarias_EmpresaId_TipoOperacao_CfopCodigo",
                table: "RegrasTributarias",
                columns: new[] { "EmpresaId", "TipoOperacao", "CfopCodigo" });

            migrationBuilder.CreateIndex(
                name: "IX_RegrasTributarias_EmpresaId_UfDestino",
                table: "RegrasTributarias",
                columns: new[] { "EmpresaId", "UfDestino" });

            migrationBuilder.CreateIndex(
                name: "IX_XmlNotasFiscais_EmpresaId_ChaveAcesso",
                table: "XmlNotasFiscais",
                columns: new[] { "EmpresaId", "ChaveAcesso" });

            migrationBuilder.CreateIndex(
                name: "IX_XmlNotasFiscais_EmpresaId_TipoXml",
                table: "XmlNotasFiscais",
                columns: new[] { "EmpresaId", "TipoXml" });

            migrationBuilder.CreateIndex(
                name: "IX_XmlNotasFiscais_NotaFiscalId",
                table: "XmlNotasFiscais",
                column: "NotaFiscalId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CartasCorrecao");

            migrationBuilder.DropTable(
                name: "CertificadosDigitais");

            migrationBuilder.DropTable(
                name: "Cfops");

            migrationBuilder.DropTable(
                name: "Ncms");

            migrationBuilder.DropTable(
                name: "NotaFiscalItens");

            migrationBuilder.DropTable(
                name: "RegimesTributarios");

            migrationBuilder.DropTable(
                name: "RegrasTributarias");

            migrationBuilder.DropTable(
                name: "XmlNotasFiscais");

            migrationBuilder.DropTable(
                name: "NotasFiscais");
        }
    }
}
