using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace muZilla.Migrations
{
    /// <inheritdoc />
    public partial class WassupPortMessage : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "CanResponeOnSupports",
                table: "AccessLevels",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CanResponeOnSupports",
                table: "AccessLevels");
        }
    }
}
