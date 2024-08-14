using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BangumiRSSAggregator.Server.Migrations
{
    /// <inheritdoc />
    public partial class RemoveSomeUnusedFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_EnabledRules",
                table: "EnabledRules");

            migrationBuilder.DropIndex(
                name: "IX_EnabledRules_FeedRuleId",
                table: "EnabledRules");

            migrationBuilder.DropColumn(
                name: "Id",
                table: "EnabledRules");

            migrationBuilder.AddPrimaryKey(
                name: "PK_EnabledRules",
                table: "EnabledRules",
                columns: new[] { "FeedRuleId", "FeedSourceId" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_EnabledRules",
                table: "EnabledRules");

            migrationBuilder.AddColumn<int>(
                name: "Id",
                table: "EnabledRules",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0)
                .Annotation("Sqlite:Autoincrement", true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_EnabledRules",
                table: "EnabledRules",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_EnabledRules_FeedRuleId",
                table: "EnabledRules",
                column: "FeedRuleId");
        }
    }
}
