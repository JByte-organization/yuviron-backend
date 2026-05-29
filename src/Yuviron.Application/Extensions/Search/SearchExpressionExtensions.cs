using System.Linq.Expressions;

namespace Yuviron.Application.Extensions;

public static class SearchExpressionExtensions
{
    public static IQueryable<TEntity> WhereAny<TEntity>(
        this IQueryable<TEntity> source,
        IEnumerable<Expression<Func<TEntity, bool>>> predicates)
    {
        var combinedPredicate = predicates.Aggregate(False<TEntity>(), static (current, predicate) => current.Or(predicate));

        return source.Where(combinedPredicate);
    }

    private static Expression<Func<TEntity, bool>> False<TEntity>() => _ => false;

    private static Expression<Func<TEntity, bool>> Or<TEntity>(
        this Expression<Func<TEntity, bool>> left,
        Expression<Func<TEntity, bool>> right)
    {
        var parameter = left.Parameters[0];
        var rightBody = new ReplaceParameterVisitor(right.Parameters[0], parameter).Visit(right.Body)!;

        return Expression.Lambda<Func<TEntity, bool>>(
            Expression.OrElse(left.Body, rightBody),
            parameter);
    }

    private sealed class ReplaceParameterVisitor : ExpressionVisitor
    {
        private readonly ParameterExpression _source;
        private readonly ParameterExpression _target;

        public ReplaceParameterVisitor(ParameterExpression source, ParameterExpression target)
        {
            _source = source;
            _target = target;
        }

        protected override Expression VisitParameter(ParameterExpression node)
        {
            return ReferenceEquals(node, _source) ? _target : base.VisitParameter(node);
        }
    }
}
