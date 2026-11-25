using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Sicofri.API.Migrations
{
    /// <inheritdoc />
    public partial class ModuloEntregas : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_viaje_usuario_id_usuario_encargado",
                table: "viaje");

            migrationBuilder.RenameColumn(
                name: "id_usuario_encargado",
                table: "viaje",
                newName: "id_usuario_creador");

            migrationBuilder.RenameIndex(
                name: "IX_viaje_id_usuario_encargado",
                table: "viaje",
                newName: "IX_viaje_id_usuario_creador");

            migrationBuilder.AddColumn<DateTime>(
                name: "fecha_hora_fin",
                table: "viaje",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "fecha_hora_inicio",
                table: "viaje",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "fecha_hora_cancelado",
                table: "pedido",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "fecha_hora_creacion",
                table: "pedido",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "fecha_hora_entregado",
                table: "pedido",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "id_usuario_cancela",
                table: "pedido",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "id_usuario_creador",
                table: "pedido",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "id_usuario_entrega",
                table: "pedido",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "viaje_encargado",
                columns: table => new
                {
                    id_viaje = table.Column<int>(type: "integer", nullable: false),
                    id_usuario = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_viaje_encargado", x => new { x.id_viaje, x.id_usuario });
                    table.ForeignKey(
                        name: "FK_viaje_encargado_usuario_id_usuario",
                        column: x => x.id_usuario,
                        principalTable: "usuario",
                        principalColumn: "id_usuario",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_viaje_encargado_viaje_id_viaje",
                        column: x => x.id_viaje,
                        principalTable: "viaje",
                        principalColumn: "id_viaje",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_pedido_estado",
                table: "pedido",
                column: "estado");

            migrationBuilder.CreateIndex(
                name: "IX_pedido_fecha_entrega",
                table: "pedido",
                column: "fecha_entrega");

            migrationBuilder.CreateIndex(
                name: "IX_pedido_id_usuario_cancela",
                table: "pedido",
                column: "id_usuario_cancela");

            migrationBuilder.CreateIndex(
                name: "IX_pedido_id_usuario_creador",
                table: "pedido",
                column: "id_usuario_creador");

            migrationBuilder.CreateIndex(
                name: "IX_pedido_id_usuario_entrega",
                table: "pedido",
                column: "id_usuario_entrega");

            migrationBuilder.CreateIndex(
                name: "IX_viaje_encargado_id_usuario",
                table: "viaje_encargado",
                column: "id_usuario");

            migrationBuilder.AddForeignKey(
                name: "FK_pedido_usuario_id_usuario_cancela",
                table: "pedido",
                column: "id_usuario_cancela",
                principalTable: "usuario",
                principalColumn: "id_usuario",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_pedido_usuario_id_usuario_creador",
                table: "pedido",
                column: "id_usuario_creador",
                principalTable: "usuario",
                principalColumn: "id_usuario",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_pedido_usuario_id_usuario_entrega",
                table: "pedido",
                column: "id_usuario_entrega",
                principalTable: "usuario",
                principalColumn: "id_usuario",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_viaje_usuario_id_usuario_creador",
                table: "viaje",
                column: "id_usuario_creador",
                principalTable: "usuario",
                principalColumn: "id_usuario",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_pedido_usuario_id_usuario_cancela",
                table: "pedido");

            migrationBuilder.DropForeignKey(
                name: "FK_pedido_usuario_id_usuario_creador",
                table: "pedido");

            migrationBuilder.DropForeignKey(
                name: "FK_pedido_usuario_id_usuario_entrega",
                table: "pedido");

            migrationBuilder.DropForeignKey(
                name: "FK_viaje_usuario_id_usuario_creador",
                table: "viaje");

            migrationBuilder.DropTable(
                name: "viaje_encargado");

            migrationBuilder.DropIndex(
                name: "IX_pedido_estado",
                table: "pedido");

            migrationBuilder.DropIndex(
                name: "IX_pedido_fecha_entrega",
                table: "pedido");

            migrationBuilder.DropIndex(
                name: "IX_pedido_id_usuario_cancela",
                table: "pedido");

            migrationBuilder.DropIndex(
                name: "IX_pedido_id_usuario_creador",
                table: "pedido");

            migrationBuilder.DropIndex(
                name: "IX_pedido_id_usuario_entrega",
                table: "pedido");

            migrationBuilder.DropColumn(
                name: "fecha_hora_fin",
                table: "viaje");

            migrationBuilder.DropColumn(
                name: "fecha_hora_inicio",
                table: "viaje");

            migrationBuilder.DropColumn(
                name: "fecha_hora_cancelado",
                table: "pedido");

            migrationBuilder.DropColumn(
                name: "fecha_hora_creacion",
                table: "pedido");

            migrationBuilder.DropColumn(
                name: "fecha_hora_entregado",
                table: "pedido");

            migrationBuilder.DropColumn(
                name: "id_usuario_cancela",
                table: "pedido");

            migrationBuilder.DropColumn(
                name: "id_usuario_creador",
                table: "pedido");

            migrationBuilder.DropColumn(
                name: "id_usuario_entrega",
                table: "pedido");

            migrationBuilder.RenameColumn(
                name: "id_usuario_creador",
                table: "viaje",
                newName: "id_usuario_encargado");

            migrationBuilder.RenameIndex(
                name: "IX_viaje_id_usuario_creador",
                table: "viaje",
                newName: "IX_viaje_id_usuario_encargado");

            migrationBuilder.AddForeignKey(
                name: "FK_viaje_usuario_id_usuario_encargado",
                table: "viaje",
                column: "id_usuario_encargado",
                principalTable: "usuario",
                principalColumn: "id_usuario",
                onDelete: ReferentialAction.Restrict);
        }
    }
}//Fin de la clase