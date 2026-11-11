using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Mignow.Atc.Infra.Database.Migrations
{
    /// <inheritdoc />
    public partial class ItemApplicationResult : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "APPLICATION_RESULT",
                schema: "public",
                table: "TB_REMEDIATION_ITEM",
                type: "character varying(40)",
                maxLength: 40,
                nullable: true);

            // itens já aplicados: copia o resultado do JSON da aplicação (a coluna passa a ser mantida pelo SetApplication)
            migrationBuilder.Sql("""
                UPDATE public."TB_REMEDIATION_ITEM"
                   SET "APPLICATION_RESULT" = left("APPLICATION"::jsonb ->> 'result', 40)
                 WHERE "APPLICATION" IS NOT NULL;
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "APPLICATION_RESULT",
                schema: "public",
                table: "TB_REMEDIATION_ITEM");
        }
    }
}
