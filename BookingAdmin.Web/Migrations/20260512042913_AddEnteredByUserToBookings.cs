using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BookingAdmin.Web.Migrations
{
    /// <inheritdoc />
    public partial class AddEnteredByUserToBookings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<bool>(
                name: "IsDefault",
                table: "Currencies",
                type: "boolean",
                nullable: false,
                defaultValue: false,
                oldClrType: typeof(bool),
                oldType: "boolean");

            migrationBuilder.AddColumn<int>(
                name: "EnteredByUserId",
                table: "Bookings",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Bookings_EnteredByUserId",
                table: "Bookings",
                column: "EnteredByUserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Bookings_Users_EnteredByUserId",
                table: "Bookings",
                column: "EnteredByUserId",
                principalTable: "Users",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Bookings_Users_EnteredByUserId",
                table: "Bookings");

            migrationBuilder.DropIndex(
                name: "IX_Bookings_EnteredByUserId",
                table: "Bookings");

            migrationBuilder.DropColumn(
                name: "EnteredByUserId",
                table: "Bookings");

            migrationBuilder.AlterColumn<bool>(
                name: "IsDefault",
                table: "Currencies",
                type: "boolean",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "boolean",
                oldDefaultValue: false);
        }
    }
}
