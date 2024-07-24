using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BangumiRSSAggregator.Migrations
{
    /// <inheritdoc />
    public partial class Initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "FeedGroups",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    Enabled = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FeedGroups", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "FeedRules",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Pattern = table.Column<string>(type: "TEXT", nullable: false),
                    Map = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FeedRules", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "FeedSources",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    Url = table.Column<string>(type: "TEXT", nullable: false),
                    UpdateInterval = table.Column<double>(type: "REAL", nullable: false),
                    LastUpdated = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FeedSources", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "EnabledRules",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    FeedRuleId = table.Column<int>(type: "INTEGER", nullable: false),
                    FeedSourceId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EnabledRules", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EnabledRules_FeedRules_FeedRuleId",
                        column: x => x.FeedRuleId,
                        principalTable: "FeedRules",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_EnabledRules_FeedSources_FeedSourceId",
                        column: x => x.FeedSourceId,
                        principalTable: "FeedSources",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "FeedItems",
                columns: table => new
                {
                    Id = table.Column<string>(type: "TEXT", nullable: false),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    Url = table.Column<string>(type: "TEXT", nullable: false),
                    RawContent = table.Column<string>(type: "TEXT", nullable: false),
                    FeedSourceId = table.Column<int>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FeedItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FeedItems_FeedSources_FeedSourceId",
                        column: x => x.FeedSourceId,
                        principalTable: "FeedSources",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "SourceGroups",
                columns: table => new
                {
                    FeedSourceId = table.Column<int>(type: "INTEGER", nullable: false),
                    FeedGroupId = table.Column<int>(type: "INTEGER", nullable: false),
                    RuleId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SourceGroups", x => new { x.FeedGroupId, x.FeedSourceId });
                    table.ForeignKey(
                        name: "FK_SourceGroups_FeedGroups_FeedGroupId",
                        column: x => x.FeedGroupId,
                        principalTable: "FeedGroups",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SourceGroups_FeedSources_FeedSourceId",
                        column: x => x.FeedSourceId,
                        principalTable: "FeedSources",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "BangumiItems",
                columns: table => new
                {
                    FeedItemId = table.Column<string>(type: "TEXT", nullable: false),
                    FeedGroupId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BangumiItems", x => new { x.FeedGroupId, x.FeedItemId });
                    table.ForeignKey(
                        name: "FK_BangumiItems_FeedGroups_FeedGroupId",
                        column: x => x.FeedGroupId,
                        principalTable: "FeedGroups",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_BangumiItems_FeedItems_FeedItemId",
                        column: x => x.FeedItemId,
                        principalTable: "FeedItems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_BangumiItems_FeedItemId",
                table: "BangumiItems",
                column: "FeedItemId");

            migrationBuilder.CreateIndex(
                name: "IX_EnabledRules_FeedRuleId",
                table: "EnabledRules",
                column: "FeedRuleId");

            migrationBuilder.CreateIndex(
                name: "IX_EnabledRules_FeedSourceId",
                table: "EnabledRules",
                column: "FeedSourceId");

            migrationBuilder.CreateIndex(
                name: "IX_FeedItems_FeedSourceId",
                table: "FeedItems",
                column: "FeedSourceId");

            migrationBuilder.CreateIndex(
                name: "IX_SourceGroups_FeedSourceId",
                table: "SourceGroups",
                column: "FeedSourceId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BangumiItems");

            migrationBuilder.DropTable(
                name: "EnabledRules");

            migrationBuilder.DropTable(
                name: "SourceGroups");

            migrationBuilder.DropTable(
                name: "FeedItems");

            migrationBuilder.DropTable(
                name: "FeedRules");

            migrationBuilder.DropTable(
                name: "FeedGroups");

            migrationBuilder.DropTable(
                name: "FeedSources");
        }
    }
}
