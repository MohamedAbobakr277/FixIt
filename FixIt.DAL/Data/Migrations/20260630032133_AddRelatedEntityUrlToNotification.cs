using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FixIt.DAL.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddRelatedEntityUrlToNotification : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "RelatedEntityUrl",
                table: "Notifications",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RelatedEntityUrl",
                table: "Notifications");
        }
    }
}
