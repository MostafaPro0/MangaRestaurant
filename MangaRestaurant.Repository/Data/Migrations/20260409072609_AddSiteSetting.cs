using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MangaRestaurant.Repository.Data.Migrations
{
    public partial class AddSiteSetting : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "SiteSettings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RestaurantName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    RestaurantNameAr = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Address = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    AddressAr = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Phone1 = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Phone2 = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Email = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CurrencyCode = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CurrencySymbol = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FacebookUrl = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    InstagramUrl = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TwitterUrl = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    OpeningHoursEn = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    OpeningHoursAr = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DeliveryFee = table.Column<decimal>(type: "decimal(18,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SiteSettings", x => x.Id);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SiteSettings");
        }
    }
}
