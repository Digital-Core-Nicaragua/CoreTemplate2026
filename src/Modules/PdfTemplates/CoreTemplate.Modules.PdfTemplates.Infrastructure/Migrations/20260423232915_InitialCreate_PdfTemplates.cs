using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CoreTemplate.Modules.PdfTemplates.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate_PdfTemplates : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "PdfTemplates");

            migrationBuilder.CreateTable(
                name: "Plantillas",
                schema: "PdfTemplates",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Codigo = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Nombre = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Modulo = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    CodigoTemplate = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    NombreEmpresa = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    LogoUrl = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    ColorEncabezado = table.Column<string>(type: "nvarchar(7)", maxLength: 7, nullable: false),
                    ColorTextoHeader = table.Column<string>(type: "nvarchar(7)", maxLength: 7, nullable: false),
                    ColorAcento = table.Column<string>(type: "nvarchar(7)", maxLength: 7, nullable: false),
                    TextoSecundario = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    TextoPiePagina = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    MostrarNumeroPagina = table.Column<bool>(type: "bit", nullable: false),
                    MostrarFechaGeneracion = table.Column<bool>(type: "bit", nullable: false),
                    MarcaDeAgua = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    EsDeSistema = table.Column<bool>(type: "bit", nullable: false),
                    EsActivo = table.Column<bool>(type: "bit", nullable: false),
                    CreadoEn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModificadoEn = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ModificadoPor = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Plantillas", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PdfPlantillas_Codigo_TenantId",
                schema: "PdfTemplates",
                table: "Plantillas",
                columns: new[] { "Codigo", "TenantId" },
                unique: true,
                filter: "[TenantId] IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Plantillas",
                schema: "PdfTemplates");
        }
    }
}
