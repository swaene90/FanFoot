using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Fanfoot.Web.Migrations
{
    /// <inheritdoc />
    public partial class AddEmailPasswordToLocalUser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Email",
                table: "LocalUsers",
                type: "character varying(256)",
                maxLength: 256,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PasswordHash",
                table: "LocalUsers",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_LocalUsers_Email",
                table: "LocalUsers",
                column: "Email",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_LocalUsers_Email",
                table: "LocalUsers");

            migrationBuilder.DropColumn(
                name: "Email",
                table: "LocalUsers");

            migrationBuilder.DropColumn(
                name: "PasswordHash",
                table: "LocalUsers");
        }
    }
}
