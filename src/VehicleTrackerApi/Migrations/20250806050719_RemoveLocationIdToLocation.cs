using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VehicleTrackerApi.Migrations
{
    /// <inheritdoc />
    public partial class RemoveLocationIdToLocation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Locations");

            migrationBuilder.DropColumn(
                name: "LocationId",
                table: "Vehicles");

            migrationBuilder.AddColumn<double>(
                name: "Location_Latitude",
                table: "Vehicles",
                type: "REAL",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "Location_Longitude",
                table: "Vehicles",
                type: "REAL",
                nullable: false,
                defaultValue: 0.0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Location_Latitude",
                table: "Vehicles");

            migrationBuilder.DropColumn(
                name: "Location_Longitude",
                table: "Vehicles");

            migrationBuilder.AddColumn<int>(
                name: "LocationId",
                table: "Vehicles",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "Locations",
                columns: table => new
                {
                    VehicleId = table.Column<int>(type: "INTEGER", nullable: false),
                    Latitude = table.Column<double>(type: "REAL", nullable: false),
                    LocationId = table.Column<int>(type: "INTEGER", nullable: false),
                    Longitude = table.Column<double>(type: "REAL", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Locations", x => x.VehicleId);
                    table.ForeignKey(
                        name: "FK_Locations_Vehicles_VehicleId",
                        column: x => x.VehicleId,
                        principalTable: "Vehicles",
                        principalColumn: "VehicleId",
                        onDelete: ReferentialAction.Cascade);
                });
        }
    }
}
