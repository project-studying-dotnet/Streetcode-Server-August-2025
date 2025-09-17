using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Streetcode.DAL.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddedDeletedPropertyManagedDeletingRepliesInComments : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_comments_comments_ParentCommentId",
                schema: "comment",
                table: "comments");

            migrationBuilder.AlterColumn<string>(
                name: "Text",
                schema: "comment",
                table: "comments",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(1000)",
                oldMaxLength: 1000);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                schema: "comment",
                table: "comments",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                schema: "comment",
                table: "comments",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddForeignKey(
                name: "FK_comments_comments_ParentCommentId",
                schema: "comment",
                table: "comments",
                column: "ParentCommentId",
                principalSchema: "comment",
                principalTable: "comments",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_comments_comments_ParentCommentId",
                schema: "comment",
                table: "comments");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                schema: "comment",
                table: "comments");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                schema: "comment",
                table: "comments");

            migrationBuilder.AlterColumn<string>(
                name: "Text",
                schema: "comment",
                table: "comments",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AddForeignKey(
                name: "FK_comments_comments_ParentCommentId",
                schema: "comment",
                table: "comments",
                column: "ParentCommentId",
                principalSchema: "comment",
                principalTable: "comments",
                principalColumn: "Id");
        }
    }
}
