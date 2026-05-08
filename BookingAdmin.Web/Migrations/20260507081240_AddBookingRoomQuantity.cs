using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BookingAdmin.Web.Migrations
{
    /// <inheritdoc />
    public partial class AddBookingRoomQuantity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Quantity",
                table: "BookingRooms",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Quantity",
                table: "BookingRooms");
        }
    }
}
