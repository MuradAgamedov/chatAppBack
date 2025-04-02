using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace chatApp.Migrations
{
    /// <inheritdoc />
    public partial class AddReactionToMessage : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Reaction",
                table: "Messages",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Reaction",
                table: "Messages");
        }
    }
}
