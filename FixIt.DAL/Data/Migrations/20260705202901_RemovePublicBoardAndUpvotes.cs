using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FixIt.DAL.Data.Migrations
{
    /// <inheritdoc />
    public partial class RemovePublicBoardAndUpvotes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "IssueUpvotes");

            migrationBuilder.DropColumn(
                name: "UpvoteCount",
                table: "Issues");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "UpvoteCount",
                table: "Issues",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "IssueUpvotes",
                columns: table => new
                {
                    IssueUpvoteId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CitizenId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    IssueId = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IssueUpvotes", x => x.IssueUpvoteId);
                    table.ForeignKey(
                        name: "FK_IssueUpvotes_AspNetUsers_CitizenId",
                        column: x => x.CitizenId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_IssueUpvotes_Issues_IssueId",
                        column: x => x.IssueId,
                        principalTable: "Issues",
                        principalColumn: "IssueId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_IssueUpvotes_CitizenId",
                table: "IssueUpvotes",
                column: "CitizenId");

            migrationBuilder.CreateIndex(
                name: "IX_IssueUpvotes_IssueId_CitizenId",
                table: "IssueUpvotes",
                columns: new[] { "IssueId", "CitizenId" },
                unique: true);
        }
    }
}
