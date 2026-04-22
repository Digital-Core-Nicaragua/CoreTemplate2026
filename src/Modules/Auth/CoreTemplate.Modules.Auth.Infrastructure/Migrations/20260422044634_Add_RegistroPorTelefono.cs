using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CoreTemplate.Modules.Auth.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Add_RegistroPorTelefono : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_UsuariosCliente_Email_TenantId",
                schema: "Auth",
                table: "UsuariosCliente");

            migrationBuilder.AlterColumn<string>(
                name: "Email",
                schema: "Auth",
                table: "UsuariosCliente",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(200)",
                oldMaxLength: 200);

            migrationBuilder.AddColumn<int>(
                name: "TipoRegistro",
                schema: "Auth",
                table: "UsuariosCliente",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_UsuariosCliente_Email_TenantId",
                schema: "Auth",
                table: "UsuariosCliente",
                columns: new[] { "Email", "TenantId" },
                unique: true,
                filter: "[Email] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_UsuariosCliente_Telefono_TenantId",
                schema: "Auth",
                table: "UsuariosCliente",
                columns: new[] { "Telefono", "TenantId" },
                unique: true,
                filter: "[Telefono] IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_UsuariosCliente_Email_TenantId",
                schema: "Auth",
                table: "UsuariosCliente");

            migrationBuilder.DropIndex(
                name: "IX_UsuariosCliente_Telefono_TenantId",
                schema: "Auth",
                table: "UsuariosCliente");

            migrationBuilder.DropColumn(
                name: "TipoRegistro",
                schema: "Auth",
                table: "UsuariosCliente");

            migrationBuilder.AlterColumn<string>(
                name: "Email",
                schema: "Auth",
                table: "UsuariosCliente",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(200)",
                oldMaxLength: 200,
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_UsuariosCliente_Email_TenantId",
                schema: "Auth",
                table: "UsuariosCliente",
                columns: new[] { "Email", "TenantId" },
                unique: true,
                filter: "[TenantId] IS NOT NULL");
        }
    }
}
