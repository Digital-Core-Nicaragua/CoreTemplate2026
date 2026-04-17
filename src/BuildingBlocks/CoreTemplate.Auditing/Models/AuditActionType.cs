namespace CoreTemplate.Auditing.Models;

/// <summary>
/// Tipo de accion registrada en el log de auditoria.
/// </summary>
public enum AuditActionType
{
    Created = 1,
    Updated = 2,
    Deleted = 3,
    Login = 4,
    Logout = 5,
    LoginFailed = 6,
    PasswordChanged = 7,
    PasswordReset = 8,
    TwoFactorEnabled = 9,
    TwoFactorDisabled = 10,
    SessionRevoked = 11,
    RoleAssigned = 12,
    RoleRemoved = 13
}
