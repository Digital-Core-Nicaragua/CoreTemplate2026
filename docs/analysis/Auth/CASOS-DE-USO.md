# Auth — Casos de Uso

---

## CU-001: Registrar Usuario

**Actor**: Usuario anónimo / Administrador  
**Precondición**: El email no existe en el sistema  
**Postcondición**: Usuario creado en estado Pendiente  

**Flujo principal**:
1. El actor envía: nombre, email, contraseña
2. El sistema valida que el email no esté registrado
3. El sistema valida que la contraseña cumpla la política
4. El sistema crea el usuario en estado `Pendiente`
5. El sistema asigna el rol `User` por defecto
6. El sistema dispara el evento `UsuarioRegistradoEvent`
7. El sistema retorna el ID del usuario creado

**Flujos alternativos**:
- 2a. El email ya existe → Error 409 "El email ya está registrado"
- 3a. La contraseña no cumple la política → Error 400 con detalle de requisitos

---

## CU-002: Login

**Actor**: Usuario registrado  
**Precondición**: El usuario existe y está activo  
**Postcondición**: AccessToken y RefreshToken generados  

**Flujo principal**:
1. El actor envía: email, contraseña
2. El sistema busca el usuario por email
3. El sistema verifica que la cuenta no esté bloqueada
4. El sistema verifica que la cuenta esté activa
5. El sistema valida la contraseña con BCrypt
6. El sistema resetea el contador de intentos fallidos
7. Si 2FA está activo para el usuario → ir a flujo 2FA (CU-008)
8. El sistema genera AccessToken y RefreshToken
9. El sistema registra el login exitoso en auditoría
10. El sistema retorna AccessToken + RefreshToken + datos básicos del usuario

**Flujos alternativos**:
- 2a. El usuario no existe → Error 401 "Credenciales inválidas" (no revelar si existe o no)
- 3a. Cuenta bloqueada → Error 401 con fecha de desbloqueo
- 4a. Cuenta inactiva → Error 401 "La cuenta está inactiva"
- 5a. Contraseña incorrecta → Incrementar contador → Error 401 "Credenciales inválidas"
- 5b. Contador llega al límite → Bloquear cuenta → Error 401 con mensaje de bloqueo

---

## CU-003: Refresh Token

**Actor**: Cliente autenticado (con RefreshToken)  
**Precondición**: RefreshToken válido y no expirado  
**Postcondición**: Nuevo AccessToken y RefreshToken generados  

**Flujo principal**:
1. El cliente envía el RefreshToken
2. El sistema busca el RefreshToken en la base de datos
3. El sistema verifica que no esté revocado
4. El sistema verifica que no esté expirado
5. El sistema verifica que el usuario asociado esté activo
6. El sistema revoca el RefreshToken actual
7. El sistema genera nuevo AccessToken y RefreshToken
8. El sistema retorna los nuevos tokens

**Flujos alternativos**:
- 2a. RefreshToken no encontrado → Error 401
- 3a. RefreshToken revocado → Error 401 (posible robo de token)
- 4a. RefreshToken expirado → Error 401 "Sesión expirada, inicie sesión nuevamente"
- 5a. Usuario inactivo o bloqueado → Error 401

---

## CU-004: Logout

**Actor**: Usuario autenticado  
**Precondición**: Usuario tiene sesión activa  
**Postcondición**: RefreshToken revocado  

**Flujo principal**:
1. El actor envía el RefreshToken a revocar
2. El sistema busca y revoca el RefreshToken
3. El sistema registra el logout en auditoría
4. El sistema retorna confirmación

---

## CU-005: Cambiar Contraseña

**Actor**: Usuario autenticado  
**Precondición**: Usuario autenticado con AccessToken válido  
**Postcondición**: Contraseña actualizada, todos los RefreshTokens revocados  

**Flujo principal**:
1. El actor envía: contraseña actual, nueva contraseña, confirmación
2. El sistema valida que la nueva contraseña y confirmación coincidan
3. El sistema valida la contraseña actual con BCrypt
4. El sistema valida que la nueva contraseña cumpla la política
5. El sistema actualiza el hash de la contraseña
6. El sistema revoca todos los RefreshTokens activos del usuario
7. El sistema registra el cambio en auditoría
8. El sistema retorna confirmación

**Flujos alternativos**:
- 2a. Nueva contraseña y confirmación no coinciden → Error 400
- 3a. Contraseña actual incorrecta → Error 400 "La contraseña actual es incorrecta"
- 4a. Nueva contraseña no cumple política → Error 400 con detalle

---

## CU-006: Solicitar Restablecimiento de Contraseña

**Actor**: Usuario anónimo  
**Precondición**: Ninguna  
**Postcondición**: Token de restablecimiento generado  

**Flujo principal**:
1. El actor envía el email
2. El sistema busca el usuario por email
3. El sistema genera un token de restablecimiento con expiración
4. El sistema dispara el evento `RestablecimientoSolicitadoEvent` (el sistema implementador envía el email)
5. El sistema retorna siempre éxito (no revelar si el email existe)

---

## CU-007: Restablecer Contraseña

**Actor**: Usuario anónimo (con token de restablecimiento)  
**Precondición**: Token válido y no expirado  
**Postcondición**: Contraseña actualizada, token invalidado  

**Flujo principal**:
1. El actor envía: token, nueva contraseña, confirmación
2. El sistema valida el token (existe, no expirado, no usado)
3. El sistema valida que nueva contraseña y confirmación coincidan
4. El sistema valida que la nueva contraseña cumpla la política
5. El sistema actualiza el hash de la contraseña
6. El sistema marca el token como usado
7. El sistema revoca todos los RefreshTokens activos
8. El sistema registra el restablecimiento en auditoría

---

## CU-008: Activar 2FA

**Actor**: Usuario autenticado  
**Precondición**: `TwoFactorEnabled = true` en configuración  
**Postcondición**: 2FA activado para el usuario  

**Flujo principal**:
1. El actor solicita activar 2FA
2. El sistema genera una clave secreta TOTP
3. El sistema genera el QR code URI (otpauth://)
4. El sistema genera 8 códigos de recuperación
5. El sistema retorna: QR code URI, clave secreta, códigos de recuperación
6. El actor escanea el QR y envía el primer código TOTP para confirmar
7. El sistema valida el código TOTP
8. El sistema activa el 2FA para el usuario
9. El sistema registra en auditoría

---

## CU-009: Login con 2FA

**Actor**: Usuario con 2FA activo  
**Precondición**: Credenciales válidas, 2FA activo  
**Postcondición**: AccessToken y RefreshToken generados  

**Flujo principal**:
1. El usuario completa el login normal (CU-002 pasos 1-6)
2. El sistema detecta que el usuario tiene 2FA activo
3. El sistema genera un token temporal de corta duración (5 minutos)
4. El sistema retorna el token temporal con indicador `requires2FA: true`
5. El cliente envía el token temporal + código TOTP
6. El sistema valida el token temporal
7. El sistema valida el código TOTP
8. El sistema genera AccessToken y RefreshToken definitivos
9. El sistema registra en auditoría

**Flujos alternativos**:
- 7a. Código TOTP inválido → Error 401
- 7b. El actor usa código de recuperación → validar y marcar como usado

---

## CU-010: Gestionar Roles (Admin)

**Actor**: Administrador  
**Precondición**: Usuario autenticado con permiso `Usuarios.Roles.Gestionar`  

**Casos**:
- Crear rol: nombre + lista de permisos
- Editar rol: actualizar nombre o permisos
- Eliminar rol: solo si no tiene usuarios asignados
- Listar roles: paginado con cantidad de usuarios por rol
- Ver detalle de rol: nombre, permisos, usuarios

---

## CU-011: Gestionar Usuarios (Admin)

**Actor**: Administrador  
**Precondición**: Usuario autenticado con permiso `Usuarios.Gestionar`  

**Casos**:
- Listar usuarios: paginado, filtrable por estado y rol
- Ver detalle: perfil completo, roles, último acceso
- Activar usuario
- Desactivar usuario
- Desbloquear usuario
- Asignar rol
- Quitar rol
