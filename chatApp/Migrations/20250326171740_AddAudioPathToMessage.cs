using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace chatApp.Migrations
{
    /// <inheritdoc />
    public partial class AddAudioPathToMessage : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AudioPath",
                table: "Messages",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AudioPath",
                table: "Messages");
        }
    }
}
