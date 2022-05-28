using Microsoft.EntityFrameworkCore.Migrations;

namespace Fafik.Data.Migrations
{
    public partial class Logs : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<ulong>(
                name: "Logs",
                table: "Guilds",
                type: "bigint unsigned",
                nullable: false,
                defaultValue: 0ul);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Logs",
                table: "Guilds");
        }
    }
}
