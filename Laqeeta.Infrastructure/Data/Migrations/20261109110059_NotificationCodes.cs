using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Laqeeta.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class NotificationCodes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "code",
                table: "notifications",
                type: "nvarchar(60)",
                maxLength: 60,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "param",
                table: "notifications",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "code",
                table: "notifications");

            migrationBuilder.DropColumn(
                name: "param",
                table: "notifications");
        }
    }
}