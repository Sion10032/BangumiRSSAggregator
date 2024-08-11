using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BangumiRSSAggregator.Server.Migrations
{
    /// <inheritdoc />
    public partial class RenameMapToReplacementInFeedRule : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Map",
                table: "FeedRules",
                newName: "Replacement");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Replacement",
                table: "FeedRules",
                newName: "Map");
        }
    }
}
