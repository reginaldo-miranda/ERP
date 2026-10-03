using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace ERP.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddConciliacaoBancariaFase3B : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ExtratosImportados",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ContaBancariaId = table.Column<int>(type: "integer", nullable: false),
                    NomeArquivo = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    DataImportacao = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DataInicioExtrato = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DataFimExtrato = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    TotalRegistros = table.Column<int>(type: "integer", nullable: false),
                    TotalCreditos = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    TotalDebitos = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
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
                    table.PrimaryKey("PK_ExtratosImportados", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ExtratosImportados_ContasBancarias_ContaBancariaId",
                        column: x => x.ContaBancariaId,
                        principalTable: "ContasBancarias",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ExtratosImportados_Empresas_EmpresaId",
                        column: x => x.EmpresaId,
                        principalTable: "Empresas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ExtratosImportadosItens",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ExtratoImportadoId = table.Column<int>(type: "integer", nullable: false),
                    TransacaoId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Data = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Valor = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    Descricao = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    TipoTransacao = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    StatusConciliacao = table.Column<int>(type: "integer", nullable: false),
                    MovimentacaoFinanceiraId = table.Column<int>(type: "integer", nullable: true),
                    ContaPagarId = table.Column<int>(type: "integer", nullable: true),
                    ContaReceberId = table.Column<int>(type: "integer", nullable: true),
                    DataConciliacao = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
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
                    table.PrimaryKey("PK_ExtratosImportadosItens", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ExtratosImportadosItens_ContasPagar_ContaPagarId",
                        column: x => x.ContaPagarId,
                        principalTable: "ContasPagar",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_ExtratosImportadosItens_ContasReceber_ContaReceberId",
                        column: x => x.ContaReceberId,
                        principalTable: "ContasReceber",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_ExtratosImportadosItens_Empresas_EmpresaId",
                        column: x => x.EmpresaId,
                        principalTable: "Empresas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ExtratosImportadosItens_ExtratosImportados_ExtratoImportado~",
                        column: x => x.ExtratoImportadoId,
                        principalTable: "ExtratosImportados",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ExtratosImportadosItens_MovimentacoesFinanceiras_Movimentac~",
                        column: x => x.MovimentacaoFinanceiraId,
                        principalTable: "MovimentacoesFinanceiras",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ExtratosImportados_ContaBancariaId",
                table: "ExtratosImportados",
                column: "ContaBancariaId");

            migrationBuilder.CreateIndex(
                name: "IX_ExtratosImportados_EmpresaId_ContaBancariaId",
                table: "ExtratosImportados",
                columns: new[] { "EmpresaId", "ContaBancariaId" });

            migrationBuilder.CreateIndex(
                name: "IX_ExtratosImportados_EmpresaId_DataImportacao",
                table: "ExtratosImportados",
                columns: new[] { "EmpresaId", "DataImportacao" });

            migrationBuilder.CreateIndex(
                name: "IX_ExtratosImportadosItens_ContaPagarId",
                table: "ExtratosImportadosItens",
                column: "ContaPagarId");

            migrationBuilder.CreateIndex(
                name: "IX_ExtratosImportadosItens_ContaReceberId",
                table: "ExtratosImportadosItens",
                column: "ContaReceberId");

            migrationBuilder.CreateIndex(
                name: "IX_ExtratosImportadosItens_EmpresaId_StatusConciliacao",
                table: "ExtratosImportadosItens",
                columns: new[] { "EmpresaId", "StatusConciliacao" });

            migrationBuilder.CreateIndex(
                name: "IX_ExtratosImportadosItens_ExtratoImportadoId_TransacaoId",
                table: "ExtratosImportadosItens",
                columns: new[] { "ExtratoImportadoId", "TransacaoId" });

            migrationBuilder.CreateIndex(
                name: "IX_ExtratosImportadosItens_MovimentacaoFinanceiraId",
                table: "ExtratosImportadosItens",
                column: "MovimentacaoFinanceiraId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ExtratosImportadosItens");

            migrationBuilder.DropTable(
                name: "ExtratosImportados");
        }
    }
}
