using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TJobs.Migrations
{
    /// <inheritdoc />
    public partial class AddApplicationUserInterestModel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ApplicationUserBriefs_ApplicationUserId",
                table: "ApplicationUserBriefs");

            migrationBuilder.CreateTable(
                name: "ApplicationUserInterests",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ApplicationUserId = table.Column<string>(type: "nvarchar(450)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ApplicationUserInterests", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ApplicationUserInterests_AspNetUsers_ApplicationUserId",
                        column: x => x.ApplicationUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ApplicationUserBriefs_ApplicationUserId",
                table: "ApplicationUserBriefs",
                column: "ApplicationUserId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ApplicationUserInterests_ApplicationUserId",
                table: "ApplicationUserInterests",
                column: "ApplicationUserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ApplicationUserInterests");

            migrationBuilder.DropIndex(
                name: "IX_ApplicationUserBriefs_ApplicationUserId",
                table: "ApplicationUserBriefs");

            migrationBuilder.CreateIndex(
                name: "IX_ApplicationUserBriefs_ApplicationUserId",
                table: "ApplicationUserBriefs",
                column: "ApplicationUserId");
        }
    }
}
