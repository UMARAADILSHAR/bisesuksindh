using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BiseHyderabad.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddMultiTenantFoundation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "TenantId",
                table: "WindowOverrides",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "TenantId",
                table: "Users",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "TenantId1",
                table: "Users",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "TenantId",
                table: "Tehsils",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "TenantId",
                table: "Settings",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "TenantId",
                table: "SecurityAuditEvents",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "TenantId",
                table: "SchoolSpecialPermissions",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "TenantId",
                table: "Schools",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "TenantId1",
                table: "Schools",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "TenantId",
                table: "Results",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "TenantId",
                table: "PermissionGrants",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "TenantId",
                table: "InvoiceSequences",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "TenantId",
                table: "Invoices",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "TenantId",
                table: "InvoiceItems",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "TenantId",
                table: "FeeRates",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "TenantId",
                table: "ExamSchedules",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "TenantId",
                table: "ExaminationForms",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "TenantId",
                table: "ExamCenterSchools",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "TenantId",
                table: "ExamCenters",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "TenantId",
                table: "Enrollments",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "TenantId",
                table: "Districts",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "TenantId1",
                table: "Districts",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RevocationReason",
                table: "Certificates",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Status",
                table: "Certificates",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "TenantId",
                table: "Certificates",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "TenantId",
                table: "ApprovalRequests",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "TenantId",
                table: "ActivityLogs",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "TenantId",
                table: "AcademicYears",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "Tenants",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Code = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tenants", x => x.Id);
                });

            migrationBuilder.InsertData(
                table: "Tenants",
                columns: new[] { "Id", "Name", "Code", "IsActive", "CreatedAt", "UpdatedAt" },
                values: new object[] { 1, "BISE Hyderabad", "BISE-HYD", true, DateTime.UtcNow, DateTime.UtcNow });

            foreach (var table in new[]
            {
                "AcademicYears", "ActivityLogs", "ApprovalRequests", "Certificates",
                "Districts", "Enrollments", "ExamCenters", "ExamCenterSchools", "ExamSchedules",
                "ExaminationForms", "FeeRates", "InvoiceItems", "InvoiceSequences", "Invoices",
                "PermissionGrants", "Results", "Schools", "SchoolSpecialPermissions",
                "SecurityAuditEvents", "Settings", "Tehsils", "Users", "WindowOverrides"
            })
            {
                migrationBuilder.Sql($"UPDATE [{table}] SET [TenantId] = 1;");
            }

            migrationBuilder.CreateTable(
                name: "CertificateStatusHistories",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TenantId = table.Column<int>(type: "int", nullable: false),
                    CertificateId = table.Column<int>(type: "int", nullable: false),
                    FromStatus = table.Column<int>(type: "int", nullable: false),
                    ToStatus = table.Column<int>(type: "int", nullable: false),
                    Reason = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ChangedByUserId = table.Column<int>(type: "int", nullable: false),
                    ChangedByUsername = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    ChangedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CertificateStatusHistories", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CertificateStatusHistories_Certificates_CertificateId",
                        column: x => x.CertificateId,
                        principalTable: "Certificates",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CertificateStatusHistories_Tenants_TenantId",
                        column: x => x.TenantId,
                        principalTable: "Tenants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ExaminationSessions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TenantId = table.Column<int>(type: "int", nullable: false),
                    Code = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    AcademicYearId = table.Column<int>(type: "int", nullable: false),
                    ExaminationType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ClassLevel = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    RegistrationOpenAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    RegistrationCloseAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ExaminationStartAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ExaminationEndAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ResultDeclarationAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExaminationSessions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ExaminationSessions_AcademicYears_AcademicYearId",
                        column: x => x.AcademicYearId,
                        principalTable: "AcademicYears",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ExaminationSessions_Tenants_TenantId",
                        column: x => x.TenantId,
                        principalTable: "Tenants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ResultCorrectionRequests",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TenantId = table.Column<int>(type: "int", nullable: false),
                    ResultId = table.Column<int>(type: "int", nullable: false),
                    RequestNumber = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Reason = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ProposedMarksJson = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    SubmittedByUserId = table.Column<int>(type: "int", nullable: false),
                    SubmittedByUsername = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ReviewedByUserId = table.Column<int>(type: "int", nullable: true),
                    ReviewedByUsername = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ReviewComment = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SubmittedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ReviewedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    AppliedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ResultCorrectionRequests", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ResultCorrectionRequests_Results_ResultId",
                        column: x => x.ResultId,
                        principalTable: "Results",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ResultCorrectionRequests_Tenants_TenantId",
                        column: x => x.TenantId,
                        principalTable: "Tenants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ResultRecheckRequests",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TenantId = table.Column<int>(type: "int", nullable: false),
                    ResultId = table.Column<int>(type: "int", nullable: false),
                    RequestNumber = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Reason = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    SubmittedByUserId = table.Column<int>(type: "int", nullable: false),
                    SubmittedByUsername = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ReviewedByUserId = table.Column<int>(type: "int", nullable: true),
                    ReviewedByUsername = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ReviewComment = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SubmittedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ReviewedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ResultRecheckRequests", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ResultRecheckRequests_Results_ResultId",
                        column: x => x.ResultId,
                        principalTable: "Results",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ResultRecheckRequests_Tenants_TenantId",
                        column: x => x.TenantId,
                        principalTable: "Tenants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "SchoolAffiliationApplications",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TenantId = table.Column<int>(type: "int", nullable: false),
                    SchoolId = table.Column<int>(type: "int", nullable: false),
                    ApplicationNumber = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    RequestedCategory = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    RequestedLevelsJson = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    SubmittedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ReviewedByUserId = table.Column<int>(type: "int", nullable: true),
                    ReviewedByUsername = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ReviewComment = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ReviewedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ValidUntil = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SchoolAffiliationApplications", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SchoolAffiliationApplications_Schools_SchoolId",
                        column: x => x.SchoolId,
                        principalTable: "Schools",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SchoolAffiliationApplications_Tenants_TenantId",
                        column: x => x.TenantId,
                        principalTable: "Tenants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "SubjectSchemes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TenantId = table.Column<int>(type: "int", nullable: false),
                    ExaminationSessionId = table.Column<int>(type: "int", nullable: false),
                    ClassLevel = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Group = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    SubjectCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    SubjectName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    MaxMarks = table.Column<int>(type: "int", nullable: false),
                    PassingMarks = table.Column<int>(type: "int", nullable: false),
                    HasPractical = table.Column<bool>(type: "bit", nullable: false),
                    PracticalMaxMarks = table.Column<int>(type: "int", nullable: false),
                    IsCompulsory = table.Column<bool>(type: "bit", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    SortOrder = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SubjectSchemes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SubjectSchemes_ExaminationSessions_ExaminationSessionId",
                        column: x => x.ExaminationSessionId,
                        principalTable: "ExaminationSessions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SubjectSchemes_Tenants_TenantId",
                        column: x => x.TenantId,
                        principalTable: "Tenants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "SchoolInspections",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TenantId = table.Column<int>(type: "int", nullable: false),
                    AffiliationApplicationId = table.Column<long>(type: "bigint", nullable: false),
                    InspectedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    InspectorUserId = table.Column<int>(type: "int", nullable: false),
                    InspectorUsername = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Findings = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Capacity = table.Column<int>(type: "int", nullable: true),
                    Passed = table.Column<bool>(type: "bit", nullable: false),
                    AttachmentPath = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SchoolInspections", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SchoolInspections_SchoolAffiliationApplications_AffiliationApplicationId",
                        column: x => x.AffiliationApplicationId,
                        principalTable: "SchoolAffiliationApplications",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SchoolInspections_Tenants_TenantId",
                        column: x => x.TenantId,
                        principalTable: "Tenants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_WindowOverrides_TenantId",
                table: "WindowOverrides",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_Users_TenantId",
                table: "Users",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_Users_TenantId1",
                table: "Users",
                column: "TenantId1");

            migrationBuilder.CreateIndex(
                name: "IX_Tehsils_TenantId",
                table: "Tehsils",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_Settings_TenantId",
                table: "Settings",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_SecurityAuditEvents_TenantId",
                table: "SecurityAuditEvents",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_SchoolSpecialPermissions_TenantId",
                table: "SchoolSpecialPermissions",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_Schools_TenantId",
                table: "Schools",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_Schools_TenantId1",
                table: "Schools",
                column: "TenantId1");

            migrationBuilder.CreateIndex(
                name: "IX_Results_TenantId",
                table: "Results",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_PermissionGrants_TenantId",
                table: "PermissionGrants",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_InvoiceSequences_TenantId",
                table: "InvoiceSequences",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_Invoices_TenantId",
                table: "Invoices",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_InvoiceItems_TenantId",
                table: "InvoiceItems",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_FeeRates_TenantId",
                table: "FeeRates",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_ExamSchedules_TenantId",
                table: "ExamSchedules",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_ExaminationForms_TenantId",
                table: "ExaminationForms",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_ExamCenterSchools_TenantId",
                table: "ExamCenterSchools",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_ExamCenters_TenantId",
                table: "ExamCenters",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_Enrollments_TenantId",
                table: "Enrollments",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_Districts_TenantId",
                table: "Districts",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_Districts_TenantId1",
                table: "Districts",
                column: "TenantId1");

            migrationBuilder.CreateIndex(
                name: "IX_Certificates_TenantId",
                table: "Certificates",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_ApprovalRequests_TenantId",
                table: "ApprovalRequests",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_ActivityLogs_TenantId",
                table: "ActivityLogs",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_AcademicYears_TenantId",
                table: "AcademicYears",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_CertificateStatusHistories_CertificateId_ChangedAt",
                table: "CertificateStatusHistories",
                columns: new[] { "CertificateId", "ChangedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_CertificateStatusHistories_TenantId",
                table: "CertificateStatusHistories",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_ExaminationSessions_AcademicYearId_ClassLevel_Status",
                table: "ExaminationSessions",
                columns: new[] { "AcademicYearId", "ClassLevel", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_ExaminationSessions_Code",
                table: "ExaminationSessions",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ExaminationSessions_TenantId",
                table: "ExaminationSessions",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_ResultCorrectionRequests_RequestNumber",
                table: "ResultCorrectionRequests",
                column: "RequestNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ResultCorrectionRequests_ResultId_Status",
                table: "ResultCorrectionRequests",
                columns: new[] { "ResultId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_ResultCorrectionRequests_TenantId",
                table: "ResultCorrectionRequests",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_ResultRecheckRequests_RequestNumber",
                table: "ResultRecheckRequests",
                column: "RequestNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ResultRecheckRequests_ResultId_Status",
                table: "ResultRecheckRequests",
                columns: new[] { "ResultId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_ResultRecheckRequests_TenantId",
                table: "ResultRecheckRequests",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_SchoolAffiliationApplications_ApplicationNumber",
                table: "SchoolAffiliationApplications",
                column: "ApplicationNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SchoolAffiliationApplications_SchoolId_Status",
                table: "SchoolAffiliationApplications",
                columns: new[] { "SchoolId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_SchoolAffiliationApplications_TenantId",
                table: "SchoolAffiliationApplications",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_SchoolInspections_AffiliationApplicationId_InspectedAt",
                table: "SchoolInspections",
                columns: new[] { "AffiliationApplicationId", "InspectedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_SchoolInspections_TenantId",
                table: "SchoolInspections",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_SubjectSchemes_ExaminationSessionId_SubjectCode_Group",
                table: "SubjectSchemes",
                columns: new[] { "ExaminationSessionId", "SubjectCode", "Group" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SubjectSchemes_TenantId",
                table: "SubjectSchemes",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_Tenants_Code",
                table: "Tenants",
                column: "Code",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_AcademicYears_Tenants_TenantId",
                table: "AcademicYears",
                column: "TenantId",
                principalTable: "Tenants",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ActivityLogs_Tenants_TenantId",
                table: "ActivityLogs",
                column: "TenantId",
                principalTable: "Tenants",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ApprovalRequests_Tenants_TenantId",
                table: "ApprovalRequests",
                column: "TenantId",
                principalTable: "Tenants",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Certificates_Tenants_TenantId",
                table: "Certificates",
                column: "TenantId",
                principalTable: "Tenants",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Districts_Tenants_TenantId",
                table: "Districts",
                column: "TenantId",
                principalTable: "Tenants",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Districts_Tenants_TenantId1",
                table: "Districts",
                column: "TenantId1",
                principalTable: "Tenants",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Enrollments_Tenants_TenantId",
                table: "Enrollments",
                column: "TenantId",
                principalTable: "Tenants",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ExamCenters_Tenants_TenantId",
                table: "ExamCenters",
                column: "TenantId",
                principalTable: "Tenants",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ExamCenterSchools_Tenants_TenantId",
                table: "ExamCenterSchools",
                column: "TenantId",
                principalTable: "Tenants",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ExaminationForms_Tenants_TenantId",
                table: "ExaminationForms",
                column: "TenantId",
                principalTable: "Tenants",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ExamSchedules_Tenants_TenantId",
                table: "ExamSchedules",
                column: "TenantId",
                principalTable: "Tenants",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_FeeRates_Tenants_TenantId",
                table: "FeeRates",
                column: "TenantId",
                principalTable: "Tenants",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_InvoiceItems_Tenants_TenantId",
                table: "InvoiceItems",
                column: "TenantId",
                principalTable: "Tenants",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Invoices_Tenants_TenantId",
                table: "Invoices",
                column: "TenantId",
                principalTable: "Tenants",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_InvoiceSequences_Tenants_TenantId",
                table: "InvoiceSequences",
                column: "TenantId",
                principalTable: "Tenants",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_PermissionGrants_Tenants_TenantId",
                table: "PermissionGrants",
                column: "TenantId",
                principalTable: "Tenants",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Results_Tenants_TenantId",
                table: "Results",
                column: "TenantId",
                principalTable: "Tenants",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Schools_Tenants_TenantId",
                table: "Schools",
                column: "TenantId",
                principalTable: "Tenants",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Schools_Tenants_TenantId1",
                table: "Schools",
                column: "TenantId1",
                principalTable: "Tenants",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_SchoolSpecialPermissions_Tenants_TenantId",
                table: "SchoolSpecialPermissions",
                column: "TenantId",
                principalTable: "Tenants",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_SecurityAuditEvents_Tenants_TenantId",
                table: "SecurityAuditEvents",
                column: "TenantId",
                principalTable: "Tenants",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Settings_Tenants_TenantId",
                table: "Settings",
                column: "TenantId",
                principalTable: "Tenants",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Tehsils_Tenants_TenantId",
                table: "Tehsils",
                column: "TenantId",
                principalTable: "Tenants",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Users_Tenants_TenantId",
                table: "Users",
                column: "TenantId",
                principalTable: "Tenants",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Users_Tenants_TenantId1",
                table: "Users",
                column: "TenantId1",
                principalTable: "Tenants",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_WindowOverrides_Tenants_TenantId",
                table: "WindowOverrides",
                column: "TenantId",
                principalTable: "Tenants",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AcademicYears_Tenants_TenantId",
                table: "AcademicYears");

            migrationBuilder.DropForeignKey(
                name: "FK_ActivityLogs_Tenants_TenantId",
                table: "ActivityLogs");

            migrationBuilder.DropForeignKey(
                name: "FK_ApprovalRequests_Tenants_TenantId",
                table: "ApprovalRequests");

            migrationBuilder.DropForeignKey(
                name: "FK_Certificates_Tenants_TenantId",
                table: "Certificates");

            migrationBuilder.DropForeignKey(
                name: "FK_Districts_Tenants_TenantId",
                table: "Districts");

            migrationBuilder.DropForeignKey(
                name: "FK_Districts_Tenants_TenantId1",
                table: "Districts");

            migrationBuilder.DropForeignKey(
                name: "FK_Enrollments_Tenants_TenantId",
                table: "Enrollments");

            migrationBuilder.DropForeignKey(
                name: "FK_ExamCenters_Tenants_TenantId",
                table: "ExamCenters");

            migrationBuilder.DropForeignKey(
                name: "FK_ExamCenterSchools_Tenants_TenantId",
                table: "ExamCenterSchools");

            migrationBuilder.DropForeignKey(
                name: "FK_ExaminationForms_Tenants_TenantId",
                table: "ExaminationForms");

            migrationBuilder.DropForeignKey(
                name: "FK_ExamSchedules_Tenants_TenantId",
                table: "ExamSchedules");

            migrationBuilder.DropForeignKey(
                name: "FK_FeeRates_Tenants_TenantId",
                table: "FeeRates");

            migrationBuilder.DropForeignKey(
                name: "FK_InvoiceItems_Tenants_TenantId",
                table: "InvoiceItems");

            migrationBuilder.DropForeignKey(
                name: "FK_Invoices_Tenants_TenantId",
                table: "Invoices");

            migrationBuilder.DropForeignKey(
                name: "FK_InvoiceSequences_Tenants_TenantId",
                table: "InvoiceSequences");

            migrationBuilder.DropForeignKey(
                name: "FK_PermissionGrants_Tenants_TenantId",
                table: "PermissionGrants");

            migrationBuilder.DropForeignKey(
                name: "FK_Results_Tenants_TenantId",
                table: "Results");

            migrationBuilder.DropForeignKey(
                name: "FK_Schools_Tenants_TenantId",
                table: "Schools");

            migrationBuilder.DropForeignKey(
                name: "FK_Schools_Tenants_TenantId1",
                table: "Schools");

            migrationBuilder.DropForeignKey(
                name: "FK_SchoolSpecialPermissions_Tenants_TenantId",
                table: "SchoolSpecialPermissions");

            migrationBuilder.DropForeignKey(
                name: "FK_SecurityAuditEvents_Tenants_TenantId",
                table: "SecurityAuditEvents");

            migrationBuilder.DropForeignKey(
                name: "FK_Settings_Tenants_TenantId",
                table: "Settings");

            migrationBuilder.DropForeignKey(
                name: "FK_Tehsils_Tenants_TenantId",
                table: "Tehsils");

            migrationBuilder.DropForeignKey(
                name: "FK_Users_Tenants_TenantId",
                table: "Users");

            migrationBuilder.DropForeignKey(
                name: "FK_Users_Tenants_TenantId1",
                table: "Users");

            migrationBuilder.DropForeignKey(
                name: "FK_WindowOverrides_Tenants_TenantId",
                table: "WindowOverrides");

            migrationBuilder.DropTable(
                name: "CertificateStatusHistories");

            migrationBuilder.DropTable(
                name: "ResultCorrectionRequests");

            migrationBuilder.DropTable(
                name: "ResultRecheckRequests");

            migrationBuilder.DropTable(
                name: "SchoolInspections");

            migrationBuilder.DropTable(
                name: "SubjectSchemes");

            migrationBuilder.DropTable(
                name: "SchoolAffiliationApplications");

            migrationBuilder.DropTable(
                name: "ExaminationSessions");

            migrationBuilder.DropTable(
                name: "Tenants");

            migrationBuilder.DropIndex(
                name: "IX_WindowOverrides_TenantId",
                table: "WindowOverrides");

            migrationBuilder.DropIndex(
                name: "IX_Users_TenantId",
                table: "Users");

            migrationBuilder.DropIndex(
                name: "IX_Users_TenantId1",
                table: "Users");

            migrationBuilder.DropIndex(
                name: "IX_Tehsils_TenantId",
                table: "Tehsils");

            migrationBuilder.DropIndex(
                name: "IX_Settings_TenantId",
                table: "Settings");

            migrationBuilder.DropIndex(
                name: "IX_SecurityAuditEvents_TenantId",
                table: "SecurityAuditEvents");

            migrationBuilder.DropIndex(
                name: "IX_SchoolSpecialPermissions_TenantId",
                table: "SchoolSpecialPermissions");

            migrationBuilder.DropIndex(
                name: "IX_Schools_TenantId",
                table: "Schools");

            migrationBuilder.DropIndex(
                name: "IX_Schools_TenantId1",
                table: "Schools");

            migrationBuilder.DropIndex(
                name: "IX_Results_TenantId",
                table: "Results");

            migrationBuilder.DropIndex(
                name: "IX_PermissionGrants_TenantId",
                table: "PermissionGrants");

            migrationBuilder.DropIndex(
                name: "IX_InvoiceSequences_TenantId",
                table: "InvoiceSequences");

            migrationBuilder.DropIndex(
                name: "IX_Invoices_TenantId",
                table: "Invoices");

            migrationBuilder.DropIndex(
                name: "IX_InvoiceItems_TenantId",
                table: "InvoiceItems");

            migrationBuilder.DropIndex(
                name: "IX_FeeRates_TenantId",
                table: "FeeRates");

            migrationBuilder.DropIndex(
                name: "IX_ExamSchedules_TenantId",
                table: "ExamSchedules");

            migrationBuilder.DropIndex(
                name: "IX_ExaminationForms_TenantId",
                table: "ExaminationForms");

            migrationBuilder.DropIndex(
                name: "IX_ExamCenterSchools_TenantId",
                table: "ExamCenterSchools");

            migrationBuilder.DropIndex(
                name: "IX_ExamCenters_TenantId",
                table: "ExamCenters");

            migrationBuilder.DropIndex(
                name: "IX_Enrollments_TenantId",
                table: "Enrollments");

            migrationBuilder.DropIndex(
                name: "IX_Districts_TenantId",
                table: "Districts");

            migrationBuilder.DropIndex(
                name: "IX_Districts_TenantId1",
                table: "Districts");

            migrationBuilder.DropIndex(
                name: "IX_Certificates_TenantId",
                table: "Certificates");

            migrationBuilder.DropIndex(
                name: "IX_ApprovalRequests_TenantId",
                table: "ApprovalRequests");

            migrationBuilder.DropIndex(
                name: "IX_ActivityLogs_TenantId",
                table: "ActivityLogs");

            migrationBuilder.DropIndex(
                name: "IX_AcademicYears_TenantId",
                table: "AcademicYears");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "WindowOverrides");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "TenantId1",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "Tehsils");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "Settings");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "SecurityAuditEvents");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "SchoolSpecialPermissions");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "Schools");

            migrationBuilder.DropColumn(
                name: "TenantId1",
                table: "Schools");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "Results");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "PermissionGrants");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "InvoiceSequences");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "Invoices");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "InvoiceItems");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "FeeRates");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "ExamSchedules");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "ExaminationForms");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "ExamCenterSchools");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "ExamCenters");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "Enrollments");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "Districts");

            migrationBuilder.DropColumn(
                name: "TenantId1",
                table: "Districts");

            migrationBuilder.DropColumn(
                name: "RevocationReason",
                table: "Certificates");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "Certificates");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "Certificates");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "ApprovalRequests");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "ActivityLogs");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "AcademicYears");
        }
    }
}
