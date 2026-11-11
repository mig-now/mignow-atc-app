using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Mignow.Atc.Infra.Database.Migrations
{
    /// <inheritdoc />
    public partial class ItemNeedsManualReview : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "NEEDS_MANUAL_REVIEW",
                schema: "public",
                table: "TB_REMEDIATION_ITEM",
                type: "boolean",
                nullable: false,
                defaultValue: false);
            // itens já analisados: a IA devolveu "pendente" (precisa de análise manual)
            migrationBuilder.Sql("""
                UPDATE public."TB_REMEDIATION_ITEM"
                   SET "NEEDS_MANUAL_REVIEW" = TRUE
                 WHERE "PROPOSAL"::jsonb ->> 'action' = 'pendente' AND "PROPOSAL"::jsonb ->> 'origin' = 'IA';
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "NEEDS_MANUAL_REVIEW",
                schema: "public",
                table: "TB_REMEDIATION_ITEM");
        }
    }
}
