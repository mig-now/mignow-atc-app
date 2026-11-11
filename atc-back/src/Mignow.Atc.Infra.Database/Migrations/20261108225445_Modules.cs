using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Mignow.Atc.Infra.Database.Migrations
{
    /// <inheritdoc />
    public partial class Modules : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_TB_SOURCE_CACHE_SAP_SYSTEM_ID_ADT_URI",
                schema: "public",
                table: "TB_SOURCE_CACHE");

            migrationBuilder.DropIndex(
                name: "IX_TB_EXEMPTION_REQUEST_BATCH_ID_ITEM_ID",
                schema: "public",
                table: "TB_EXEMPTION_REQUEST");

            migrationBuilder.AddColumn<string>(
                name: "EXCLUDED_ROWS_JSON",
                schema: "public",
                table: "TB_REMEDIATION_SESSION",
                type: "jsonb",
                nullable: false,
                defaultValue: "[]");

            migrationBuilder.AddColumn<int>(
                name: "BATCH",
                schema: "public",
                table: "TB_MIGNOW_OCCURRENCE",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "ELEMENT",
                schema: "public",
                table: "TB_MIGNOW_OCCURRENCE",
                type: "character varying(200)",
                maxLength: 200,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "COLUMNS_JSON",
                schema: "public",
                table: "TB_ATC_EXTRACTION",
                type: "jsonb",
                nullable: false,
                defaultValue: "[]");

            migrationBuilder.AddColumn<string>(
                name: "EXECUTION_JSON",
                schema: "public",
                table: "TB_ATC_EXTRACTION",
                type: "jsonb",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IS_COMPLETE",
                schema: "public",
                table: "TB_ATC_EXTRACTION",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateTable(
                name: "TB_REPORT_HIGHLIGHTS",
                schema: "public",
                columns: table => new
                {
                    ID = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CUSTOMER_ID = table.Column<int>(type: "integer", nullable: false),
                    HIGHLIGHTS = table.Column<string>(type: "jsonb", nullable: false),
                    PENDING = table.Column<string>(type: "jsonb", nullable: false),
                    PREMISE = table.Column<string>(type: "jsonb", nullable: true),
                    GUID = table.Column<Guid>(type: "uuid", nullable: false),
                    CREATED_AT = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CREATED_BY = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    UPDATED_AT = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    UPDATED_BY = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TB_REPORT_HIGHLIGHTS", x => x.ID);
                    table.ForeignKey(
                        name: "FK_TB_REPORT_HIGHLIGHTS_TB_CUSTOMER_CUSTOMER_ID",
                        column: x => x.CUSTOMER_ID,
                        principalSchema: "public",
                        principalTable: "TB_CUSTOMER",
                        principalColumn: "ID");
                });

            migrationBuilder.CreateIndex(
                name: "IX_TB_SOURCE_CACHE_SAP_SYSTEM_ID_OBJECT_NAME_ADT_URI",
                schema: "public",
                table: "TB_SOURCE_CACHE",
                columns: new[] { "SAP_SYSTEM_ID", "OBJECT_NAME", "ADT_URI" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TB_MIGNOW_OCCURRENCE_SCAN_ID_BATCH",
                schema: "public",
                table: "TB_MIGNOW_OCCURRENCE",
                columns: new[] { "SCAN_ID", "BATCH" });

            migrationBuilder.CreateIndex(
                name: "IX_TB_EXEMPTION_REQUEST_BATCH_ID_ITEM_ID",
                schema: "public",
                table: "TB_EXEMPTION_REQUEST",
                columns: new[] { "BATCH_ID", "ITEM_ID" });

            migrationBuilder.CreateIndex(
                name: "IX_TB_EXEMPTION_REQUEST_BATCH_ID_MARKER_ID",
                schema: "public",
                table: "TB_EXEMPTION_REQUEST",
                columns: new[] { "BATCH_ID", "MARKER_ID" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TB_EXEMPTION_REQUEST_MARKER_ID",
                schema: "public",
                table: "TB_EXEMPTION_REQUEST",
                column: "MARKER_ID");

            migrationBuilder.CreateIndex(
                name: "IX_TB_REPORT_HIGHLIGHTS_CUSTOMER_ID",
                schema: "public",
                table: "TB_REPORT_HIGHLIGHTS",
                column: "CUSTOMER_ID",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TB_REPORT_HIGHLIGHTS_GUID",
                schema: "public",
                table: "TB_REPORT_HIGHLIGHTS",
                column: "GUID",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TB_REPORT_HIGHLIGHTS",
                schema: "public");

            migrationBuilder.DropIndex(
                name: "IX_TB_SOURCE_CACHE_SAP_SYSTEM_ID_OBJECT_NAME_ADT_URI",
                schema: "public",
                table: "TB_SOURCE_CACHE");

            migrationBuilder.DropIndex(
                name: "IX_TB_MIGNOW_OCCURRENCE_SCAN_ID_BATCH",
                schema: "public",
                table: "TB_MIGNOW_OCCURRENCE");

            migrationBuilder.DropIndex(
                name: "IX_TB_EXEMPTION_REQUEST_BATCH_ID_ITEM_ID",
                schema: "public",
                table: "TB_EXEMPTION_REQUEST");

            migrationBuilder.DropIndex(
                name: "IX_TB_EXEMPTION_REQUEST_BATCH_ID_MARKER_ID",
                schema: "public",
                table: "TB_EXEMPTION_REQUEST");

            migrationBuilder.DropIndex(
                name: "IX_TB_EXEMPTION_REQUEST_MARKER_ID",
                schema: "public",
                table: "TB_EXEMPTION_REQUEST");

            migrationBuilder.DropColumn(
                name: "EXCLUDED_ROWS_JSON",
                schema: "public",
                table: "TB_REMEDIATION_SESSION");

            migrationBuilder.DropColumn(
                name: "BATCH",
                schema: "public",
                table: "TB_MIGNOW_OCCURRENCE");

            migrationBuilder.DropColumn(
                name: "ELEMENT",
                schema: "public",
                table: "TB_MIGNOW_OCCURRENCE");

            migrationBuilder.DropColumn(
                name: "COLUMNS_JSON",
                schema: "public",
                table: "TB_ATC_EXTRACTION");

            migrationBuilder.DropColumn(
                name: "EXECUTION_JSON",
                schema: "public",
                table: "TB_ATC_EXTRACTION");

            migrationBuilder.DropColumn(
                name: "IS_COMPLETE",
                schema: "public",
                table: "TB_ATC_EXTRACTION");

            migrationBuilder.CreateIndex(
                name: "IX_TB_SOURCE_CACHE_SAP_SYSTEM_ID_ADT_URI",
                schema: "public",
                table: "TB_SOURCE_CACHE",
                columns: new[] { "SAP_SYSTEM_ID", "ADT_URI" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TB_EXEMPTION_REQUEST_BATCH_ID_ITEM_ID",
                schema: "public",
                table: "TB_EXEMPTION_REQUEST",
                columns: new[] { "BATCH_ID", "ITEM_ID" },
                unique: true);
        }
    }
}
