using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TEcommerceWebApi.Migrations
{
    /// <inheritdoc />
    public partial class AddOwnerEmailToTenant : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "OwnerEmail",
                table: "Tenants",
                type: "character varying(200)",
                maxLength: 200,
                nullable: false,
                defaultValue: "");

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "RoleId",
                keyValue: new Guid("20000000-0000-0000-0000-000000000002"),
                column: "Description",
                value: "Regular store customer");

            migrationBuilder.CreateIndex(
                name: "IX_Tenants_OwnerEmail",
                table: "Tenants",
                column: "OwnerEmail");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Tenants_OwnerEmail",
                table: "Tenants");

            migrationBuilder.DropColumn(
                name: "OwnerEmail",
                table: "Tenants");

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "RoleId",
                keyValue: new Guid("20000000-0000-0000-0000-000000000002"),
                column: "Description",
                value: "Regular store shopper");
        }
    }
}
