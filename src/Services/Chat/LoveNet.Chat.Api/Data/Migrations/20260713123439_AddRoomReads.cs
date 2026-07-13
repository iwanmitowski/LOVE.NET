using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LoveNet.Chat.Api.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddRoomReads : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "room_reads",
                columns: table => new
                {
                    RoomId = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    UserId = table.Column<string>(type: "text", nullable: false),
                    LastReadAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_room_reads", x => new { x.RoomId, x.UserId });
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "room_reads");
        }
    }
}
