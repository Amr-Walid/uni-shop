using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FTD.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RebrandUniShopSeedData : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "ContactInfos",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "Email", "Facebook", "Instagram" },
                values: new object[] { "info@unishop.com", "https://facebook.com/unishop", "https://instagram.com/unishop" });

            migrationBuilder.UpdateData(
                table: "ContentBlocks",
                keyColumn: "Id",
                keyValue: 14,
                column: "BodyAr",
                value: "لماذا Uni-Shop؟");

            migrationBuilder.UpdateData(
                table: "ContentBlocks",
                keyColumn: "Id",
                keyValue: 15,
                column: "BodyEn",
                value: "Why Uni-Shop?");

            migrationBuilder.UpdateData(
                table: "SiteSettings",
                keyColumn: "Id",
                keyValue: 1,
                column: "Value",
                value: "Uni-Shop");

            migrationBuilder.UpdateData(
                table: "SiteSettings",
                keyColumn: "Id",
                keyValue: 2,
                column: "Value",
                value: "يونى شوب للمبيعات");

            migrationBuilder.UpdateData(
                table: "SiteSettings",
                keyColumn: "Id",
                keyValue: 3,
                column: "Value",
                value: "Uni-Shop E-Commerce");

            migrationBuilder.UpdateData(
                table: "SiteSettings",
                keyColumn: "Id",
                keyValue: 7,
                column: "Value",
                value: "Uni-Shop — وجهتك للتسوق");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "ContactInfos",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "Email", "Facebook", "Instagram" },
                values: new object[] { "info@ftdtechzone.com", "https://facebook.com/ftdtechzone", "https://instagram.com/ftdtechzone" });

            migrationBuilder.UpdateData(
                table: "ContentBlocks",
                keyColumn: "Id",
                keyValue: 14,
                column: "BodyAr",
                value: "لماذا FTD TechZone؟");

            migrationBuilder.UpdateData(
                table: "ContentBlocks",
                keyColumn: "Id",
                keyValue: 15,
                column: "BodyEn",
                value: "Why FTD TechZone?");

            migrationBuilder.UpdateData(
                table: "SiteSettings",
                keyColumn: "Id",
                keyValue: 1,
                column: "Value",
                value: "FTD TechZone");

            migrationBuilder.UpdateData(
                table: "SiteSettings",
                keyColumn: "Id",
                keyValue: 2,
                column: "Value",
                value: "الفجر للتجارة");

            migrationBuilder.UpdateData(
                table: "SiteSettings",
                keyColumn: "Id",
                keyValue: 3,
                column: "Value",
                value: "Alfajr For Trade");

            migrationBuilder.UpdateData(
                table: "SiteSettings",
                keyColumn: "Id",
                keyValue: 7,
                column: "Value",
                value: "FTD TechZone — الفجر للتجارة");
        }
    }
}
