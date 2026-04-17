HasDefaultSchema, UseNpgsql y UseSqlServer requieren los paquetes Relational de EF Core. Agrego los paquetes necesarios al .csproj de Auditing:

CoreTemplate.Auditing.csproj
+3
-0

También necesito agregar Microsoft.EntityFrameworkCore.Relational al Directory.Packages.props:

1 file read
Directory.Packages.props
An unexpected error occurred, check the logs for more information.