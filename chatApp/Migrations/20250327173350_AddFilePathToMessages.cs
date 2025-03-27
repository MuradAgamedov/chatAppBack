using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace chatApp.Migrations
{
    /// <inheritdoc />
    public partial class AddFilePathToMessages : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "FilePath",
                table: "Messages",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FilePath",
                table: "Messages");
        }
    }
}
