using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Ordivo.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddTenantSlug : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Slug",
                table: "tenants",
                type: "character varying(150)",
                maxLength: 150,
                nullable: true);

            migrationBuilder.Sql("""
                UPDATE tenants
                SET "Slug" =
                    LEFT(
                        COALESCE(
                            NULLIF(
                                BTRIM(
                                    REGEXP_REPLACE(
                                        TRANSLATE(
                                            LOWER("Name"),
                                            'áàãâäéèêëíìîïóòõôöúùûüç',
                                            'aaaaaeeeeiiiiooooouuuuc'),
                                        '[^a-z0-9]+', '-', 'g'),
                                    '-'),
                                ''),
                            'tenant'),
                        140)
                    || '-' || LEFT(REPLACE("Id"::text, '-', ''), 8);
                """);

            migrationBuilder.AlterColumn<string>(
                name: "Slug",
                table: "tenants",
                type: "character varying(150)",
                maxLength: 150,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(150)",
                oldMaxLength: 150,
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_tenants_Slug",
                table: "tenants",
                column: "Slug",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_tenants_Slug",
                table: "tenants");

            migrationBuilder.DropColumn(
                name: "Slug",
                table: "tenants");
        }
    }
}
