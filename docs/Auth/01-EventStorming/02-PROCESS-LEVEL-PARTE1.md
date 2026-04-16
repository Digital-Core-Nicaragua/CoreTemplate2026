# Event Storming — Process Level Parte 1

> **Procesos:** Autenticación, Sesiones, Token Blacklist  
> **Fecha:** 2026-04-15

---

## PROCESO 1: Login Completo (con y sin 2FA)

```
┌─────────────────────────────────────────────────────────────┐
│ FASE 1: VALIDACIÓN DE CREDENCIALES                         │
└─────────────────────────────────────────────────────────────┘

👤 Usuario
  → 🔵 IniciarSesion
      Input: { email, password, canal, ip, userAgent }

🟡 Usuario (Aggregate)
  → Buscar por email
  → Verificar PuedeAutenticarse()
      ├── Estado == Activo?
      └── BloqueadoHasta < UtcNow? (desbloqueo automático)

  SI no puede autenticarse:
    🟠 LoginFallido { motivo: "CuentaBloqueada" | "CuentaInactiva" }
    → Fin

  → Verificar contraseña (BCrypt)

  SI contraseña incorrecta:
    🟡 Usuario → IncrementarIntentosFallidos
    🟣 POLÍTICA: Si TipoUsuario == Humano Y IntentosFallidos >= Max
        → Bloquear(DateTime.UtcNow + LockoutDuration)
        🟠 UsuarioBloqueado
    🟠 LoginFallido { motivo: "CredencialesInvalidas" }
    → Fin

  SI contraseña correcta:
    🟡 Usuario → ResetearIntentosFallidos
    🟡 Usuario → RegistrarAcceso
    🟠 CredencialesValidadas

┌─────────────────────────────────────────────────────────────┐
│ FASE 2: VERIFICACIÓN 2FA (solo TipoUsuario.Humano)         │
└─────────────────────────────────────────────────────────────┘

  SI TwoFactorActivo == true Y TipoUsuario == Humano:
    🔵 GenerarTokenTemporal2FA { usuarioId, expira: +5min }
    🟠 DosFactoresRequerido
    → Retornar { requires2FA: true, tokenTemporal }
    → Esperar CU-AUTH-010 (VerificarCodigo2FA)

  SI sin 2FA (o TipoUsuario != Humano):
    → Continuar FASE 3

┌─────────────────────────────────────────────────────────────┐
│ FASE 3: LÍMITE DE SESIONES                                 │
└─────────────────────────────────────────────────────────────┘

  🤖 ISesionService → VerificarYAplicarLimite { usuarioId, tipoUsuario }

  SI TipoUsuario == Sistema | Integracion:
    → Sin límite, continuar

  SI TipoUsuario == Humano:
    🟢 ObtenerLimite:
        1. ConfiguracionTenant.MaxSesionesSimultaneas (si EnableSessionLimitsPerTenant)
        2. AuthSettings.MaxSesionesSimultaneas
        3. Default: 5
    🟢 ContarSesionesActivas { usuarioId }

    SI activas < límite:
      → Continuar

    SI activas >= límite Y AccionAlLlegarLimiteSesiones == CerrarMasAntigua:
      🟢 ObtenerSesionMasAntigua { usuarioId }
      🟡 Sesion → Revocar
      🟠 SesionMasAntiguaRevocada
      → Continuar

    SI activas >= límite Y AccionAlLlegarLimiteSesiones == BloquearNuevoLogin:
      🟠 LimiteSesionesAlcanzado
      → Retornar error
      → Fin

┌─────────────────────────────────────────────────────────────┐
│ FASE 4: CREACIÓN DE SESIÓN Y TOKENS                        │
└─────────────────────────────────────────────────────────────┘

  🔵 GenerarRefreshToken → string aleatorio (64 bytes)
  🔵 ComputarHash(refreshToken) → SHA256 hex
  🟡 Sesion → Crear {
      usuarioId, tenantId,
      refreshTokenHash,
      expiraEn: UtcNow + RefreshTokenExpirationDays,
      canal, ip, userAgent, dispositivo
  }
  🟠 SesionCreada

  🔵 GenerarAccessToken → JWT {
      sub, email, name, jti,
      tipo_usuario, tenant_id,
      roles[], branch_id (si EnableBranches)
  }
  🟠 AccessTokenGenerado

  🟠 LoginExitoso
  → Retornar { accessToken, refreshToken, accessTokenExpiraEn, usuario }
```

---

## PROCESO 2: Verificación de Código 2FA

```
👤 Usuario
  → 🔵 VerificarCodigo2FA
      Input: { tokenTemporal, codigo, ip, userAgent }

🟡 JwtService → ValidarTokenTemporal2FA
  SI inválido o expirado:
    🟠 TokenTemporal2FAInvalido
    → Fin

  → Obtener usuarioId del token

🟡 Usuario → Buscar por id
  → Verificar TwoFactorActivo Y TwoFactorSecretKey != null

  🔵 TotpService → ValidarCodigo { secretKey, codigo }

  SI código TOTP inválido:
    🔵 TotpService → HashCodigoRecuperacion(codigo)
    🟡 Usuario → UsarCodigoRecuperacion(hash)

    SI código de recuperación inválido:
      🟠 DosFactoresFallido
      → Fin

  🟡 Usuario → RegistrarAcceso
  🟠 DosFactoresVerificado

  → Continuar FASE 3 y 4 del Proceso 1
```

---

## PROCESO 3: Refresh Token

```
👤 Usuario
  → 🔵 RefrescarToken
      Input: { refreshToken, ip }

  🔵 ComputarHash(refreshToken) → SHA256 hex
  🟡 ISesionRepository → GetActivaByRefreshTokenHash(hash)

  SI sesión no encontrada o no válida:
    🟠 RefreshTokenInvalido
    → Fin

  🟡 IUsuarioRepository → GetById(sesion.UsuarioId)
  SI usuario no puede autenticarse:
    🟠 RefreshTokenInvalido
    → Fin

  🔵 GenerarNuevoRefreshToken
  🔵 ComputarHash(nuevoRefreshToken)
  🟡 Sesion → Renovar { nuevoHash, nuevaExpiracion }
  🟠 SesionRenovada

  🔵 GenerarAccessToken
  🟠 AccessTokenGenerado

  → Retornar { accessToken, refreshToken, expiraEn }
```

---

## PROCESO 4: Logout

```
👤 Usuario (autenticado)
  → 🔵 CerrarSesion
      Input: { refreshToken, accessToken }

  🔵 ComputarHash(refreshToken)
  🟡 ISesionRepository → GetActivaByRefreshTokenHash(hash)

  SI sesión encontrada:
    🟡 Sesion → Revocar
    🟠 SesionRevocada

  🟣 POLÍTICA: Si EnableTokenBlacklist Y accessToken no vacío:
    🔵 JwtService → ExtraerJti(accessToken)
    🔵 JwtService → ExtraerExpiracion(accessToken)
    🔵 ITokenBlacklistService → Agregar(jti, ttl)
    🟠 TokenAgregadoABlacklist

  🟠 LogoutExitoso
```

---

## PROCESO 5: Token Blacklist — Verificación en cada Request

```
🤖 TokenBlacklistMiddleware (ejecuta en cada request HTTP)

  → Extraer Bearer token del header Authorization
  SI no hay token:
    → Continuar (request anónimo)

  → Extraer JTI del token (sin validar firma)
  SI JTI extraído:
    🟢 ITokenBlacklistService → EstaEnBlacklist(jti)

    SI está en blacklist:
      🟠 TokenRevocado
      → Retornar HTTP 401 { message: "El token ha sido revocado." }
      → Fin

  → Continuar al siguiente middleware (UseAuthentication, UseAuthorization)
```

---

## PROCESO 6: Gestión de Sesiones (usuario propio)

```
👤 Usuario → 🔵 VerMisSesiones
  🟢 ISesionRepository → GetActivasByUsuario(currentUser.Id)
  → Retornar lista de SesionDto

👤 Usuario → 🔵 CerrarSesionEspecifica { sesionId }
  🟡 ISesionRepository → GetById(sesionId)
  → Verificar sesion.UsuarioId == currentUser.Id
  🟡 Sesion → Revocar
  🟠 SesionRevocada

👤 Usuario → 🔵 CerrarOtrasSesiones { sesionActualId }
  🟢 ISesionRepository → GetActivasByUsuario(currentUser.Id)
  → Para cada sesión donde Id != sesionActualId:
    🟡 Sesion → Revocar
  🟠 OtrasSesionesRevocadas
```

---

**Estado:** ✅ Completo  
**Fecha:** 2026-04-15
