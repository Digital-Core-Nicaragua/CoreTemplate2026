using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CoreTemplate.Modules.Auth.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate_Auth : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "Auth");

            migrationBuilder.CreateTable(
                name: "Permisos",
                schema: "Auth",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Codigo = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Nombre = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Descripcion = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    Modulo = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    CreadoEn = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Permisos", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "RegistrosAuditoria",
                schema: "Auth",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    UsuarioId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Email = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Evento = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Ip = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    UserAgent = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    Detalle = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    CreadoEn = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RegistrosAuditoria", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Roles",
                schema: "Auth",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Nombre = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Descripcion = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    EsSistema = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    CreadoEn = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Roles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Usuarios",
                schema: "Auth",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Email = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Nombre = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    PasswordHash = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    Estado = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IntentosFallidos = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    BloqueadoHasta = table.Column<DateTime>(type: "datetime2", nullable: true),
                    TwoFactorActivo = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    TwoFactorSecretKey = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    UltimoAcceso = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreadoEn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModificadoEn = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Usuarios", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "RolPermisos",
                schema: "Auth",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RolId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PermisoId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RolPermisos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RolPermisos_Roles_RolId",
                        column: x => x.RolId,
                        principalSchema: "Auth",
                        principalTable: "Roles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CodigosRecuperacion2FA",
                schema: "Auth",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UsuarioId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CodigoHash = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    UsadoEn = table.Column<DateTime>(type: "datetime2", nullable: true),
                    EsUsado = table.Column<bool>(type: "bit", nullable: false),
                    CreadoEn = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CodigosRecuperacion2FA", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CodigosRecuperacion2FA_Usuarios_UsuarioId",
                        column: x => x.UsuarioId,
                        principalSchema: "Auth",
                        principalTable: "Usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "RefreshTokens",
                schema: "Auth",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UsuarioId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Token = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    ExpiraEn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    RevocarEn = table.Column<DateTime>(type: "datetime2", nullable: true),
                    EsRevocado = table.Column<bool>(type: "bit", nullable: false),
                    CreadoEn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreadoDesdeIp = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RefreshTokens", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RefreshTokens_Usuarios_UsuarioId",
                        column: x => x.UsuarioId,
                        principalSchema: "Auth",
                        principalTable: "Usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TokensRestablecimiento",
                schema: "Auth",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UsuarioId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Token = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    ExpiraEn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UsadoEn = table.Column<DateTime>(type: "datetime2", nullable: true),
                    EsUsado = table.Column<bool>(type: "bit", nullable: false),
                    CreadoEn = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TokensRestablecimiento", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TokensRestablecimiento_Usuarios_UsuarioId",
                        column: x => x.UsuarioId,
                        principalSchema: "Auth",
                        principalTable: "Usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UsuarioRoles",
                schema: "Auth",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UsuarioId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RolId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AsignadoEn = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UsuarioRoles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UsuarioRoles_Usuarios_UsuarioId",
                        column: x => x.UsuarioId,
                        principalSchema: "Auth",
                        principalTable: "Usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CodigosRecuperacion2FA_UsuarioId",
                schema: "Auth",
                table: "CodigosRecuperacion2FA",
                column: "UsuarioId");

            migrationBuilder.CreateIndex(
                name: "IX_Permisos_Codigo",
                schema: "Auth",
                table: "Permisos",
                column: "Codigo",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RefreshTokens_Token",
                schema: "Auth",
                table: "RefreshTokens",
                column: "Token",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RefreshTokens_UsuarioId",
                schema: "Auth",
                table: "RefreshTokens",
                column: "UsuarioId");

            migrationBuilder.CreateIndex(
                name: "IX_RegistrosAuditoria_CreadoEn",
                schema: "Auth",
                table: "RegistrosAuditoria",
                column: "CreadoEn");

            migrationBuilder.CreateIndex(
                name: "IX_RegistrosAuditoria_UsuarioId",
                schema: "Auth",
                table: "RegistrosAuditoria",
                column: "UsuarioId");

            migrationBuilder.CreateIndex(
                name: "IX_Roles_TenantId_Nombre",
                schema: "Auth",
                table: "Roles",
                columns: new[] { "TenantId", "Nombre" },
                unique: true,
                filter: "[TenantId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_RolPermisos_RolId_PermisoId",
                schema: "Auth",
                table: "RolPermisos",
                columns: new[] { "RolId", "PermisoId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TokensRestablecimiento_Token",
                schema: "Auth",
                table: "TokensRestablecimiento",
                column: "Token",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TokensRestablecimiento_UsuarioId",
                schema: "Auth",
                table: "TokensRestablecimiento",
                column: "UsuarioId");

            migrationBuilder.CreateIndex(
                name: "IX_UsuarioRoles_UsuarioId_RolId",
                schema: "Auth",
                table: "UsuarioRoles",
                columns: new[] { "UsuarioId", "RolId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Usuarios_Email",
                schema: "Auth",
                table: "Usuarios",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Usuarios_TenantId",
                schema: "Auth",
                table: "Usuarios",
                column: "TenantId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CodigosRecuperacion2FA",
                schema: "Auth");

            migrationBuilder.DropTable(
                name: "Permisos",
                schema: "Auth");

            migrationBuilder.DropTable(
                name: "RefreshTokens",
                schema: "Auth");

            migrationBuilder.DropTable(
                name: "RegistrosAuditoria",
                schema: "Auth");

            migrationBuilder.DropTable(
                name: "RolPermisos",
                schema: "Auth");

            migrationBuilder.DropTable(
                name: "TokensRestablecimiento",
                schema: "Auth");

            migrationBuilder.DropTable(
                name: "UsuarioRoles",
                schema: "Auth");

            migrationBuilder.DropTable(
                name: "Roles",
                schema: "Auth");

            migrationBuilder.DropTable(
                name: "Usuarios",
                schema: "Auth");
        }
    }
}
