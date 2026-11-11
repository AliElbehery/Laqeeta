using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Laqeeta.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "metro_lines",
                columns: table => new
                {
                    line_id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    line_name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_metro_lines", x => x.line_id);
                });

            migrationBuilder.CreateTable(
                name: "stations",
                columns: table => new
                {
                    station_id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    station_name = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    location_detail = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    accepts_items = table.Column<bool>(type: "bit", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_stations", x => x.station_id);
                });

            migrationBuilder.CreateTable(
                name: "station_lines",
                columns: table => new
                {
                    station_id = table.Column<long>(type: "bigint", nullable: false),
                    line_id = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_station_lines", x => new { x.station_id, x.line_id });
                    table.ForeignKey(
                        name: "fk_station_lines_metro_lines_line_id",
                        column: x => x.line_id,
                        principalTable: "metro_lines",
                        principalColumn: "line_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_station_lines_stations_station_id",
                        column: x => x.station_id,
                        principalTable: "stations",
                        principalColumn: "station_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "users",
                columns: table => new
                {
                    user_id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    full_name = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    username = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    email = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    phone = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    password_hash = table.Column<string>(type: "nvarchar(512)", maxLength: 512, nullable: false),
                    role = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false, defaultValue: "User"),
                    station_id = table.Column<long>(type: "bigint", nullable: true),
                    is_active = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_users", x => x.user_id);
                    table.ForeignKey(
                        name: "fk_users_stations_station_id",
                        column: x => x.station_id,
                        principalTable: "stations",
                        principalColumn: "station_id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "found_items",
                columns: table => new
                {
                    item_id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    found_station_id = table.Column<long>(type: "bigint", nullable: false),
                    current_station_id = table.Column<long>(type: "bigint", nullable: false),
                    logged_by = table.Column<long>(type: "bigint", nullable: false),
                    category = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    color = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    private_identifying_details = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    found_at = table.Column<DateTime>(type: "datetime2", nullable: false),
                    logged_at = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()"),
                    item_status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false, defaultValue: "InStorage"),
                    returned_at = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_found_items", x => x.item_id);
                    table.ForeignKey(
                        name: "fk_found_items_stations_current_station_id",
                        column: x => x.current_station_id,
                        principalTable: "stations",
                        principalColumn: "station_id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_found_items_stations_found_station_id",
                        column: x => x.found_station_id,
                        principalTable: "stations",
                        principalColumn: "station_id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_found_items_users_logged_by",
                        column: x => x.logged_by,
                        principalTable: "users",
                        principalColumn: "user_id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "lost_reports",
                columns: table => new
                {
                    report_id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    user_id = table.Column<long>(type: "bigint", nullable: false),
                    line_id = table.Column<long>(type: "bigint", nullable: false),
                    contact_phone = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    category = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    color = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    lost_date = table.Column<DateOnly>(type: "date", nullable: false),
                    approximate_time = table.Column<TimeOnly>(type: "time", nullable: true),
                    direction = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false, defaultValue: "Open"),
                    closure_reason = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_lost_reports", x => x.report_id);
                    table.ForeignKey(
                        name: "fk_lost_reports_metro_lines_line_id",
                        column: x => x.line_id,
                        principalTable: "metro_lines",
                        principalColumn: "line_id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_lost_reports_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "user_id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "possible_matches",
                columns: table => new
                {
                    match_id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    report_id = table.Column<long>(type: "bigint", nullable: false),
                    item_id = table.Column<long>(type: "bigint", nullable: false),
                    match_status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false, defaultValue: "Suggested"),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_possible_matches", x => x.match_id);
                    table.ForeignKey(
                        name: "fk_possible_matches_found_items_item_id",
                        column: x => x.item_id,
                        principalTable: "found_items",
                        principalColumn: "item_id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_possible_matches_lost_reports_report_id",
                        column: x => x.report_id,
                        principalTable: "lost_reports",
                        principalColumn: "report_id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "notifications",
                columns: table => new
                {
                    notification_id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    user_id = table.Column<long>(type: "bigint", nullable: false),
                    type = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    title = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    message = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    report_id = table.Column<long>(type: "bigint", nullable: true),
                    match_id = table.Column<long>(type: "bigint", nullable: true),
                    is_read = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()"),
                    read_at = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_notifications", x => x.notification_id);
                    table.ForeignKey(
                        name: "fk_notifications_lost_reports_report_id",
                        column: x => x.report_id,
                        principalTable: "lost_reports",
                        principalColumn: "report_id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_notifications_possible_matches_match_id",
                        column: x => x.match_id,
                        principalTable: "possible_matches",
                        principalColumn: "match_id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_notifications_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "user_id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ownership_claims",
                columns: table => new
                {
                    claim_id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    match_id = table.Column<long>(type: "bigint", nullable: false),
                    claimed_by = table.Column<long>(type: "bigint", nullable: false),
                    verified_by = table.Column<long>(type: "bigint", nullable: true),
                    claim_status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false, defaultValue: "Pending"),
                    id_checked = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    secret_answer_passed = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    claimed_at = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()"),
                    verified_at = table.Column<DateTime>(type: "datetime2", nullable: true),
                    handed_over_at = table.Column<DateTime>(type: "datetime2", nullable: true),
                    verification_notes = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_ownership_claims", x => x.claim_id);
                    table.ForeignKey(
                        name: "fk_ownership_claims_possible_matches_match_id",
                        column: x => x.match_id,
                        principalTable: "possible_matches",
                        principalColumn: "match_id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_ownership_claims_users_claimed_by",
                        column: x => x.claimed_by,
                        principalTable: "users",
                        principalColumn: "user_id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_ownership_claims_users_verified_by",
                        column: x => x.verified_by,
                        principalTable: "users",
                        principalColumn: "user_id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "ix_found_items_current_station_id",
                table: "found_items",
                column: "current_station_id");

            migrationBuilder.CreateIndex(
                name: "ix_found_items_found_station_id",
                table: "found_items",
                column: "found_station_id");

            migrationBuilder.CreateIndex(
                name: "ix_found_items_item_status_category_current_station_id",
                table: "found_items",
                columns: new[] { "item_status", "category", "current_station_id" });

            migrationBuilder.CreateIndex(
                name: "ix_found_items_logged_by",
                table: "found_items",
                column: "logged_by");

            migrationBuilder.CreateIndex(
                name: "ix_lost_reports_line_id",
                table: "lost_reports",
                column: "line_id");

            migrationBuilder.CreateIndex(
                name: "ix_lost_reports_status_category_line_id",
                table: "lost_reports",
                columns: new[] { "status", "category", "line_id" });

            migrationBuilder.CreateIndex(
                name: "ix_lost_reports_user_id",
                table: "lost_reports",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "ix_metro_lines_line_name",
                table: "metro_lines",
                column: "line_name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_notifications_match_id",
                table: "notifications",
                column: "match_id");

            migrationBuilder.CreateIndex(
                name: "ix_notifications_report_id",
                table: "notifications",
                column: "report_id");

            migrationBuilder.CreateIndex(
                name: "ix_notifications_user_id_is_read",
                table: "notifications",
                columns: new[] { "user_id", "is_read" });

            migrationBuilder.CreateIndex(
                name: "ix_ownership_claims_claimed_by",
                table: "ownership_claims",
                column: "claimed_by");

            migrationBuilder.CreateIndex(
                name: "ix_ownership_claims_match_id",
                table: "ownership_claims",
                column: "match_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_ownership_claims_verified_by",
                table: "ownership_claims",
                column: "verified_by");

            migrationBuilder.CreateIndex(
                name: "ix_possible_matches_item_id",
                table: "possible_matches",
                column: "item_id");

            migrationBuilder.CreateIndex(
                name: "ix_possible_matches_report_id_item_id",
                table: "possible_matches",
                columns: new[] { "report_id", "item_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_station_lines_line_id",
                table: "station_lines",
                column: "line_id");

            migrationBuilder.CreateIndex(
                name: "ix_users_email",
                table: "users",
                column: "email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_users_station_id",
                table: "users",
                column: "station_id");

            migrationBuilder.CreateIndex(
                name: "ix_users_username",
                table: "users",
                column: "username",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "notifications");

            migrationBuilder.DropTable(
                name: "ownership_claims");

            migrationBuilder.DropTable(
                name: "station_lines");

            migrationBuilder.DropTable(
                name: "possible_matches");

            migrationBuilder.DropTable(
                name: "found_items");

            migrationBuilder.DropTable(
                name: "lost_reports");

            migrationBuilder.DropTable(
                name: "metro_lines");

            migrationBuilder.DropTable(
                name: "users");

            migrationBuilder.DropTable(
                name: "stations");
        }
    }
}