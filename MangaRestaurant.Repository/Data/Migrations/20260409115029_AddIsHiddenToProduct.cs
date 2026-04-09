using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MangaRestaurant.Repository.Data.Migrations
{
    public partial class AddIsHiddenToProduct : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsHidden",
                table: "Products",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsHidden",
                table: "Products");
        }
    }
}
