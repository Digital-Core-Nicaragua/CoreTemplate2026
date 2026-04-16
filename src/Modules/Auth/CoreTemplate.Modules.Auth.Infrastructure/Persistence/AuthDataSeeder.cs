using CoreTemplate.Modules.Auth.Domain.Aggregates;
using CoreTemplate.Modules.Auth.Domain.ValueObjects;
using CoreTemplate.Modules.Auth.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace CoreTemplate.Modules.Auth.Infrastructure.Persistence;

/// <summary>
/// Seeder del módulo Auth.
/// Crea los permisos del sistema, los roles iniciales (SuperAdmin, Admin, User)
/// y el usuario SuperAdmin al arrancar la aplicación por primera vez.
/// </summary>
public static class AuthDataSeeder
{
    public static async Task SeedAsync(IServiceProvider services)
    {
        var db = services.GetRequiredService<AuthDbContext>();
        await db.Database.MigrateAsync();

        await SeedPermisosAsync(db);
        await SeedRolesAsync(db);
        await SeedSuperAdminAsync(db, services);
        await SeedAccionesAsync(db, services);
    }

    private static async Task SeedPermisosAsync(AuthDbContext db)
    {
        if (await db.Permisos.AnyAsync())
        {
            return;
        }

        var permisos = new[]
        {
            // Usuarios
            ("Usuarios.Ver", "Ver usuarios", "Listar y consultar usuarios", "Usuarios"),
            ("Usuarios.Crear", "Crear usuarios", "Registrar nuevos usuarios", "Usuarios"),
            ("Usuarios.Gestionar", "Gestionar usuarios", "Activar, desactivar y desbloquear usuarios", "Usuarios"),
            ("Usuarios.Roles.Gestionar", "Gestionar roles de usuarios", "Asignar y quitar roles a usuarios", "Usuarios"),

            // Roles
            ("Roles.Ver", "Ver roles", "Listar y consultar roles", "Roles"),
            ("Roles.Crear", "Crear roles", "Crear nuevos roles", "Roles"),
            ("Roles.Editar", "Editar roles", "Modificar roles existentes", "Roles"),
            ("Roles.Eliminar", "Eliminar roles", "Eliminar roles no del sistema", "Roles"),

            // Catálogos
            ("Catalogos.Ver", "Ver catálogos", "Listar y consultar ítems de catálogo", "Catalogos"),
            ("Catalogos.Crear", "Crear ítems", "Crear nuevos ítems de catálogo", "Catalogos"),
            ("Catalogos.Gestionar", "Gestionar catálogos", "Activar y desactivar ítems", "Catalogos"),
        };

        foreach (var (codigo, nombre, descripcion, modulo) in permisos)
        {
            var result = Permiso.Crear(codigo, nombre, descripcion, modulo);
            if (result.IsSuccess)
            {
                await db.Permisos.AddAsync(result.Value!);
            }
        }

        await db.SaveChangesAsync();
    }

    private static async Task SeedRolesAsync(AuthDbContext db)
    {
        if (await db.Roles.AnyAsync())
        {
            return;
        }

        var todosPermisos = await db.Permisos.ToListAsync();

        // SuperAdmin — todos los permisos
        var superAdminResult = Rol.Crear("SuperAdmin", "Acceso total al sistema", esSistema: true);
        if (superAdminResult.IsSuccess)
        {
            var superAdmin = superAdminResult.Value!;
            foreach (var permiso in todosPermisos)
            {
                superAdmin.AgregarPermiso(permiso.Id);
            }

            await db.Roles.AddAsync(superAdmin);
        }

        // Admin — permisos de gestión sin eliminar roles
        var adminResult = Rol.Crear("Admin", "Administrador del sistema", esSistema: true);
        if (adminResult.IsSuccess)
        {
            var admin = adminResult.Value!;
            var permisosAdmin = todosPermisos
                .Where(p => p.Codigo != "Roles.Eliminar")
                .ToList();

            foreach (var permiso in permisosAdmin)
            {
                admin.AgregarPermiso(permiso.Id);
            }

            await db.Roles.AddAsync(admin);
        }

        // User — solo permisos de lectura y catálogos básicos
        var userResult = Rol.Crear("User", "Usuario estándar", esSistema: true);
        if (userResult.IsSuccess)
        {
            var user = userResult.Value!;
            var permisosUser = todosPermisos
                .Where(p => p.Codigo.EndsWith(".Ver") || p.Codigo == "Catalogos.Crear")
                .ToList();

            foreach (var permiso in permisosUser)
            {
                user.AgregarPermiso(permiso.Id);
            }

            await db.Roles.AddAsync(user);
        }

        await db.SaveChangesAsync();
    }

    private static async Task SeedSuperAdminAsync(AuthDbContext db, IServiceProvider services)
    {
        if (await db.Usuarios.AnyAsync())
        {
            return;
        }

        var passwordService = services.GetRequiredService<Application.Abstractions.IPasswordService>();

        var emailResult = Email.Crear("admin@coretemplate.com");
        var hash = passwordService.HashPassword("Admin@1234!");
        var passwordHashResult = PasswordHash.Crear(hash);

        if (!emailResult.IsSuccess || !passwordHashResult.IsSuccess)
        {
            return;
        }

        var usuarioResult = Usuario.Crear(emailResult.Value!, "Administrador", passwordHashResult.Value!);
        if (!usuarioResult.IsSuccess)
        {
            return;
        }

        var usuario = usuarioResult.Value!;
        usuario.Activar();

        var rolSuperAdmin = await db.Roles.FirstOrDefaultAsync(r => r.Nombre == "SuperAdmin");
        if (rolSuperAdmin is not null)
        {
            usuario.AsignarRol(rolSuperAdmin.Id);
        }

        await db.Usuarios.AddAsync(usuario);
        await db.SaveChangesAsync();
    }

    private static async Task SeedAccionesAsync(AuthDbContext db, IServiceProvider services)
    {
        // Solo ejecutar si la tabla Acciones existe en el modelo (UseActionCatalog = true)
        if (db.Model.FindEntityType(typeof(Accion)) is null) return;
        if (await db.Acciones.AnyAsync()) return;

        var acciones = new[]
        {
            ("Usuarios.Ver", "Ver usuarios", "Usuarios"),
            ("Usuarios.Crear", "Crear usuarios", "Usuarios"),
            ("Usuarios.Gestionar", "Gestionar usuarios", "Usuarios"),
            ("Usuarios.Roles.Gestionar", "Gestionar roles de usuarios", "Usuarios"),
            ("Roles.Ver", "Ver roles", "Roles"),
            ("Roles.Crear", "Crear roles", "Roles"),
            ("Roles.Editar", "Editar roles", "Roles"),
            ("Roles.Eliminar", "Eliminar roles", "Roles"),
            ("Catalogos.Ver", "Ver cat\u00e1logos", "Catalogos"),
            ("Catalogos.Crear", "Crear \u00edtems", "Catalogos"),
            ("Catalogos.Gestionar", "Gestionar cat\u00e1logos", "Catalogos"),
        };

        foreach (var (codigo, nombre, modulo) in acciones)
        {
            var result = Accion.Crear(codigo, nombre, modulo);
            if (result.IsSuccess)
                await db.Acciones.AddAsync(result.Value!);
        }

        await db.SaveChangesAsync();
    }
}
