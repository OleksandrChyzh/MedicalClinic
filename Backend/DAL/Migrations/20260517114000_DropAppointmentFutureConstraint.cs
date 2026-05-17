using DAL.Data;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DAL.Migrations
{
    /// <inheritdoc />
    [DbContext(typeof(AppDbContext))]
    [Migration("20260517114000_DropAppointmentFutureConstraint")]
    public partial class DropAppointmentFutureConstraint : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("ALTER TABLE \"Appointments\" DROP CONSTRAINT IF EXISTS \"CHK_Appointment_Date_Future\";");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddCheckConstraint(
                name: "CHK_Appointment_Date_Future",
                table: "Appointments",
                sql: "\"AppointmentDate\" >= LOCALTIMESTAMP");
        }
    }
}
