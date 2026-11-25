using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Sicofri.API.Migrations
{
    /// <inheritdoc />
    public partial class NotificacionAtencion : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_notificacion_id_producto",
                table: "notificacion");

            migrationBuilder.AddColumn<DateTime>(
                name: "fecha_hora_atendida",
                table: "notificacion",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "id_usuario_atiende",
                table: "notificacion",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_notificacion_atendida",
                table: "notificacion",
                column: "atendida");

            migrationBuilder.CreateIndex(
                name: "IX_notificacion_id_producto",
                table: "notificacion",
                column: "id_producto",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_notificacion_id_usuario_atiende",
                table: "notificacion",
                column: "id_usuario_atiende");

            migrationBuilder.AddForeignKey(
                name: "FK_notificacion_usuario_id_usuario_atiende",
                table: "notificacion",
                column: "id_usuario_atiende",
                principalTable: "usuario",
                principalColumn: "id_usuario",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_notificacion_usuario_id_usuario_atiende",
                table: "notificacion");

            migrationBuilder.DropIndex(
                name: "IX_notificacion_atendida",
                table: "notificacion");

            migrationBuilder.DropIndex(
                name: "IX_notificacion_id_producto",
                table: "notificacion");

            migrationBuilder.DropIndex(
                name: "IX_notificacion_id_usuario_atiende",
                table: "notificacion");

            migrationBuilder.DropColumn(
                name: "fecha_hora_atendida",
                table: "notificacion");

            migrationBuilder.DropColumn(
                name: "id_usuario_atiende",
                table: "notificacion");

            migrationBuilder.CreateIndex(
                name: "IX_notificacion_id_producto",
                table: "notificacion",
                column: "id_producto");
        }
    }
}//Finn de la clase
