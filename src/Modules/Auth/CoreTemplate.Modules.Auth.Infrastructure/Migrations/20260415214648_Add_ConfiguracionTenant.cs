using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CoreTemplate.Modules.Auth.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Add_ConfiguracionTenant : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ConfiguracionesTenant",
                schema: "Auth",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    MaxSesionesSimultaneas = table.Column<int>(type: "int", nullable: true),
                    ModificadoEn = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ConfiguracionesTenant", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ConfiguracionesTenant_TenantId",
                schema: "Auth",
                table: "ConfiguracionesTenant",
                column: "TenantId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ConfiguracionesTenant",
                schema: "Auth");
        }
    }
}
