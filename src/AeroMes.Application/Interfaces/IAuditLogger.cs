using AeroMes.Application.Auth;

namespace AeroMes.Application.Interfaces;

public interface IAuditLogger
{
    void Log(SecurityAuditEvent auditEvent);
}
