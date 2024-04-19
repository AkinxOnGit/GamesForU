using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GamesForU.Migrations
{
    /// <inheritdoc />
    public partial class initAllOther2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Category_Games_GamesId",
                table: "Category");

            migrationBuilder.DropIndex(
                name: "IX_Category_GamesId",
                table: "Category");

            migrationBuilder.DropColumn(
                name: "GamesId",
                table: "Category");

            migrationBuilder.CreateTable(
                name: "CategoryGames",
                columns: table => new
                {
                    CategoryId = table.Column<int>(type: "int", nullable: false),
                    GamesId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CategoryGames", x => new { x.CategoryId, x.GamesId });
                    table.ForeignKey(
                        name: "FK_CategoryGames_Category_CategoryId",
                        column: x => x.CategoryId,
                        principalTable: "Category",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CategoryGames_Games_GamesId",
                        column: x => x.GamesId,
                        principalTable: "Games",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CategoryGames_GamesId",
                table: "CategoryGames",
                column: "GamesId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CategoryGames");

            migrationBuilder.AddColumn<int>(
                name: "GamesId",
                table: "Category",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Category_GamesId",
                table: "Category",
                column: "GamesId");

            migrationBuilder.AddForeignKey(
                name: "FK_Category_Games_GamesId",
                table: "Category",
                column: "GamesId",
                principalTable: "Games",
                principalColumn: "Id");
        }
    }
}
