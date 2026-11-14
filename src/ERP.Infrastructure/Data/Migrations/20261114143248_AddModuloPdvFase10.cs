using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace ERP.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddModuloPdvFase10 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "TerminaisPdv",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Codigo = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Nome = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    Tipo = table.Column<int>(type: "integer", nullable: false),
                    DepositoId = table.Column<int>(type: "integer", nullable: true),
                    ContaBancariaId = table.Column<int>(type: "integer", nullable: true),
                    ImpressoraCupom = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: true),
                    CriadoEm = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CriadoPor = table.Column<string>(type: "text", nullable: true),
                    AlteradoEm = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    AlteradoPor = table.Column<string>(type: "text", nullable: true),
                    Ativo = table.Column<bool>(type: "boolean", nullable: false),
                    EmpresaId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TerminaisPdv", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TerminaisPdv_ContasBancarias_ContaBancariaId",
                        column: x => x.ContaBancariaId,
                        principalTable: "ContasBancarias",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_TerminaisPdv_Depositos_DepositoId",
                        column: x => x.DepositoId,
                        principalTable: "Depositos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_TerminaisPdv_Empresas_EmpresaId",
                        column: x => x.EmpresaId,
                        principalTable: "Empresas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SessoesCaixaPdv",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    TerminalPdvId = table.Column<int>(type: "integer", nullable: false),
                    OperadorId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    OperadorNome = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    DataAbertura = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ValorAbertura = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    DataFechamento = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ValorFechamentoInformado = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    ValorFechamentoCalculado = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    DiferencaQuebra = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    ObservacoesFechamento = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    CriadoEm = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CriadoPor = table.Column<string>(type: "text", nullable: true),
                    AlteradoEm = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    AlteradoPor = table.Column<string>(type: "text", nullable: true),
                    Ativo = table.Column<bool>(type: "boolean", nullable: false),
                    EmpresaId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SessoesCaixaPdv", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SessoesCaixaPdv_Empresas_EmpresaId",
                        column: x => x.EmpresaId,
                        principalTable: "Empresas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SessoesCaixaPdv_TerminaisPdv_TerminalPdvId",
                        column: x => x.TerminalPdvId,
                        principalTable: "TerminaisPdv",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "MovimentosCaixaPdv",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    SessaoCaixaPdvId = table.Column<int>(type: "integer", nullable: false),
                    Tipo = table.Column<int>(type: "integer", nullable: false),
                    Valor = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    Motivo = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    DataMovimento = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    OperadorNome = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: true),
                    CriadoEm = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CriadoPor = table.Column<string>(type: "text", nullable: true),
                    AlteradoEm = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    AlteradoPor = table.Column<string>(type: "text", nullable: true),
                    Ativo = table.Column<bool>(type: "boolean", nullable: false),
                    EmpresaId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MovimentosCaixaPdv", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MovimentosCaixaPdv_Empresas_EmpresaId",
                        column: x => x.EmpresaId,
                        principalTable: "Empresas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_MovimentosCaixaPdv_SessoesCaixaPdv_SessaoCaixaPdvId",
                        column: x => x.SessaoCaixaPdvId,
                        principalTable: "SessoesCaixaPdv",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "VendasPdv",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Numero = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    TerminalPdvId = table.Column<int>(type: "integer", nullable: false),
                    SessaoCaixaPdvId = table.Column<int>(type: "integer", nullable: true),
                    ClienteId = table.Column<int>(type: "integer", nullable: true),
                    ConsumidorNome = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    ConsumidorCpf = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    VendedorId = table.Column<int>(type: "integer", nullable: true),
                    VendedorNome = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: true),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    DataVenda = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ValorBruto = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    ValorDesconto = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    ValorTotal = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    Troco = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    Observacoes = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    CriadoEm = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CriadoPor = table.Column<string>(type: "text", nullable: true),
                    AlteradoEm = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    AlteradoPor = table.Column<string>(type: "text", nullable: true),
                    Ativo = table.Column<bool>(type: "boolean", nullable: false),
                    EmpresaId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VendasPdv", x => x.Id);
                    table.ForeignKey(
                        name: "FK_VendasPdv_Clientes_ClienteId",
                        column: x => x.ClienteId,
                        principalTable: "Clientes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_VendasPdv_Empresas_EmpresaId",
                        column: x => x.EmpresaId,
                        principalTable: "Empresas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_VendasPdv_SessoesCaixaPdv_SessaoCaixaPdvId",
                        column: x => x.SessaoCaixaPdvId,
                        principalTable: "SessoesCaixaPdv",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_VendasPdv_TerminaisPdv_TerminalPdvId",
                        column: x => x.TerminalPdvId,
                        principalTable: "TerminaisPdv",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_VendasPdv_Vendedores_VendedorId",
                        column: x => x.VendedorId,
                        principalTable: "Vendedores",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "VendaPdvItens",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    VendaPdvId = table.Column<int>(type: "integer", nullable: false),
                    ProdutoId = table.Column<int>(type: "integer", nullable: false),
                    CodigoProduto = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    NomeProduto = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Quantidade = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    ValorUnitario = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    ValorDesconto = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    ValorTotal = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    CriadoEm = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CriadoPor = table.Column<string>(type: "text", nullable: true),
                    AlteradoEm = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    AlteradoPor = table.Column<string>(type: "text", nullable: true),
                    Ativo = table.Column<bool>(type: "boolean", nullable: false),
                    EmpresaId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VendaPdvItens", x => x.Id);
                    table.ForeignKey(
                        name: "FK_VendaPdvItens_Empresas_EmpresaId",
                        column: x => x.EmpresaId,
                        principalTable: "Empresas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_VendaPdvItens_Produtos_ProdutoId",
                        column: x => x.ProdutoId,
                        principalTable: "Produtos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_VendaPdvItens_VendasPdv_VendaPdvId",
                        column: x => x.VendaPdvId,
                        principalTable: "VendasPdv",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "VendaPdvPagamentos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    VendaPdvId = table.Column<int>(type: "integer", nullable: false),
                    FormaPagamentoId = table.Column<int>(type: "integer", nullable: true),
                    DescricaoForma = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Valor = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    CriadoEm = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CriadoPor = table.Column<string>(type: "text", nullable: true),
                    AlteradoEm = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    AlteradoPor = table.Column<string>(type: "text", nullable: true),
                    Ativo = table.Column<bool>(type: "boolean", nullable: false),
                    EmpresaId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VendaPdvPagamentos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_VendaPdvPagamentos_Empresas_EmpresaId",
                        column: x => x.EmpresaId,
                        principalTable: "Empresas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_VendaPdvPagamentos_FormasPagamento_FormaPagamentoId",
                        column: x => x.FormaPagamentoId,
                        principalTable: "FormasPagamento",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_VendaPdvPagamentos_VendasPdv_VendaPdvId",
                        column: x => x.VendaPdvId,
                        principalTable: "VendasPdv",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_MovimentosCaixaPdv_EmpresaId",
                table: "MovimentosCaixaPdv",
                column: "EmpresaId");

            migrationBuilder.CreateIndex(
                name: "IX_MovimentosCaixaPdv_SessaoCaixaPdvId",
                table: "MovimentosCaixaPdv",
                column: "SessaoCaixaPdvId");

            migrationBuilder.CreateIndex(
                name: "IX_SessoesCaixaPdv_EmpresaId_TerminalPdvId_Status",
                table: "SessoesCaixaPdv",
                columns: new[] { "EmpresaId", "TerminalPdvId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_SessoesCaixaPdv_TerminalPdvId",
                table: "SessoesCaixaPdv",
                column: "TerminalPdvId");

            migrationBuilder.CreateIndex(
                name: "IX_TerminaisPdv_ContaBancariaId",
                table: "TerminaisPdv",
                column: "ContaBancariaId");

            migrationBuilder.CreateIndex(
                name: "IX_TerminaisPdv_DepositoId",
                table: "TerminaisPdv",
                column: "DepositoId");

            migrationBuilder.CreateIndex(
                name: "IX_TerminaisPdv_EmpresaId_Codigo",
                table: "TerminaisPdv",
                columns: new[] { "EmpresaId", "Codigo" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_VendaPdvItens_EmpresaId",
                table: "VendaPdvItens",
                column: "EmpresaId");

            migrationBuilder.CreateIndex(
                name: "IX_VendaPdvItens_ProdutoId",
                table: "VendaPdvItens",
                column: "ProdutoId");

            migrationBuilder.CreateIndex(
                name: "IX_VendaPdvItens_VendaPdvId",
                table: "VendaPdvItens",
                column: "VendaPdvId");

            migrationBuilder.CreateIndex(
                name: "IX_VendaPdvPagamentos_EmpresaId",
                table: "VendaPdvPagamentos",
                column: "EmpresaId");

            migrationBuilder.CreateIndex(
                name: "IX_VendaPdvPagamentos_FormaPagamentoId",
                table: "VendaPdvPagamentos",
                column: "FormaPagamentoId");

            migrationBuilder.CreateIndex(
                name: "IX_VendaPdvPagamentos_VendaPdvId",
                table: "VendaPdvPagamentos",
                column: "VendaPdvId");

            migrationBuilder.CreateIndex(
                name: "IX_VendasPdv_ClienteId",
                table: "VendasPdv",
                column: "ClienteId");

            migrationBuilder.CreateIndex(
                name: "IX_VendasPdv_EmpresaId_Numero",
                table: "VendasPdv",
                columns: new[] { "EmpresaId", "Numero" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_VendasPdv_EmpresaId_Status",
                table: "VendasPdv",
                columns: new[] { "EmpresaId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_VendasPdv_SessaoCaixaPdvId",
                table: "VendasPdv",
                column: "SessaoCaixaPdvId");

            migrationBuilder.CreateIndex(
                name: "IX_VendasPdv_TerminalPdvId",
                table: "VendasPdv",
                column: "TerminalPdvId");

            migrationBuilder.CreateIndex(
                name: "IX_VendasPdv_VendedorId",
                table: "VendasPdv",
                column: "VendedorId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MovimentosCaixaPdv");

            migrationBuilder.DropTable(
                name: "VendaPdvItens");

            migrationBuilder.DropTable(
                name: "VendaPdvPagamentos");

            migrationBuilder.DropTable(
                name: "VendasPdv");

            migrationBuilder.DropTable(
                name: "SessoesCaixaPdv");

            migrationBuilder.DropTable(
                name: "TerminaisPdv");
        }
    }
}
