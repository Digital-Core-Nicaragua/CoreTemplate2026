# SDD — Diagramas de Diseño

> Complementa: `docs/Auth/02-Analisis-Tradicional/13-DIAGRAMAS.md`  
> Fecha: 2026-04-15

---

## Diagramas de Secuencia

### Diagrama 1: Login Normal

```mermaid
sequenceDiagram
    actor U as Usuario
    participant C as AuthController
    participant H as LoginHandler
    participant UR as UsuarioRepository
    participant UA as Usuario Aggregate
    participant SS as SesionService
    participant SR as SesionRepository
    participant JWT as JwtService
    participant AUD as AuditoriaRepository

    U->>C: POST /api/auth/login { email, password, canal }
    C->>H: Send(LoginCommand)
    H->>UR: GetByEmailAsync(email)

    alt Email no encontrado
        UR-->>H: null
        H->>AUD: AddAsync(LoginFallido, "Email no encontrado")
        H-->>C: Failure(CredencialesInvalidas)
        C-->>U: 401
    else Usuario bloqueado o inactivo
        UR-->>H: Usuario
        H->>UA: PuedeAutenticarse() false
        H->>AUD: AddAsync(LoginFallido, "Estado: Bloqueado")
        H-->>C: Failure(CuentaBloqueada)
        C-->>U: 403
    else Password incorrecta
        H->>UA: VerifyPassword(password) false
        H->>UA: IncrementarIntentosFallidos(max, minutos)
        H->>UR: UpdateAsync(usuario)
        H->>AUD: AddAsync(LoginFallido, "Intento 2/5")
        H-->>C: Failure(CredencialesInvalidas)
        C-->>U: 401
    else Login exitoso
        H->>UA: VerifyPassword(password) true
        H->>UA: ResetearIntentosFallidos()
        H->>UA: RegistrarAcceso()
        H->>SS: VerificarYAplicarLimiteAsync(usuarioId, tipoUsuario)
        SS-->>H: OK
        H->>SR: AddAsync(Sesion.Crear(...))
        H->>UR: UpdateAsync(usuario)
        H->>JWT: GenerarAccessToken(usuario)
        JWT-->>H: accessToken
        H->>AUD: AddAsync(Login, ip, userAgent)
        H-->>C: Result accessToken + refreshToken + usuario
        C-->>U: 200 ApiResponse
    end
```

---

### Diagrama 2: Login con 2FA

```mermaid
sequenceDiagram
    actor U as Usuario
    participant C as AuthController
    participant LH as LoginHandler
    participant UR as UsuarioRepository
    participant VH as Verificar2FAHandler
    participant TOTP as TotpService
    participant SR as SesionRepository
    participant JWT as JwtService
    participant AUD as AuditoriaRepository

    U->>C: POST /login { email, password, canal }
    C->>LH: Send(LoginCommand)
    LH->>UR: GetByEmailAsync(email)
    UR-->>LH: Usuario
    LH->>LH: VerifyPassword() true
    LH->>LH: TwoFactorActivo = true
    LH->>UR: UpdateAsync(ResetearIntentos + RegistrarAcceso)
    LH->>JWT: GenerarTokenTemporal2FA(usuarioId)
    JWT-->>LH: tokenTemporal 5 min
    LH-->>C: Result requires2FA true + tokenTemporal
    C-->>U: 200 requires2FA true

    U->>C: POST /2fa/verificar { tokenTemporal, codigo }
    C->>VH: Send(Verificar2FACommand)
    VH->>VH: ValidarTokenTemporal firma + expiracion
    VH->>TOTP: ValidarCodigo(secretKey, codigo)

    alt Codigo invalido
        TOTP-->>VH: false
        VH->>AUD: AddAsync(LoginFallido, "Codigo TOTP invalido")
        VH-->>C: Failure(CodigoInvalido)
        C-->>U: 401
    else Codigo valido
        TOTP-->>VH: true
        VH->>SR: AddAsync(Sesion.Crear(...))
        VH->>JWT: GenerarAccessToken(usuario)
        JWT-->>VH: accessToken
        VH->>AUD: AddAsync(Login, ip, userAgent)
        VH-->>C: Result accessToken + refreshToken + usuario
        C-->>U: 200 ApiResponse
    end
```

---

### Diagrama 3: Refresh Token

```mermaid
sequenceDiagram
    actor U as Usuario
    participant C as AuthController
    participant H as RefreshTokenHandler
    participant UR as UsuarioRepository
    participant SR as SesionRepository
    participant JWT as JwtService
    participant AUD as AuditoriaRepository

    U->>C: POST /refresh { refreshToken }
    C->>H: Send(RefreshTokenCommand)
    H->>H: SHA256(refreshToken) hash
    H->>SR: GetActivaByRefreshTokenHash(hash)

    alt Token invalido o expirado
        SR-->>H: null o EsValida false
        H-->>C: Failure(RefreshTokenInvalido)
        C-->>U: 401
    else Usuario no puede autenticarse
        H->>UR: GetByIdAsync(sesion.UsuarioId)
        UR-->>H: null o PuedeAutenticarse false
        H-->>C: Failure(RefreshTokenInvalido)
        C-->>U: 401
    else Refresh exitoso
        SR-->>H: Sesion valida
        H->>UR: GetByIdAsync(sesion.UsuarioId)
        UR-->>H: Usuario
        H->>SR: sesion.Renovar(nuevoHash, nuevaExpiracion)
        H->>JWT: GenerarAccessToken(usuario)
        JWT-->>H: nuevoAccessToken
        H->>AUD: AddAsync(TokenRefrescado, ip)
        H-->>C: Result accessToken + refreshToken + expiraEn
        C-->>U: 200 ApiResponse
    end
```

---

### Diagrama 4: Logout con Blacklist

```mermaid
sequenceDiagram
    actor U as Usuario
    participant C as AuthController
    participant H as LogoutHandler
    participant UR as UsuarioRepository
    participant SR as SesionRepository
    participant BL as TokenBlacklist
    participant AUD as AuditoriaRepository

    U->>C: POST /logout { refreshToken } + Bearer accessToken
    C->>H: Send(LogoutCommand)
    H->>H: SHA256(refreshToken) hash
    H->>SR: GetActivaByRefreshTokenHash(hash)
    SR-->>H: Sesion o null
    H->>SR: sesion.Revocar()
    H->>H: Extraer JTI del accessToken
    H->>H: Calcular TTL = expiracion - UtcNow
    H->>BL: AgregarAsync(jti, ttl)
    H->>UR: GetByIdAsync(currentUser.Id)
    UR-->>H: Usuario
    H->>AUD: AddAsync(Logout, ip, userAgent)
    H-->>C: Result OK
    C-->>U: 200 Sesion cerrada

    Note over U,BL: Request posterior con el mismo accessToken
    U->>C: GET /cualquier-endpoint + Bearer accessToken
    C->>C: TokenBlacklistMiddleware
    C->>BL: EstaEnBlacklist(jti)?
    BL-->>C: true
    C-->>U: 401 Token revocado
```

---

### Diagrama 5: Cambiar Password

```mermaid
sequenceDiagram
    actor U as Usuario
    participant C as PerfilController
    participant H as CambiarPasswordHandler
    participant UR as UsuarioRepository
    participant SR as SesionRepository
    participant BL as TokenBlacklist
    participant AUD as AuditoriaRepository

    U->>C: PUT /perfil/cambiar-password { passwordActual, passwordNuevo }
    C->>H: Send(CambiarPasswordCommand)
    H->>UR: GetByIdAsync(currentUser.Id)

    alt Password actual incorrecta
        UR-->>H: Usuario
        H->>H: VerifyPassword(passwordActual) false
        H-->>C: Failure(PasswordActualIncorrecto)
        C-->>U: 400
    else Cambio exitoso
        H->>H: VerifyPassword(passwordActual) true
        H->>H: ValidarPolitica(nuevoPassword)
        H->>UR: usuario.CambiarPassword(nuevoHash)
        H->>SR: RevocarTodasAsync(usuarioId)
        H->>UR: UpdateAsync(usuario)
        H->>BL: AgregarAsync(jtiActual, ttl)
        H->>AUD: AddAsync(CambioPassword, ip, userAgent)
        H-->>C: Result OK
        C-->>U: 200 Password actualizado
    end
```

---

## Diagramas de Estado

### Diagrama 6: Estados del Usuario

```mermaid
stateDiagram-v2
    [*] --> Pendiente: Crear()
    Pendiente --> Activo: Activar()
    Activo --> Inactivo: Desactivar()
    Inactivo --> Activo: Activar()
    Activo --> Bloqueado: IncrementarIntentos >= Max
    Bloqueado --> Activo: Desbloquear() o BloqueadoHasta expiró
    Bloqueado --> Bloqueado: Nuevo intento fallido
```

---

### Diagrama 7: Estados de la Sesión

```mermaid
stateDiagram-v2
    [*] --> Activa: Sesion.Crear()
    Activa --> Renovada: Renovar(nuevoHash)
    Renovada --> Activa: nueva sesión activa
    Activa --> Revocada: Revocar() / Logout / CambioPassword
    Activa --> Expirada: ExpiraEn < UtcNow
    Revocada --> [*]
    Expirada --> [*]
```

---

### Diagrama 8: Estados del Token JWT

```mermaid
stateDiagram-v2
    [*] --> Activo: Login exitoso
    Activo --> Revocado: Logout / CambioPassword / Refresh
    Activo --> Expirado: TTL vencido (15 min)
    Revocado --> Rechazado: Request con token revocado
    Rechazado --> [*]: 401 Unauthorized
    Revocado --> [*]: Blacklist entry expira
    Expirado --> [*]
```

---

## Diagramas de Actividad

### Diagrama 9: Flujo de Decisión del Login

```mermaid
flowchart TD
    START([Inicio]) --> CREDS{Credenciales\nválidas?}
    CREDS -- No --> INCR[IncrementarIntentosFallidos]
    INCR --> LOCK{Intentos\n>= Max?}
    LOCK -- Sí --> BLOQUEAR[Bloquear usuario\nhasta X minutos]
    BLOQUEAR --> ERR403[403 Cuenta bloqueada]
    LOCK -- No --> ERR401[401 Credenciales inválidas]

    CREDS -- Sí --> RESET[ResetearIntentosFallidos]
    RESET --> TIPO{TipoUsuario}
    TIPO -- Sistema / Integración --> SESIONES
    TIPO -- Humano --> FA{2FA\nhabilitado\ny activo?}

    FA -- No --> SESIONES
    FA -- Sí --> TEMP[Generar tokenTemporal\n200 requires2FA: true]
    TEMP --> TOTP[POST /2fa/verificar]
    TOTP --> VALID{Código\nTOTP válido?}
    VALID -- No --> ERR401B[401 Código inválido]
    VALID -- Sí --> SESIONES

    SESIONES{Sesiones activas\n>= límite?}
    SESIONES -- No --> TOKEN[Crear Sesión\nGenerar tokens\n200 OK]
    SESIONES -- Sí --> ACCION{AccionAlLlegarLimite}
    ACCION -- CerrarMasAntigua --> CIERRA[Revocar sesión\nmás antigua]
    CIERRA --> TOKEN
    ACCION -- BloquearNuevoLogin --> DENY[403 Límite de sesiones]
```

---

### Diagrama 10: Jerarquía de Límite de Sesiones

```mermaid
flowchart TD
    START([Verificar límite]) --> TIPO{TipoUsuario\n== Sistema\no Integración?}
    TIPO -- Sí --> PERMITIR([Sin límite — permitir])
    TIPO -- No --> MULTI{IsMultiTenant\n+ EnableSessionLimits\nPerTenant?}
    MULTI -- Sí --> TENANT{ConfiguracionTenant\n.MaxSesiones\n!= null?}
    TENANT -- Sí --> USAR_TENANT[Usar límite del Tenant]
    TENANT -- No --> GLOBAL
    MULTI -- No --> GLOBAL[Usar AuthSettings\n.MaxSesionesSimultaneas]
    USAR_TENANT --> VERIFICAR
    GLOBAL --> VERIFICAR{Sesiones activas\n>= límite?}
    VERIFICAR -- No --> PERMITIR2([Permitir nueva sesión])
    VERIFICAR -- Sí --> ACCION{AccionAlLlegarLimite}
    ACCION -- CerrarMasAntigua --> REVOCAR[Revocar más antigua\nPermitir nueva]
    ACCION -- BloquearNuevoLogin --> RECHAZAR([403 Rechazar])
```
