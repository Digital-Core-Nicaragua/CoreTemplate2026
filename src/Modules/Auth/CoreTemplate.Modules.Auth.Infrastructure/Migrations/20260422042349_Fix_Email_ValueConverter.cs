using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CoreTemplate.Modules.Auth.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Fix_Email_ValueConverter : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_UsuariosCliente_Email",
                schema: "Auth",
                table: "UsuariosCliente");

            migrationBuilder.CreateIndex(
                name: "IX_UsuariosCliente_Email_TenantId",
                schema: "Auth",
                table: "UsuariosCliente",
                columns: new[] { "Email", "TenantId" },
                unique: true,
                filter: "[TenantId] IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_UsuariosCliente_Email_TenantId",
                schema: "Auth",
                table: "UsuariosCliente");

            migrationBuilder.CreateIndex(
                name: "IX_UsuariosCliente_Email",
                schema: "Auth",
                table: "UsuariosCliente",
                column: "Email");
        }
    }
}
