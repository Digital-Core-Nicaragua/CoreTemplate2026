# Casos de Uso — Autenticación

> **Grupo:** Autenticación  
> **Códigos:** CU-AUTH-001 a CU-AUTH-012  
> **Fecha:** 2026-04-15

---

## CU-AUTH-001: Registrar Usuario

**Actor:** Administrador / Sistema  
**Prioridad:** Crítica  
**Aggregate:** Usuario  
**Endpoint:** `POST /api/auth/registro`

**Precondiciones:**
- Email no registrado en el tenant

**Flujo Principal:**
1. Actor envía `{ email, nombre, password, confirmPassword, tipoUsuario? }`
2. Sistema valida política de contraseña
3. Sistema verifica email único
4. Sistema crea usuario en estado `Pendiente`
5. Sistema asigna rol `User` por defecto
6. Sistema retorna `{ id }`

**Flujos Alternativos:**
- 2a. Contraseña no cumple política → Error con detalle
- 3a. Email ya registrado → Error "El email ya está registrado"

**Postcondiciones:**
- Usuario creado en estado `Pendiente`
- Evento `UsuarioRegistradoEvent` disparado

---

## CU-AUTH-002: Iniciar Sesión con Credenciales

**Actor:** Usuario  
**Prioridad:** Crítica  
**Aggregates:** Usuario, Sesion  
**Endpoint:** `POST /api/auth/login`

**Precondiciones:**
- Usuario existe y está activo

**Flujo Principal:**
1. Actor envía `{ email, password, canal, dispositivo? }`
2. Sistema busca usuario por email
3. Sistema verifica estado del usuario
4. Sistema verifica contraseña
5. Sistema resetea intentos fallidos
6. Sistema verifica límite de sesiones
7. Sistema crea sesión con RefreshToken (hash SHA256)
8. Sistema genera AccessToken JWT
9. Sistema retorna `{ accessToken, refreshToken, usuario }`

**Flujos Alternativos:**
- 2a. Email no existe → Error genérico "Credenciales inválidas"
- 3a. Usuario bloqueado → Error "Cuenta bloqueada temporalmente"
- 3b. Usuario inactivo/pendiente → Error "Cuenta inactiva"
- 4a. Contraseña incorrecta → Incrementar intentos, Error genérico
- 4b. Intentos >= máximo → Bloquear cuenta
- 5a. Usuario tiene 2FA activo → Retornar `{ requires2FA: true, tokenTemporal }`
- 6a. Límite alcanzado + CerrarMasAntigua → Cerrar sesión más antigua, continuar
- 6b. Límite alcanzado + BloquearNuevoLogin → Error "Límite de sesiones alcanzado"

**Postcondiciones:**
- Sesión creada y activa
- Auditoría registrada

---

## CU-AUTH-003: Renovar Access Token

**Actor:** Usuario (sistema)  
**Prioridad:** Crítica  
**Aggregate:** Sesion  
**Endpoint:** `POST /api/auth/refresh`

**Precondiciones:**
- Refresh token válido y no expirado

**Flujo Principal:**
1. Actor envía `{ refreshToken }`
2. Sistema computa hash SHA256 del token
3. Sistema busca sesión activa por hash
4. Sistema verifica `EsValida`
5. Sistema verifica que usuario puede autenticarse
6. Sistema rota el refresh token (nuevo hash, nueva expiración)
7. Sistema genera nuevo AccessToken
8. Sistema retorna `{ accessToken, refreshToken, expiraEn }`

**Flujos Alternativos:**
- 3a. Sesión no encontrada → Error "Refresh token inválido"
- 4a. Sesión expirada → Error "Refresh token inválido"
- 5a. Usuario bloqueado/inactivo → Error "Refresh token inválido"

---

## CU-AUTH-004: Cerrar Sesión (Logout)

**Actor:** Usuario autenticado  
**Prioridad:** Crítica  
**Aggregate:** Sesion  
**Endpoint:** `POST /api/auth/logout`

**Precondiciones:**
- Usuario autenticado con AccessToken válido

**Flujo Principal:**
1. Actor envía `{ refreshToken }` con AccessToken en header
2. Sistema computa hash del refreshToken
3. Sistema busca sesión activa por hash
4. Sistema revoca la sesión
5. Si `EnableTokenBlacklist = true`: Sistema agrega AccessToken a blacklist (TTL = tiempo restante)
6. Sistema registra auditoría
7. Sistema retorna éxito

**Postcondiciones:**
- Sesión revocada
- AccessToken en blacklist (si habilitado)
- Auditoría registrada

---

## CU-AUTH-005: Solicitar Restablecimiento de Contraseña

**Actor:** Usuario  
**Prioridad:** Alta  
**Aggregate:** Usuario  
**Endpoint:** `POST /api/auth/solicitar-restablecimiento`

**Flujo Principal:**
1. Actor envía `{ email }`
2. Sistema busca usuario por email
3. Si existe: Sistema genera token (Base64, 64 bytes) y lo agrega al usuario
4. Evento `RestablecimientoSolicitadoEvent` disparado (contiene el token)
5. Sistema retorna éxito **siempre** (no revelar si email existe)

**Nota:** El sistema implementador escucha `RestablecimientoSolicitadoEvent` y envía el email.

---

## CU-AUTH-006: Restablecer Contraseña con Token

**Actor:** Usuario  
**Prioridad:** Alta  
**Aggregate:** Usuario, Sesion  
**Endpoint:** `POST /api/auth/restablecer-password`

**Precondiciones:**
- Token válido y no expirado

**Flujo Principal:**
1. Actor envía `{ token, nuevoPassword, confirmPassword }`
2. Sistema busca usuario con token válido
3. Sistema valida política de contraseña
4. Sistema marca token como usado
5. Sistema cambia contraseña (nuevo hash BCrypt)
6. Sistema revoca todas las sesiones activas
7. Sistema registra auditoría
8. Sistema retorna éxito

**Flujos Alternativos:**
- 2a. Token inválido o expirado → Error
- 3a. Contraseña no cumple política → Error

---

## CU-AUTH-007: Cambiar Contraseña (usuario autenticado)

**Actor:** Usuario autenticado  
**Prioridad:** Alta  
**Aggregate:** Usuario, Sesion  
**Endpoint:** `PUT /api/perfil/cambiar-password`

**Flujo Principal:**
1. Actor envía `{ passwordActual, nuevoPassword, confirmPassword }`
2. Sistema verifica contraseña actual
3. Sistema valida política de nueva contraseña
4. Sistema cambia contraseña
5. Sistema revoca todas las sesiones activas
6. Si `EnableTokenBlacklist = true`: Sistema agrega AccessToken actual a blacklist
7. Sistema registra auditoría
8. Sistema retorna éxito

---

## CU-AUTH-008: Activar 2FA (generar QR)

**Actor:** Usuario autenticado  
**Prioridad:** Media  
**Aggregate:** Usuario  
**Endpoint:** `POST /api/auth/2fa/activar`

**Precondiciones:**
- `AuthSettings:TwoFactorEnabled = true`
- Usuario no tiene 2FA activo

**Flujo Principal:**
1. Sistema genera secret key TOTP aleatoria
2. Sistema genera QR URI (otpauth://totp/...)
3. Sistema genera 8 códigos de recuperación
4. Sistema guarda secret key temporal en usuario
5. Sistema retorna `{ qrCodeUri, secretKey, codigosRecuperacion }`

**Nota:** La activación definitiva ocurre en CU-AUTH-009.

---

## CU-AUTH-009: Confirmar Activación de 2FA

**Actor:** Usuario autenticado  
**Prioridad:** Media  
**Aggregate:** Usuario  
**Endpoint:** `POST /api/auth/2fa/confirmar`

**Precondiciones:**
- Secret key temporal guardada (CU-AUTH-008 completado)

**Flujo Principal:**
1. Actor envía `{ codigo }` (primer código TOTP del autenticador)
2. Sistema valida código TOTP contra secret key temporal
3. Sistema genera 8 nuevos códigos de recuperación (hash SHA256)
4. Sistema activa 2FA definitivamente
5. Sistema retorna éxito

---

## CU-AUTH-010: Verificar Código TOTP en Login

**Actor:** Usuario  
**Prioridad:** Media  
**Aggregates:** Usuario, Sesion  
**Endpoint:** `POST /api/auth/2fa/verificar`

**Precondiciones:**
- Token temporal válido (de CU-AUTH-002 con 2FA)

**Flujo Principal:**
1. Actor envía `{ tokenTemporal, codigo }`
2. Sistema valida token temporal (no expirado, tipo "2fa_temp")
3. Sistema obtiene usuario por id del token
4. Sistema valida código TOTP
5. Si TOTP inválido: Sistema intenta con código de recuperación
6. Sistema crea sesión y genera tokens definitivos
7. Sistema retorna `{ accessToken, refreshToken, usuario }`

---

## CU-AUTH-011: Desactivar 2FA

**Actor:** Usuario autenticado  
**Prioridad:** Media  
**Aggregate:** Usuario  
**Endpoint:** `POST /api/auth/2fa/desactivar`

**Flujo Principal:**
1. Actor envía `{ codigo }` (código TOTP actual)
2. Sistema valida código TOTP
3. Sistema desactiva 2FA (limpia secret key y códigos)
4. Sistema retorna éxito

---

## CU-AUTH-012: Usar Código de Recuperación 2FA

**Actor:** Usuario  
**Prioridad:** Media  
**Aggregate:** Usuario  
**Endpoint:** `POST /api/auth/2fa/verificar` (mismo endpoint, código de recuperación)

**Flujo Principal:**
1. Actor envía `{ tokenTemporal, codigo }` (código de recuperación en lugar de TOTP)
2. Sistema intenta validar como TOTP → falla
3. Sistema hashea el código y busca en códigos de recuperación
4. Sistema marca código como usado
5. Sistema continúa con creación de sesión y tokens

---

**Fecha:** 2026-04-15
