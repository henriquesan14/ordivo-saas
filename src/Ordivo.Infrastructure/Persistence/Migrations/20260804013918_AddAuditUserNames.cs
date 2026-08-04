using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Ordivo.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddAuditUserNames : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CreatedByName",
                table: "users",
                type: "character varying(120)",
                maxLength: 120,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "UpdatedByName",
                table: "users",
                type: "character varying(120)",
                maxLength: 120,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CreatedByName",
                table: "tenants",
                type: "character varying(120)",
                maxLength: 120,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "UpdatedByName",
                table: "tenants",
                type: "character varying(120)",
                maxLength: 120,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CreatedByName",
                table: "service_orders",
                type: "character varying(120)",
                maxLength: 120,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "UpdatedByName",
                table: "service_orders",
                type: "character varying(120)",
                maxLength: 120,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CreatedByName",
                table: "customers",
                type: "character varying(120)",
                maxLength: 120,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "UpdatedByName",
                table: "customers",
                type: "character varying(120)",
                maxLength: 120,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CreatedByName",
                table: "users");

            migrationBuilder.DropColumn(
                name: "UpdatedByName",
                table: "users");

            migrationBuilder.DropColumn(
                name: "CreatedByName",
                table: "tenants");

            migrationBuilder.DropColumn(
                name: "UpdatedByName",
                table: "tenants");

            migrationBuilder.DropColumn(
                name: "CreatedByName",
                table: "service_orders");

            migrationBuilder.DropColumn(
                name: "UpdatedByName",
                table: "service_orders");

            migrationBuilder.DropColumn(
                name: "CreatedByName",
                table: "customers");

            migrationBuilder.DropColumn(
                name: "UpdatedByName",
                table: "customers");
        }
    }
}
