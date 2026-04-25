using Microsoft.EntityFrameworkCore;
using MundoVs.Core.Entities;
using MundoVs.Core.Interfaces;
using MundoVs.Infrastructure.Data;
using System.Linq.Expressions;

namespace MundoVs.Infrastructure.Repositories;

public class Repository<T> : IRepository<T> where T : class
{
    protected readonly CrmDbContext _context;
    protected readonly DbSet<T> _dbSet;

    public Repository(CrmDbContext context)
    {
        _context = context;
        _dbSet = context.Set<T>();
    }

    public virtual async Task<T?> GetByIdAsync(Guid id)
    {
        return await _dbSet.FirstOrDefaultAsync(BuildIdPredicate(id));
    }

    public virtual async Task<IEnumerable<T>> GetAllAsync()
    {
        return await _dbSet.ToListAsync();
    }

    public virtual async Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate)
    {
        return await _dbSet.Where(predicate).ToListAsync();
    }

    public virtual async Task<T> AddAsync(T entity)
    {
        if (entity is BaseEntity baseEntity)
        {
            if (baseEntity.Id == Guid.Empty)
            {
                baseEntity.Id = Guid.NewGuid();
            }

            if (baseEntity.CreatedAt == default)
            {
                baseEntity.CreatedAt = DateTime.UtcNow;
            }
        }

        await _dbSet.AddAsync(entity);
        await _context.SaveChangesAsync();
        return entity;
    }

    public virtual async Task UpdateAsync(T entity)
    {
        await PreserveEmpresaIdAsync(entity);

        // If another instance with the same key is already tracked, detach it to avoid identity conflicts.
        var key = _context.Model.FindEntityType(typeof(T))?.FindPrimaryKey();
        if (key != null)
        {
            var keyValues = key.Properties
                .Select(p => typeof(T).GetProperty(p.Name)?.GetValue(entity))
                .ToArray();

            var existingEntry = _context.ChangeTracker.Entries<T>()
                .FirstOrDefault(e => key.Properties
                    .Select(p => typeof(T).GetProperty(p.Name)?.GetValue(e.Entity))
                    .SequenceEqual(keyValues));

            if (existingEntry != null && !ReferenceEquals(existingEntry.Entity, entity))
            {
                existingEntry.State = EntityState.Detached;
            }
        }

        _context.Attach(entity);
        _context.Entry(entity).State = EntityState.Modified;
        
        await _context.SaveChangesAsync();
    }

    private async Task PreserveEmpresaIdAsync(T entity)
    {
        var empresaIdProperty = typeof(T).GetProperty("EmpresaId");
        if (empresaIdProperty?.PropertyType != typeof(Guid))
        {
            return;
        }

        if (empresaIdProperty.GetValue(entity) is not Guid empresaId || empresaId != Guid.Empty)
        {
            return;
        }

        if (entity is not BaseEntity baseEntity || baseEntity.Id == Guid.Empty)
        {
            return;
        }

        var existingEntity = await _dbSet.AsNoTracking().FirstOrDefaultAsync(BuildIdPredicate(baseEntity.Id));
        if (existingEntity == null)
        {
            return;
        }

        if (empresaIdProperty.GetValue(existingEntity) is Guid existingEmpresaId && existingEmpresaId != Guid.Empty)
        {
            empresaIdProperty.SetValue(entity, existingEmpresaId);
        }
    }

    public virtual async Task DeleteAsync(T entity)
    {
        _dbSet.Remove(entity);
        await _context.SaveChangesAsync();
    }

    public virtual async Task<bool> ExistsAsync(Guid id)
    {
        return await _dbSet.AnyAsync(BuildIdPredicate(id));
    }

    private Expression<Func<T, bool>> BuildIdPredicate(Guid id)
    {
        var entityType = _context.Model.FindEntityType(typeof(T))
            ?? throw new InvalidOperationException($"No se encontró metadata para {typeof(T).Name}.");

        var primaryKey = entityType.FindPrimaryKey()
            ?? throw new InvalidOperationException($"{typeof(T).Name} no tiene llave primaria configurada.");

        if (primaryKey.Properties.Count != 1)
            throw new InvalidOperationException($"{typeof(T).Name} tiene llave compuesta y no es compatible con GetByIdAsync(Guid).");

        var keyProperty = primaryKey.Properties[0];
        if (keyProperty.ClrType != typeof(Guid))
            throw new InvalidOperationException($"{typeof(T).Name} usa una llave primaria no Guid y no es compatible con GetByIdAsync(Guid).");

        var parameter = Expression.Parameter(typeof(T), "e");
        var property = Expression.Call(
            typeof(EF),
            nameof(EF.Property),
            [typeof(Guid)],
            parameter,
            Expression.Constant(keyProperty.Name));

        var body = Expression.Equal(property, Expression.Constant(id));
        return Expression.Lambda<Func<T, bool>>(body, parameter);
    }

    public virtual async Task<int> CountAsync()
    {
        return await _dbSet.CountAsync();
    }
}
