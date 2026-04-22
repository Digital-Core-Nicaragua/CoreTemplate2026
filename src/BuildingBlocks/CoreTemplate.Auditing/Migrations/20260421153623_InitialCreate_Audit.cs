using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CoreTemplate.Auditing.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate_Audit : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "Shared");

            migrationBuilder.CreateTable(
                name: "AuditLogs",
                schema: "Shared",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    UsuarioId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    NombreEntidad = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    EntidadId = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Accion = table.Column<int>(type: "int", nullable: false),
                    ValoresAnteriores = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ValoresNuevos = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    OcurridoEn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DireccionIp = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    UserAgent = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CorrelationId = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AuditLogs", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AuditLogs_NombreEntidad_EntidadId",
                schema: "Shared",
                table: "AuditLogs",
                columns: new[] { "NombreEntidad", "EntidadId" });

            migrationBuilder.CreateIndex(
                name: "IX_AuditLogs_OcurridoEn",
                schema: "Shared",
                table: "AuditLogs",
                column: "OcurridoEn");

            migrationBuilder.CreateIndex(
                name: "IX_AuditLogs_TenantId",
                schema: "Shared",
                table: "AuditLogs",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_AuditLogs_UsuarioId",
                schema: "Shared",
                table: "AuditLogs",
                column: "UsuarioId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AuditLogs",
                schema: "Shared");
        }
    }
}
