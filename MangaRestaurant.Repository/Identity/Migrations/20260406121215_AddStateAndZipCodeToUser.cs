using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MangaRestaurant.Repository.Identity.Migrations
{
    public partial class AddStateAndZipCodeToUser : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "State",
                table: "UserAddress",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ZipCode",
                table: "UserAddress",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }   

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "State",
                table: "UserAddress");

            migrationBuilder.DropColumn(
                name: "ZipCode",
                table: "UserAddress");
        }
    }
}
