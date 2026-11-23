using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Sicofri.API.Migrations
{
    /// <inheritdoc />
    public partial class Inicial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "cliente",
                columns: table => new
                {
                    id_cliente = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    nombre = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: false),
                    provincia = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    ubicacion_detalle = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_cliente", x => x.id_cliente);
                });

            migrationBuilder.CreateTable(
                name: "configuracion",
                columns: table => new
                {
                    id_configuracion = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    dias_anticipacion = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_configuracion", x => x.id_configuracion);
                    table.CheckConstraint("ck_configuracion_dias", "dias_anticipacion > 0");
                });

            migrationBuilder.CreateTable(
                name: "ubicacion",
                columns: table => new
                {
                    id_ubicacion = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    nombre = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    zona = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ubicacion", x => x.id_ubicacion);
                });

            migrationBuilder.CreateTable(
                name: "usuario",
                columns: table => new
                {
                    id_usuario = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    nombre = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: false),
                    nombre_usuario = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    contrasena_hash = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    rol = table.Column<string>(type: "character varying(15)", maxLength: 15, nullable: false),
                    activo = table.Column<bool>(type: "boolean", nullable: false),
                    token_fcm = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_usuario", x => x.id_usuario);
                });

            migrationBuilder.CreateTable(
                name: "producto",
                columns: table => new
                {
                    id_producto = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    nombre = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: false),
                    nombre_proveedor = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: false),
                    cantidad = table.Column<decimal>(type: "numeric(10,2)", precision: 10, scale: 2, nullable: false),
                    fecha_ingreso = table.Column<DateOnly>(type: "date", nullable: false),
                    fecha_produccion = table.Column<DateOnly>(type: "date", nullable: false),
                    fecha_caducidad = table.Column<DateOnly>(type: "date", nullable: false),
                    id_ubicacion = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_producto", x => x.id_producto);
                    table.CheckConstraint("ck_producto_cantidad", "cantidad >= 0");
                    table.ForeignKey(
                        name: "FK_producto_ubicacion_id_ubicacion",
                        column: x => x.id_ubicacion,
                        principalTable: "ubicacion",
                        principalColumn: "id_ubicacion",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "viaje",
                columns: table => new
                {
                    id_viaje = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    fecha = table.Column<DateOnly>(type: "date", nullable: false),
                    estado = table.Column<string>(type: "character varying(15)", maxLength: 15, nullable: false),
                    id_usuario_encargado = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_viaje", x => x.id_viaje);
                    table.ForeignKey(
                        name: "FK_viaje_usuario_id_usuario_encargado",
                        column: x => x.id_usuario_encargado,
                        principalTable: "usuario",
                        principalColumn: "id_usuario",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "movimiento_traslado",
                columns: table => new
                {
                    id_movimiento = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    cantidad = table.Column<decimal>(type: "numeric(10,2)", precision: 10, scale: 2, nullable: false),
                    fecha_hora = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    id_producto_origen = table.Column<int>(type: "integer", nullable: false),
                    id_producto_destino = table.Column<int>(type: "integer", nullable: false),
                    id_ubicacion_origen = table.Column<int>(type: "integer", nullable: false),
                    id_ubicacion_destino = table.Column<int>(type: "integer", nullable: false),
                    id_usuario = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_movimiento_traslado", x => x.id_movimiento);
                    table.CheckConstraint("ck_movimiento_cantidad", "cantidad > 0");
                    table.ForeignKey(
                        name: "FK_movimiento_traslado_producto_id_producto_destino",
                        column: x => x.id_producto_destino,
                        principalTable: "producto",
                        principalColumn: "id_producto",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_movimiento_traslado_producto_id_producto_origen",
                        column: x => x.id_producto_origen,
                        principalTable: "producto",
                        principalColumn: "id_producto",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_movimiento_traslado_ubicacion_id_ubicacion_destino",
                        column: x => x.id_ubicacion_destino,
                        principalTable: "ubicacion",
                        principalColumn: "id_ubicacion",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_movimiento_traslado_ubicacion_id_ubicacion_origen",
                        column: x => x.id_ubicacion_origen,
                        principalTable: "ubicacion",
                        principalColumn: "id_ubicacion",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_movimiento_traslado_usuario_id_usuario",
                        column: x => x.id_usuario,
                        principalTable: "usuario",
                        principalColumn: "id_usuario",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "notificacion",
                columns: table => new
                {
                    id_notificacion = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    fecha_generada = table.Column<DateOnly>(type: "date", nullable: false),
                    atendida = table.Column<bool>(type: "boolean", nullable: false),
                    id_producto = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_notificacion", x => x.id_notificacion);
                    table.ForeignKey(
                        name: "FK_notificacion_producto_id_producto",
                        column: x => x.id_producto,
                        principalTable: "producto",
                        principalColumn: "id_producto",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "pedido",
                columns: table => new
                {
                    id_pedido = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    estado = table.Column<string>(type: "character varying(15)", maxLength: 15, nullable: false),
                    fecha_entrega = table.Column<DateOnly>(type: "date", nullable: false),
                    id_cliente = table.Column<int>(type: "integer", nullable: false),
                    id_viaje = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_pedido", x => x.id_pedido);
                    table.ForeignKey(
                        name: "FK_pedido_cliente_id_cliente",
                        column: x => x.id_cliente,
                        principalTable: "cliente",
                        principalColumn: "id_cliente",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_pedido_viaje_id_viaje",
                        column: x => x.id_viaje,
                        principalTable: "viaje",
                        principalColumn: "id_viaje",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "linea_pedido",
                columns: table => new
                {
                    id_detalle = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    cantidad = table.Column<decimal>(type: "numeric(10,2)", precision: 10, scale: 2, nullable: false),
                    id_pedido = table.Column<int>(type: "integer", nullable: false),
                    id_producto = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_linea_pedido", x => x.id_detalle);
                    table.CheckConstraint("ck_linea_pedido_cantidad", "cantidad > 0");
                    table.ForeignKey(
                        name: "FK_linea_pedido_pedido_id_pedido",
                        column: x => x.id_pedido,
                        principalTable: "pedido",
                        principalColumn: "id_pedido",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_linea_pedido_producto_id_producto",
                        column: x => x.id_producto,
                        principalTable: "producto",
                        principalColumn: "id_producto",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.InsertData(
                table: "configuracion",
                columns: new[] { "id_configuracion", "dias_anticipacion" },
                values: new object[] { 1, 10 });

            migrationBuilder.InsertData(
                table: "ubicacion",
                columns: new[] { "id_ubicacion", "nombre", "zona" },
                values: new object[,]
                {
                    { 1, "Congelador 1", "Puntarenas" },
                    { 2, "Congelador 2", "Puntarenas" },
                    { 3, "Congelador 3", "Puntarenas" },
                    { 4, "Congelador 4", "Puntarenas" },
                    { 5, "Congelador 5", "Puntarenas" },
                    { 6, "Cámara San José", "SanJose" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_linea_pedido_id_pedido",
                table: "linea_pedido",
                column: "id_pedido");

            migrationBuilder.CreateIndex(
                name: "IX_linea_pedido_id_producto",
                table: "linea_pedido",
                column: "id_producto");

            migrationBuilder.CreateIndex(
                name: "IX_movimiento_traslado_id_producto_destino",
                table: "movimiento_traslado",
                column: "id_producto_destino");

            migrationBuilder.CreateIndex(
                name: "IX_movimiento_traslado_id_producto_origen",
                table: "movimiento_traslado",
                column: "id_producto_origen");

            migrationBuilder.CreateIndex(
                name: "IX_movimiento_traslado_id_ubicacion_destino",
                table: "movimiento_traslado",
                column: "id_ubicacion_destino");

            migrationBuilder.CreateIndex(
                name: "IX_movimiento_traslado_id_ubicacion_origen",
                table: "movimiento_traslado",
                column: "id_ubicacion_origen");

            migrationBuilder.CreateIndex(
                name: "IX_movimiento_traslado_id_usuario",
                table: "movimiento_traslado",
                column: "id_usuario");

            migrationBuilder.CreateIndex(
                name: "IX_notificacion_id_producto",
                table: "notificacion",
                column: "id_producto");

            migrationBuilder.CreateIndex(
                name: "IX_pedido_id_cliente",
                table: "pedido",
                column: "id_cliente");

            migrationBuilder.CreateIndex(
                name: "IX_pedido_id_viaje",
                table: "pedido",
                column: "id_viaje");

            migrationBuilder.CreateIndex(
                name: "IX_producto_id_ubicacion",
                table: "producto",
                column: "id_ubicacion");

            migrationBuilder.CreateIndex(
                name: "IX_usuario_nombre_usuario",
                table: "usuario",
                column: "nombre_usuario",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_viaje_id_usuario_encargado",
                table: "viaje",
                column: "id_usuario_encargado");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "configuracion");

            migrationBuilder.DropTable(
                name: "linea_pedido");

            migrationBuilder.DropTable(
                name: "movimiento_traslado");

            migrationBuilder.DropTable(
                name: "notificacion");

            migrationBuilder.DropTable(
                name: "pedido");

            migrationBuilder.DropTable(
                name: "producto");

            migrationBuilder.DropTable(
                name: "cliente");

            migrationBuilder.DropTable(
                name: "viaje");

            migrationBuilder.DropTable(
                name: "ubicacion");

            migrationBuilder.DropTable(
                name: "usuario");
        }
    }
}
