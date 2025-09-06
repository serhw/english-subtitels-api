using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Subtitles.Data.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Subtitles",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    Language = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false, defaultValue: "en"),
                    FilePath = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    Format = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false, defaultValue: "srt"),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Subtitles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SubtitleEntries",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    SubtitleId = table.Column<int>(type: "integer", nullable: false),
                    SequenceNumber = table.Column<int>(type: "integer", nullable: false),
                    StartTime = table.Column<TimeSpan>(type: "interval", nullable: false),
                    EndTime = table.Column<TimeSpan>(type: "interval", nullable: false),
                    Text = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SubtitleEntries", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SubtitleEntries_Subtitles_SubtitleId",
                        column: x => x.SubtitleId,
                        principalTable: "Subtitles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SubtitleEntries_SequenceNumber",
                table: "SubtitleEntries",
                column: "SequenceNumber");

            migrationBuilder.CreateIndex(
                name: "IX_SubtitleEntries_SubtitleId",
                table: "SubtitleEntries",
                column: "SubtitleId");

            migrationBuilder.CreateIndex(
                name: "IX_SubtitleEntries_SubtitleId_SequenceNumber",
                table: "SubtitleEntries",
                columns: new[] { "SubtitleId", "SequenceNumber" });

            migrationBuilder.CreateIndex(
                name: "IX_Subtitles_CreatedAt",
                table: "Subtitles",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_Subtitles_Language",
                table: "Subtitles",
                column: "Language");

            migrationBuilder.CreateIndex(
                name: "IX_Subtitles_Title_Language",
                table: "Subtitles",
                columns: new[] { "Title", "Language" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SubtitleEntries");

            migrationBuilder.DropTable(
                name: "Subtitles");
        }
    }
}

