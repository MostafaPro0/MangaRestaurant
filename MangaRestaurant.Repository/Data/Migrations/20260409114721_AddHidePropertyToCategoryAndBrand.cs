using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MangaRestaurant.Repository.Data.Migrations
{
    public partial class AddHidePropertyToCategoryAndBrand : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsHidden",
                table: "ProductCategories",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsHidden",
                table: "ProductBrands",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsHidden",
                table: "ProductCategories");

            migrationBuilder.DropColumn(
                name: "IsHidden",
                table: "ProductBrands");
        }
    }
}
