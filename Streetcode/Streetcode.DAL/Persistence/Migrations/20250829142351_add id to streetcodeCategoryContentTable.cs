using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Streetcode.DAL.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddidtostreetcodeCategoryContentTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_streetcode_source_link_categories",
                schema: "sources",
                table: "streetcode_source_link_categories");

            migrationBuilder.AlterColumn<string>(
                name: "StreetcodeType",
                schema: "streetcode",
                table: "streetcodes",
                type: "nvarchar(21)",
                maxLength: 21,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AddColumn<int>(
                name: "Id",
                schema: "sources",
                table: "streetcode_source_link_categories",
                type: "int",
                nullable: false,
                defaultValue: 0)
                .Annotation("SqlServer:Identity", "1, 1");

            migrationBuilder.AlterColumn<string>(
                name: "CoordinateType",
                schema: "add_content",
                table: "coordinates",
                type: "nvarchar(21)",
                maxLength: 21,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AddPrimaryKey(
                name: "PK_streetcode_source_link_categories",
                schema: "sources",
                table: "streetcode_source_link_categories",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_streetcode_source_link_categories_SourceLinkCategoryId",
                schema: "sources",
                table: "streetcode_source_link_categories",
                column: "SourceLinkCategoryId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_streetcode_source_link_categories",
                schema: "sources",
                table: "streetcode_source_link_categories");

            migrationBuilder.DropIndex(
                name: "IX_streetcode_source_link_categories_SourceLinkCategoryId",
                schema: "sources",
                table: "streetcode_source_link_categories");

            migrationBuilder.DropColumn(
                name: "Id",
                schema: "sources",
                table: "streetcode_source_link_categories");

            migrationBuilder.AlterColumn<string>(
                name: "StreetcodeType",
                schema: "streetcode",
                table: "streetcodes",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(21)",
                oldMaxLength: 21);

            migrationBuilder.AlterColumn<string>(
                name: "CoordinateType",
                schema: "add_content",
                table: "coordinates",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(21)",
                oldMaxLength: 21);

            migrationBuilder.AddPrimaryKey(
                name: "PK_streetcode_source_link_categories",
                schema: "sources",
                table: "streetcode_source_link_categories",
                columns: new[] { "SourceLinkCategoryId", "StreetcodeId" });
        }
    }
}
