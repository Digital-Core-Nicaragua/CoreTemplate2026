using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CoreTemplate.Modules.Configuracion.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate_Configuracion : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "Configuracion");

            migrationBuilder.CreateTable(
                name: "Items",
                schema: "Configuracion",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Clave = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Valor = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    Tipo = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Descripcion = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    Grupo = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    EsEditable = table.Column<bool>(type: "bit", nullable: false),
                    CreadoEn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModificadoEn = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ModificadoPor = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Items", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Configuracion_Clave_TenantId",
                schema: "Configuracion",
                table: "Items",
                columns: new[] { "Clave", "TenantId" },
                unique: true,
                filter: "[TenantId] IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Items",
                schema: "Configuracion");
        }
    }
}
