using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace proyecto_final.Migrations
{
    /// <inheritdoc />
    public partial class AgregarHoraCancelado : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<TimeSpan>(
                name: "HoraCancelado",
                table: "Pedidos",
                type: "time(6)",
                nullable: true);

            migrationBuilder.AddColumn<TimeSpan>(
                name: "HoraEnPreparacion",
                table: "Pedidos",
                type: "time(6)",
                nullable: true);

            migrationBuilder.AddColumn<TimeSpan>(
                name: "HoraEntregado",
                table: "Pedidos",
                type: "time(6)",
                nullable: true);

            migrationBuilder.AddColumn<TimeSpan>(
                name: "HoraListo",
                table: "Pedidos",
                type: "time(6)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "HoraCancelado",
                table: "Pedidos");

            migrationBuilder.DropColumn(
                name: "HoraEnPreparacion",
                table: "Pedidos");

            migrationBuilder.DropColumn(
                name: "HoraEntregado",
                table: "Pedidos");

            migrationBuilder.DropColumn(
                name: "HoraListo",
                table: "Pedidos");
        }
    }
}
