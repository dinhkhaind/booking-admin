using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BookingAdmin.Web.Migrations
{
    /// <inheritdoc />
    public partial class AddColorToRoomTypes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Color",
                table: "RoomTypes",
                type: "character varying(7)",
                maxLength: 7,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Color",
                table: "RoomTypes");
        }
    }
}
