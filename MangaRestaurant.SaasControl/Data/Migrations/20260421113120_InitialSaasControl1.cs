using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MangaRestaurant.SaasControl.Data.Migrations
{
    public partial class InitialSaasControl1 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Plans",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    NameAr = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    MonthlyPrice = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    MaxProducts = table.Column<int>(type: "int", nullable: false),
                    MaxStaff = table.Column<int>(type: "int", nullable: false),
                    HasLuckyRewards = table.Column<bool>(type: "bit", nullable: false),
                    HasAdvancedReports = table.Column<bool>(type: "bit", nullable: false),
                    HasCustomDomain = table.Column<bool>(type: "bit", nullable: false),
                    HasDeliveryTracking = table.Column<bool>(type: "bit", nullable: false),
                    HasEmailNotifications = table.Column<bool>(type: "bit", nullable: false),
                    SortOrder = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Plans", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Tenants",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    NameAr = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Slug = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    StoreDbName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    IdentityDbName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    AdminEmail = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    CustomDomain = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    LogoUrl = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    PlanId = table.Column<int>(type: "int", nullable: false, defaultValue: 1),
                    SubscriptionStartDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    SubscriptionEndDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    SuspensionReason = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tenants", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Tenants_Plans_PlanId",
                        column: x => x.PlanId,
                        principalTable: "Plans",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "AuditLogs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TenantId = table.Column<int>(type: "int", nullable: true),
                    EventType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PerformedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AuditLogs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AuditLogs_Tenants_TenantId",
                        column: x => x.TenantId,
                        principalTable: "Tenants",
                        principalColumn: "Id");
                });

            migrationBuilder.InsertData(
                table: "Plans",
                columns: new[] { "Id", "HasAdvancedReports", "HasCustomDomain", "HasDeliveryTracking", "HasEmailNotifications", "HasLuckyRewards", "MaxProducts", "MaxStaff", "MonthlyPrice", "Name", "NameAr", "SortOrder" },
                values: new object[] { 1, false, false, false, false, false, 20, 2, 0m, "Free", "مجاني", 1 });

            migrationBuilder.InsertData(
                table: "Plans",
                columns: new[] { "Id", "HasAdvancedReports", "HasCustomDomain", "HasDeliveryTracking", "HasEmailNotifications", "HasLuckyRewards", "MaxProducts", "MaxStaff", "MonthlyPrice", "Name", "NameAr", "SortOrder" },
                values: new object[] { 2, true, false, true, true, true, 200, 10, 99m, "Professional", "احترافي", 2 });

            migrationBuilder.InsertData(
                table: "Plans",
                columns: new[] { "Id", "HasAdvancedReports", "HasCustomDomain", "HasDeliveryTracking", "HasEmailNotifications", "HasLuckyRewards", "MaxProducts", "MaxStaff", "MonthlyPrice", "Name", "NameAr", "SortOrder" },
                values: new object[] { 3, true, true, true, true, true, 2147483647, 2147483647, 299m, "Enterprise", "مؤسسي", 3 });

            migrationBuilder.CreateIndex(
                name: "IX_AuditLogs_TenantId",
                table: "AuditLogs",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_Tenants_CustomDomain",
                table: "Tenants",
                column: "CustomDomain",
                unique: true,
                filter: "[CustomDomain] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Tenants_PlanId",
                table: "Tenants",
                column: "PlanId");

            migrationBuilder.CreateIndex(
                name: "IX_Tenants_Slug",
                table: "Tenants",
                column: "Slug",
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AuditLogs");

            migrationBuilder.DropTable(
                name: "Tenants");

            migrationBuilder.DropTable(
                name: "Plans");
        }
    }
}
