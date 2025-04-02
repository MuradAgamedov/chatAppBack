using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace chatApp.Migrations
{
    /// <inheritdoc />
    public partial class AddLastSeenToUsers12 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LastSeen",
                table: "Messages");

            migrationBuilder.AddColumn<DateTime>(
                name: "LastSeen",
                table: "AspNetUsers",
                type: "datetime2",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LastSeen",
                table: "AspNetUsers");

            migrationBuilder.AddColumn<DateTime>(
                name: "LastSeen",
                table: "Messages",
                type: "datetime2",
                nullable: true);
        }
    }
}
