using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FTD.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RemoveMarqueeFromSeeds : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "ContentBlocks",
                keyColumn: "Id",
                keyValue: 65);

            migrationBuilder.DeleteData(
                table: "SiteSettings",
                keyColumn: "Id",
                keyValue: 14);

            migrationBuilder.UpdateData(
                table: "SiteSettings",
                keyColumn: "Id",
                keyValue: 23,
                column: "Value",
                value: "hero,values,categories,featured,about,mission,cta,contact");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "ContentBlocks",
                columns: new[] { "Id", "BodyAr", "BodyEn", "ImagePath", "Key", "TitleAr", "TitleEn", "Type", "UpdatedAt" },
                values: new object[] { 65, "DOOGEE,JISULIFE,DREAME,ضمان معتمد,منتجات أصلية,توصيل سريع", "DOOGEE,JISULIFE,DREAME,CERTIFIED WARRANTY,ORIGINAL PRODUCTS,FAST DELIVERY", null, "marquee.items", null, null, "text", new DateTime(2026, 7, 12, 0, 0, 0, 0, DateTimeKind.Unspecified) });

            migrationBuilder.UpdateData(
                table: "SiteSettings",
                keyColumn: "Id",
                keyValue: 23,
                column: "Value",
                value: "hero,values,marquee,categories,featured,about,mission,cta,contact");

            migrationBuilder.InsertData(
                table: "SiteSettings",
                columns: new[] { "Id", "Description", "Key", "Type", "UpdatedAt", "Value" },
                values: new object[] { 14, "إظهار شريط البراندات المتحرك", "homepage.show.marquee", "bool", new DateTime(2026, 7, 12, 0, 0, 0, 0, DateTimeKind.Unspecified), "1" });
        }
    }
}
