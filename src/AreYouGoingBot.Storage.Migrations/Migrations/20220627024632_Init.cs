using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AreYouGoingBot.Storage.Migrations.Migrations
{
    public partial class Init : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ChatEvents",
                columns: table => new
                {
                    ChatEventId = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    TelegramChatId = table.Column<long>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChatEvents", x => x.ChatEventId);
                });

            migrationBuilder.CreateTable(
                name: "ChatEventParticipant",
                columns: table => new
                {
                    ChatEventParticipantId = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    TelegramUserId = table.Column<long>(type: "INTEGER", nullable: false),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    ChatEventId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChatEventParticipant", x => x.ChatEventParticipantId);
                    table.ForeignKey(
                        name: "FK_ChatEventParticipant_ChatEvents_ChatEventId",
                        column: x => x.ChatEventId,
                        principalTable: "ChatEvents",
                        principalColumn: "ChatEventId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ChatEventParticipant_ChatEventId",
                table: "ChatEventParticipant",
                column: "ChatEventId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ChatEventParticipant");

            migrationBuilder.DropTable(
                name: "ChatEvents");
        }
    }
}
