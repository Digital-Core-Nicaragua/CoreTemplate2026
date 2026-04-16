using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CoreTemplate.Modules.Catalogos.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate_Catalogos : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "Catalogos");

            migrationBuilder.CreateTable(
                name: "CatalogoItems",
                schema: "Catalogos",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Codigo = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Nombre = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Descripcion = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    EsActivo = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    CreadoEn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModificadoEn = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CatalogoItems", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CatalogoItems_TenantId_Codigo",
                schema: "Catalogos",
                table: "CatalogoItems",
                columns: new[] { "TenantId", "Codigo" },
                unique: true,
                filter: "[TenantId] IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CatalogoItems",
                schema: "Catalogos");
        }
    }
}
