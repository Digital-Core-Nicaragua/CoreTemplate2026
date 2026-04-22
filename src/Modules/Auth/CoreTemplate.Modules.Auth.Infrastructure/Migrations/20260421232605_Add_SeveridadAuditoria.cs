using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CoreTemplate.Modules.Auth.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Add_SeveridadAuditoria : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Severidad",
                schema: "Auth",
                table: "RegistrosAuditoria",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "UsuariosCliente",
                schema: "Auth",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Email = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    PasswordHash = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Nombre = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Apellido = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Telefono = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    Estado = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    EmailVerificado = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    TelefonoVerificado = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    TokenVerificacionEmail = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    CodigoVerificacionTelefono = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    TokenExpiraEn = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IntentosFallidos = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    BloqueadoHasta = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreadoEn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModificadoEn = table.Column<DateTime>(type: "datetime2", nullable: true),
                    TokenRestablecimiento = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    TokenRestablecimientoExpiraEn = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UsuariosCliente", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "UsuarioClienteProveedores",
                schema: "Auth",
                columns: table => new
                {
                    Proveedor = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    UsuarioClienteId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ExternalId = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    VinculadoEn = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UsuarioClienteProveedores", x => new { x.UsuarioClienteId, x.Proveedor });
                    table.ForeignKey(
                        name: "FK_UsuarioClienteProveedores_UsuariosCliente_UsuarioClienteId",
                        column: x => x.UsuarioClienteId,
                        principalSchema: "Auth",
                        principalTable: "UsuariosCliente",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_UsuarioClienteProveedores_Proveedor_ExternalId",
                schema: "Auth",
                table: "UsuarioClienteProveedores",
                columns: new[] { "Proveedor", "ExternalId" });

            migrationBuilder.CreateIndex(
                name: "IX_UsuariosCliente_Email",
                schema: "Auth",
                table: "UsuariosCliente",
                column: "Email");

            migrationBuilder.CreateIndex(
                name: "IX_UsuariosCliente_TenantId",
                schema: "Auth",
                table: "UsuariosCliente",
                column: "TenantId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "UsuarioClienteProveedores",
                schema: "Auth");

            migrationBuilder.DropTable(
                name: "UsuariosCliente",
                schema: "Auth");

            migrationBuilder.DropColumn(
                name: "Severidad",
                schema: "Auth",
                table: "RegistrosAuditoria");
        }
    }
}
