using PreciousMetalsTradingSystem.Application.Common.Exceptions;
using PreciousMetalsTradingSystem.Application.Common.Extensions;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace PreciousMetalsTradingSystem.Application.Common.Extensions
{
    public static class IQueryableExtension
    {
        public static IQueryable<T> Sort<T>(this IQueryable<T> query, string? sort)
        {
            if (string.IsNullOrEmpty(sort))
            {
                return query;
            }
            var sortColumns = sort
                .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .ToArray();
            if (sortColumns == null || sortColumns.Length == 0)
            {
                return query;
            }

            IOrderedQueryable<T> sorted;
            if (sortColumns[0].StartsWith('-'))
            {
                sorted = query.OrderByDescending(sortColumns[0].TrimStart('-'));
            }
            else
            {
                sorted = query.OrderBy(sortColumns[0]);
            }
            for (int i = 1; i < sortColumns.Length; i++)
            {
                if (sortColumns[i].StartsWith('-'))
                {
                    sorted = sorted.ThenByDescending(sortColumns[i].TrimStart('-'));
                }
                else
                {
                    sorted = sorted.ThenBy(sortColumns[i]);
                }
            }

            return sorted;
        }

        public static IOrderedQueryable<T> OrderBy<T>(this IQueryable<T> source, string propertyName)
        {
            return source.OrderBy(ToLambda<T>(propertyName));
        }

        public static IOrderedQueryable<T> OrderByDescending<T>(this IQueryable<T> source, string propertyName)
        {
            return source.OrderByDescending(ToLambda<T>(propertyName));
        }

        public static IOrderedQueryable<T> ThenBy<T>(this IOrderedQueryable<T> source, string propertyName)
        {
            return source.ThenBy(ToLambda<T>(propertyName));
        }

        public static IOrderedQueryable<T> ThenByDescending<T>(this IOrderedQueryable<T> source, string propertyName)
        {
            return source.ThenByDescending(ToLambda<T>(propertyName));
        }

        private static Expression<Func<T, object>> ToLambda<T>(string propertyName)
        {
            var parameter = Expression.Parameter(typeof(T), "x");
            Expression member = parameter;

            try
            {
                foreach (var namePart in propertyName.Split('.'))
                {
                    member = Expression.Property(member, namePart);
                }
            }
            catch (Exception)
            {
                throw new PropertyAccessException(propertyName);
            }

            if (member.Type.IsEnum)
            {
                var enumValues = Enum.GetValues(member.Type)
                    .Cast<Enum>()
                    .Select(e => new { Key = e, Value = e.ToEnumOrderValue() })
                    .ToArray();

                var caseExpression = enumValues
                    .Select(e => Expression.Condition(
                        Expression.Equal(member, Expression.Constant(e.Key)),
                        Expression.Constant(e.Value),
                        Expression.Constant(string.Empty) 
                    ))
                    .Aggregate((prev, next) => Expression.Condition(
                        Expression.Equal(prev, Expression.Constant(string.Empty)),
                        next,
                        prev
                    ));

                member = caseExpression;
            }

            // Cast member to object to handle value types like DateOnly
            var convertedMember = Expression.Convert(member, typeof(object));
            return Expression.Lambda<Func<T, object>>(convertedMember, parameter);
        }
    }
}
