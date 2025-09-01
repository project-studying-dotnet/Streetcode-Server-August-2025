using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Streetcode.DAL.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddStreetcodeArtSlides : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_streetcode_art_streetcodes_StreetcodeId",
                schema: "streetcode",
                table: "streetcode_art");

            migrationBuilder.DropPrimaryKey(
                name: "PK_streetcode_art",
                schema: "streetcode",
                table: "streetcode_art");

            migrationBuilder.DropIndex(
                name: "IX_streetcode_art_ArtId_StreetcodeId",
                schema: "streetcode",
                table: "streetcode_art");

            migrationBuilder.AlterColumn<string>(
                name: "StreetcodeType",
                schema: "streetcode",
                table: "streetcodes",
                type: "nvarchar(21)",
                maxLength: 21,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<int>(
                name: "StreetcodeId",
                schema: "streetcode",
                table: "streetcode_art",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddColumn<int>(
                name: "Id",
                schema: "streetcode",
                table: "streetcode_art",
                type: "int",
                nullable: false,
                defaultValue: 0)
                .Annotation("SqlServer:Identity", "1, 1");

            migrationBuilder.AddColumn<int>(
                name: "StreetcodeArtSlideId",
                schema: "streetcode",
                table: "streetcode_art",
                type: "int",
                nullable: true);

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
                name: "PK_streetcode_art",
                schema: "streetcode",
                table: "streetcode_art",
                column: "Id");

            migrationBuilder.CreateTable(
                name: "streetcode_art_slides",
                schema: "streetcode",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Index = table.Column<int>(type: "int", nullable: false, defaultValue: 1),
                    Template = table.Column<int>(type: "int", nullable: false),
                    StreetcodeId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_streetcode_art_slides", x => x.Id);
                    table.ForeignKey(
                        name: "FK_streetcode_art_slides_streetcodes_StreetcodeId",
                        column: x => x.StreetcodeId,
                        principalSchema: "streetcode",
                        principalTable: "streetcodes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_streetcode_art_ArtId_StreetcodeArtSlideId",
                schema: "streetcode",
                table: "streetcode_art",
                columns: new[] { "ArtId", "StreetcodeArtSlideId" });

            migrationBuilder.CreateIndex(
                name: "IX_streetcode_art_StreetcodeArtSlideId",
                schema: "streetcode",
                table: "streetcode_art",
                column: "StreetcodeArtSlideId");

            migrationBuilder.CreateIndex(
                name: "IX_streetcode_art_slides_StreetcodeId",
                schema: "streetcode",
                table: "streetcode_art_slides",
                column: "StreetcodeId");

            migrationBuilder.AddForeignKey(
                name: "FK_streetcode_art_streetcode_art_slides_StreetcodeArtSlideId",
                schema: "streetcode",
                table: "streetcode_art",
                column: "StreetcodeArtSlideId",
                principalSchema: "streetcode",
                principalTable: "streetcode_art_slides",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_streetcode_art_streetcodes_StreetcodeId",
                schema: "streetcode",
                table: "streetcode_art",
                column: "StreetcodeId",
                principalSchema: "streetcode",
                principalTable: "streetcodes",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_streetcode_art_streetcode_art_slides_StreetcodeArtSlideId",
                schema: "streetcode",
                table: "streetcode_art");

            migrationBuilder.DropForeignKey(
                name: "FK_streetcode_art_streetcodes_StreetcodeId",
                schema: "streetcode",
                table: "streetcode_art");

            migrationBuilder.DropTable(
                name: "streetcode_art_slides",
                schema: "streetcode");

            migrationBuilder.DropPrimaryKey(
                name: "PK_streetcode_art",
                schema: "streetcode",
                table: "streetcode_art");

            migrationBuilder.DropIndex(
                name: "IX_streetcode_art_ArtId_StreetcodeArtSlideId",
                schema: "streetcode",
                table: "streetcode_art");

            migrationBuilder.DropIndex(
                name: "IX_streetcode_art_StreetcodeArtSlideId",
                schema: "streetcode",
                table: "streetcode_art");

            migrationBuilder.DropColumn(
                name: "Id",
                schema: "streetcode",
                table: "streetcode_art");

            migrationBuilder.DropColumn(
                name: "StreetcodeArtSlideId",
                schema: "streetcode",
                table: "streetcode_art");

            migrationBuilder.AlterColumn<string>(
                name: "StreetcodeType",
                schema: "streetcode",
                table: "streetcodes",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(21)",
                oldMaxLength: 21);

            migrationBuilder.AlterColumn<int>(
                name: "StreetcodeId",
                schema: "streetcode",
                table: "streetcode_art",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

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
                name: "PK_streetcode_art",
                schema: "streetcode",
                table: "streetcode_art",
                columns: new[] { "ArtId", "StreetcodeId" });

            migrationBuilder.CreateIndex(
                name: "IX_streetcode_art_ArtId_StreetcodeId",
                schema: "streetcode",
                table: "streetcode_art",
                columns: new[] { "ArtId", "StreetcodeId" });

            migrationBuilder.AddForeignKey(
                name: "FK_streetcode_art_streetcodes_StreetcodeId",
                schema: "streetcode",
                table: "streetcode_art",
                column: "StreetcodeId",
                principalSchema: "streetcode",
                principalTable: "streetcodes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
