using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Sicofri.API.Migrations
{
    /// <inheritdoc />
    public partial class AgregaTipoLoteYCodigoLote : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "codigo_lote",
                table: "producto",
                type: "character varying(40)",
                maxLength: 40,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "tipo_lote",
                table: "producto",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_producto_fecha_caducidad",
                table: "producto",
                column: "fecha_caducidad");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_producto_fecha_caducidad",
                table: "producto");

            migrationBuilder.DropColumn(
                name: "codigo_lote",
                table: "producto");

            migrationBuilder.DropColumn(
                name: "tipo_lote",
                table: "producto");
        }
    }
}
