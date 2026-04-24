using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CoreTemplate.Modules.Notificaciones.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate_Notificaciones : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "Notificaciones");

            migrationBuilder.CreateTable(
                name: "Notificaciones",
                schema: "Notificaciones",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    UsuarioId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Titulo = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Mensaje = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    Tipo = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Url = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    EsLeida = table.Column<bool>(type: "bit", nullable: false),
                    EntregadaEnTiempoReal = table.Column<bool>(type: "bit", nullable: false),
                    CreadaEn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LeidaEn = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Notificaciones", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Notificaciones_TenantId_UsuarioId",
                schema: "Notificaciones",
                table: "Notificaciones",
                columns: new[] { "TenantId", "UsuarioId" });

            migrationBuilder.CreateIndex(
                name: "IX_Notificaciones_UsuarioId_EsLeida",
                schema: "Notificaciones",
                table: "Notificaciones",
                columns: new[] { "UsuarioId", "EsLeida" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Notificaciones",
                schema: "Notificaciones");
        }
    }
}
