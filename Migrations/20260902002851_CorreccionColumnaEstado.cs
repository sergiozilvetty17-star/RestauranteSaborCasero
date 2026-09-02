using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace proyecto_final.Migrations
{
    /// <inheritdoc />
    public partial class CorreccionColumnaEstado : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Estado",
                table: "Compras",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Estado",
                table: "Compras");
        }
    }
}
