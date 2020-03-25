using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace BoschBot.Migrations
{
    public partial class InitialCreate : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "UserScores",
                columns: table => new
                {
                    UserID = table.Column<ulong>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Score = table.Column<ulong>(nullable: false),
                    LastDailyClaimed = table.Column<DateTime>(nullable: true),
                    CurrentDailyStreak = table.Column<ulong>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserScores", x => x.UserID);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "UserScores");
        }
    }
}
