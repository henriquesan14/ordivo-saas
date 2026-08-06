using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Ordivo.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class ExpandServiceOrders : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("CREATE SEQUENCE service_order_number_sequence START WITH 1 INCREMENT BY 1;");
            migrationBuilder.AddColumn<Guid>(
                name: "AssignedUserId",
                table: "service_orders",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Number",
                table: "service_orders",
                type: "character varying(32)",
                maxLength: 32,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "ScheduledAt",
                table: "service_orders",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "service_order_attachments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ServiceOrderId = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    UserName = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    FileName = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    ContentType = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    Size = table.Column<long>(type: "bigint", nullable: false),
                    StorageKey = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_service_order_attachments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_service_order_attachments_service_orders_ServiceOrderId",
                        column: x => x.ServiceOrderId,
                        principalTable: "service_orders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "service_order_comments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ServiceOrderId = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    UserName = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    Text = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_service_order_comments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_service_order_comments_service_orders_ServiceOrderId",
                        column: x => x.ServiceOrderId,
                        principalTable: "service_orders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "service_order_status_history",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ServiceOrderId = table.Column<Guid>(type: "uuid", nullable: false),
                    Status = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    ChangedByName = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    Note = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    ChangedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_service_order_status_history", x => x.Id);
                    table.ForeignKey(
                        name: "FK_service_order_status_history_service_orders_ServiceOrderId",
                        column: x => x.ServiceOrderId,
                        principalTable: "service_orders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.Sql("""
                UPDATE service_orders
                SET "Number" = 'OS-' || EXTRACT(YEAR FROM "CreatedAt")::int || '-' || LPAD(nextval('service_order_number_sequence')::text, 6, '0');

                INSERT INTO service_order_status_history ("Id", "ServiceOrderId", "Status", "ChangedByName", "ChangedAt")
                SELECT md5(random()::text || clock_timestamp()::text)::uuid, "Id", "Status", "CreatedByName", "CreatedAt"
                FROM service_orders;
                """);

            migrationBuilder.CreateIndex(
                name: "IX_service_orders_AssignedUserId",
                table: "service_orders",
                column: "AssignedUserId");

            migrationBuilder.CreateIndex(
                name: "IX_service_orders_Number",
                table: "service_orders",
                column: "Number",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_service_order_attachments_ServiceOrderId",
                table: "service_order_attachments",
                column: "ServiceOrderId");

            migrationBuilder.CreateIndex(
                name: "IX_service_order_comments_ServiceOrderId",
                table: "service_order_comments",
                column: "ServiceOrderId");

            migrationBuilder.CreateIndex(
                name: "IX_service_order_status_history_ServiceOrderId",
                table: "service_order_status_history",
                column: "ServiceOrderId");

            migrationBuilder.AddForeignKey(
                name: "FK_service_orders_users_AssignedUserId",
                table: "service_orders",
                column: "AssignedUserId",
                principalTable: "users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_service_orders_users_AssignedUserId",
                table: "service_orders");

            migrationBuilder.DropTable(
                name: "service_order_attachments");

            migrationBuilder.DropTable(
                name: "service_order_comments");

            migrationBuilder.DropTable(
                name: "service_order_status_history");

            migrationBuilder.DropIndex(
                name: "IX_service_orders_AssignedUserId",
                table: "service_orders");

            migrationBuilder.DropIndex(
                name: "IX_service_orders_Number",
                table: "service_orders");

            migrationBuilder.DropColumn(
                name: "AssignedUserId",
                table: "service_orders");

            migrationBuilder.DropColumn(
                name: "Number",
                table: "service_orders");

            migrationBuilder.DropColumn(
                name: "ScheduledAt",
                table: "service_orders");

            migrationBuilder.Sql("DROP SEQUENCE IF EXISTS service_order_number_sequence;");
        }
    }
}
