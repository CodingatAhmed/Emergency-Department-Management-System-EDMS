using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EDMS.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialSupabaseSchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AllergyRegistries",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    AllergenName = table.Column<string>(type: "text", nullable: false),
                    ConflictingDrugs = table.Column<string[]>(type: "text[]", nullable: false),
                    Severity = table.Column<string>(type: "text", nullable: false),
                    Category = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AllergyRegistries", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Appointments",
                columns: table => new
                {
                    AppointmentId = table.Column<Guid>(type: "uuid", nullable: false),
                    PatientId = table.Column<Guid>(type: "uuid", nullable: false),
                    QRCode = table.Column<string>(type: "text", nullable: false),
                    ScheduledTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Department = table.Column<string>(type: "text", nullable: false),
                    DoctorName = table.Column<string>(type: "text", nullable: false),
                    Status = table.Column<string>(type: "text", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Appointments", x => x.AppointmentId);
                });

            migrationBuilder.CreateTable(
                name: "Encounters",
                columns: table => new
                {
                    EncounterId = table.Column<Guid>(type: "uuid", nullable: false),
                    PatientId = table.Column<Guid>(type: "uuid", nullable: false),
                    QueueModelId = table.Column<Guid>(type: "uuid", nullable: true),
                    ResourceId = table.Column<Guid>(type: "uuid", nullable: true),
                    AppointmentId = table.Column<Guid>(type: "uuid", nullable: true),
                    EncounterType = table.Column<string>(type: "text", nullable: false),
                    ArrivalTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ServiceStartTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DischargeTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    TriageCategory = table.Column<int>(type: "integer", nullable: true),
                    EstimatedWaitMin = table.Column<decimal>(type: "numeric", nullable: true),
                    ActualWaitMin = table.Column<decimal>(type: "numeric", nullable: true),
                    CurrentState = table.Column<int>(type: "integer", nullable: false),
                    FinalDisposition = table.Column<int>(type: "integer", nullable: true),
                    AssignedRoom = table.Column<string>(type: "text", nullable: true),
                    AssignedDoctorId = table.Column<string>(type: "text", nullable: true),
                    BloodPressureSystolic = table.Column<float>(type: "real", nullable: true),
                    BloodPressureDiastolic = table.Column<float>(type: "real", nullable: true),
                    HeartRate = table.Column<float>(type: "real", nullable: true),
                    Temperature = table.Column<float>(type: "real", nullable: true),
                    SpO2 = table.Column<float>(type: "real", nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Encounters", x => x.EncounterId);
                });

            migrationBuilder.CreateTable(
                name: "Patients",
                columns: table => new
                {
                    PatientId = table.Column<Guid>(type: "uuid", nullable: false),
                    FirstName = table.Column<string>(type: "text", nullable: false),
                    LastName = table.Column<string>(type: "text", nullable: false),
                    DateOfBirth = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    MRN = table.Column<string>(type: "text", nullable: false),
                    ContactNumber = table.Column<string>(type: "text", nullable: true),
                    Email = table.Column<string>(type: "text", nullable: true),
                    Gender = table.Column<string>(type: "text", nullable: false),
                    AllergiesList = table.Column<string>(type: "text", nullable: false),
                    ActiveProblems = table.Column<string>(type: "text", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    IsTemporary = table.Column<bool>(type: "boolean", nullable: false),
                    IsJohnDoe = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Patients", x => x.PatientId);
                });

            migrationBuilder.CreateTable(
                name: "QueueAlerts",
                columns: table => new
                {
                    AlertId = table.Column<Guid>(type: "uuid", nullable: false),
                    SnapshotId = table.Column<Guid>(type: "uuid", nullable: false),
                    EncounterId = table.Column<Guid>(type: "uuid", nullable: true),
                    AlertType = table.Column<string>(type: "text", nullable: false),
                    ThresholdMinutes = table.Column<double>(type: "double precision", nullable: false),
                    ActualWqHours = table.Column<double>(type: "double precision", nullable: false),
                    Severity = table.Column<string>(type: "text", nullable: false),
                    Message = table.Column<string>(type: "text", nullable: true),
                    TriggeredAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ResolvedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsResolved = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_QueueAlerts", x => x.AlertId);
                });

            migrationBuilder.CreateTable(
                name: "QueueModels",
                columns: table => new
                {
                    ModelId = table.Column<Guid>(type: "uuid", nullable: false),
                    ModelType = table.Column<string>(type: "text", nullable: false),
                    ArrivalDistribution = table.Column<string>(type: "text", nullable: false),
                    ServiceDistribution = table.Column<string>(type: "text", nullable: false),
                    DefaultSigmaS2 = table.Column<double>(type: "double precision", nullable: true),
                    DefaultSigmaA2 = table.Column<double>(type: "double precision", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_QueueModels", x => x.ModelId);
                });

            migrationBuilder.CreateTable(
                name: "QueueSnapshots",
                columns: table => new
                {
                    SnapshotId = table.Column<Guid>(type: "uuid", nullable: false),
                    EncounterId = table.Column<Guid>(type: "uuid", nullable: true),
                    ModelId = table.Column<Guid>(type: "uuid", nullable: false),
                    Lambda = table.Column<double>(type: "double precision", nullable: false),
                    Mu = table.Column<double>(type: "double precision", nullable: false),
                    SigmaS2 = table.Column<double>(type: "double precision", nullable: true),
                    SigmaA2 = table.Column<double>(type: "double precision", nullable: true),
                    Rho = table.Column<double>(type: "double precision", nullable: false),
                    Lq = table.Column<double>(type: "double precision", nullable: false),
                    Wq = table.Column<double>(type: "double precision", nullable: false),
                    L = table.Column<double>(type: "double precision", nullable: false),
                    W = table.Column<double>(type: "double precision", nullable: false),
                    Cv = table.Column<double>(type: "double precision", nullable: true),
                    CaSquared = table.Column<double>(type: "double precision", nullable: true),
                    CsSquared = table.Column<double>(type: "double precision", nullable: true),
                    ComputedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_QueueSnapshots", x => x.SnapshotId);
                });

            migrationBuilder.CreateTable(
                name: "Resources",
                columns: table => new
                {
                    ResourceId = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    ResourceType = table.Column<string>(type: "text", nullable: false),
                    Status = table.Column<string>(type: "text", nullable: false),
                    Department = table.Column<string>(type: "text", nullable: true),
                    OccupiedSince = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    AvailableFrom = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Resources", x => x.ResourceId);
                });

            migrationBuilder.CreateTable(
                name: "StaffUsers",
                columns: table => new
                {
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    Username = table.Column<string>(type: "text", nullable: false),
                    PasswordHash = table.Column<string>(type: "text", nullable: false),
                    Role = table.Column<int>(type: "integer", nullable: false),
                    DepartmentId = table.Column<string>(type: "text", nullable: true),
                    FullName = table.Column<string>(type: "text", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    LastLoginAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StaffUsers", x => x.UserId);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AllergyRegistries_AllergenName",
                table: "AllergyRegistries",
                column: "AllergenName");

            migrationBuilder.CreateIndex(
                name: "IX_Appointments_PatientId",
                table: "Appointments",
                column: "PatientId");

            migrationBuilder.CreateIndex(
                name: "IX_Appointments_QRCode",
                table: "Appointments",
                column: "QRCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Appointments_ScheduledTime",
                table: "Appointments",
                column: "ScheduledTime");

            migrationBuilder.CreateIndex(
                name: "IX_Encounters_ArrivalTime",
                table: "Encounters",
                column: "ArrivalTime");

            migrationBuilder.CreateIndex(
                name: "IX_Encounters_PatientId",
                table: "Encounters",
                column: "PatientId");

            migrationBuilder.CreateIndex(
                name: "IX_Patients_MRN",
                table: "Patients",
                column: "MRN",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_QueueAlerts_AlertType_TriggeredAt",
                table: "QueueAlerts",
                columns: new[] { "AlertType", "TriggeredAt" });

            migrationBuilder.CreateIndex(
                name: "IX_QueueModels_ModelType",
                table: "QueueModels",
                column: "ModelType",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_QueueSnapshots_ModelId_ComputedAtUtc",
                table: "QueueSnapshots",
                columns: new[] { "ModelId", "ComputedAtUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_Resources_ResourceType",
                table: "Resources",
                column: "ResourceType");

            migrationBuilder.CreateIndex(
                name: "IX_Resources_Status",
                table: "Resources",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_StaffUsers_Username",
                table: "StaffUsers",
                column: "Username",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AllergyRegistries");

            migrationBuilder.DropTable(
                name: "Appointments");

            migrationBuilder.DropTable(
                name: "Encounters");

            migrationBuilder.DropTable(
                name: "Patients");

            migrationBuilder.DropTable(
                name: "QueueAlerts");

            migrationBuilder.DropTable(
                name: "QueueModels");

            migrationBuilder.DropTable(
                name: "QueueSnapshots");

            migrationBuilder.DropTable(
                name: "Resources");

            migrationBuilder.DropTable(
                name: "StaffUsers");
        }
    }
}
