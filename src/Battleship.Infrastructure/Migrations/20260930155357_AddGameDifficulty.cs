using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Battleship.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddGameDifficulty : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Difficulty",
                table: "Games",
                type: "text",
                nullable: false,
                defaultValue: "Easy");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Difficulty",
                table: "Games");
        }
    }
}
