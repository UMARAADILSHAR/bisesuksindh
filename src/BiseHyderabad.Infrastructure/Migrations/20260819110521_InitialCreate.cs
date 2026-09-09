using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BiseHyderabad.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AcademicYears",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    YearName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    StartDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EndDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    IsEnrollmentOpen = table.Column<bool>(type: "bit", nullable: false),
                    EnrollmentOpenStart = table.Column<DateTime>(type: "datetime2", nullable: true),
                    EnrollmentOpenEnd = table.Column<DateTime>(type: "datetime2", nullable: true),
                    EnrollmentGraceEnd = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsEnrollmentGraceEnabled = table.Column<bool>(type: "bit", nullable: false),
                    EnrollmentLateFeeType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    EnrollmentLateFeeAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    IsExamOpen = table.Column<bool>(type: "bit", nullable: false),
                    ExamOpenStart = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ExamOpenEnd = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ExamGraceEnd = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsExamGraceEnabled = table.Column<bool>(type: "bit", nullable: false),
                    ExamLateFeeType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ExamLateFeeAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    ExamTimetableAnnounced = table.Column<bool>(type: "bit", nullable: false),
                    ResultsDeclared = table.Column<bool>(type: "bit", nullable: false),
                    PromotionDone = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AcademicYears", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ActivityLogs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    LogName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SubjectType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SubjectId = table.Column<int>(type: "int", nullable: true),
                    UserId = table.Column<int>(type: "int", nullable: true),
                    CausedByUsername = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PropertiesJson = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ActivityLogs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Districts",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ShortCode = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Districts", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "InvoiceSequences",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SequenceType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    AcademicYearId = table.Column<int>(type: "int", nullable: true),
                    SchoolId = table.Column<int>(type: "int", nullable: true),
                    DistrictCode = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Zone = table.Column<int>(type: "int", nullable: true),
                    GroupCode = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Prefix = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LastNumber = table.Column<int>(type: "int", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InvoiceSequences", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Settings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Key = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Value = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Settings", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "WindowOverrides",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ScopeType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ScopeId = table.Column<int>(type: "int", nullable: false),
                    WindowType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    NormalStart = table.Column<DateTime>(type: "datetime2", nullable: true),
                    NormalEnd = table.Column<DateTime>(type: "datetime2", nullable: true),
                    GraceEnd = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsGraceEnabled = table.Column<bool>(type: "bit", nullable: false),
                    LateFeeType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    LateFeeAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Reason = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WindowOverrides", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ExamSchedules",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AcademicYearId = table.Column<int>(type: "int", nullable: false),
                    ClassLevel = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Group = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SubjectName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ExamDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Session = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PaperDurationMinutes = table.Column<int>(type: "int", nullable: false),
                    MaxMarks = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExamSchedules", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ExamSchedules_AcademicYears_AcademicYearId",
                        column: x => x.AcademicYearId,
                        principalTable: "AcademicYears",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "FeeRates",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AcademicYearId = table.Column<int>(type: "int", nullable: false),
                    FeeType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ClassLevel = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    GroupName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    StudentType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FeeSlab = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    StandardFee = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    LateFee = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FeeRates", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FeeRates_AcademicYears_AcademicYearId",
                        column: x => x.AcademicYearId,
                        principalTable: "AcademicYears",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ExamCenters",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CenterCode = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DistrictId = table.Column<int>(type: "int", nullable: false),
                    SuperintendentName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SuperintendentPhone = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Capacity = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExamCenters", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ExamCenters_Districts_DistrictId",
                        column: x => x.DistrictId,
                        principalTable: "Districts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Tehsils",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DistrictId = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tehsils", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Tehsils_Districts_DistrictId",
                        column: x => x.DistrictId,
                        principalTable: "Districts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Schools",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SemisCode = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Code = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DistrictId = table.Column<int>(type: "int", nullable: false),
                    TehsilId = table.Column<int>(type: "int", nullable: true),
                    Type = table.Column<int>(type: "int", nullable: false),
                    Zone = table.Column<int>(type: "int", nullable: false),
                    Address = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ContactNumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    HeadName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    HeadPhone = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    HeadCnic = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    HeadEmail = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AllowedLevelsJson = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Schools", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Schools_Districts_DistrictId",
                        column: x => x.DistrictId,
                        principalTable: "Districts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Schools_Tehsils_TehsilId",
                        column: x => x.TehsilId,
                        principalTable: "Tehsils",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "ExamCenterSchools",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ExamCenterId = table.Column<int>(type: "int", nullable: false),
                    SchoolId = table.Column<int>(type: "int", nullable: false),
                    AssignedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExamCenterSchools", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ExamCenterSchools_ExamCenters_ExamCenterId",
                        column: x => x.ExamCenterId,
                        principalTable: "ExamCenters",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ExamCenterSchools_Schools_SchoolId",
                        column: x => x.SchoolId,
                        principalTable: "Schools",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Invoices",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Uuid = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SchoolId = table.Column<int>(type: "int", nullable: false),
                    AcademicYearId = table.Column<int>(type: "int", nullable: false),
                    InvoiceType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    InvoiceNumber = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ClassGroupStudentType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    StudentCount = table.Column<int>(type: "int", nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    Phase = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    RejectionReason = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    GeneratedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    VerifiedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CancelledAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Invoices", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Invoices_AcademicYears_AcademicYearId",
                        column: x => x.AcademicYearId,
                        principalTable: "AcademicYears",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Invoices_Schools_SchoolId",
                        column: x => x.SchoolId,
                        principalTable: "Schools",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "SchoolSpecialPermissions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SchoolId = table.Column<int>(type: "int", nullable: false),
                    PermissionType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Reason = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ExpiryDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SchoolSpecialPermissions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SchoolSpecialPermissions_Schools_SchoolId",
                        column: x => x.SchoolId,
                        principalTable: "Schools",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Username = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    PasswordHash = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Email = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Role = table.Column<int>(type: "int", nullable: false),
                    SchoolId = table.Column<int>(type: "int", nullable: true),
                    DistrictId = table.Column<int>(type: "int", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    MustChangePassword = table.Column<bool>(type: "bit", nullable: false),
                    FailedLoginAttempts = table.Column<int>(type: "int", nullable: false),
                    LockoutEnd = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastLoginAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Users_Districts_DistrictId",
                        column: x => x.DistrictId,
                        principalTable: "Districts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_Users_Schools_SchoolId",
                        column: x => x.SchoolId,
                        principalTable: "Schools",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "Enrollments",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Uuid = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SchoolId = table.Column<int>(type: "int", nullable: false),
                    AcademicYearId = table.Column<int>(type: "int", nullable: false),
                    StudentName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FatherName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FatherCnic = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Surname = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    GrNumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IdentificationMark = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Cnic = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    DateOfBirth = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DateOfBirthWords = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Gender = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Medium = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Religion = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Nationality = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Address = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    MobileNumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PhotoPath = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ClassLevel = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Group = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    StudentType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SubjectsJson = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    AdmissionDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    BoardRegNo = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    EligibilityNo = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PreviousBoard = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    BoardPassingDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    EnrollmentNumber = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    EnrollmentNumberAllottedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    InvoiceId = table.Column<int>(type: "int", nullable: true),
                    ChallanStatus = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Status = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Enrollments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Enrollments_AcademicYears_AcademicYearId",
                        column: x => x.AcademicYearId,
                        principalTable: "AcademicYears",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Enrollments_Invoices_InvoiceId",
                        column: x => x.InvoiceId,
                        principalTable: "Invoices",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_Enrollments_Schools_SchoolId",
                        column: x => x.SchoolId,
                        principalTable: "Schools",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ExaminationForms",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Uuid = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EnrollmentId = table.Column<int>(type: "int", nullable: false),
                    SchoolId = table.Column<int>(type: "int", nullable: false),
                    AcademicYearId = table.Column<int>(type: "int", nullable: false),
                    ClassLevel = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Group = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    StudentType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PreviousSeatNumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PreviousYear = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RollNumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ExamCenterId = table.Column<int>(type: "int", nullable: true),
                    SubjectsJson = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExaminationForms", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ExaminationForms_AcademicYears_AcademicYearId",
                        column: x => x.AcademicYearId,
                        principalTable: "AcademicYears",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ExaminationForms_Enrollments_EnrollmentId",
                        column: x => x.EnrollmentId,
                        principalTable: "Enrollments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ExaminationForms_ExamCenters_ExamCenterId",
                        column: x => x.ExamCenterId,
                        principalTable: "ExamCenters",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_ExaminationForms_Schools_SchoolId",
                        column: x => x.SchoolId,
                        principalTable: "Schools",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "InvoiceItems",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    InvoiceId = table.Column<int>(type: "int", nullable: false),
                    EnrollmentId = table.Column<int>(type: "int", nullable: false),
                    IsIncluded = table.Column<bool>(type: "bit", nullable: false),
                    FeeAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    InvoiceTypeSnapshot = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InvoiceItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_InvoiceItems_Enrollments_EnrollmentId",
                        column: x => x.EnrollmentId,
                        principalTable: "Enrollments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_InvoiceItems_Invoices_InvoiceId",
                        column: x => x.InvoiceId,
                        principalTable: "Invoices",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Results",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ExaminationFormId = table.Column<int>(type: "int", nullable: false),
                    MarksJson = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TotalObtained = table.Column<int>(type: "int", nullable: false),
                    TotalMaxMarks = table.Column<int>(type: "int", nullable: false),
                    Percentage = table.Column<decimal>(type: "decimal(5,2)", nullable: false),
                    Grade = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsPassed = table.Column<bool>(type: "bit", nullable: false),
                    Remarks = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DeclaredAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Results", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Results_ExaminationForms_ExaminationFormId",
                        column: x => x.ExaminationFormId,
                        principalTable: "ExaminationForms",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Certificates",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Uuid = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ResultId = table.Column<int>(type: "int", nullable: false),
                    CertificateNumber = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    VerificationToken = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    StudentName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FatherName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    EnrollmentNumber = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    RollNumber = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SchoolName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ClassLevel = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Group = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Grade = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TotalMarksObtained = table.Column<int>(type: "int", nullable: false),
                    TotalMaxMarks = table.Column<int>(type: "int", nullable: false),
                    IssueDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsRevoked = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Certificates", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Certificates_Results_ResultId",
                        column: x => x.ResultId,
                        principalTable: "Results",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Certificates_CertificateNumber",
                table: "Certificates",
                column: "CertificateNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Certificates_ResultId",
                table: "Certificates",
                column: "ResultId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Certificates_VerificationToken",
                table: "Certificates",
                column: "VerificationToken",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Enrollments_AcademicYearId",
                table: "Enrollments",
                column: "AcademicYearId");

            migrationBuilder.CreateIndex(
                name: "IX_Enrollments_EnrollmentNumber",
                table: "Enrollments",
                column: "EnrollmentNumber",
                unique: true,
                filter: "[EnrollmentNumber] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Enrollments_InvoiceId",
                table: "Enrollments",
                column: "InvoiceId");

            migrationBuilder.CreateIndex(
                name: "IX_Enrollments_SchoolId_AcademicYearId_Cnic",
                table: "Enrollments",
                columns: new[] { "SchoolId", "AcademicYearId", "Cnic" });

            migrationBuilder.CreateIndex(
                name: "IX_ExamCenters_DistrictId",
                table: "ExamCenters",
                column: "DistrictId");

            migrationBuilder.CreateIndex(
                name: "IX_ExamCenterSchools_ExamCenterId",
                table: "ExamCenterSchools",
                column: "ExamCenterId");

            migrationBuilder.CreateIndex(
                name: "IX_ExamCenterSchools_SchoolId",
                table: "ExamCenterSchools",
                column: "SchoolId");

            migrationBuilder.CreateIndex(
                name: "IX_ExaminationForms_AcademicYearId",
                table: "ExaminationForms",
                column: "AcademicYearId");

            migrationBuilder.CreateIndex(
                name: "IX_ExaminationForms_EnrollmentId",
                table: "ExaminationForms",
                column: "EnrollmentId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ExaminationForms_ExamCenterId",
                table: "ExaminationForms",
                column: "ExamCenterId");

            migrationBuilder.CreateIndex(
                name: "IX_ExaminationForms_SchoolId",
                table: "ExaminationForms",
                column: "SchoolId");

            migrationBuilder.CreateIndex(
                name: "IX_ExamSchedules_AcademicYearId",
                table: "ExamSchedules",
                column: "AcademicYearId");

            migrationBuilder.CreateIndex(
                name: "IX_FeeRates_AcademicYearId",
                table: "FeeRates",
                column: "AcademicYearId");

            migrationBuilder.CreateIndex(
                name: "IX_InvoiceItems_EnrollmentId",
                table: "InvoiceItems",
                column: "EnrollmentId");

            migrationBuilder.CreateIndex(
                name: "IX_InvoiceItems_InvoiceId",
                table: "InvoiceItems",
                column: "InvoiceId");

            migrationBuilder.CreateIndex(
                name: "IX_Invoices_AcademicYearId",
                table: "Invoices",
                column: "AcademicYearId");

            migrationBuilder.CreateIndex(
                name: "IX_Invoices_InvoiceNumber",
                table: "Invoices",
                column: "InvoiceNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Invoices_SchoolId",
                table: "Invoices",
                column: "SchoolId");

            migrationBuilder.CreateIndex(
                name: "IX_Results_ExaminationFormId",
                table: "Results",
                column: "ExaminationFormId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Schools_DistrictId",
                table: "Schools",
                column: "DistrictId");

            migrationBuilder.CreateIndex(
                name: "IX_Schools_SemisCode",
                table: "Schools",
                column: "SemisCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Schools_TehsilId",
                table: "Schools",
                column: "TehsilId");

            migrationBuilder.CreateIndex(
                name: "IX_SchoolSpecialPermissions_SchoolId",
                table: "SchoolSpecialPermissions",
                column: "SchoolId");

            migrationBuilder.CreateIndex(
                name: "IX_Tehsils_DistrictId",
                table: "Tehsils",
                column: "DistrictId");

            migrationBuilder.CreateIndex(
                name: "IX_Users_DistrictId",
                table: "Users",
                column: "DistrictId");

            migrationBuilder.CreateIndex(
                name: "IX_Users_SchoolId",
                table: "Users",
                column: "SchoolId");

            migrationBuilder.CreateIndex(
                name: "IX_Users_Username",
                table: "Users",
                column: "Username",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ActivityLogs");

            migrationBuilder.DropTable(
                name: "Certificates");

            migrationBuilder.DropTable(
                name: "ExamCenterSchools");

            migrationBuilder.DropTable(
                name: "ExamSchedules");

            migrationBuilder.DropTable(
                name: "FeeRates");

            migrationBuilder.DropTable(
                name: "InvoiceItems");

            migrationBuilder.DropTable(
                name: "InvoiceSequences");

            migrationBuilder.DropTable(
                name: "SchoolSpecialPermissions");

            migrationBuilder.DropTable(
                name: "Settings");

            migrationBuilder.DropTable(
                name: "Users");

            migrationBuilder.DropTable(
                name: "WindowOverrides");

            migrationBuilder.DropTable(
                name: "Results");

            migrationBuilder.DropTable(
                name: "ExaminationForms");

            migrationBuilder.DropTable(
                name: "Enrollments");

            migrationBuilder.DropTable(
                name: "ExamCenters");

            migrationBuilder.DropTable(
                name: "Invoices");

            migrationBuilder.DropTable(
                name: "AcademicYears");

            migrationBuilder.DropTable(
                name: "Schools");

            migrationBuilder.DropTable(
                name: "Tehsils");

            migrationBuilder.DropTable(
                name: "Districts");
        }
    }
}
