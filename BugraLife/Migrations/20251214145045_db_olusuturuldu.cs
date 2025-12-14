using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BugraLife.Migrations
{
    /// <inheritdoc />
    public partial class db_olusuturuldu : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "WebSites",
                columns: table => new
                {
                    website_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    website_name = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    website_url = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    website_description = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WebSites", x => x.website_id);
                });

            migrationBuilder.CreateTable(
                name: "WebSitePasswords",
                columns: table => new
                {
                    websitepassword_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    website_id = table.Column<int>(type: "int", nullable: false),
                    websitepassword_username = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    websitepassword_password = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    websitepassword_description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false),
                    updated_at = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WebSitePasswords", x => x.websitepassword_id);
                    table.ForeignKey(
                        name: "FK_WebSitePasswords_WebSites_website_id",
                        column: x => x.website_id,
                        principalTable: "WebSites",
                        principalColumn: "website_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_WebSitePasswords_website_id",
                table: "WebSitePasswords",
                column: "website_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "WebSitePasswords");

            migrationBuilder.DropTable(
                name: "WebSites");
        }
    }
}
