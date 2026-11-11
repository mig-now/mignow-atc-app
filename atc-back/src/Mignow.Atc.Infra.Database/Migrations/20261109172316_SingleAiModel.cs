using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Mignow.Atc.Infra.Database.Migrations
{
    /// <inheritdoc />
    public partial class SingleAiModel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Um só modelo para todos os usos (decisão de 09.11.2026). Antes de tirar o uso, deixa um único padrão: o padrão
            // ativo da análise (Claude Opus); sem ele, o padrão ativo mais antigo. Os demais cadastros ficam, sem padrão.
            migrationBuilder.Sql("""
                WITH keep AS (
                    SELECT "ID" FROM public."TB_AI_PROVIDER"
                    WHERE "IS_DEFAULT" AND "ACTIVE"
                    ORDER BY ("USAGE" = 'Analysis') DESC, "ID"
                    LIMIT 1)
                UPDATE public."TB_AI_PROVIDER" SET "IS_DEFAULT" = ("ID" IN (SELECT "ID" FROM keep));
                """);
            migrationBuilder.DropColumn(
                name: "USAGE",
                schema: "public",
                table: "TB_AI_PROVIDER");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "USAGE",
                schema: "public",
                table: "TB_AI_PROVIDER",
                type: "character varying(40)",
                maxLength: 40,
                nullable: false,
                defaultValue: "Analysis");
        }
    }
}
