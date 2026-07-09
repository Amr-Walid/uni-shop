using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FTD.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RemoveBrandLogoWhite : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LogoWhitePath",
                table: "Brands");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "LogoWhitePath",
                table: "Brands",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.UpdateData(
                table: "Brands",
                keyColumn: "Id",
                keyValue: 1,
                column: "LogoWhitePath",
                value: "/images/brands/doogee-white.png");

            migrationBuilder.UpdateData(
                table: "Brands",
                keyColumn: "Id",
                keyValue: 2,
                column: "LogoWhitePath",
                value: "/images/brands/jisulife-white.png");

            migrationBuilder.UpdateData(
                table: "Brands",
                keyColumn: "Id",
                keyValue: 3,
                column: "LogoWhitePath",
                value: "/images/brands/dreame-white.png");
        }
    }
}

