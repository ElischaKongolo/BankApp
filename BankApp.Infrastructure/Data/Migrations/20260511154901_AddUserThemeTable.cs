using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace BankApp.Infrastructure.Data.Migrations
{
    public partial class AddUserThemeTable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "UserThemes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ThemeName = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    PrimaryColor = table.Column<string>(type: "TEXT", maxLength: 7, nullable: false),
                    SecondaryColor = table.Column<string>(type: "TEXT", maxLength: 7, nullable: false),
                    BackgroundColor = table.Column<string>(type: "TEXT", maxLength: 7, nullable: false),
                    SurfaceColor = table.Column<string>(type: "TEXT", maxLength: 7, nullable: false),
                    TextColor = table.Column<string>(type: "TEXT", maxLength: 7, nullable: false),
                    TextMutedColor = table.Column<string>(type: "TEXT", maxLength: 7, nullable: false),
                    IsDarkMode = table.Column<bool>(type: "INTEGER", nullable: false),
                    IsCustom = table.Column<bool>(type: "INTEGER", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UserId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserThemes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserThemes_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_UserThemes_UserId",
                table: "UserThemes",
                column: "UserId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "UserThemes");
        }
    }
}
