using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MangaRestaurant.Repository.Data.Migrations
{
    public partial class GachaSystemIntegration1 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "UserGachaRewards");

            migrationBuilder.DropTable(
                name: "GachaPrizes");

            migrationBuilder.RenameColumn(
                name: "IsGachaEnabled",
                table: "SiteSettings",
                newName: "IsLuckyRewardsEnabled");

            migrationBuilder.CreateTable(
                name: "LuckyPrizes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Title = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TitleAr = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DescriptionAr = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Icon = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Color = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ProbabilityWeight = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LuckyPrizes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "UserLuckyRewards",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AppUserId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    LuckyPrizeId = table.Column<int>(type: "int", nullable: false),
                    WonAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsRedeemed = table.Column<bool>(type: "bit", nullable: false),
                    PromoCode = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserLuckyRewards", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserLuckyRewards_LuckyPrizes_LuckyPrizeId",
                        column: x => x.LuckyPrizeId,
                        principalTable: "LuckyPrizes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_UserLuckyRewards_LuckyPrizeId",
                table: "UserLuckyRewards",
                column: "LuckyPrizeId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "UserLuckyRewards");

            migrationBuilder.DropTable(
                name: "LuckyPrizes");

            migrationBuilder.RenameColumn(
                name: "IsLuckyRewardsEnabled",
                table: "SiteSettings",
                newName: "IsGachaEnabled");

            migrationBuilder.CreateTable(
                name: "GachaPrizes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Color = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DescriptionAr = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Icon = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    ProbabilityWeight = table.Column<int>(type: "int", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TitleAr = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GachaPrizes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "UserGachaRewards",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    GachaPrizeId = table.Column<int>(type: "int", nullable: false),
                    AppUserId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsRedeemed = table.Column<bool>(type: "bit", nullable: false),
                    PromoCode = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    WonAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserGachaRewards", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserGachaRewards_GachaPrizes_GachaPrizeId",
                        column: x => x.GachaPrizeId,
                        principalTable: "GachaPrizes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_UserGachaRewards_GachaPrizeId",
                table: "UserGachaRewards",
                column: "GachaPrizeId");
        }
    }
}
