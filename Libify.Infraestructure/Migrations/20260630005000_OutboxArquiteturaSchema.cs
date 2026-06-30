using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Libify.Infraestructure.Migrations
{
    /// <inheritdoc />
    public partial class OutboxArquiteturaSchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "Arquitetura");

            migrationBuilder.RenameTable(
                name: "InboxState",
                newName: "InboxState",
                newSchema: "Arquitetura");

            migrationBuilder.RenameTable(
                name: "OutboxMessage",
                newName: "OutboxMessage",
                newSchema: "Arquitetura");

            migrationBuilder.RenameTable(
                name: "OutboxState",
                newName: "OutboxState",
                newSchema: "Arquitetura");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameTable(
                name: "InboxState",
                schema: "Arquitetura",
                newName: "InboxState");

            migrationBuilder.RenameTable(
                name: "OutboxMessage",
                schema: "Arquitetura",
                newName: "OutboxMessage");

            migrationBuilder.RenameTable(
                name: "OutboxState",
                schema: "Arquitetura",
                newName: "OutboxState");
        }
    }
}
