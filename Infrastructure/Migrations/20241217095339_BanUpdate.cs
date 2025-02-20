using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace muZilla.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class BanUpdate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Reason",
                table: "Bans",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(500)",
                oldMaxLength: 500);

            migrationBuilder.AlterColumn<int>(
                name: "BannedUserId",
                table: "Bans",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddColumn<int>(
                name: "BanType",
                table: "Bans",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "BannedCollectionId",
                table: "Bans",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "BannedSongId",
                table: "Bans",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Bans_BannedCollectionId",
                table: "Bans",
                column: "BannedCollectionId");

            migrationBuilder.CreateIndex(
                name: "IX_Bans_BannedSongId",
                table: "Bans",
                column: "BannedSongId");

            migrationBuilder.AddForeignKey(
                name: "FK_Bans_Collections_BannedCollectionId",
                table: "Bans",
                column: "BannedCollectionId",
                principalTable: "Collections",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Bans_Songs_BannedSongId",
                table: "Bans",
                column: "BannedSongId",
                principalTable: "Songs",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Bans_Collections_BannedCollectionId",
                table: "Bans");

            migrationBuilder.DropForeignKey(
                name: "FK_Bans_Songs_BannedSongId",
                table: "Bans");

            migrationBuilder.DropIndex(
                name: "IX_Bans_BannedCollectionId",
                table: "Bans");

            migrationBuilder.DropIndex(
                name: "IX_Bans_BannedSongId",
                table: "Bans");

            migrationBuilder.DropColumn(
                name: "BanType",
                table: "Bans");

            migrationBuilder.DropColumn(
                name: "BannedCollectionId",
                table: "Bans");

            migrationBuilder.DropColumn(
                name: "BannedSongId",
                table: "Bans");

            migrationBuilder.AlterColumn<string>(
                name: "Reason",
                table: "Bans",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<int>(
                name: "BannedUserId",
                table: "Bans",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);
        }
    }
}
