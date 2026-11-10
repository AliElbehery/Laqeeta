using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Laqeeta.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddConcurrencyStamp : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "concurrency_stamp",
                table: "possible_matches",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "concurrency_stamp",
                table: "ownership_claims",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "concurrency_stamp",
                table: "lost_reports",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "concurrency_stamp",
                table: "found_items",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "concurrency_stamp",
                table: "possible_matches");

            migrationBuilder.DropColumn(
                name: "concurrency_stamp",
                table: "ownership_claims");

            migrationBuilder.DropColumn(
                name: "concurrency_stamp",
                table: "lost_reports");

            migrationBuilder.DropColumn(
                name: "concurrency_stamp",
                table: "found_items");
        }
    }
}
