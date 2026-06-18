using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using Assura.Domain.Entities;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace Assura.Infrastructure.Persistence;

public class AuditEntry
{
    public AuditEntry(EntityEntry entry)
    {
        Entry = entry;
    }

    public EntityEntry Entry { get; }
    public string EntityName { get; set; } = string.Empty;
    public string Action { get; set; } = string.Empty;
    public string? CreatedBy { get; set; }
    public string? IpAddress { get; set; }
    public Dictionary<string, object?> KeyValues { get; } = new();
    public Dictionary<string, object?> OldValues { get; } = new();
    public Dictionary<string, object?> NewValues { get; } = new();
    public List<PropertyEntry> TemporaryProperties { get; } = new();

    public bool HasTemporaryProperties => TemporaryProperties.Count > 0;

    public AuditLog ToAuditLog()
    {
        return new AuditLog
        {
            EntityName = EntityName,
            EntityId = KeyValues.Count > 0 ? JsonSerializer.Serialize(KeyValues) : "N/A",
            Action = Action,
            OldValues = OldValues.Count == 0 ? null : JsonSerializer.Serialize(OldValues),
            NewValues = NewValues.Count == 0 ? null : JsonSerializer.Serialize(NewValues),
            IpAddress = IpAddress ?? "N/A"
        };
    }
}
