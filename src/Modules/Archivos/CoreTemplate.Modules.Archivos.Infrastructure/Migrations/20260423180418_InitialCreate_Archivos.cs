using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CoreTemplate.Modules.Archivos.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate_Archivos : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "Archivos");

            migrationBuilder.CreateTable(
                name: "Archivos",
                schema: "Archivos",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    NombreOriginal = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    NombreAlmacenado = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    RutaAlmacenada = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    Url = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    ContentType = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    TamanioBytes = table.Column<long>(type: "bigint", nullable: false),
                    Proveedor = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Contexto = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    ModuloOrigen = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    EntidadId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    SubidoPor = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FechaSubida = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EsActivo = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Archivos", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Archivos_ModuloOrigen_EntidadId",
                schema: "Archivos",
                table: "Archivos",
                columns: new[] { "ModuloOrigen", "EntidadId" });

            migrationBuilder.CreateIndex(
                name: "IX_Archivos_RutaAlmacenada",
                schema: "Archivos",
                table: "Archivos",
                column: "RutaAlmacenada");

            migrationBuilder.CreateIndex(
                name: "IX_Archivos_TenantId_ModuloOrigen",
                schema: "Archivos",
                table: "Archivos",
                columns: new[] { "TenantId", "ModuloOrigen" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Archivos",
                schema: "Archivos");
        }
    }
}
