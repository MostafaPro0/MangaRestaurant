using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MangaRestaurant.Repository.Data.Migrations
{
    public partial class AddLocation : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<double>(
                name: "ShippingAddress_Latitude",
                table: "Orders",
                type: "float",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ShippingAddress_LocationUrl",
                table: "Orders",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "ShippingAddress_Longitude",
                table: "Orders",
                type: "float",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ShippingAddress_Latitude",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "ShippingAddress_LocationUrl",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "ShippingAddress_Longitude",
                table: "Orders");
        }
    }
}
