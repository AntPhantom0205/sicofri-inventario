using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Sicofri.API.Migrations
{
    /// <inheritdoc />
    public partial class ClienteContacto : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "nombre_contacto",
                table: "cliente",
                type: "character varying(60)",
                maxLength: 60,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "telefono",
                table: "cliente",
                type: "character varying(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_cliente_provincia",
                table: "cliente",
                column: "provincia");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_cliente_provincia",
                table: "cliente");

            migrationBuilder.DropColumn(
                name: "nombre_contacto",
                table: "cliente");

            migrationBuilder.DropColumn(
                name: "telefono",
                table: "cliente");
        }
    }
}
