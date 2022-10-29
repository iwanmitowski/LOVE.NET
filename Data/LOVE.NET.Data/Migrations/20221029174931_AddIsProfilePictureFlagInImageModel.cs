using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LOVE.NET.Data.Migrations
{
    public partial class AddIsProfilePictureFlagInImageModel : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsProfilePicture",
                table: "Images",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsProfilePicture",
                table: "Images");
        }
    }
}
