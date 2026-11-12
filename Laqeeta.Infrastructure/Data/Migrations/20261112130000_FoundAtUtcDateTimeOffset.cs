using Laqeeta.Infrastructure.Data;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Laqeeta.Infrastructure.Data.Migrations;

[DbContext(typeof(AppDbContext))]
[Migration("20261112130000_FoundAtUtcDateTimeOffset")]
public partial class FoundAtUtcDateTimeOffset : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AlterColumn<DateTimeOffset>(
            name: "found_at",
            table: "found_items",
            type: "datetimeoffset",
            nullable: false,
            oldClrType: typeof(DateTime),
            oldType: "datetime2");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AlterColumn<DateTime>(
            name: "found_at",
            table: "found_items",
            type: "datetime2",
            nullable: false,
            oldClrType: typeof(DateTimeOffset),
            oldType: "datetimeoffset");
    }
}
