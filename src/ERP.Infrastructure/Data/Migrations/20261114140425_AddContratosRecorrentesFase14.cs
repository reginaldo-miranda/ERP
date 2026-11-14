using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace ERP.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddContratosRecorrentesFase14 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ContratosRecorrentes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Numero = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    Titulo = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Descricao = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    ClienteId = table.Column<int>(type: "integer", nullable: false),
                    ServicoId = table.Column<int>(type: "integer", nullable: true),
                    FormaPagamentoId = table.Column<int>(type: "integer", nullable: true),
                    Periodicidade = table.Column<int>(type: "integer", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    DataInicio = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DataFim = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DiaVencimento = table.Column<int>(type: "integer", nullable: false),
                    ValorMensal = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    PercentualReajustePadrao = table.Column<decimal>(type: "numeric(10,4)", precision: 10, scale: 4, nullable: false),
                    IndiceReajuste = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    RenovacaoAutomatica = table.Column<bool>(type: "boolean", nullable: false),
                    ProximaCobranca = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DataUltimoFaturamento = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DataUltimoReajuste = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Observacoes = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    CriadoEm = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CriadoPor = table.Column<string>(type: "text", nullable: true),
                    AlteradoEm = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    AlteradoPor = table.Column<string>(type: "text", nullable: true),
                    Ativo = table.Column<bool>(type: "boolean", nullable: false),
                    EmpresaId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ContratosRecorrentes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ContratosRecorrentes_Clientes_ClienteId",
                        column: x => x.ClienteId,
                        principalTable: "Clientes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ContratosRecorrentes_Empresas_EmpresaId",
                        column: x => x.EmpresaId,
                        principalTable: "Empresas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ContratosRecorrentes_FormasPagamento_FormaPagamentoId",
                        column: x => x.FormaPagamentoId,
                        principalTable: "FormasPagamento",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ContratosRecorrentes_Servicos_ServicoId",
                        column: x => x.ServicoId,
                        principalTable: "Servicos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ContratoFaturamentos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ContratoRecorrenteId = table.Column<int>(type: "integer", nullable: false),
                    Competencia = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    DataReferencia = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DataVencimento = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ValorFaturado = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    ContaReceberId = table.Column<int>(type: "integer", nullable: true),
                    CriadoEm = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CriadoPor = table.Column<string>(type: "text", nullable: true),
                    AlteradoEm = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    AlteradoPor = table.Column<string>(type: "text", nullable: true),
                    Ativo = table.Column<bool>(type: "boolean", nullable: false),
                    EmpresaId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ContratoFaturamentos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ContratoFaturamentos_ContasReceber_ContaReceberId",
                        column: x => x.ContaReceberId,
                        principalTable: "ContasReceber",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_ContratoFaturamentos_ContratosRecorrentes_ContratoRecorrent~",
                        column: x => x.ContratoRecorrenteId,
                        principalTable: "ContratosRecorrentes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ContratoFaturamentos_Empresas_EmpresaId",
                        column: x => x.EmpresaId,
                        principalTable: "Empresas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ContratoReajustes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ContratoRecorrenteId = table.Column<int>(type: "integer", nullable: false),
                    DataReajuste = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ValorAnterior = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    ValorNovo = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    PercentualAplicado = table.Column<decimal>(type: "numeric(10,4)", precision: 10, scale: 4, nullable: false),
                    Indice = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
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
                    table.PrimaryKey("PK_ContratoReajustes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ContratoReajustes_ContratosRecorrentes_ContratoRecorrenteId",
                        column: x => x.ContratoRecorrenteId,
                        principalTable: "ContratosRecorrentes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ContratoReajustes_Empresas_EmpresaId",
                        column: x => x.EmpresaId,
                        principalTable: "Empresas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ContratoFaturamentos_ContaReceberId",
                table: "ContratoFaturamentos",
                column: "ContaReceberId");

            migrationBuilder.CreateIndex(
                name: "IX_ContratoFaturamentos_ContratoRecorrenteId",
                table: "ContratoFaturamentos",
                column: "ContratoRecorrenteId");

            migrationBuilder.CreateIndex(
                name: "IX_ContratoFaturamentos_EmpresaId",
                table: "ContratoFaturamentos",
                column: "EmpresaId");

            migrationBuilder.CreateIndex(
                name: "IX_ContratoReajustes_ContratoRecorrenteId",
                table: "ContratoReajustes",
                column: "ContratoRecorrenteId");

            migrationBuilder.CreateIndex(
                name: "IX_ContratoReajustes_EmpresaId",
                table: "ContratoReajustes",
                column: "EmpresaId");

            migrationBuilder.CreateIndex(
                name: "IX_ContratosRecorrentes_ClienteId",
                table: "ContratosRecorrentes",
                column: "ClienteId");

            migrationBuilder.CreateIndex(
                name: "IX_ContratosRecorrentes_EmpresaId_Numero",
                table: "ContratosRecorrentes",
                columns: new[] { "EmpresaId", "Numero" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ContratosRecorrentes_EmpresaId_Status_ProximaCobranca",
                table: "ContratosRecorrentes",
                columns: new[] { "EmpresaId", "Status", "ProximaCobranca" });

            migrationBuilder.CreateIndex(
                name: "IX_ContratosRecorrentes_FormaPagamentoId",
                table: "ContratosRecorrentes",
                column: "FormaPagamentoId");

            migrationBuilder.CreateIndex(
                name: "IX_ContratosRecorrentes_ServicoId",
                table: "ContratosRecorrentes",
                column: "ServicoId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ContratoFaturamentos");

            migrationBuilder.DropTable(
                name: "ContratoReajustes");

            migrationBuilder.DropTable(
                name: "ContratosRecorrentes");
        }
    }
}
