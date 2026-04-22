ok, implementalo entonces, tambien documenta o comenta el codigo para que sea entendible


Context
Voy a revisar la estructura actual del módulo Auth para implementar todo de forma consistente.

2 directories listed
CoreTemplate.Modules.Auth.Domain
CoreTemplate.Modules.Auth.Application
3 files read
Usuario.cs
Email.cs
AuthRepositories.cs
Tengo todo el contexto necesario. Voy a implementar por capas: Domain → Application → Infrastructure → Api. Empiezo con el Domain.

2 files read
AuthEvents.cs
AuthSettings.cs
Tengo todo el contexto. Implemento por capas. Empiezo con Domain :

Crear el enum EstadoUsuarioCliente

EstadoUsuarioCliente.cs
+31
-0

Undo
Crear el enum TipoProveedorOAuth

TipoProveedorOAuth.cs
+20
-0

Undo
Crear el aggregate UsuarioCliente

UsuarioCliente.cs
+462
-0

Undo

Undo all changes
Ahora agrego los eventos de dominio del portal al archivo de eventos existente:

Agregar eventos de dominio del portal de clientes al archivo de eventos existente

AuthEvents.cs
+46
-0

Undo
Ahora agrego el repositorio al archivo de repositorios existente:

Agregar IUsuarioClienteRepository al archivo de repositorios

AuthRepositories.cs
+40
-0

Undo
Necesito agregar el using de Aggregates al archivo de repositorios:

Verificar usings — ya están correctos

AuthRepositories.cs
+0
-0

Undo
Ahora la capa Application — settings, abstracciones y commands:

Agregar CustomerPortalSettings y configuraciones OAuth al archivo de settings

AuthSettings.cs
+81
-0

Undo
Ahora agrego la abstracción IProveedorOAuthService al archivo de servicios:

Agregar IProveedorOAuthService y OAuthUsuarioInfo al archivo de abstracciones

IAuthServices.cs
+32
-0

Undo
Ahora creo los DTOs y Commands del portal:

Crear DTOs del portal de clientes

ClienteDtos.cs
+39
-0

Undo
Crear todos los commands del portal de clientes en un solo archivo

PortalClientesCommands.cs