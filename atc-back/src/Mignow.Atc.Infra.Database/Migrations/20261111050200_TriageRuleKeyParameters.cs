using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Mignow.Atc.Infra.Database.Migrations
{
    /// <inheritdoc />
    public partial class TriageRuleKeyParameters : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "KEY",
                schema: "public",
                table: "TB_TRIAGE_RULE",
                type: "character varying(40)",
                maxLength: 40,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "PARAMETERS",
                schema: "public",
                table: "TB_TRIAGE_RULE",
                type: "jsonb",
                nullable: false,
                defaultValue: "{}");

            // linhas já gravadas: a chave fixa é o código atual (padrão R1..R9/produto ou o código da regra nova)
            migrationBuilder.Sql("""UPDATE public."TB_TRIAGE_RULE" SET "KEY" = "CODE" WHERE "KEY" = '';""");

            migrationBuilder.CreateIndex(
                name: "IX_TB_TRIAGE_RULE_KEY",
                schema: "public",
                table: "TB_TRIAGE_RULE",
                column: "KEY",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_TB_TRIAGE_RULE_KEY",
                schema: "public",
                table: "TB_TRIAGE_RULE");

            migrationBuilder.DropColumn(
                name: "KEY",
                schema: "public",
                table: "TB_TRIAGE_RULE");

            migrationBuilder.DropColumn(
                name: "PARAMETERS",
                schema: "public",
                table: "TB_TRIAGE_RULE");
        }
    }
}
