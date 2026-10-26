using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace ERP.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddImportacaoXmlNfeEDeParaProdutos : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "XmlNfeOriginal",
                table: "EntradasMercadoria",
                type: "text",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "ProdutosFornecedores",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ProdutoId = table.Column<int>(type: "integer", nullable: false),
                    FornecedorId = table.Column<int>(type: "integer", nullable: false),
                    CodigoProdutoFornecedor = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: false),
                    DescricaoProdutoFornecedor = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    UnidadeMedidaFornecedor = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    FatorConversao = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false, defaultValue: 1.0m),
                    CriadoEm = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CriadoPor = table.Column<string>(type: "text", nullable: true),
                    AlteradoEm = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    AlteradoPor = table.Column<string>(type: "text", nullable: true),
                    Ativo = table.Column<bool>(type: "boolean", nullable: false),
                    EmpresaId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProdutosFornecedores", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProdutosFornecedores_Empresas_EmpresaId",
                        column: x => x.EmpresaId,
                        principalTable: "Empresas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ProdutosFornecedores_Fornecedores_FornecedorId",
                        column: x => x.FornecedorId,
                        principalTable: "Fornecedores",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ProdutosFornecedores_Produtos_ProdutoId",
                        column: x => x.ProdutoId,
                        principalTable: "Produtos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ProdutosFornecedores_EmpresaId_FornecedorId_CodigoProdutoFo~",
                table: "ProdutosFornecedores",
                columns: new[] { "EmpresaId", "FornecedorId", "CodigoProdutoFornecedor" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ProdutosFornecedores_FornecedorId",
                table: "ProdutosFornecedores",
                column: "FornecedorId");

            migrationBuilder.CreateIndex(
                name: "IX_ProdutosFornecedores_ProdutoId",
                table: "ProdutosFornecedores",
                column: "ProdutoId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ProdutosFornecedores");

            migrationBuilder.DropColumn(
                name: "XmlNfeOriginal",
                table: "EntradasMercadoria");
        }
    }
}
