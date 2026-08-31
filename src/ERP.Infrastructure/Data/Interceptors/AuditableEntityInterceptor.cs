using System.Text.Json;
using ERP.Application.Common.Interfaces;
using ERP.Domain.Common;
using ERP.Domain.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace ERP.Infrastructure.Data.Interceptors;

/// <summary>
/// Interceptor do EF Core para:
/// 1. Preencher automaticamente campos de auditoria (CriadoEm/Por, AlteradoEm/Por).
/// 2. Gerar registros detalhados na tabela AuditLog em JSONB.
/// </summary>
public class AuditableEntityInterceptor : SaveChangesInterceptor
{
    private readonly ICurrentUserService _currentUserService;
    private readonly ICurrentEmpresaService _currentEmpresaService;

    public AuditableEntityInterceptor(
        ICurrentUserService currentUserService,
        ICurrentEmpresaService currentEmpresaService)
    {
        _currentUserService = currentUserService;
        _currentEmpresaService = currentEmpresaService;
    }

    public override InterceptionResult<int> SavingChanges(DbContextEventData eventData, InterceptionResult<int> result)
    {
        UpdateEntities(eventData.Context);
        CreateAuditLogs(eventData.Context);
        return base.SavingChanges(eventData, result);
    }

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(DbContextEventData eventData, InterceptionResult<int> result, CancellationToken cancellationToken = default)
    {
        UpdateEntities(eventData.Context);
        CreateAuditLogs(eventData.Context);
        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    private void UpdateEntities(DbContext? context)
    {
        if (context == null) return;

        var userId = _currentUserService.UserId;
        var now = DateTime.UtcNow;

        foreach (var entry in context.ChangeTracker.Entries<BaseAuditableEntity>())
        {
            if (entry.State == EntityState.Added)
            {
                entry.Entity.CriadoEm = now;
                entry.Entity.CriadoPor = userId;
            }

            if (entry.State == EntityState.Added || entry.State == EntityState.Modified)
            {
                entry.Entity.AlteradoEm = now;
                entry.Entity.AlteradoPor = userId;
            }
        }
    }

    private void CreateAuditLogs(DbContext? context)
    {
        if (context == null) return;

        var userId = _currentUserService.UserId;
        var userName = _currentUserService.UserName;
        var empresaId = _currentEmpresaService.EmpresaId;
        var now = DateTime.UtcNow;

        var auditEntries = new List<AuditLog>();

        foreach (var entry in context.ChangeTracker.Entries<BaseEntity>())
        {
            if (entry.Entity is AuditLog || entry.State == EntityState.Detached || entry.State == EntityState.Unchanged)
                continue;

            var oldValues = new Dictionary<string, object?>();
            var newValues = new Dictionary<string, object?>();
            var changedColumns = new List<string>();

            var tableName = entry.Metadata.GetTableName() ?? entry.Entity.GetType().Name;
            var primaryKey = entry.Property("Id").CurrentValue?.ToString() ?? "0";

            foreach (var property in entry.Properties)
            {
                string propertyName = property.Metadata.Name;

                if (property.Metadata.IsPrimaryKey())
                    continue;

                switch (entry.State)
                {
                    case EntityState.Added:
                        newValues[propertyName] = property.CurrentValue;
                        break;

                    case EntityState.Deleted:
                        oldValues[propertyName] = property.OriginalValue;
                        break;

                    case EntityState.Modified:
                        if (property.IsModified)
                        {
                            changedColumns.Add(propertyName);
                            oldValues[propertyName] = property.OriginalValue;
                            newValues[propertyName] = property.CurrentValue;
                        }
                        break;
                }
            }

            var auditLog = new AuditLog
            {
                EmpresaId = empresaId,
                UserId = userId,
                UserName = userName,
                Timestamp = now,
                TableName = tableName,
                EntityId = primaryKey,
                Action = entry.State.ToString().ToUpper(),
                OldValues = oldValues.Count == 0 ? null : JsonSerializer.Serialize(oldValues),
                NewValues = newValues.Count == 0 ? null : JsonSerializer.Serialize(newValues),
                ChangedColumns = changedColumns.Count == 0 ? null : JsonSerializer.Serialize(changedColumns)
            };

            auditEntries.Add(auditLog);
        }

        if (auditEntries.Count > 0)
        {
            context.Set<AuditLog>().AddRange(auditEntries);
        }
    }
}
