using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DAL.Migrations
{
    /// <inheritdoc />
    public partial class AddServiceToMedicalRecord : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CHK_Patient_Gender",
                table: "Patients");

            migrationBuilder.AddColumn<string>(
                name: "Result",
                table: "MedicalRecords",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ServiceId",
                table: "MedicalRecords",
                type: "integer",
                nullable: true);

            migrationBuilder.AddCheckConstraint(
                name: "CHK_Patient_Gender",
                table: "Patients",
                sql: "\"Gender\" IN ('Male', 'Female')");

            migrationBuilder.CreateIndex(
                name: "IX_MedicalRecords_ServiceId",
                table: "MedicalRecords",
                column: "ServiceId");

            migrationBuilder.AddForeignKey(
                name: "FK_MedicalRecords_Services_ServiceId",
                table: "MedicalRecords",
                column: "ServiceId",
                principalTable: "Services",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_MedicalRecords_Services_ServiceId",
                table: "MedicalRecords");

            migrationBuilder.DropCheckConstraint(
                name: "CHK_Patient_Gender",
                table: "Patients");

            migrationBuilder.DropIndex(
                name: "IX_MedicalRecords_ServiceId",
                table: "MedicalRecords");

            migrationBuilder.DropColumn(
                name: "Result",
                table: "MedicalRecords");

            migrationBuilder.DropColumn(
                name: "ServiceId",
                table: "MedicalRecords");

            migrationBuilder.AddCheckConstraint(
                name: "CHK_Patient_Gender",
                table: "Patients",
                sql: "\"Gender\" IN ('Male', 'Female', 'Other')");
        }
    }
}
