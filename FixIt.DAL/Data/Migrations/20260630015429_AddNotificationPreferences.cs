using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FixIt.DAL.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddNotificationPreferences : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "AppDirectMessages",
                table: "AspNetUsers",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "AppRealTimePush",
                table: "AspNetUsers",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "EmailIssueUpdates",
                table: "AspNetUsers",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "EmailMaintenanceAlerts",
                table: "AspNetUsers",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "EmailWeeklyReports",
                table: "AspNetUsers",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AppDirectMessages",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "AppRealTimePush",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "EmailIssueUpdates",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "EmailMaintenanceAlerts",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "EmailWeeklyReports",
                table: "AspNetUsers");
        }
    }
}
