using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MangaRestaurant.Repository.Identity.Migrations
{
    public partial class AddLocation : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<double>(
                name: "Latitude",
                table: "UserAddress",
                type: "float",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LocationUrl",
                table: "UserAddress",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "Longitude",
                table: "UserAddress",
                type: "float",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Latitude",
                table: "UserAddress");

            migrationBuilder.DropColumn(
                name: "LocationUrl",
                table: "UserAddress");

            migrationBuilder.DropColumn(
                name: "Longitude",
                table: "UserAddress");
        }
    }
}
