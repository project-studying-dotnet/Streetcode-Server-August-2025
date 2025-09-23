using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Streetcode.DAL.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddedIsRestrictedToCommentContent : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsReviewed",
                schema: "comment",
                table: "comments");

            migrationBuilder.AddColumn<bool>(
                name: "IsRestricted",
                schema: "comment",
                table: "comments",
                type: "bit",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsRestricted",
                schema: "comment",
                table: "comments");

            migrationBuilder.AddColumn<bool>(
                name: "IsReviewed",
                schema: "comment",
                table: "comments",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }
    }
}
