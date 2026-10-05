using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace ERP.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddModuloEstoqueFase4B : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Lotes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ProdutoId = table.Column<int>(type: "integer", nullable: false),
                    DepositoId = table.Column<int>(type: "integer", nullable: false),
                    CodigoLote = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    DataFabricacao = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DataValidade = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    QuantidadeInicial = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    QuantidadeAtual = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    CustoUnitario = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false, defaultValue: 0m),
                    Ativo = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    Observacoes = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    CriadoEm = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CriadoPor = table.Column<string>(type: "text", nullable: true),
                    AlteradoEm = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    AlteradoPor = table.Column<string>(type: "text", nullable: true),
                    EmpresaId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Lotes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Lotes_Depositos_DepositoId",
                        column: x => x.DepositoId,
                        principalTable: "Depositos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Lotes_Empresas_EmpresaId",
                        column: x => x.EmpresaId,
                        principalTable: "Empresas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Lotes_Produtos_ProdutoId",
                        column: x => x.ProdutoId,
                        principalTable: "Produtos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "NumerosSerie",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ProdutoId = table.Column<int>(type: "integer", nullable: false),
                    DepositoId = table.Column<int>(type: "integer", nullable: false),
                    LoteId = table.Column<int>(type: "integer", nullable: true),
                    Numero = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    DataEntrada = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DataSaida = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Observacoes = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    CriadoEm = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CriadoPor = table.Column<string>(type: "text", nullable: true),
                    AlteradoPor = table.Column<string>(type: "text", nullable: true),
                    Ativo = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    EmpresaId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NumerosSerie", x => x.Id);
                    table.ForeignKey(
                        name: "FK_NumerosSerie_Depositos_DepositoId",
                        column: x => x.DepositoId,
                        principalTable: "Depositos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_NumerosSerie_Empresas_EmpresaId",
                        column: x => x.EmpresaId,
                        principalTable: "Empresas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_NumerosSerie_Lotes_LoteId",
                        column: x => x.LoteId,
                        principalTable: "Lotes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_NumerosSerie_Produtos_ProdutoId",
                        column: x => x.ProdutoId,
                        principalTable: "Produtos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Inventarios",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    DepositoId = table.Column<int>(type: "integer", nullable: false),
                    CategoriaId = table.Column<int>(type: "integer", nullable: true),
                    Protocolo = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    DataAbertura = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DataFechamento = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    Responsavel = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    Observacao = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    CriadoEm = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CriadoPor = table.Column<string>(type: "text", nullable: true),
                    AlteradoEm = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    AlteradoPor = table.Column<string>(type: "text", nullable: true),
                    Ativo = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    EmpresaId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Inventarios", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Inventarios_Categorias_CategoriaId",
                        column: x => x.CategoriaId,
                        principalTable: "Categorias",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_Inventarios_Depositos_DepositoId",
                        column: x => x.DepositoId,
                        principalTable: "Depositos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Inventarios_Empresas_EmpresaId",
                        column: x => x.EmpresaId,
                        principalTable: "Empresas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "InventarioItens",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    InventarioId = table.Column<int>(type: "integer", nullable: false),
                    ProdutoId = table.Column<int>(type: "integer", nullable: false),
                    LoteId = table.Column<int>(type: "integer", nullable: true),
                    QuantidadeSistema = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    QuantidadeContada = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    CustoUnitario = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false, defaultValue: 0m),
                    AjusteAplicado = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    Observacao = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: true),
                    CriadoEm = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CriadoPor = table.Column<string>(type: "text", nullable: true),
                    AlteradoEm = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    AlteradoPor = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InventarioItens", x => x.Id);
                    table.ForeignKey(
                        name: "FK_InventarioItens_Inventarios_InventarioId",
                        column: x => x.InventarioId,
                        principalTable: "Inventarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_InventarioItens_Lotes_LoteId",
                        column: x => x.LoteId,
                        principalTable: "Lotes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_InventarioItens_Produtos_ProdutoId",
                        column: x => x.ProdutoId,
                        principalTable: "Produtos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.AddColumn<int>(
                name: "LoteId",
                table: "MovimentacoesEstoque",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "NumeroSerieId",
                table: "MovimentacoesEstoque",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_MovimentacoesEstoque_LoteId",
                table: "MovimentacoesEstoque",
                column: "LoteId");

            migrationBuilder.CreateIndex(
                name: "IX_MovimentacoesEstoque_NumeroSerieId",
                table: "MovimentacoesEstoque",
                column: "NumeroSerieId");

            migrationBuilder.CreateIndex(
                name: "IX_InventarioItens_InventarioId_ProdutoId_LoteId",
                table: "InventarioItens",
                columns: new[] { "InventarioId", "ProdutoId", "LoteId" });

            migrationBuilder.CreateIndex(
                name: "IX_InventarioItens_LoteId",
                table: "InventarioItens",
                column: "LoteId");

            migrationBuilder.CreateIndex(
                name: "IX_InventarioItens_ProdutoId",
                table: "InventarioItens",
                column: "ProdutoId");

            migrationBuilder.CreateIndex(
                name: "IX_Inventarios_CategoriaId",
                table: "Inventarios",
                column: "CategoriaId");

            migrationBuilder.CreateIndex(
                name: "IX_Inventarios_DepositoId",
                table: "Inventarios",
                column: "DepositoId");

            migrationBuilder.CreateIndex(
                name: "IX_Inventarios_EmpresaId_Protocolo",
                table: "Inventarios",
                columns: new[] { "EmpresaId", "Protocolo" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Inventarios_EmpresaId_Status",
                table: "Inventarios",
                columns: new[] { "EmpresaId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_Lotes_DepositoId",
                table: "Lotes",
                column: "DepositoId");

            migrationBuilder.CreateIndex(
                name: "IX_Lotes_EmpresaId_DataValidade",
                table: "Lotes",
                columns: new[] { "EmpresaId", "DataValidade" });

            migrationBuilder.CreateIndex(
                name: "IX_Lotes_EmpresaId_ProdutoId_CodigoLote_DepositoId",
                table: "Lotes",
                columns: new[] { "EmpresaId", "ProdutoId", "CodigoLote", "DepositoId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Lotes_ProdutoId",
                table: "Lotes",
                column: "ProdutoId");

            migrationBuilder.CreateIndex(
                name: "IX_NumerosSerie_DepositoId",
                table: "NumerosSerie",
                column: "DepositoId");

            migrationBuilder.CreateIndex(
                name: "IX_NumerosSerie_EmpresaId_ProdutoId_Numero",
                table: "NumerosSerie",
                columns: new[] { "EmpresaId", "ProdutoId", "Numero" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_NumerosSerie_EmpresaId_Status",
                table: "NumerosSerie",
                columns: new[] { "EmpresaId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_NumerosSerie_LoteId",
                table: "NumerosSerie",
                column: "LoteId");

            migrationBuilder.CreateIndex(
                name: "IX_NumerosSerie_ProdutoId",
                table: "NumerosSerie",
                column: "ProdutoId");

            migrationBuilder.AddForeignKey(
                name: "FK_MovimentacoesEstoque_Lotes_LoteId",
                table: "MovimentacoesEstoque",
                column: "LoteId",
                principalTable: "Lotes",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_MovimentacoesEstoque_NumerosSerie_NumeroSerieId",
                table: "MovimentacoesEstoque",
                column: "NumeroSerieId",
                principalTable: "NumerosSerie",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_MovimentacoesEstoque_Lotes_LoteId",
                table: "MovimentacoesEstoque");

            migrationBuilder.DropForeignKey(
                name: "FK_MovimentacoesEstoque_NumerosSerie_NumeroSerieId",
                table: "MovimentacoesEstoque");

            migrationBuilder.DropTable(
                name: "InventarioItens");

            migrationBuilder.DropTable(
                name: "Inventarios");

            migrationBuilder.DropTable(
                name: "NumerosSerie");

            migrationBuilder.DropTable(
                name: "Lotes");

            migrationBuilder.DropIndex(
                name: "IX_MovimentacoesEstoque_LoteId",
                table: "MovimentacoesEstoque");

            migrationBuilder.DropIndex(
                name: "IX_MovimentacoesEstoque_NumeroSerieId",
                table: "MovimentacoesEstoque");

            migrationBuilder.DropColumn(
                name: "LoteId",
                table: "MovimentacoesEstoque");

            migrationBuilder.DropColumn(
                name: "NumeroSerieId",
                table: "MovimentacoesEstoque");
        }
    }
}
