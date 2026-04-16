using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CoreTemplate.Modules.Auth.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Add_Sucursales : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Sucursales",
                schema: "Auth",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Codigo = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Nombre = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    EsActiva = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    CreadoEn = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Sucursales", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "UsuarioSucursales",
                schema: "Auth",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UsuarioId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SucursalId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EsPrincipal = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    AsignadoEn = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UsuarioSucursales", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UsuarioSucursales_Usuarios_UsuarioId",
                        column: x => x.UsuarioId,
                        principalSchema: "Auth",
                        principalTable: "Usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Sucursales_TenantId_Codigo",
                schema: "Auth",
                table: "Sucursales",
                columns: new[] { "TenantId", "Codigo" },
                unique: true,
                filter: "[TenantId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_UsuarioSucursales_UsuarioId_SucursalId",
                schema: "Auth",
                table: "UsuarioSucursales",
                columns: new[] { "UsuarioId", "SucursalId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Sucursales",
                schema: "Auth");

            migrationBuilder.DropTable(
                name: "UsuarioSucursales",
                schema: "Auth");
        }
    }
}
