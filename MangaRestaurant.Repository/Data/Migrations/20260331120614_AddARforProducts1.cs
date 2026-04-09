using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MangaRestaurant.Repository.Data.Migrations
{
    public partial class AddARforProducts1 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ProductItemOrder_ProductNameAr",
                table: "OrderItems",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ProductItemOrder_ProductNameAr",
                table: "OrderItems");
        }
    }
}
