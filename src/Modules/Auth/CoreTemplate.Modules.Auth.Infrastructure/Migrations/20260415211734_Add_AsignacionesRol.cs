using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CoreTemplate.Modules.Auth.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Add_AsignacionesRol : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AsignacionesRol",
                schema: "Auth",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UsuarioId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SucursalId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RolId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AsignadoEn = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AsignacionesRol", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AsignacionesRol_UsuarioId_SucursalId",
                schema: "Auth",
                table: "AsignacionesRol",
                columns: new[] { "UsuarioId", "SucursalId" });

            migrationBuilder.CreateIndex(
                name: "IX_AsignacionesRol_UsuarioId_SucursalId_RolId",
                schema: "Auth",
                table: "AsignacionesRol",
                columns: new[] { "UsuarioId", "SucursalId", "RolId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AsignacionesRol",
                schema: "Auth");
        }
    }
}
