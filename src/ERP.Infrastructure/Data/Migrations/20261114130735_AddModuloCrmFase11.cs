using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace ERP.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddModuloCrmFase11 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CrmLeads",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Nome = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    EmpresaContato = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    TipoPessoa = table.Column<int>(type: "integer", nullable: false),
                    CpfCnpj = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    Email = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    Telefone = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    Celular = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    Cargo = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Origem = table.Column<int>(type: "integer", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    Observacoes = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    ResponsavelId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    ResponsavelNome = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: true),
                    ClienteConvertidoId = table.Column<int>(type: "integer", nullable: true),
                    DataConversao = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CriadoEm = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CriadoPor = table.Column<string>(type: "text", nullable: true),
                    AlteradoEm = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    AlteradoPor = table.Column<string>(type: "text", nullable: true),
                    Ativo = table.Column<bool>(type: "boolean", nullable: false),
                    EmpresaId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CrmLeads", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CrmLeads_Clientes_ClienteConvertidoId",
                        column: x => x.ClienteConvertidoId,
                        principalTable: "Clientes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CrmLeads_Empresas_EmpresaId",
                        column: x => x.EmpresaId,
                        principalTable: "Empresas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CrmOportunidades",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Codigo = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    Titulo = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Descricao = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    LeadId = table.Column<int>(type: "integer", nullable: true),
                    ClienteId = table.Column<int>(type: "integer", nullable: true),
                    Etapa = table.Column<int>(type: "integer", nullable: false),
                    ValorEstimado = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    Probabilidade = table.Column<int>(type: "integer", nullable: false),
                    DataFechamentoPrevista = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DataFechamentoReal = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ResponsavelId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    ResponsavelNome = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: true),
                    MotivoPerda = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    OrcamentoId = table.Column<int>(type: "integer", nullable: true),
                    PedidoVendaId = table.Column<int>(type: "integer", nullable: true),
                    CriadoEm = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CriadoPor = table.Column<string>(type: "text", nullable: true),
                    AlteradoEm = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    AlteradoPor = table.Column<string>(type: "text", nullable: true),
                    Ativo = table.Column<bool>(type: "boolean", nullable: false),
                    EmpresaId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CrmOportunidades", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CrmOportunidades_Clientes_ClienteId",
                        column: x => x.ClienteId,
                        principalTable: "Clientes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CrmOportunidades_CrmLeads_LeadId",
                        column: x => x.LeadId,
                        principalTable: "CrmLeads",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CrmOportunidades_Empresas_EmpresaId",
                        column: x => x.EmpresaId,
                        principalTable: "Empresas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CrmOportunidades_Orcamentos_OrcamentoId",
                        column: x => x.OrcamentoId,
                        principalTable: "Orcamentos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CrmOportunidades_PedidosVenda_PedidoVendaId",
                        column: x => x.PedidoVendaId,
                        principalTable: "PedidosVenda",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "CrmAtividades",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Tipo = table.Column<int>(type: "integer", nullable: false),
                    Assunto = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Descricao = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    DataPrevista = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DataConclusao = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Concluida = table.Column<bool>(type: "boolean", nullable: false),
                    LeadId = table.Column<int>(type: "integer", nullable: true),
                    OportunidadeId = table.Column<int>(type: "integer", nullable: true),
                    ResponsavelId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    ResponsavelNome = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: true),
                    CriadoEm = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CriadoPor = table.Column<string>(type: "text", nullable: true),
                    AlteradoEm = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    AlteradoPor = table.Column<string>(type: "text", nullable: true),
                    Ativo = table.Column<bool>(type: "boolean", nullable: false),
                    EmpresaId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CrmAtividades", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CrmAtividades_CrmLeads_LeadId",
                        column: x => x.LeadId,
                        principalTable: "CrmLeads",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CrmAtividades_CrmOportunidades_OportunidadeId",
                        column: x => x.OportunidadeId,
                        principalTable: "CrmOportunidades",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CrmAtividades_Empresas_EmpresaId",
                        column: x => x.EmpresaId,
                        principalTable: "Empresas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CrmAtividades_EmpresaId_Concluida_DataPrevista",
                table: "CrmAtividades",
                columns: new[] { "EmpresaId", "Concluida", "DataPrevista" });

            migrationBuilder.CreateIndex(
                name: "IX_CrmAtividades_LeadId",
                table: "CrmAtividades",
                column: "LeadId");

            migrationBuilder.CreateIndex(
                name: "IX_CrmAtividades_OportunidadeId",
                table: "CrmAtividades",
                column: "OportunidadeId");

            migrationBuilder.CreateIndex(
                name: "IX_CrmLeads_ClienteConvertidoId",
                table: "CrmLeads",
                column: "ClienteConvertidoId");

            migrationBuilder.CreateIndex(
                name: "IX_CrmLeads_Email",
                table: "CrmLeads",
                column: "Email");

            migrationBuilder.CreateIndex(
                name: "IX_CrmLeads_EmpresaId_Status",
                table: "CrmLeads",
                columns: new[] { "EmpresaId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_CrmLeads_ResponsavelId",
                table: "CrmLeads",
                column: "ResponsavelId");

            migrationBuilder.CreateIndex(
                name: "IX_CrmOportunidades_ClienteId",
                table: "CrmOportunidades",
                column: "ClienteId");

            migrationBuilder.CreateIndex(
                name: "IX_CrmOportunidades_EmpresaId_Codigo",
                table: "CrmOportunidades",
                columns: new[] { "EmpresaId", "Codigo" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CrmOportunidades_EmpresaId_Etapa",
                table: "CrmOportunidades",
                columns: new[] { "EmpresaId", "Etapa" });

            migrationBuilder.CreateIndex(
                name: "IX_CrmOportunidades_LeadId",
                table: "CrmOportunidades",
                column: "LeadId");

            migrationBuilder.CreateIndex(
                name: "IX_CrmOportunidades_OrcamentoId",
                table: "CrmOportunidades",
                column: "OrcamentoId");

            migrationBuilder.CreateIndex(
                name: "IX_CrmOportunidades_PedidoVendaId",
                table: "CrmOportunidades",
                column: "PedidoVendaId");

            migrationBuilder.CreateIndex(
                name: "IX_CrmOportunidades_ResponsavelId",
                table: "CrmOportunidades",
                column: "ResponsavelId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CrmAtividades");

            migrationBuilder.DropTable(
                name: "CrmOportunidades");

            migrationBuilder.DropTable(
                name: "CrmLeads");
        }
    }
}
