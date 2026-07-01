using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Libify.Infraestructure.Migrations
{
    /// <inheritdoc />
    public partial class AsaasIntegracao : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AsaasStatusRaw",
                table: "Cobranca",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AsaasPlatformCustomerId",
                table: "Usuario",
                type: "character varying(255)",
                maxLength: 255,
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "AsaasApiKey",
                table: "Usuario",
                type: "character varying(512)",
                maxLength: 512,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(255)",
                oldMaxLength: 255,
                oldNullable: true);

            migrationBuilder.CreateTable(
                name: "AsaasWebhookEvent",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    EventId = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    EventType = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                    Payload = table.Column<string>(type: "text", nullable: false),
                    ProcessadoEm = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Resultado = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    Version = table.Column<long>(type: "bigint", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AsaasWebhookEvent", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Cobranca_AsaasPaymentId",
                table: "Cobranca",
                column: "AsaasPaymentId");

            migrationBuilder.CreateIndex(
                name: "IX_Plano_AsaasSubscriptionId",
                table: "Plano",
                column: "AsaasSubscriptionId");

            migrationBuilder.CreateIndex(
                name: "IX_AsaasWebhookEvent_EventId",
                table: "AsaasWebhookEvent",
                column: "EventId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "AsaasWebhookEvent");

            migrationBuilder.DropIndex(name: "IX_Cobranca_AsaasPaymentId", table: "Cobranca");
            migrationBuilder.DropIndex(name: "IX_Plano_AsaasSubscriptionId", table: "Plano");

            migrationBuilder.DropColumn(name: "AsaasStatusRaw", table: "Cobranca");
            migrationBuilder.DropColumn(name: "AsaasPlatformCustomerId", table: "Usuario");

            migrationBuilder.AlterColumn<string>(
                name: "AsaasApiKey",
                table: "Usuario",
                type: "character varying(255)",
                maxLength: 255,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(512)",
                oldMaxLength: 512,
                oldNullable: true);
        }
    }
}
