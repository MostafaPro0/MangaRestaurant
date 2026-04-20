using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MangaRestaurant.Repository.Identity.Migrations
{
    public partial class GachaSystemIntegration1 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "MangaCoins",
                table: "AspNetUsers",
                newName: "LuckyCoins");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "LuckyCoins",
                table: "AspNetUsers",
                newName: "MangaCoins");
        }
    }
}
