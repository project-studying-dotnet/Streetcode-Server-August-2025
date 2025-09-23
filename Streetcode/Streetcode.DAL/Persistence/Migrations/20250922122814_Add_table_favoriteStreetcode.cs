using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Streetcode.DAL.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class Add_table_favoriteStreetcode : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "favourite_streetcode");

            migrationBuilder.CreateTable(
                name: "favourite_streetcodes",
                schema: "favourite_streetcode",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    StreetcodeId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_favourite_streetcodes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_favourite_streetcodes_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_favourite_streetcodes_streetcodes_StreetcodeId",
                        column: x => x.StreetcodeId,
                        principalSchema: "streetcode",
                        principalTable: "streetcodes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_favourite_streetcodes_StreetcodeId",
                schema: "favourite_streetcode",
                table: "favourite_streetcodes",
                column: "StreetcodeId");

            migrationBuilder.CreateIndex(
                name: "IX_favourite_streetcodes_UserId",
                schema: "favourite_streetcode",
                table: "favourite_streetcodes",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "favourite_streetcodes",
                schema: "favourite_streetcode");
        }
    }
}
