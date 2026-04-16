using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CoreTemplate.Modules.Auth.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Add_Sesiones_TipoUsuario : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "RefreshTokens",
                schema: "Auth");

            migrationBuilder.AddColumn<string>(
                name: "TipoUsuario",
                schema: "Auth",
                table: "Usuarios",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateTable(
                name: "Sesiones",
                schema: "Auth",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UsuarioId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    RefreshTokenHash = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    Canal = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Dispositivo = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Ip = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    UserAgent = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    UltimaActividad = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ExpiraEn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreadoEn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EsActiva = table.Column<bool>(type: "bit", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Sesiones", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Sesiones_RefreshTokenHash",
                schema: "Auth",
                table: "Sesiones",
                column: "RefreshTokenHash",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Sesiones_UsuarioId_EsActiva",
                schema: "Auth",
                table: "Sesiones",
                columns: new[] { "UsuarioId", "EsActiva" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Sesiones",
                schema: "Auth");

            migrationBuilder.DropColumn(
                name: "TipoUsuario",
                schema: "Auth",
                table: "Usuarios");

            migrationBuilder.CreateTable(
                name: "RefreshTokens",
                schema: "Auth",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreadoDesdeIp = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    CreadoEn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EsRevocado = table.Column<bool>(type: "bit", nullable: false),
                    ExpiraEn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    RevocarEn = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Token = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    UsuarioId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
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
        }
    }
}
