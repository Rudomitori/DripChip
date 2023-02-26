using System.Linq.Expressions;

namespace Common.Core.Extensions;

public static class LinqExtensions
{
    public static IQueryable<TEntity> WithPaging<TEntity, TOrderKey>(
        this IQueryable<TEntity> queryable,
        Expression<Func<TEntity, TOrderKey>> orderKeySelector,
        int offset,
        int size,
        bool descendingOrder = false
    )
    {
        return descendingOrder
            ? queryable.OrderByDescending(orderKeySelector).Skip(offset).Take(size)
            : queryable.OrderBy(orderKeySelector).Skip(offset).Take(size);
    }
}
