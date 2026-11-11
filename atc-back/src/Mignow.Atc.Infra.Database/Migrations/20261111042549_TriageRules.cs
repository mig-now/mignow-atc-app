using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Mignow.Atc.Infra.Database.Migrations
{
    /// <inheritdoc />
    public partial class TriageRules : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "TB_TRIAGE_RULE",
                schema: "public",
                columns: table => new
                {
                    ID = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CODE = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    DESCRIPTION = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    EFFECT = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    SCOPE = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    ACTIVE = table.Column<bool>(type: "boolean", nullable: false),
                    BUILT_IN = table.Column<bool>(type: "boolean", nullable: false),
                    REF_OBJECTS = table.Column<string>(type: "jsonb", nullable: false),
                    OBJECT_PATTERN = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: true),
                    MESSAGE_PATTERN = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    OBS = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    GUID = table.Column<Guid>(type: "uuid", nullable: false),
                    CREATED_AT = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CREATED_BY = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    UPDATED_AT = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    UPDATED_BY = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TB_TRIAGE_RULE", x => x.ID);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TB_TRIAGE_RULE_CODE",
                schema: "public",
                table: "TB_TRIAGE_RULE",
                column: "CODE",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TB_TRIAGE_RULE_GUID",
                schema: "public",
                table: "TB_TRIAGE_RULE",
                column: "GUID",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TB_TRIAGE_RULE",
                schema: "public");
        }
    }
}
