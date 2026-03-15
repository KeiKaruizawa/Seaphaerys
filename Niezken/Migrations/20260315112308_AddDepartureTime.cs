using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Niezken.Migrations
{
    /// <inheritdoc />
    public partial class AddDepartureTime : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "DepartureTime",
                table: "Ships",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "DepartureTime",
                table: "Bookings",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DepartureTime",
                table: "Ships");

            migrationBuilder.DropColumn(
                name: "DepartureTime",
                table: "Bookings");
        }
    }
}
