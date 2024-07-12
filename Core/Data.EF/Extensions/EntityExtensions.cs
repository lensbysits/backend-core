using AutoMapper;
using AutoMapper.QueryableExtensions;
using Lens.Core.Data.EF.Entities;
using Lens.Core.Lib.Exceptions;
using Lens.Core.Lib.Models;
using LinqKit;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;
using EFCore = Microsoft.EntityFrameworkCore.EF;

namespace Lens.Core.Data.EF;

public static class EntityExtensions
{
    public static async Task<TEntity> GetById<TEntity>(this IQueryable<TEntity> entities, Guid id)
        where TEntity : class, IIdEntity
    {
        TEntity? result = await entities.FirstOrDefaultAsync(entity => entity.Id == id);
        if (result == null)
        {
            throw new NotFoundException($"{typeof(TEntity).Name} with id {id} not found");
        }
        return result;
    }

    public static IQueryable<TEntity> GetByQueryModel<TEntity>(this IQueryable<TEntity> entities, QueryModel queryModel, Expression<Func<TEntity, bool>>? searchPredicate = null)
       where TEntity : class, IEntity
    {
        // apply default filters
        if (typeof(ITagsEntity).IsAssignableFrom(typeof(TEntity)))
        {
            if (!string.IsNullOrEmpty(queryModel.Tags))
            {
                var tagWhere = queryModel.Tags.Split(",")
                    .Select(tag => {
                        Expression<Func<TEntity, bool>> expr = entity => EFCore.Property<string>(entity, ShadowProperties.Tag).Contains($"\"{tag}\"");
                        return expr;
                    });
                entities = entities
                    .Where(tagWhere.ToOrCompositePredicate());
            }
            if (!string.IsNullOrEmpty(queryModel.Tag))
            {
                entities = entities
                    .Where(entity => EFCore.Property<string>(entity, ShadowProperties.Tag).Contains($"\"{queryModel.Tag}\""));
            }
        }

        if (typeof(ICreatedUpdatedEntity).IsAssignableFrom(typeof(TEntity)))
        {
            if (!string.IsNullOrEmpty(queryModel.CreatedBy))
            {
                entities = entities
                    .Where(entity => EFCore.Property<string>(entity, ShadowProperties.CreatedBy) == queryModel.CreatedBy);
            }

            if (!string.IsNullOrEmpty(queryModel.UpdatedBy))
            {
                entities = entities
                    .Where(entity => EFCore.Property<string>(entity, ShadowProperties.UpdatedBy) == queryModel.UpdatedBy);
            }

            if (queryModel.CreatedSince.HasValue)
            {
                entities = entities
                    .Where(entity => EFCore.Property<DateTime>(entity, ShadowProperties.CreatedOn) >= queryModel.CreatedSince);
            }

            if (queryModel.UpdatedSince.HasValue)
            {
                entities = entities
                    .Where(entity => EFCore.Property<DateTime>(entity, ShadowProperties.UpdatedOn) >= queryModel.UpdatedSince);
            }
        }

        // search by predicate
        if (searchPredicate != null)
        {
            entities = entities.Where(searchPredicate);
        }

        // apply sorting
        entities = ApplySort(entities, queryModel);

        return entities;
    }

    public static async Task<(IQueryable<TEntity> query, int totalSize)> ApplyPaging<TEntity>(this IQueryable<TEntity> entities, QueryModel queryModel)
    {
        var totalSize = await entities.CountAsync();

        if (!queryModel.NoLimit)
        {
            var pagingQuery = entities
                .Skip(queryModel.Offset)
                .Take(queryModel.Limit);

            return (pagingQuery, totalSize);
        }

        return (entities, totalSize);
    }

    public static async Task<ResultPagedListModel<TModel>> ToPagedResultListModel<TEntity, TModel>(this (IQueryable<TEntity> query, int totalSize) pagingResult, QueryModel queryModel, IConfigurationProvider configuration)
    {
        var result = await pagingResult.query
            .ProjectTo<TModel>(configuration)
            .ToListAsync();

        return new ResultPagedListModel<TModel>(result)
        {
            TotalSize = pagingResult.totalSize,
            OriginalQueryModel = queryModel
        };
    }

    public static void DeleteWhere<TEntity>(this DbSet<TEntity> entities, Expression<Func<TEntity, bool>> predicate)
        where TEntity : class, IEntity
    {
        var entitiesToDelete = entities.Where(predicate);
        entities.DeleteRange(entitiesToDelete);
    }

    public static void DeleteRange<TEntity>(this DbSet<TEntity> entities, IQueryable<TEntity> entitiesToDelete)
        where TEntity : class, IEntity
    {
        entities.AttachRange(entitiesToDelete);
        entities.RemoveRange(entitiesToDelete);
    }

    public static Expression<Func<TEntity, bool>> ToAndCompositePredicate<TEntity>(this IEnumerable<Expression<Func<TEntity, bool>>> expressions)
        where TEntity : class, IEntity
    {
        ExpressionStarter<TEntity>? predicate = null;
        foreach (var expression in expressions)
        {
            predicate = (predicate == null) ? PredicateBuilder.New(expression) : predicate.And(expression);
        }

        return predicate;
    }

    public static Expression<Func<TEntity, bool>> ToOrCompositePredicate<TEntity>(this IEnumerable<Expression<Func<TEntity, bool>>> expressions)
        where TEntity : class, IEntity
    {
        ExpressionStarter<TEntity>? predicate = null;
        foreach (var expression in expressions)
        {
            predicate = (predicate == null) ? PredicateBuilder.New(expression) : predicate.Or(expression);
        }

        return predicate;
    }

    public static TEntity CloneEntity<TEntity>(this TEntity sourceEntity, ApplicationDbContext applicationDbContext)
        where TEntity : BaseEntity
    {
        var newEntity = (TEntity)applicationDbContext.Entry(sourceEntity).CurrentValues.ToObject();
        newEntity.Id = Guid.NewGuid();

        return newEntity;
    }

    #region Private static methods

    private static IQueryable<TEntity> ApplySort<TEntity>(IQueryable<TEntity> entities, QueryModel queryModel)
    {
        if (!entities.Any()) return entities;

        // default sorting when 'order by' query param is missing.
        if (string.IsNullOrWhiteSpace(queryModel.OrderBy) && typeof(ICreatedUpdatedEntity).IsAssignableFrom(typeof(TEntity)))
        {
            entities.OrderByDescending(entity => EFCore.Property<DateTime>(entity!, ShadowProperties.UpdatedOn));
            return entities;
        }

        var orderParams = queryModel.OrderBy?.Split(',', StringSplitOptions.TrimEntries) ?? Array.Empty<string>();
        var propertyInfos = typeof(TEntity).GetProperties(BindingFlags.Public | BindingFlags.Instance);
        var orderQueryBuilder = new StringBuilder();

        foreach (var param in orderParams)
        {
            if (string.IsNullOrWhiteSpace(param)) continue;

            var propertyFromQueryName = param.Split(" ")[0];
            var objectProperty = propertyInfos
                .FirstOrDefault(pi => pi.Name.Equals(propertyFromQueryName, StringComparison.InvariantCultureIgnoreCase));

            if (objectProperty == null) continue;

            var sortingOrder = param.EndsWith(" desc") ? "descending" : "ascending";
            orderQueryBuilder.Append($"{objectProperty.Name} {sortingOrder}, ");
        }

        var orderQuery = orderQueryBuilder.ToString().TrimEnd(',', ' ');

        return string.IsNullOrWhiteSpace(orderQuery) ? entities : entities.OrderBy(orderQuery);
    }

    #endregion Private static methods
}