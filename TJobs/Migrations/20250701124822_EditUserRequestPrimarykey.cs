using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TJobs.Migrations
{
    /// <inheritdoc />
    public partial class EditUserRequestPrimarykey : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_UserRequests",
                table: "UserRequests");

            migrationBuilder.DropColumn(
                name: "File",
                table: "UserRequests");

            migrationBuilder.AddColumn<int>(
                name: "Id",
                table: "UserRequests",
                type: "int",
                nullable: false,
                defaultValue: 0)
                .Annotation("SqlServer:Identity", "1, 1");

            migrationBuilder.AddColumn<string>(
                name: "File",
                table: "AspNetUsers",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Img",
                table: "AspNetUsers",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_UserRequests",
                table: "UserRequests",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_UserRequests_ApplicationUserId",
                table: "UserRequests",
                column: "ApplicationUserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_UserRequests",
                table: "UserRequests");

            migrationBuilder.DropIndex(
                name: "IX_UserRequests_ApplicationUserId",
                table: "UserRequests");

            migrationBuilder.DropColumn(
                name: "Id",
                table: "UserRequests");

            migrationBuilder.DropColumn(
                name: "File",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "Img",
                table: "AspNetUsers");

            migrationBuilder.AddColumn<string>(
                name: "File",
                table: "UserRequests",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_UserRequests",
                table: "UserRequests",
                columns: new[] { "ApplicationUserId", "RequestId" });
        }
    }
}
