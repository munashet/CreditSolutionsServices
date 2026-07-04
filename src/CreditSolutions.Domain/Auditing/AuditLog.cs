using CreditSolutions.Domain.Common;

namespace CreditSolutions.Domain.Auditing;

public sealed class AuditLog : Entity
{
    private AuditLog() { }

    public AuditLog(string action, string entityName, Guid entityId, string details, Guid? userId = null)
    {
        Action = action;
        EntityName = entityName;
        EntityId = entityId;
        Details = details;
        UserId = userId;
    }

    public string Action { get; private set; } = string.Empty;
    public string EntityName { get; private set; } = string.Empty;
    public Guid EntityId { get; private set; }
    public string Details { get; private set; } = string.Empty;
    public Guid? UserId { get; private set; }
}
