using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BangumiRSSAggregator.Server.Migrations
{
    /// <inheritdoc />
    public partial class AddPubDateForFeedItem : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "PubDate",
                table: "FeedItems",
                type: "TEXT",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PubDate",
                table: "FeedItems");
        }
    }
}
