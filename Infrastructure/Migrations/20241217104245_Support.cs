using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace muZilla.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class SupportZZZZZZZZZZZZZZZZZZZZDROPDATABASE : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "CanResponeOnSupports",
                table: "AccessLevels",
                newName: "CanManageSupports");

            migrationBuilder.AddColumn<int>(
                name: "SupporterId",
                table: "SupportMessages",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SupporterId",
                table: "SupportMessages");

            migrationBuilder.RenameColumn(
                name: "CanManageSupports",
                table: "AccessLevels",
                newName: "CanResponeOnSupports");
        }
    }
}
