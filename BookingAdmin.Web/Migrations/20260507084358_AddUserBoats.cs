using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BookingAdmin.Web.Migrations
{
    /// <inheritdoc />
    public partial class AddUserBoats : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "UserBoats",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    BoatId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserBoats", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserBoats_Boats_BoatId",
                        column: x => x.BoatId,
                        principalTable: "Boats",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserBoats_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_UserBoats_BoatId",
                table: "UserBoats",
                column: "BoatId");

            migrationBuilder.CreateIndex(
                name: "IX_UserBoats_UserId_BoatId",
                table: "UserBoats",
                columns: new[] { "UserId", "BoatId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "UserBoats");
        }
    }
}
