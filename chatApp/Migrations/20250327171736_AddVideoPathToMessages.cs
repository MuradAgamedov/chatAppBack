using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace chatApp.Migrations
{
    /// <inheritdoc />
    public partial class AddVideoPathToMessages : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "VideoPath",
                table: "Messages",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "VideoPath",
                table: "Messages");
        }
    }
}
