using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TEcommerceWebApi.Migrations
{
    /// <inheritdoc />
    public partial class AddMultiTenancySchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Users_Email",
                table: "Users");

            migrationBuilder.AddColumn<Guid>(
                name: "TenantId",
                table: "Users",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "TenantId",
                table: "Products",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "TenantId",
                table: "Orders",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "TenantId",
                table: "Categories",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateTable(
                name: "Tenants",
                columns: table => new
                {
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    StoreName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Subdomain = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    CustomDomain = table.Column<string>(type: "text", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tenants", x => x.TenantId);
                });

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "RoleId",
                keyValue: new Guid("10000000-0000-0000-0000-000000000001"),
                column: "Description",
                value: "Full store administrator access");

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "RoleId",
                keyValue: new Guid("20000000-0000-0000-0000-000000000002"),
                column: "Description",
                value: "Regular store shopper");

            migrationBuilder.CreateIndex(
                name: "IX_Users_TenantId_Email",
                table: "Users",
                columns: new[] { "TenantId", "Email" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Products_TenantId_Name",
                table: "Products",
                columns: new[] { "TenantId", "Name" });

            migrationBuilder.CreateIndex(
                name: "IX_Orders_TenantId",
                table: "Orders",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_Categories_TenantId_Name",
                table: "Categories",
                columns: new[] { "TenantId", "Name" });

            migrationBuilder.CreateIndex(
                name: "IX_Tenants_CustomDomain",
                table: "Tenants",
                column: "CustomDomain",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Tenants_Subdomain",
                table: "Tenants",
                column: "Subdomain",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Categories_Tenants_TenantId",
                table: "Categories",
                column: "TenantId",
                principalTable: "Tenants",
                principalColumn: "TenantId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Orders_Tenants_TenantId",
                table: "Orders",
                column: "TenantId",
                principalTable: "Tenants",
                principalColumn: "TenantId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Products_Tenants_TenantId",
                table: "Products",
                column: "TenantId",
                principalTable: "Tenants",
                principalColumn: "TenantId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Users_Tenants_TenantId",
                table: "Users",
                column: "TenantId",
                principalTable: "Tenants",
                principalColumn: "TenantId",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Categories_Tenants_TenantId",
                table: "Categories");

            migrationBuilder.DropForeignKey(
                name: "FK_Orders_Tenants_TenantId",
                table: "Orders");

            migrationBuilder.DropForeignKey(
                name: "FK_Products_Tenants_TenantId",
                table: "Products");

            migrationBuilder.DropForeignKey(
                name: "FK_Users_Tenants_TenantId",
                table: "Users");

            migrationBuilder.DropTable(
                name: "Tenants");

            migrationBuilder.DropIndex(
                name: "IX_Users_TenantId_Email",
                table: "Users");

            migrationBuilder.DropIndex(
                name: "IX_Products_TenantId_Name",
                table: "Products");

            migrationBuilder.DropIndex(
                name: "IX_Orders_TenantId",
                table: "Orders");

            migrationBuilder.DropIndex(
                name: "IX_Categories_TenantId_Name",
                table: "Categories");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "Categories");

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "RoleId",
                keyValue: new Guid("10000000-0000-0000-0000-000000000001"),
                column: "Description",
                value: "Full system access");

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "RoleId",
                keyValue: new Guid("20000000-0000-0000-0000-000000000002"),
                column: "Description",
                value: "Regular store customer");

            migrationBuilder.CreateIndex(
                name: "IX_Users_Email",
                table: "Users",
                column: "Email",
                unique: true);
        }
    }
}
