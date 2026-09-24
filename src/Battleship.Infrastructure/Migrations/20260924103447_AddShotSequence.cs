using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Battleship.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddShotSequence : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Shots_GameId",
                table: "Shots");

            migrationBuilder.AddColumn<int>(
                name: "Sequence",
                table: "Shots",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Shots_GameId_Sequence",
                table: "Shots",
                columns: new[] { "GameId", "Sequence" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Shots_GameId_Sequence",
                table: "Shots");

            migrationBuilder.DropColumn(
                name: "Sequence",
                table: "Shots");

            migrationBuilder.CreateIndex(
                name: "IX_Shots_GameId",
                table: "Shots",
                column: "GameId");
        }
    }
}
