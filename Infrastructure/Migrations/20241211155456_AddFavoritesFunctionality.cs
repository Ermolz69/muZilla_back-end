using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace muZilla.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddFavoritesFunctionality : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "FavoritesCollectionId",
                table: "Users",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Likes",
                table: "Collections",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Views",
                table: "Collections",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "UserLikedCollections",
                columns: table => new
                {
                    CollectionId = table.Column<int>(type: "int", nullable: false),
                    UserId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserLikedCollections", x => new { x.CollectionId, x.UserId });
                    table.ForeignKey(
                        name: "FK_UserLikedCollections_Collections_CollectionId",
                        column: x => x.CollectionId,
                        principalTable: "Collections",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserLikedCollections_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Users_FavoritesCollectionId",
                table: "Users",
                column: "FavoritesCollectionId",
                unique: true,
                filter: "[FavoritesCollectionId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_UserLikedCollections_UserId",
                table: "UserLikedCollections",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Users_Collections_FavoritesCollectionId",
                table: "Users",
                column: "FavoritesCollectionId",
                principalTable: "Collections",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Users_Collections_FavoritesCollectionId",
                table: "Users");

            migrationBuilder.DropTable(
                name: "UserLikedCollections");

            migrationBuilder.DropIndex(
                name: "IX_Users_FavoritesCollectionId",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "FavoritesCollectionId",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "Likes",
                table: "Collections");

            migrationBuilder.DropColumn(
                name: "Views",
                table: "Collections");
        }
    }
}
