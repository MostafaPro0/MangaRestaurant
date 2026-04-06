using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MangaRestaurant.Repository.Identity.Migrations
{
    public partial class MultipleAddressesSupport : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_UserAddress_AppUserId",
                table: "UserAddress");

            migrationBuilder.CreateIndex(
                name: "IX_UserAddress_AppUserId",
                table: "UserAddress",
                column: "AppUserId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_UserAddress_AppUserId",
                table: "UserAddress");

            migrationBuilder.CreateIndex(
                name: "IX_UserAddress_AppUserId",
                table: "UserAddress",
                column: "AppUserId",
                unique: true);
        }
    }
}
