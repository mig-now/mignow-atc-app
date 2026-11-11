using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Mignow.Atc.Infra.Database.Migrations
{
    /// <inheritdoc />
    public partial class ItemAwaitingApproval : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "NEEDS_MANUAL_REVIEW",
                schema: "public",
                table: "TB_REMEDIATION_ITEM",
                newName: "AWAITING_APPROVAL");

            // regra nova: toda proposta esperando o consultor, menos "sem cartão na KB" (proposta KB pendente, ainda falta a IA)
            migrationBuilder.Sql("""
                UPDATE public."TB_REMEDIATION_ITEM"
                   SET "AWAITING_APPROVAL" = "PROPOSAL" IS NOT NULL
                       AND NOT ("PROPOSAL"::jsonb ->> 'origin' = 'KB' AND "PROPOSAL"::jsonb ->> 'action' = 'pendente');
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "AWAITING_APPROVAL",
                schema: "public",
                table: "TB_REMEDIATION_ITEM",
                newName: "NEEDS_MANUAL_REVIEW");
        }
    }
}
