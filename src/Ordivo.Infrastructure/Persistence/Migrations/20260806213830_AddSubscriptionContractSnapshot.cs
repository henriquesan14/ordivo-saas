using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Ordivo.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddSubscriptionContractSnapshot : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ContractCurrency",
                table: "subscriptions",
                type: "character varying(3)",
                maxLength: 3,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ContractInterval",
                table: "subscriptions",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "ContractMaxCustomers",
                table: "subscriptions",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "ContractMaxServiceOrders",
                table: "subscriptions",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "ContractMaxUsers",
                table: "subscriptions",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<decimal>(
                name: "ContractPrice",
                table: "subscriptions",
                type: "numeric(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<int>(
                name: "ContractTrialDays",
                table: "subscriptions",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "PlanCode",
                table: "subscriptions",
                type: "character varying(60)",
                maxLength: 60,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "PlanName",
                table: "subscriptions",
                type: "character varying(120)",
                maxLength: 120,
                nullable: false,
                defaultValue: "");

            migrationBuilder.Sql("""
                UPDATE subscriptions AS s
                SET "PlanName" = p."Name",
                    "PlanCode" = p."Code",
                    "ContractPrice" = p."Price",
                    "ContractCurrency" = p."Currency",
                    "ContractInterval" = p."Interval",
                    "ContractTrialDays" = p."TrialDays",
                    "ContractMaxUsers" = p."MaxUsers",
                    "ContractMaxCustomers" = p."MaxCustomers",
                    "ContractMaxServiceOrders" = p."MaxServiceOrders"
                FROM plans AS p
                WHERE s."PlanId" = p."Id";
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ContractCurrency",
                table: "subscriptions");

            migrationBuilder.DropColumn(
                name: "ContractInterval",
                table: "subscriptions");

            migrationBuilder.DropColumn(
                name: "ContractMaxCustomers",
                table: "subscriptions");

            migrationBuilder.DropColumn(
                name: "ContractMaxServiceOrders",
                table: "subscriptions");

            migrationBuilder.DropColumn(
                name: "ContractMaxUsers",
                table: "subscriptions");

            migrationBuilder.DropColumn(
                name: "ContractPrice",
                table: "subscriptions");

            migrationBuilder.DropColumn(
                name: "ContractTrialDays",
                table: "subscriptions");

            migrationBuilder.DropColumn(
                name: "PlanCode",
                table: "subscriptions");

            migrationBuilder.DropColumn(
                name: "PlanName",
                table: "subscriptions");
        }
    }
}
