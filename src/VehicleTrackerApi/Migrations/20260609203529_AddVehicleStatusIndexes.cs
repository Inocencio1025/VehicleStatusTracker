using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VehicleTrackerApi.Migrations
{
    /// <inheritdoc />
    public partial class AddVehicleStatusIndexes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_VehicleStatuses_Vehicles_VehicleId",
                table: "VehicleStatuses");

            migrationBuilder.DropIndex(
                name: "IX_VehicleStatuses_VehicleId",
                table: "VehicleStatuses");

            migrationBuilder.CreateIndex(
                name: "IX_VehicleStatuses_VehicleId_Timestamp",
                table: "VehicleStatuses",
                columns: new[] { "VehicleId", "Timestamp" });

            migrationBuilder.AddForeignKey(
                name: "FK_VehicleStatuses_Vehicles_VehicleId",
                table: "VehicleStatuses",
                column: "VehicleId",
                principalTable: "Vehicles",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_VehicleStatuses_Vehicles_VehicleId",
                table: "VehicleStatuses");

            migrationBuilder.DropIndex(
                name: "IX_VehicleStatuses_VehicleId_Timestamp",
                table: "VehicleStatuses");

            migrationBuilder.CreateIndex(
                name: "IX_VehicleStatuses_VehicleId",
                table: "VehicleStatuses",
                column: "VehicleId");

            migrationBuilder.AddForeignKey(
                name: "FK_VehicleStatuses_Vehicles_VehicleId",
                table: "VehicleStatuses",
                column: "VehicleId",
                principalTable: "Vehicles",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
