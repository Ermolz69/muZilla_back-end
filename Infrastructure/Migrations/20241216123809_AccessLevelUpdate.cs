using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace muZilla.Migrations
{
    /// <inheritdoc />
    public partial class AccessLevelUpdate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "CanManageReports",
                table: "AccessLevels",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CanManageReports",
                table: "AccessLevels");
        }
    }
}
