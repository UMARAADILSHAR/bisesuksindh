using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BiseSukkur.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddInvoicePaymentReconciliation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Invoices_SchoolId",
                table: "Invoices");

            migrationBuilder.AddColumn<string>(
                name: "PaymentMethod",
                table: "Invoices",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "PaymentReceivedAt",
                table: "Invoices",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PaymentReference",
                table: "Invoices",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PaymentVerifiedByUsername",
                table: "Invoices",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Invoices_PaymentReference",
                table: "Invoices",
                column: "PaymentReference",
                unique: true,
                filter: "[PaymentReference] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Invoices_SchoolId_AcademicYearId_Status",
                table: "Invoices",
                columns: new[] { "SchoolId", "AcademicYearId", "Status" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Invoices_PaymentReference",
                table: "Invoices");

            migrationBuilder.DropIndex(
                name: "IX_Invoices_SchoolId_AcademicYearId_Status",
                table: "Invoices");

            migrationBuilder.DropColumn(
                name: "PaymentMethod",
                table: "Invoices");

            migrationBuilder.DropColumn(
                name: "PaymentReceivedAt",
                table: "Invoices");

            migrationBuilder.DropColumn(
                name: "PaymentReference",
                table: "Invoices");

            migrationBuilder.DropColumn(
                name: "PaymentVerifiedByUsername",
                table: "Invoices");

            migrationBuilder.CreateIndex(
                name: "IX_Invoices_SchoolId",
                table: "Invoices",
                column: "SchoolId");
        }
    }
}
