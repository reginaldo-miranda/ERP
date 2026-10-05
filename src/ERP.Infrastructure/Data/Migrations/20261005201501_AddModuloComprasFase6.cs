using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace ERP.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddModuloComprasFase6 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "SolicitacoesCompra",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Numero = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    SolicitanteId = table.Column<string>(type: "character varying(450)", maxLength: 450, nullable: false),
                    SolicitanteNome = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    DataSolicitacao = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DataNecessidade = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    Prioridade = table.Column<int>(type: "integer", nullable: false),
                    Departamento = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Justificativa = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    Observacoes = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    DataAprovacao = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    AprovadorNome = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: true),
                    MotivoRejeicao = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    CriadoEm = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CriadoPor = table.Column<string>(type: "text", nullable: true),
                    AlteradoEm = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    AlteradoPor = table.Column<string>(type: "text", nullable: true),
                    Ativo = table.Column<bool>(type: "boolean", nullable: false),
                    EmpresaId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SolicitacoesCompra", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SolicitacoesCompra_Empresas_EmpresaId",
                        column: x => x.EmpresaId,
                        principalTable: "Empresas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PedidosCompra",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Numero = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    FornecedorId = table.Column<int>(type: "integer", nullable: false),
                    SolicitacaoCompraId = table.Column<int>(type: "integer", nullable: true),
                    DataPedido = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    PrevisaoEntrega = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    CompradorNome = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: true),
                    DepositoDestinoId = table.Column<int>(type: "integer", nullable: false),
                    FormaPagamentoId = table.Column<int>(type: "integer", nullable: true),
                    NumeroParcelas = table.Column<int>(type: "integer", nullable: false),
                    IntervaloDias = table.Column<int>(type: "integer", nullable: false),
                    DiasPrimeiroVencimento = table.Column<int>(type: "integer", nullable: false),
                    ValorBruto = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    ValorDesconto = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    ValorFrete = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    ValorSeguro = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    ValorOutrasDespesas = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    ValorTotal = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    Observacoes = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    DataAprovacao = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DataCancelamento = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    MotivoCancelamento = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    CriadoEm = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CriadoPor = table.Column<string>(type: "text", nullable: true),
                    AlteradoEm = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    AlteradoPor = table.Column<string>(type: "text", nullable: true),
                    Ativo = table.Column<bool>(type: "boolean", nullable: false),
                    EmpresaId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PedidosCompra", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PedidosCompra_Depositos_DepositoDestinoId",
                        column: x => x.DepositoDestinoId,
                        principalTable: "Depositos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PedidosCompra_Empresas_EmpresaId",
                        column: x => x.EmpresaId,
                        principalTable: "Empresas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PedidosCompra_FormasPagamento_FormaPagamentoId",
                        column: x => x.FormaPagamentoId,
                        principalTable: "FormasPagamento",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PedidosCompra_Fornecedores_FornecedorId",
                        column: x => x.FornecedorId,
                        principalTable: "Fornecedores",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PedidosCompra_SolicitacoesCompra_SolicitacaoCompraId",
                        column: x => x.SolicitacaoCompraId,
                        principalTable: "SolicitacoesCompra",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "SolicitacaoCompraItens",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    SolicitacaoCompraId = table.Column<int>(type: "integer", nullable: false),
                    ProdutoId = table.Column<int>(type: "integer", nullable: false),
                    Quantidade = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    QuantidadeAtendida = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    Observacao = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    CriadoEm = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CriadoPor = table.Column<string>(type: "text", nullable: true),
                    AlteradoEm = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    AlteradoPor = table.Column<string>(type: "text", nullable: true),
                    Ativo = table.Column<bool>(type: "boolean", nullable: false),
                    EmpresaId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SolicitacaoCompraItens", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SolicitacaoCompraItens_Empresas_EmpresaId",
                        column: x => x.EmpresaId,
                        principalTable: "Empresas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SolicitacaoCompraItens_Produtos_ProdutoId",
                        column: x => x.ProdutoId,
                        principalTable: "Produtos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SolicitacaoCompraItens_SolicitacoesCompra_SolicitacaoCompra~",
                        column: x => x.SolicitacaoCompraId,
                        principalTable: "SolicitacoesCompra",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "EntradasMercadoria",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Numero = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    PedidoCompraId = table.Column<int>(type: "integer", nullable: true),
                    FornecedorId = table.Column<int>(type: "integer", nullable: false),
                    DepositoId = table.Column<int>(type: "integer", nullable: false),
                    NumeroNotaFiscal = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    SerieNotaFiscal = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    ChaveAcessoNfe = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: true),
                    DataEmissao = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DataEntrada = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    ConferenteNome = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: true),
                    ValorProdutos = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    ValorFrete = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    ValorSeguro = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    ValorOutrasDespesas = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    ValorDesconto = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    ValorTotal = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    IdParcelamento = table.Column<Guid>(type: "uuid", nullable: true),
                    Observacoes = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    DataCancelamento = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    MotivoCancelamento = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    CriadoEm = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CriadoPor = table.Column<string>(type: "text", nullable: true),
                    AlteradoEm = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    AlteradoPor = table.Column<string>(type: "text", nullable: true),
                    Ativo = table.Column<bool>(type: "boolean", nullable: false),
                    EmpresaId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EntradasMercadoria", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EntradasMercadoria_Depositos_DepositoId",
                        column: x => x.DepositoId,
                        principalTable: "Depositos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_EntradasMercadoria_Empresas_EmpresaId",
                        column: x => x.EmpresaId,
                        principalTable: "Empresas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_EntradasMercadoria_Fornecedores_FornecedorId",
                        column: x => x.FornecedorId,
                        principalTable: "Fornecedores",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_EntradasMercadoria_PedidosCompra_PedidoCompraId",
                        column: x => x.PedidoCompraId,
                        principalTable: "PedidosCompra",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PedidoCompraItens",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    PedidoCompraId = table.Column<int>(type: "integer", nullable: false),
                    ProdutoId = table.Column<int>(type: "integer", nullable: false),
                    Quantidade = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    QuantidadeEntregue = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    PrecoUnitario = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    ValorDesconto = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    SubTotal = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    Observacao = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    CriadoEm = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CriadoPor = table.Column<string>(type: "text", nullable: true),
                    AlteradoEm = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    AlteradoPor = table.Column<string>(type: "text", nullable: true),
                    Ativo = table.Column<bool>(type: "boolean", nullable: false),
                    EmpresaId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PedidoCompraItens", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PedidoCompraItens_Empresas_EmpresaId",
                        column: x => x.EmpresaId,
                        principalTable: "Empresas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PedidoCompraItens_PedidosCompra_PedidoCompraId",
                        column: x => x.PedidoCompraId,
                        principalTable: "PedidosCompra",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PedidoCompraItens_Produtos_ProdutoId",
                        column: x => x.ProdutoId,
                        principalTable: "Produtos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "EntradaMercadoriaItens",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    EntradaMercadoriaId = table.Column<int>(type: "integer", nullable: false),
                    PedidoCompraItemId = table.Column<int>(type: "integer", nullable: true),
                    ProdutoId = table.Column<int>(type: "integer", nullable: false),
                    Quantidade = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    CustoUnitario = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    CustoTotal = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    LoteId = table.Column<int>(type: "integer", nullable: true),
                    CodigoLote = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    DataValidadeLote = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DataFabricacaoLote = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    NumerosSerieTexto = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    Observacao = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    CriadoEm = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CriadoPor = table.Column<string>(type: "text", nullable: true),
                    AlteradoEm = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    AlteradoPor = table.Column<string>(type: "text", nullable: true),
                    Ativo = table.Column<bool>(type: "boolean", nullable: false),
                    EmpresaId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EntradaMercadoriaItens", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EntradaMercadoriaItens_Empresas_EmpresaId",
                        column: x => x.EmpresaId,
                        principalTable: "Empresas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_EntradaMercadoriaItens_EntradasMercadoria_EntradaMercadoria~",
                        column: x => x.EntradaMercadoriaId,
                        principalTable: "EntradasMercadoria",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_EntradaMercadoriaItens_Lotes_LoteId",
                        column: x => x.LoteId,
                        principalTable: "Lotes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_EntradaMercadoriaItens_PedidoCompraItens_PedidoCompraItemId",
                        column: x => x.PedidoCompraItemId,
                        principalTable: "PedidoCompraItens",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_EntradaMercadoriaItens_Produtos_ProdutoId",
                        column: x => x.ProdutoId,
                        principalTable: "Produtos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_EntradaMercadoriaItens_EmpresaId",
                table: "EntradaMercadoriaItens",
                column: "EmpresaId");

            migrationBuilder.CreateIndex(
                name: "IX_EntradaMercadoriaItens_EntradaMercadoriaId",
                table: "EntradaMercadoriaItens",
                column: "EntradaMercadoriaId");

            migrationBuilder.CreateIndex(
                name: "IX_EntradaMercadoriaItens_LoteId",
                table: "EntradaMercadoriaItens",
                column: "LoteId");

            migrationBuilder.CreateIndex(
                name: "IX_EntradaMercadoriaItens_PedidoCompraItemId",
                table: "EntradaMercadoriaItens",
                column: "PedidoCompraItemId");

            migrationBuilder.CreateIndex(
                name: "IX_EntradaMercadoriaItens_ProdutoId",
                table: "EntradaMercadoriaItens",
                column: "ProdutoId");

            migrationBuilder.CreateIndex(
                name: "IX_EntradasMercadoria_DepositoId",
                table: "EntradasMercadoria",
                column: "DepositoId");

            migrationBuilder.CreateIndex(
                name: "IX_EntradasMercadoria_EmpresaId_FornecedorId_NumeroNotaFiscal_~",
                table: "EntradasMercadoria",
                columns: new[] { "EmpresaId", "FornecedorId", "NumeroNotaFiscal", "SerieNotaFiscal" });

            migrationBuilder.CreateIndex(
                name: "IX_EntradasMercadoria_EmpresaId_Numero",
                table: "EntradasMercadoria",
                columns: new[] { "EmpresaId", "Numero" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_EntradasMercadoria_FornecedorId",
                table: "EntradasMercadoria",
                column: "FornecedorId");

            migrationBuilder.CreateIndex(
                name: "IX_EntradasMercadoria_IdParcelamento",
                table: "EntradasMercadoria",
                column: "IdParcelamento");

            migrationBuilder.CreateIndex(
                name: "IX_EntradasMercadoria_PedidoCompraId",
                table: "EntradasMercadoria",
                column: "PedidoCompraId");

            migrationBuilder.CreateIndex(
                name: "IX_PedidoCompraItens_EmpresaId",
                table: "PedidoCompraItens",
                column: "EmpresaId");

            migrationBuilder.CreateIndex(
                name: "IX_PedidoCompraItens_PedidoCompraId",
                table: "PedidoCompraItens",
                column: "PedidoCompraId");

            migrationBuilder.CreateIndex(
                name: "IX_PedidoCompraItens_ProdutoId",
                table: "PedidoCompraItens",
                column: "ProdutoId");

            migrationBuilder.CreateIndex(
                name: "IX_PedidosCompra_DepositoDestinoId",
                table: "PedidosCompra",
                column: "DepositoDestinoId");

            migrationBuilder.CreateIndex(
                name: "IX_PedidosCompra_EmpresaId_Numero",
                table: "PedidosCompra",
                columns: new[] { "EmpresaId", "Numero" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PedidosCompra_EmpresaId_Status",
                table: "PedidosCompra",
                columns: new[] { "EmpresaId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_PedidosCompra_FormaPagamentoId",
                table: "PedidosCompra",
                column: "FormaPagamentoId");

            migrationBuilder.CreateIndex(
                name: "IX_PedidosCompra_FornecedorId",
                table: "PedidosCompra",
                column: "FornecedorId");

            migrationBuilder.CreateIndex(
                name: "IX_PedidosCompra_SolicitacaoCompraId",
                table: "PedidosCompra",
                column: "SolicitacaoCompraId");

            migrationBuilder.CreateIndex(
                name: "IX_SolicitacaoCompraItens_EmpresaId",
                table: "SolicitacaoCompraItens",
                column: "EmpresaId");

            migrationBuilder.CreateIndex(
                name: "IX_SolicitacaoCompraItens_ProdutoId",
                table: "SolicitacaoCompraItens",
                column: "ProdutoId");

            migrationBuilder.CreateIndex(
                name: "IX_SolicitacaoCompraItens_SolicitacaoCompraId",
                table: "SolicitacaoCompraItens",
                column: "SolicitacaoCompraId");

            migrationBuilder.CreateIndex(
                name: "IX_SolicitacoesCompra_EmpresaId_Numero",
                table: "SolicitacoesCompra",
                columns: new[] { "EmpresaId", "Numero" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SolicitacoesCompra_EmpresaId_Status",
                table: "SolicitacoesCompra",
                columns: new[] { "EmpresaId", "Status" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "EntradaMercadoriaItens");

            migrationBuilder.DropTable(
                name: "SolicitacaoCompraItens");

            migrationBuilder.DropTable(
                name: "EntradasMercadoria");

            migrationBuilder.DropTable(
                name: "PedidoCompraItens");

            migrationBuilder.DropTable(
                name: "PedidosCompra");

            migrationBuilder.DropTable(
                name: "SolicitacoesCompra");
        }
    }
}
