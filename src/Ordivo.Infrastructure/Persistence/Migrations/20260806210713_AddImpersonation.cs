using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Ordivo.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddImpersonation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "impersonation_sessions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PlatformUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    TargetUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    Reason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    StartedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    ExpiresAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    EndedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    CreatedByName = table.Column<string>(type: "text", nullable: false),
                    UpdatedByName = table.Column<string>(type: "text", nullable: true),
                    Version = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_impersonation_sessions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_impersonation_sessions_platform_users_PlatformUserId",
                        column: x => x.PlatformUserId,
                        principalTable: "platform_users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_impersonation_sessions_tenants_TenantId",
                        column: x => x.TenantId,
                        principalTable: "tenants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_impersonation_sessions_users_TargetUserId",
                        column: x => x.TargetUserId,
                        principalTable: "users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_impersonation_sessions_ExpiresAt",
                table: "impersonation_sessions",
                column: "ExpiresAt");

            migrationBuilder.CreateIndex(
                name: "IX_impersonation_sessions_PlatformUserId_EndedAt",
                table: "impersonation_sessions",
                columns: new[] { "PlatformUserId", "EndedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_impersonation_sessions_TargetUserId",
                table: "impersonation_sessions",
                column: "TargetUserId");

            migrationBuilder.CreateIndex(
                name: "IX_impersonation_sessions_TenantId",
                table: "impersonation_sessions",
                column: "TenantId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "impersonation_sessions");
        }
    }
}
