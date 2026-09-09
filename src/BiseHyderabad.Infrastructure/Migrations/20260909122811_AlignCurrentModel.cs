using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BiseHyderabad.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AlignCurrentModel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Districts_Tenants_TenantId1",
                table: "Districts");

            migrationBuilder.DropForeignKey(
                name: "FK_Schools_Tenants_TenantId1",
                table: "Schools");

            migrationBuilder.DropForeignKey(
                name: "FK_Users_Tenants_TenantId1",
                table: "Users");

            migrationBuilder.DropIndex(
                name: "IX_Users_TenantId1",
                table: "Users");

            migrationBuilder.DropIndex(
                name: "IX_Schools_TenantId1",
                table: "Schools");

            migrationBuilder.DropIndex(
                name: "IX_Districts_TenantId1",
                table: "Districts");

            migrationBuilder.DropColumn(
                name: "TenantId1",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "TenantId1",
                table: "Schools");

            migrationBuilder.DropColumn(
                name: "TenantId1",
                table: "Districts");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "TenantId1",
                table: "Users",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "TenantId1",
                table: "Schools",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "TenantId1",
                table: "Districts",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Users_TenantId1",
                table: "Users",
                column: "TenantId1");

            migrationBuilder.CreateIndex(
                name: "IX_Schools_TenantId1",
                table: "Schools",
                column: "TenantId1");

            migrationBuilder.CreateIndex(
                name: "IX_Districts_TenantId1",
                table: "Districts",
                column: "TenantId1");

            migrationBuilder.AddForeignKey(
                name: "FK_Districts_Tenants_TenantId1",
                table: "Districts",
                column: "TenantId1",
                principalTable: "Tenants",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Schools_Tenants_TenantId1",
                table: "Schools",
                column: "TenantId1",
                principalTable: "Tenants",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Users_Tenants_TenantId1",
                table: "Users",
                column: "TenantId1",
                principalTable: "Tenants",
                principalColumn: "Id");
        }
    }
}
