using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TriggerMail.Service.Migrations
{
    /// <inheritdoc />
    public partial class EmailLogs_RetryColumns : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterDatabase()
                .Annotation("Npgsql:PostgresExtension:uuid-ossp", ",,");

            migrationBuilder.CreateTable(
                name: "email_logs",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    message_id = table.Column<Guid>(type: "uuid", nullable: false),
                    template_key = table.Column<string>(type: "text", nullable: false),
                    lang = table.Column<string>(type: "text", nullable: false, defaultValue: "pt-BR"),
                    subject = table.Column<string>(type: "text", nullable: false),
                    recipients = table.Column<string[]>(type: "text[]", nullable: false),
                    created_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    status = table.Column<string>(type: "text", nullable: false, defaultValue: "Queued"),
                    provider_message_id = table.Column<string>(type: "text", nullable: true),
                    attempt_count = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    last_error = table.Column<string>(type: "text", nullable: true),
                    next_attempt_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    updated_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_email_logs", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "email_templates",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    key = table.Column<string>(type: "text", nullable: false),
                    subject = table.Column<string>(type: "text", nullable: false),
                    html = table.Column<string>(type: "text", nullable: false),
                    text = table.Column<string>(type: "text", nullable: true),
                    version = table.Column<int>(type: "integer", nullable: false, defaultValue: 1),
                    is_active = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_email_templates", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "email_triggers",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    alias = table.Column<string>(type: "text", nullable: false),
                    template_key = table.Column<string>(type: "text", nullable: false),
                    lang = table.Column<string>(type: "text", nullable: false, defaultValue: "pt-BR"),
                    auth_type = table.Column<string>(type: "text", nullable: false, defaultValue: "hmac"),
                    auth_secret = table.Column<string>(type: "text", nullable: true),
                    default_recipients = table.Column<string[]>(type: "text[]", nullable: true),
                    mapping_config_json = table.Column<string>(type: "text", nullable: true),
                    enabled = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_email_triggers", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_email_logs_message_id",
                table: "email_logs",
                column: "message_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_email_templates_key_version",
                table: "email_templates",
                columns: new[] { "key", "version" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_email_triggers_alias",
                table: "email_triggers",
                column: "alias",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "email_logs");

            migrationBuilder.DropTable(
                name: "email_templates");

            migrationBuilder.DropTable(
                name: "email_triggers");
        }
    }
}
