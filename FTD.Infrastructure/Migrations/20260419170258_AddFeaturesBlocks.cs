using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace FTD.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddFeaturesBlocks : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LogoPath",
                table: "Categories");

            migrationBuilder.DropColumn(
                name: "LogoWhitePath",
                table: "Categories");

            migrationBuilder.InsertData(
                table: "ContentBlocks",
                columns: new[] { "Id", "BodyAr", "BodyEn", "ImagePath", "Key", "TitleAr", "TitleEn", "Type", "UpdatedAt" },
                values: new object[,]
                {
                    { 14, "لماذا FTD TechZone؟", null, null, "features.title.ar", null, null, "text", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified) },
                    { 15, null, "Why FTD TechZone?", null, "features.title.en", null, null, "text", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified) },
                    { 16, "ضمان أصلي على جميع المنتجات", "Original warranty on all products", null, "features.1", "✓", null, "text", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified) },
                    { 17, "توصيل سريع لجميع المحافظات", "Fast delivery to all governorates", null, "features.2", "🚚", null, "text", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified) },
                    { 18, "دعم فني على مدار الساعة", "24/7 technical support", null, "features.3", "💬", null, "text", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified) },
                    { 19, "سياسة إرجاع 14 يوم", "14-day return policy", null, "features.4", "↩", null, "text", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified) },
                    { 20, "تقسيط بدون فوائد", "0% interest installments", null, "features.5", "💳", null, "text", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified) },
                    { 21, "منتجات أصلية 100% معتمدة", "100% authentic certified products", null, "features.6", "⭐", null, "text", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified) }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "ContentBlocks",
                keyColumn: "Id",
                keyValue: 14);

            migrationBuilder.DeleteData(
                table: "ContentBlocks",
                keyColumn: "Id",
                keyValue: 15);

            migrationBuilder.DeleteData(
                table: "ContentBlocks",
                keyColumn: "Id",
                keyValue: 16);

            migrationBuilder.DeleteData(
                table: "ContentBlocks",
                keyColumn: "Id",
                keyValue: 17);

            migrationBuilder.DeleteData(
                table: "ContentBlocks",
                keyColumn: "Id",
                keyValue: 18);

            migrationBuilder.DeleteData(
                table: "ContentBlocks",
                keyColumn: "Id",
                keyValue: 19);

            migrationBuilder.DeleteData(
                table: "ContentBlocks",
                keyColumn: "Id",
                keyValue: 20);

            migrationBuilder.DeleteData(
                table: "ContentBlocks",
                keyColumn: "Id",
                keyValue: 21);

            migrationBuilder.AddColumn<string>(
                name: "LogoPath",
                table: "Categories",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LogoWhitePath",
                table: "Categories",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "LogoPath", "LogoWhitePath" },
                values: new object[] { "/images/brands/doogee.png", "/images/brands/doogee-white.png" });

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "LogoPath", "LogoWhitePath" },
                values: new object[] { "/images/brands/jisulife.png", "/images/brands/jisulife-white.png" });

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "LogoPath", "LogoWhitePath" },
                values: new object[] { "/images/brands/dreame.png", "/images/brands/dreame-white.png" });
        }
    }
}

