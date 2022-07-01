using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AreYouGoingBot.Storage.Migrations.Migrations
{
    public partial class ChatEvent_AddTelegramEventMessageId : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "TelegramEventMessageId",
                table: "ChatEvents",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TelegramEventMessageId",
                table: "ChatEvents");
        }
    }
}
