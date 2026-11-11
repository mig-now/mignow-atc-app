using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Mignow.Atc.Infra.Database.Migrations
{
    /// <inheritdoc />
    public partial class Initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "public");

            migrationBuilder.CreateTable(
                name: "TB_AI_PROVIDER",
                schema: "public",
                columns: table => new
                {
                    ID = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    NAME = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    KIND = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    USAGE = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    MODEL = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    API_KEY_SECRET_REF = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    IS_DEFAULT = table.Column<bool>(type: "boolean", nullable: false),
                    ACTIVE = table.Column<bool>(type: "boolean", nullable: false),
                    GUID = table.Column<Guid>(type: "uuid", nullable: false),
                    CREATED_AT = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CREATED_BY = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    UPDATED_AT = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    UPDATED_BY = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TB_AI_PROVIDER", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "TB_CUSTOMER",
                schema: "public",
                columns: table => new
                {
                    ID = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    NAME = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    CODE = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: true),
                    ACTIVE = table.Column<bool>(type: "boolean", nullable: false),
                    GUID = table.Column<Guid>(type: "uuid", nullable: false),
                    CREATED_AT = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CREATED_BY = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    UPDATED_AT = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    UPDATED_BY = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TB_CUSTOMER", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "TB_GENERAL_SETTINGS",
                schema: "public",
                columns: table => new
                {
                    ID = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    PARALLELISM = table.Column<int>(type: "integer", nullable: false),
                    SOURCE_READ_PARALLELISM = table.Column<int>(type: "integer", nullable: false),
                    BATCH_SIZE = table.Column<int>(type: "integer", nullable: false),
                    KB_PATH = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    MIGNOW_RULES_PATH = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    MIGNOW_PARALLEL = table.Column<int>(type: "integer", nullable: false),
                    MIGNOW_WEIGHT_PER_BATCH = table.Column<int>(type: "integer", nullable: false),
                    MANUAL_OBJECTS_PER_DAY = table.Column<int>(type: "integer", nullable: false),
                    HOURS_PER_DAY = table.Column<int>(type: "integer", nullable: false),
                    GUID = table.Column<Guid>(type: "uuid", nullable: false),
                    CREATED_AT = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CREATED_BY = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    UPDATED_AT = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    UPDATED_BY = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TB_GENERAL_SETTINGS", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "TB_SECRET",
                schema: "public",
                columns: table => new
                {
                    ID = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    REF = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    PURPOSE = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    NONCE = table.Column<byte[]>(type: "bytea", nullable: false),
                    CIPHER = table.Column<byte[]>(type: "bytea", nullable: false),
                    TAG = table.Column<byte[]>(type: "bytea", nullable: false),
                    UPDATED_AT = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TB_SECRET", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "TB_SAP_SYSTEM",
                schema: "public",
                columns: table => new
                {
                    ID = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CUSTOMER_ID = table.Column<int>(type: "integer", nullable: false),
                    SID = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    CLIENT = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    HOST = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    ADT_PORT = table.Column<int>(type: "integer", nullable: false),
                    HTTPS = table.Column<bool>(type: "boolean", nullable: false),
                    LANGUAGE = table.Column<string>(type: "character varying(2)", maxLength: 2, nullable: false),
                    CERT_THUMBPRINT = table.Column<string>(type: "character varying(95)", maxLength: 95, nullable: true),
                    IS_PRODUCTION = table.Column<bool>(type: "boolean", nullable: false),
                    PROJECT_TYPE = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    AUTHOR = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    DEFAULT_REQUEST = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    EXEMPTION_APPROVER = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: true),
                    USERNAME = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    PASSWORD_SECRET_REF = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    ACTIVE = table.Column<bool>(type: "boolean", nullable: false),
                    RELEASE = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: true),
                    CLIENT_CATEGORY = table.Column<string>(type: "character varying(1)", maxLength: 1, nullable: true),
                    LAST_CONNECTION_TEST_AT = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    LAST_CONNECTION_TEST_OK = table.Column<bool>(type: "boolean", nullable: true),
                    LAST_CONNECTION_TEST_MESSAGE = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    GUID = table.Column<Guid>(type: "uuid", nullable: false),
                    CREATED_AT = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CREATED_BY = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    UPDATED_AT = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    UPDATED_BY = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TB_SAP_SYSTEM", x => x.ID);
                    table.ForeignKey(
                        name: "FK_TB_SAP_SYSTEM_TB_CUSTOMER_CUSTOMER_ID",
                        column: x => x.CUSTOMER_ID,
                        principalSchema: "public",
                        principalTable: "TB_CUSTOMER",
                        principalColumn: "ID");
                });

            migrationBuilder.CreateTable(
                name: "TB_ATC_EXTRACTION",
                schema: "public",
                columns: table => new
                {
                    ID = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    SAP_SYSTEM_ID = table.Column<int>(type: "integer", nullable: false),
                    SERIES = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    DISPLAY_ID = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    TITLE = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
                    EXECUTED_AT_UTC = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    EXTRACTED_AT = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    TOTAL_COUNT = table.Column<int>(type: "integer", nullable: false),
                    IN_BASELINE_COUNT = table.Column<int>(type: "integer", nullable: false),
                    P1_TOTAL = table.Column<int>(type: "integer", nullable: false),
                    P2_TOTAL = table.Column<int>(type: "integer", nullable: false),
                    P3_TOTAL = table.Column<int>(type: "integer", nullable: false),
                    P1_NEW = table.Column<int>(type: "integer", nullable: false),
                    P2_NEW = table.Column<int>(type: "integer", nullable: false),
                    P3_NEW = table.Column<int>(type: "integer", nullable: false),
                    VARIATION_JSON = table.Column<string>(type: "jsonb", nullable: true),
                    SITUATION = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    GUID = table.Column<Guid>(type: "uuid", nullable: false),
                    CREATED_AT = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CREATED_BY = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    UPDATED_AT = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    UPDATED_BY = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TB_ATC_EXTRACTION", x => x.ID);
                    table.ForeignKey(
                        name: "FK_TB_ATC_EXTRACTION_TB_SAP_SYSTEM_SAP_SYSTEM_ID",
                        column: x => x.SAP_SYSTEM_ID,
                        principalSchema: "public",
                        principalTable: "TB_SAP_SYSTEM",
                        principalColumn: "ID");
                });

            migrationBuilder.CreateTable(
                name: "TB_REMEDIATION_SESSION",
                schema: "public",
                columns: table => new
                {
                    ID = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CUSTOMER_ID = table.Column<int>(type: "integer", nullable: false),
                    SAP_SYSTEM_ID = table.Column<int>(type: "integer", nullable: false),
                    SOURCE = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    SOURCE_REF = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
                    PRIORITIES = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    REQUEST = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    AUTHOR = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    PROJECT_TYPE = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    CSV_COLUMNS_JSON = table.Column<string>(type: "jsonb", nullable: false),
                    CSV_ENCODING = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    CSV_DELIMITER = table.Column<string>(type: "character varying(5)", maxLength: 5, nullable: false),
                    CYCLE = table.Column<int>(type: "integer", nullable: false),
                    ARCHIVED = table.Column<bool>(type: "boolean", nullable: false),
                    ARCHIVED_AT = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    AUTO_MODE = table.Column<bool>(type: "boolean", nullable: false),
                    GUID = table.Column<Guid>(type: "uuid", nullable: false),
                    CREATED_AT = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CREATED_BY = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    UPDATED_AT = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    UPDATED_BY = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TB_REMEDIATION_SESSION", x => x.ID);
                    table.ForeignKey(
                        name: "FK_TB_REMEDIATION_SESSION_TB_CUSTOMER_CUSTOMER_ID",
                        column: x => x.CUSTOMER_ID,
                        principalSchema: "public",
                        principalTable: "TB_CUSTOMER",
                        principalColumn: "ID");
                    table.ForeignKey(
                        name: "FK_TB_REMEDIATION_SESSION_TB_SAP_SYSTEM_SAP_SYSTEM_ID",
                        column: x => x.SAP_SYSTEM_ID,
                        principalSchema: "public",
                        principalTable: "TB_SAP_SYSTEM",
                        principalColumn: "ID");
                });

            migrationBuilder.CreateTable(
                name: "TB_RUN",
                schema: "public",
                columns: table => new
                {
                    ID = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    KIND = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    CUSTOMER_ID = table.Column<int>(type: "integer", nullable: false),
                    SAP_SYSTEM_ID = table.Column<int>(type: "integer", nullable: false),
                    SUBJECT_ID = table.Column<Guid>(type: "uuid", nullable: true),
                    PARAMETERS_JSON = table.Column<string>(type: "jsonb", nullable: false),
                    RESULT_JSON = table.Column<string>(type: "jsonb", nullable: true),
                    STATUS = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    STARTED_BY = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    CANCELLED_BY = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    QUEUED_AT = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    STARTED_AT = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    FINISHED_AT = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    HEARTBEAT_AT = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    FAILURE_CODE = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    FAILURE_ARGS = table.Column<string>(type: "text", nullable: true),
                    PARALLELISM = table.Column<int>(type: "integer", nullable: false),
                    AI_INPUT_TOKENS = table.Column<int>(type: "integer", nullable: false),
                    AI_OUTPUT_TOKENS = table.Column<int>(type: "integer", nullable: false),
                    AI_COST = table.Column<decimal>(type: "numeric(18,10)", precision: 18, scale: 10, nullable: false),
                    AI_CALLS_WITHOUT_COST = table.Column<int>(type: "integer", nullable: false),
                    AI_CALLS = table.Column<int>(type: "integer", nullable: false),
                    GUID = table.Column<Guid>(type: "uuid", nullable: false),
                    CREATED_AT = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CREATED_BY = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    UPDATED_AT = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    UPDATED_BY = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TB_RUN", x => x.ID);
                    table.ForeignKey(
                        name: "FK_TB_RUN_TB_CUSTOMER_CUSTOMER_ID",
                        column: x => x.CUSTOMER_ID,
                        principalSchema: "public",
                        principalTable: "TB_CUSTOMER",
                        principalColumn: "ID");
                    table.ForeignKey(
                        name: "FK_TB_RUN_TB_SAP_SYSTEM_SAP_SYSTEM_ID",
                        column: x => x.SAP_SYSTEM_ID,
                        principalSchema: "public",
                        principalTable: "TB_SAP_SYSTEM",
                        principalColumn: "ID");
                });

            migrationBuilder.CreateTable(
                name: "TB_SOURCE_CACHE",
                schema: "public",
                columns: table => new
                {
                    ID = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    SAP_SYSTEM_ID = table.Column<int>(type: "integer", nullable: false),
                    OBJECT_NAME = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    INCLUDE_NAME = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    ADT_URI = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    SOURCE = table.Column<string>(type: "text", nullable: false),
                    HASH = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    READ_AT = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    GUID = table.Column<Guid>(type: "uuid", nullable: false),
                    CREATED_AT = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CREATED_BY = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    UPDATED_AT = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    UPDATED_BY = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TB_SOURCE_CACHE", x => x.ID);
                    table.ForeignKey(
                        name: "FK_TB_SOURCE_CACHE_TB_SAP_SYSTEM_SAP_SYSTEM_ID",
                        column: x => x.SAP_SYSTEM_ID,
                        principalSchema: "public",
                        principalTable: "TB_SAP_SYSTEM",
                        principalColumn: "ID");
                });

            migrationBuilder.CreateTable(
                name: "TB_ATC_FINDING",
                schema: "public",
                columns: table => new
                {
                    ID = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    EXTRACTION_ID = table.Column<int>(type: "integer", nullable: false),
                    SEQ = table.Column<int>(type: "integer", nullable: false),
                    PRIORITY = table.Column<int>(type: "integer", nullable: true),
                    CHECK_TITLE = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
                    MESSAGE_TITLE = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    OBJECT_NAME = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    OBJECT_TYPE = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
                    PACKAGE = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: true),
                    REF_OBJECT_NAME = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    REF_OBJECT_TYPE = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
                    NOTE = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: true),
                    FINDING_ID = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
                    IN_BASELINE = table.Column<bool>(type: "boolean", nullable: false),
                    ALL_COLUMNS_JSON = table.Column<string>(type: "jsonb", nullable: false),
                    GUID = table.Column<Guid>(type: "uuid", nullable: false),
                    CREATED_AT = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CREATED_BY = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    UPDATED_AT = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    UPDATED_BY = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TB_ATC_FINDING", x => x.ID);
                    table.ForeignKey(
                        name: "FK_TB_ATC_FINDING_TB_ATC_EXTRACTION_EXTRACTION_ID",
                        column: x => x.EXTRACTION_ID,
                        principalSchema: "public",
                        principalTable: "TB_ATC_EXTRACTION",
                        principalColumn: "ID");
                });

            migrationBuilder.CreateTable(
                name: "TB_MIGNOW_SCAN",
                schema: "public",
                columns: table => new
                {
                    ID = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    SAP_SYSTEM_ID = table.Column<int>(type: "integer", nullable: false),
                    EXTRACTION_ID = table.Column<int>(type: "integer", nullable: true),
                    RULES_SNAPSHOT_JSON = table.Column<string>(type: "jsonb", nullable: false),
                    SCOPE_COUNTS_JSON = table.Column<string>(type: "jsonb", nullable: true),
                    BATCHES = table.Column<string>(type: "jsonb", nullable: false),
                    SUMMARY_JSON = table.Column<string>(type: "jsonb", nullable: true),
                    GUID = table.Column<Guid>(type: "uuid", nullable: false),
                    CREATED_AT = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CREATED_BY = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    UPDATED_AT = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    UPDATED_BY = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TB_MIGNOW_SCAN", x => x.ID);
                    table.ForeignKey(
                        name: "FK_TB_MIGNOW_SCAN_TB_ATC_EXTRACTION_EXTRACTION_ID",
                        column: x => x.EXTRACTION_ID,
                        principalSchema: "public",
                        principalTable: "TB_ATC_EXTRACTION",
                        principalColumn: "ID");
                    table.ForeignKey(
                        name: "FK_TB_MIGNOW_SCAN_TB_SAP_SYSTEM_SAP_SYSTEM_ID",
                        column: x => x.SAP_SYSTEM_ID,
                        principalSchema: "public",
                        principalTable: "TB_SAP_SYSTEM",
                        principalColumn: "ID");
                });

            migrationBuilder.CreateTable(
                name: "TB_EXEMPTION_BATCH",
                schema: "public",
                columns: table => new
                {
                    ID = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    SESSION_ID = table.Column<int>(type: "integer", nullable: false),
                    SAP_SYSTEM_ID = table.Column<int>(type: "integer", nullable: false),
                    PRIORITIES = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    APPROVER = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: true),
                    REASON = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    VALIDITY = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    JUSTIFICATION_PREFIX = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    DISCOVERY_JSON = table.Column<string>(type: "jsonb", nullable: true),
                    STATUS = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    GUID = table.Column<Guid>(type: "uuid", nullable: false),
                    CREATED_AT = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CREATED_BY = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    UPDATED_AT = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    UPDATED_BY = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TB_EXEMPTION_BATCH", x => x.ID);
                    table.ForeignKey(
                        name: "FK_TB_EXEMPTION_BATCH_TB_REMEDIATION_SESSION_SESSION_ID",
                        column: x => x.SESSION_ID,
                        principalSchema: "public",
                        principalTable: "TB_REMEDIATION_SESSION",
                        principalColumn: "ID");
                    table.ForeignKey(
                        name: "FK_TB_EXEMPTION_BATCH_TB_SAP_SYSTEM_SAP_SYSTEM_ID",
                        column: x => x.SAP_SYSTEM_ID,
                        principalSchema: "public",
                        principalTable: "TB_SAP_SYSTEM",
                        principalColumn: "ID");
                });

            migrationBuilder.CreateTable(
                name: "TB_REMEDIATION_ITEM",
                schema: "public",
                columns: table => new
                {
                    ID = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    SESSION_ID = table.Column<int>(type: "integer", nullable: false),
                    N = table.Column<int>(type: "integer", nullable: false),
                    OBJECT_NAME = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    OBJECT_TYPE = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    REF_OBJECT = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    REF_TYPE = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
                    CHECK = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
                    MESSAGE = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    NOTE = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: true),
                    TEXT = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    PACKAGE = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: true),
                    CLASSIFICATION = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    PRIORITY = table.Column<string>(type: "character varying(5)", maxLength: 5, nullable: true),
                    FINDING_ID = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
                    ORIGIN = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    RAW_ROW_JSON = table.Column<string>(type: "jsonb", nullable: false),
                    STATUS = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    OBS = table.Column<string>(type: "text", nullable: false),
                    STATUS_DATE = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    STATUS_HISTORY = table.Column<string>(type: "jsonb", nullable: false),
                    TRIAGE_FLAGS = table.Column<string>(type: "jsonb", nullable: false),
                    PREPARATION = table.Column<string>(type: "jsonb", nullable: true),
                    PROPOSAL = table.Column<string>(type: "jsonb", nullable: true),
                    APPLICATION = table.Column<string>(type: "jsonb", nullable: true),
                    CORRECTION_SHEET_MD = table.Column<string>(type: "text", nullable: true),
                    GUID = table.Column<Guid>(type: "uuid", nullable: false),
                    CREATED_AT = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CREATED_BY = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    UPDATED_AT = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    UPDATED_BY = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TB_REMEDIATION_ITEM", x => x.ID);
                    table.ForeignKey(
                        name: "FK_TB_REMEDIATION_ITEM_TB_REMEDIATION_SESSION_SESSION_ID",
                        column: x => x.SESSION_ID,
                        principalSchema: "public",
                        principalTable: "TB_REMEDIATION_SESSION",
                        principalColumn: "ID");
                });

            migrationBuilder.CreateTable(
                name: "TB_RUN_AI_CALL",
                schema: "public",
                columns: table => new
                {
                    ID = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    RUN_ID = table.Column<int>(type: "integer", nullable: false),
                    USAGE = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    MODEL = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    INPUT_TOKENS = table.Column<int>(type: "integer", nullable: false),
                    OUTPUT_TOKENS = table.Column<int>(type: "integer", nullable: false),
                    COST = table.Column<decimal>(type: "numeric(18,10)", precision: 18, scale: 10, nullable: true),
                    OK = table.Column<bool>(type: "boolean", nullable: false),
                    ERROR_CODE = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    ITEM_REF = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    AT = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    GUID = table.Column<Guid>(type: "uuid", nullable: false),
                    CREATED_AT = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CREATED_BY = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    UPDATED_AT = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    UPDATED_BY = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TB_RUN_AI_CALL", x => x.ID);
                    table.ForeignKey(
                        name: "FK_TB_RUN_AI_CALL_TB_RUN_RUN_ID",
                        column: x => x.RUN_ID,
                        principalSchema: "public",
                        principalTable: "TB_RUN",
                        principalColumn: "ID");
                });

            migrationBuilder.CreateTable(
                name: "TB_RUN_EVENT",
                schema: "public",
                columns: table => new
                {
                    ID = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    RUN_ID = table.Column<int>(type: "integer", nullable: false),
                    AT = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    LEVEL = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    STEP_CODE = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: true),
                    CODE = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    ARGS_JSON = table.Column<string>(type: "jsonb", nullable: true),
                    ITEM_REF = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    GUID = table.Column<Guid>(type: "uuid", nullable: false),
                    CREATED_AT = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CREATED_BY = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    UPDATED_AT = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    UPDATED_BY = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TB_RUN_EVENT", x => x.ID);
                    table.ForeignKey(
                        name: "FK_TB_RUN_EVENT_TB_RUN_RUN_ID",
                        column: x => x.RUN_ID,
                        principalSchema: "public",
                        principalTable: "TB_RUN",
                        principalColumn: "ID");
                });

            migrationBuilder.CreateTable(
                name: "TB_RUN_STEP",
                schema: "public",
                columns: table => new
                {
                    ID = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    RUN_ID = table.Column<int>(type: "integer", nullable: false),
                    CODE = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    ORDER = table.Column<int>(type: "integer", nullable: false),
                    STATUS = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    STARTED_AT = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    FINISHED_AT = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    MESSAGE_CODE = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    MESSAGE_ARGS = table.Column<string>(type: "text", nullable: true),
                    PROGRESS_DONE = table.Column<int>(type: "integer", nullable: false),
                    PROGRESS_TOTAL = table.Column<int>(type: "integer", nullable: false),
                    GUID = table.Column<Guid>(type: "uuid", nullable: false),
                    CREATED_AT = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CREATED_BY = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    UPDATED_AT = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    UPDATED_BY = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TB_RUN_STEP", x => x.ID);
                    table.ForeignKey(
                        name: "FK_TB_RUN_STEP_TB_RUN_RUN_ID",
                        column: x => x.RUN_ID,
                        principalSchema: "public",
                        principalTable: "TB_RUN",
                        principalColumn: "ID");
                });

            migrationBuilder.CreateTable(
                name: "TB_MIGNOW_OCCURRENCE",
                schema: "public",
                columns: table => new
                {
                    ID = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    SCAN_ID = table.Column<int>(type: "integer", nullable: false),
                    RULE_ID = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: false),
                    OBJECT_NAME = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    OBJECT_TYPE = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    INCLUDE = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    ADT_SOURCE = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    LINE = table.Column<int>(type: "integer", nullable: false),
                    LINE_END = table.Column<int>(type: "integer", nullable: true),
                    SNIPPET = table.Column<string>(type: "text", nullable: true),
                    CROSS = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: true),
                    CLASS = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    GUID = table.Column<Guid>(type: "uuid", nullable: false),
                    CREATED_AT = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CREATED_BY = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    UPDATED_AT = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    UPDATED_BY = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TB_MIGNOW_OCCURRENCE", x => x.ID);
                    table.ForeignKey(
                        name: "FK_TB_MIGNOW_OCCURRENCE_TB_MIGNOW_SCAN_SCAN_ID",
                        column: x => x.SCAN_ID,
                        principalSchema: "public",
                        principalTable: "TB_MIGNOW_SCAN",
                        principalColumn: "ID");
                });

            migrationBuilder.CreateTable(
                name: "TB_EXEMPTION_REQUEST",
                schema: "public",
                columns: table => new
                {
                    ID = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    BATCH_ID = table.Column<int>(type: "integer", nullable: false),
                    ITEM_ID = table.Column<int>(type: "integer", nullable: false),
                    MARKER_ID = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    SEQ = table.Column<int>(type: "integer", nullable: false),
                    REASON = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    JUSTIFICATION = table.Column<string>(type: "text", nullable: false),
                    STATUS = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    PROBABLY_NO_SCOPE = table.Column<bool>(type: "boolean", nullable: false),
                    REQUEST_XML = table.Column<string>(type: "text", nullable: true),
                    RESPONSE_XML = table.Column<string>(type: "text", nullable: true),
                    MESSAGE = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    GUID = table.Column<Guid>(type: "uuid", nullable: false),
                    CREATED_AT = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CREATED_BY = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    UPDATED_AT = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    UPDATED_BY = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TB_EXEMPTION_REQUEST", x => x.ID);
                    table.ForeignKey(
                        name: "FK_TB_EXEMPTION_REQUEST_TB_EXEMPTION_BATCH_BATCH_ID",
                        column: x => x.BATCH_ID,
                        principalSchema: "public",
                        principalTable: "TB_EXEMPTION_BATCH",
                        principalColumn: "ID");
                    table.ForeignKey(
                        name: "FK_TB_EXEMPTION_REQUEST_TB_REMEDIATION_ITEM_ITEM_ID",
                        column: x => x.ITEM_ID,
                        principalSchema: "public",
                        principalTable: "TB_REMEDIATION_ITEM",
                        principalColumn: "ID");
                });

            migrationBuilder.CreateIndex(
                name: "IX_TB_AI_PROVIDER_GUID",
                schema: "public",
                table: "TB_AI_PROVIDER",
                column: "GUID",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TB_ATC_EXTRACTION_GUID",
                schema: "public",
                table: "TB_ATC_EXTRACTION",
                column: "GUID",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TB_ATC_EXTRACTION_SAP_SYSTEM_ID_DISPLAY_ID",
                schema: "public",
                table: "TB_ATC_EXTRACTION",
                columns: new[] { "SAP_SYSTEM_ID", "DISPLAY_ID" });

            migrationBuilder.CreateIndex(
                name: "IX_TB_ATC_FINDING_EXTRACTION_ID_OBJECT_NAME",
                schema: "public",
                table: "TB_ATC_FINDING",
                columns: new[] { "EXTRACTION_ID", "OBJECT_NAME" });

            migrationBuilder.CreateIndex(
                name: "IX_TB_ATC_FINDING_EXTRACTION_ID_PRIORITY_IN_BASELINE",
                schema: "public",
                table: "TB_ATC_FINDING",
                columns: new[] { "EXTRACTION_ID", "PRIORITY", "IN_BASELINE" });

            migrationBuilder.CreateIndex(
                name: "IX_TB_ATC_FINDING_EXTRACTION_ID_SEQ",
                schema: "public",
                table: "TB_ATC_FINDING",
                columns: new[] { "EXTRACTION_ID", "SEQ" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TB_ATC_FINDING_GUID",
                schema: "public",
                table: "TB_ATC_FINDING",
                column: "GUID",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TB_CUSTOMER_GUID",
                schema: "public",
                table: "TB_CUSTOMER",
                column: "GUID",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TB_EXEMPTION_BATCH_GUID",
                schema: "public",
                table: "TB_EXEMPTION_BATCH",
                column: "GUID",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TB_EXEMPTION_BATCH_SAP_SYSTEM_ID",
                schema: "public",
                table: "TB_EXEMPTION_BATCH",
                column: "SAP_SYSTEM_ID");

            migrationBuilder.CreateIndex(
                name: "IX_TB_EXEMPTION_BATCH_SESSION_ID",
                schema: "public",
                table: "TB_EXEMPTION_BATCH",
                column: "SESSION_ID");

            migrationBuilder.CreateIndex(
                name: "IX_TB_EXEMPTION_REQUEST_BATCH_ID_ITEM_ID",
                schema: "public",
                table: "TB_EXEMPTION_REQUEST",
                columns: new[] { "BATCH_ID", "ITEM_ID" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TB_EXEMPTION_REQUEST_BATCH_ID_SEQ",
                schema: "public",
                table: "TB_EXEMPTION_REQUEST",
                columns: new[] { "BATCH_ID", "SEQ" });

            migrationBuilder.CreateIndex(
                name: "IX_TB_EXEMPTION_REQUEST_GUID",
                schema: "public",
                table: "TB_EXEMPTION_REQUEST",
                column: "GUID",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TB_EXEMPTION_REQUEST_ITEM_ID",
                schema: "public",
                table: "TB_EXEMPTION_REQUEST",
                column: "ITEM_ID");

            migrationBuilder.CreateIndex(
                name: "IX_TB_GENERAL_SETTINGS_GUID",
                schema: "public",
                table: "TB_GENERAL_SETTINGS",
                column: "GUID",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TB_MIGNOW_OCCURRENCE_GUID",
                schema: "public",
                table: "TB_MIGNOW_OCCURRENCE",
                column: "GUID",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TB_MIGNOW_OCCURRENCE_SCAN_ID_OBJECT_NAME",
                schema: "public",
                table: "TB_MIGNOW_OCCURRENCE",
                columns: new[] { "SCAN_ID", "OBJECT_NAME" });

            migrationBuilder.CreateIndex(
                name: "IX_TB_MIGNOW_OCCURRENCE_SCAN_ID_RULE_ID",
                schema: "public",
                table: "TB_MIGNOW_OCCURRENCE",
                columns: new[] { "SCAN_ID", "RULE_ID" });

            migrationBuilder.CreateIndex(
                name: "IX_TB_MIGNOW_SCAN_EXTRACTION_ID",
                schema: "public",
                table: "TB_MIGNOW_SCAN",
                column: "EXTRACTION_ID");

            migrationBuilder.CreateIndex(
                name: "IX_TB_MIGNOW_SCAN_GUID",
                schema: "public",
                table: "TB_MIGNOW_SCAN",
                column: "GUID",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TB_MIGNOW_SCAN_SAP_SYSTEM_ID",
                schema: "public",
                table: "TB_MIGNOW_SCAN",
                column: "SAP_SYSTEM_ID");

            migrationBuilder.CreateIndex(
                name: "IX_TB_REMEDIATION_ITEM_GUID",
                schema: "public",
                table: "TB_REMEDIATION_ITEM",
                column: "GUID",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TB_REMEDIATION_ITEM_SESSION_ID_N",
                schema: "public",
                table: "TB_REMEDIATION_ITEM",
                columns: new[] { "SESSION_ID", "N" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TB_REMEDIATION_ITEM_SESSION_ID_OBJECT_NAME",
                schema: "public",
                table: "TB_REMEDIATION_ITEM",
                columns: new[] { "SESSION_ID", "OBJECT_NAME" });

            migrationBuilder.CreateIndex(
                name: "IX_TB_REMEDIATION_ITEM_SESSION_ID_STATUS",
                schema: "public",
                table: "TB_REMEDIATION_ITEM",
                columns: new[] { "SESSION_ID", "STATUS" });

            migrationBuilder.CreateIndex(
                name: "IX_TB_REMEDIATION_SESSION_CUSTOMER_ID",
                schema: "public",
                table: "TB_REMEDIATION_SESSION",
                column: "CUSTOMER_ID");

            migrationBuilder.CreateIndex(
                name: "IX_TB_REMEDIATION_SESSION_GUID",
                schema: "public",
                table: "TB_REMEDIATION_SESSION",
                column: "GUID",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TB_REMEDIATION_SESSION_SAP_SYSTEM_ID_ARCHIVED",
                schema: "public",
                table: "TB_REMEDIATION_SESSION",
                columns: new[] { "SAP_SYSTEM_ID", "ARCHIVED" });

            migrationBuilder.CreateIndex(
                name: "IX_TB_RUN_CUSTOMER_ID",
                schema: "public",
                table: "TB_RUN",
                column: "CUSTOMER_ID");

            migrationBuilder.CreateIndex(
                name: "IX_TB_RUN_GUID",
                schema: "public",
                table: "TB_RUN",
                column: "GUID",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TB_RUN_STATUS_QUEUED_AT",
                schema: "public",
                table: "TB_RUN",
                columns: new[] { "STATUS", "QUEUED_AT" });

            migrationBuilder.CreateIndex(
                name: "IX_TB_RUN_SUBJECT_ID",
                schema: "public",
                table: "TB_RUN",
                column: "SUBJECT_ID");

            migrationBuilder.CreateIndex(
                name: "UX_RUN_ACTIVE_PER_SAP_SYSTEM",
                schema: "public",
                table: "TB_RUN",
                column: "SAP_SYSTEM_ID",
                unique: true,
                filter: "\"STATUS\" IN ('Queued', 'Running', 'Cancelling')");

            migrationBuilder.CreateIndex(
                name: "IX_TB_RUN_AI_CALL_GUID",
                schema: "public",
                table: "TB_RUN_AI_CALL",
                column: "GUID",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TB_RUN_AI_CALL_RUN_ID",
                schema: "public",
                table: "TB_RUN_AI_CALL",
                column: "RUN_ID");

            migrationBuilder.CreateIndex(
                name: "IX_TB_RUN_EVENT_GUID",
                schema: "public",
                table: "TB_RUN_EVENT",
                column: "GUID",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TB_RUN_EVENT_RUN_ID_ID",
                schema: "public",
                table: "TB_RUN_EVENT",
                columns: new[] { "RUN_ID", "ID" });

            migrationBuilder.CreateIndex(
                name: "IX_TB_RUN_STEP_GUID",
                schema: "public",
                table: "TB_RUN_STEP",
                column: "GUID",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TB_RUN_STEP_RUN_ID_CODE",
                schema: "public",
                table: "TB_RUN_STEP",
                columns: new[] { "RUN_ID", "CODE" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TB_SAP_SYSTEM_CUSTOMER_ID_SID_CLIENT",
                schema: "public",
                table: "TB_SAP_SYSTEM",
                columns: new[] { "CUSTOMER_ID", "SID", "CLIENT" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TB_SAP_SYSTEM_GUID",
                schema: "public",
                table: "TB_SAP_SYSTEM",
                column: "GUID",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TB_SECRET_REF",
                schema: "public",
                table: "TB_SECRET",
                column: "REF",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TB_SOURCE_CACHE_GUID",
                schema: "public",
                table: "TB_SOURCE_CACHE",
                column: "GUID",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TB_SOURCE_CACHE_SAP_SYSTEM_ID_ADT_URI",
                schema: "public",
                table: "TB_SOURCE_CACHE",
                columns: new[] { "SAP_SYSTEM_ID", "ADT_URI" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TB_SOURCE_CACHE_SAP_SYSTEM_ID_OBJECT_NAME",
                schema: "public",
                table: "TB_SOURCE_CACHE",
                columns: new[] { "SAP_SYSTEM_ID", "OBJECT_NAME" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TB_AI_PROVIDER",
                schema: "public");

            migrationBuilder.DropTable(
                name: "TB_ATC_FINDING",
                schema: "public");

            migrationBuilder.DropTable(
                name: "TB_EXEMPTION_REQUEST",
                schema: "public");

            migrationBuilder.DropTable(
                name: "TB_GENERAL_SETTINGS",
                schema: "public");

            migrationBuilder.DropTable(
                name: "TB_MIGNOW_OCCURRENCE",
                schema: "public");

            migrationBuilder.DropTable(
                name: "TB_RUN_AI_CALL",
                schema: "public");

            migrationBuilder.DropTable(
                name: "TB_RUN_EVENT",
                schema: "public");

            migrationBuilder.DropTable(
                name: "TB_RUN_STEP",
                schema: "public");

            migrationBuilder.DropTable(
                name: "TB_SECRET",
                schema: "public");

            migrationBuilder.DropTable(
                name: "TB_SOURCE_CACHE",
                schema: "public");

            migrationBuilder.DropTable(
                name: "TB_EXEMPTION_BATCH",
                schema: "public");

            migrationBuilder.DropTable(
                name: "TB_REMEDIATION_ITEM",
                schema: "public");

            migrationBuilder.DropTable(
                name: "TB_MIGNOW_SCAN",
                schema: "public");

            migrationBuilder.DropTable(
                name: "TB_RUN",
                schema: "public");

            migrationBuilder.DropTable(
                name: "TB_REMEDIATION_SESSION",
                schema: "public");

            migrationBuilder.DropTable(
                name: "TB_ATC_EXTRACTION",
                schema: "public");

            migrationBuilder.DropTable(
                name: "TB_SAP_SYSTEM",
                schema: "public");

            migrationBuilder.DropTable(
                name: "TB_CUSTOMER",
                schema: "public");
        }
    }
}
