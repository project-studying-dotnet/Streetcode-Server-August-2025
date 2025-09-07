using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Streetcode.DAL.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddorderfieldforFactsentiti : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Description",
                schema: "media",
                table: "image_details");

            migrationBuilder.AddColumn<int>(
                name: "Order",
                schema: "streetcode",
                table: "facts",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Order",
                schema: "streetcode",
                table: "facts");

            migrationBuilder.AddColumn<string>(
                name: "Description",
                schema: "media",
                table: "image_details",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);
        }
    }
}
