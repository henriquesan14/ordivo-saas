using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Ordivo.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddIdentitySecurity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "EmailVerifiedAt",
                table: "users",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "FamilyId",
                table: "auth_sessions",
                type: "uuid",
                nullable: true);

            migrationBuilder.Sql("""
                UPDATE users SET "EmailVerifiedAt" = "CreatedAt" WHERE "EmailVerifiedAt" IS NULL;
                UPDATE auth_sessions SET "FamilyId" = "Id" WHERE "FamilyId" IS NULL;
                """);

            migrationBuilder.AlterColumn<Guid>(
                name: "FamilyId",
                table: "auth_sessions",
                type: "uuid",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);

            migrationBuilder.CreateTable(
                name: "identity_tokens",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    Email = table.Column<string>(type: "character varying(254)", maxLength: 254, nullable: false),
                    Type = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    TokenHash = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    ExpiresAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    ConsumedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    CreatedByName = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    UpdatedByName = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_identity_tokens", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_auth_sessions_FamilyId",
                table: "auth_sessions",
                column: "FamilyId");

            migrationBuilder.CreateIndex(
                name: "IX_identity_tokens_TokenHash",
                table: "identity_tokens",
                column: "TokenHash",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_identity_tokens_UserId_Type",
                table: "identity_tokens",
                columns: new[] { "UserId", "Type" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "identity_tokens");

            migrationBuilder.DropIndex(
                name: "IX_auth_sessions_FamilyId",
                table: "auth_sessions");

            migrationBuilder.DropColumn(
                name: "EmailVerifiedAt",
                table: "users");

            migrationBuilder.DropColumn(
                name: "FamilyId",
                table: "auth_sessions");
        }
    }
}
