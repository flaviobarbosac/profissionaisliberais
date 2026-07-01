using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Libify.Infraestructure.Migrations
{
  /// <inheritdoc />
  public partial class PropostaLinkPublico : Migration
  {
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
      migrationBuilder.AddColumn<DateTime>(
        name: "LinkExpiraEm",
        table: "Proposta",
        type: "timestamp with time zone",
        nullable: true);

      migrationBuilder.AddColumn<string>(
        name: "MotivoRecusa",
        table: "Proposta",
        type: "character varying(500)",
        maxLength: 500,
        nullable: true);

      migrationBuilder.AddColumn<string>(
        name: "RespondidoPor",
        table: "Proposta",
        type: "character varying(150)",
        maxLength: 150,
        nullable: true);

      migrationBuilder.AddColumn<string>(
        name: "TokenPublico",
        table: "Proposta",
        type: "character varying(64)",
        maxLength: 64,
        nullable: true);

      migrationBuilder.CreateIndex(
        name: "IX_Proposta_TokenPublico",
        table: "Proposta",
        column: "TokenPublico",
        unique: true,
        filter: "\"TokenPublico\" IS NOT NULL");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
      migrationBuilder.DropIndex(name: "IX_Proposta_TokenPublico", table: "Proposta");

      migrationBuilder.DropColumn(name: "LinkExpiraEm", table: "Proposta");
      migrationBuilder.DropColumn(name: "MotivoRecusa", table: "Proposta");
      migrationBuilder.DropColumn(name: "RespondidoPor", table: "Proposta");
      migrationBuilder.DropColumn(name: "TokenPublico", table: "Proposta");
    }
  }
}
