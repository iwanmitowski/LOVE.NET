using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LoveNet.Chat.Api.Data.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "chatrooms",
                columns: table => new
                {
                    Id = table.Column<string>(type: "text", nullable: false),
                    Title = table.Column<string>(type: "text", nullable: false),
                    Url = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_chatrooms", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "inbox_messages",
                columns: table => new
                {
                    MessageId = table.Column<Guid>(type: "uuid", nullable: false),
                    Consumer = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    ProcessedOn = table.Column<DateTime>(type: "timestamp without time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_inbox_messages", x => new { x.MessageId, x.Consumer });
                });

            // Hand-written: messages is range-partitioned by created_on (write-heavy,
            // read-recent workload). EF's CreateTable cannot emit PARTITION BY; the
            // model snapshot still describes the logical shape, which is all EF needs
            // for querying. Monthly partitions are created at startup
            // (PartitionMaintenance); the DEFAULT partition catches everything else.
            migrationBuilder.Sql(
                """
                CREATE TABLE messages (
                    "Id" character varying(64) NOT NULL,
                    "CreatedOn" timestamp without time zone NOT NULL,
                    "RoomId" character varying(128) NOT NULL,
                    "SenderId" text NOT NULL,
                    "Text" text NULL,
                    "ImageUrl" text NULL,
                    CONSTRAINT "PK_messages" PRIMARY KEY ("Id", "CreatedOn")
                ) PARTITION BY RANGE ("CreatedOn");

                CREATE TABLE messages_default PARTITION OF messages DEFAULT;
                """);

            migrationBuilder.CreateTable(
                name: "outbox_messages",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Topic = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    Key = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    Payload = table.Column<string>(type: "text", nullable: false),
                    OccurredOn = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    ProcessedOn = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    Attempts = table.Column<int>(type: "integer", nullable: false),
                    LastError = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_outbox_messages", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "rooms",
                columns: table => new
                {
                    RoomId = table.Column<string>(type: "text", nullable: false),
                    UserAId = table.Column<string>(type: "text", nullable: false),
                    UserBId = table.Column<string>(type: "text", nullable: false),
                    CreatedOn = table.Column<DateTime>(type: "timestamp without time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_rooms", x => x.RoomId);
                });

            migrationBuilder.CreateTable(
                name: "user_profiles_lite",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "text", nullable: false),
                    UserName = table.Column<string>(type: "text", nullable: false),
                    AvatarUrl = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_user_profiles_lite", x => x.UserId);
                });

            migrationBuilder.CreateIndex(
                name: "IX_messages_RoomId_CreatedOn",
                table: "messages",
                columns: new[] { "RoomId", "CreatedOn" },
                descending: new[] { false, true });

            migrationBuilder.CreateIndex(
                name: "IX_outbox_messages_ProcessedOn",
                table: "outbox_messages",
                column: "ProcessedOn");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "chatrooms");

            migrationBuilder.DropTable(
                name: "inbox_messages");

            migrationBuilder.DropTable(
                name: "messages");

            migrationBuilder.DropTable(
                name: "outbox_messages");

            migrationBuilder.DropTable(
                name: "rooms");

            migrationBuilder.DropTable(
                name: "user_profiles_lite");
        }
    }
}
