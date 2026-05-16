using System.Linq.Expressions;
using Data.Shared.Data.Repositories;
using Data.Shared.Errors;
using Microsoft.Extensions.Logging;
using Models.Shared.Common;
using Models.Shared.Converters;
using Models.Shared.Entities;
using Models.Shared.Models;
using NetBlocks.Models;

namespace Data.Shared.Managers;

public abstract class Manager<TEntity, TModel, TRepository, TConverter> : IManager<TEntity, TModel>
    where TEntity : class, IEntity
    where TModel : class, IModel, new()
    where TRepository : IRepository<TEntity>
    where TConverter : IEntityToModelConverter<TEntity, TModel>
{
    protected Manager(TRepository repository, ILogger<Manager<TEntity, TModel, TRepository, TConverter>> logger)
    {
        Repository = repository;
        Logger = logger;
    }

    protected TRepository Repository;
    protected ILogger<Manager<TEntity, TModel, TRepository, TConverter>> Logger;
    
    public virtual async Task<ResultContainer<bool>> Exists(TModel entity)
    {
        try
        {
            var exists = await Repository.ExistsAsync(entity.Id);
            return ResultContainer<bool>.CreatePassResult(exists);
        }
        catch (Exception ex)
        {
            return ResultContainer<bool>.CreateFailResult(ex.Message);
        }
    }

    public async Task<ResultContainer<TModel>> GetById(long id)
    {
        try
        {
            var entity = await Repository.GetByIdAsync(id);
            if (entity is null) return ResultContainer<TModel>.CreateFailResult("Entity not found.");

            return ResultContainer<TModel>.CreatePassResult(TConverter.Convert(entity));
        }
        catch (Exception e)
        {
            return ResultContainer<TModel>.CreateFailResult(e.Message);
        }
    }

    public async Task<ResultContainer<IEnumerable<TModel>>> Get(Expression<Func<TEntity, bool>>? predicate = null)
    {
        try
        {
            IEnumerable<TEntity> entities;
            if (predicate is null)
            {
                entities = await Repository.GetAllAsync();
            }
            else
            {
                entities = await Repository.FindAsync(predicate);
            }
            
            return ResultContainer<IEnumerable<TModel>>.CreatePassResult(entities.Select(TConverter.Convert));
        }
        catch (Exception e)
        {
            return ResultContainer<IEnumerable<TModel>>.CreateFailResult(e.Message);
        }
    }
    
    public async Task<ResultContainer<int>> GetPageCount(Expression<Func<TEntity, bool>> predicate, PagingParameters<TEntity> pagingParameters)
    {
        try
        {
            return ResultContainer<int>.CreatePassResult
            (
                await Repository.GetPageCountAsync(predicate, pagingParameters)
            );
        }
        catch (Exception e)
        {
            return ResultContainer<int>.CreateFailResult(e.Message);
        }
    }

    public virtual async Task<ResultContainer<PagedResult<TModel>>> GetPage(Expression<Func<TEntity, bool>> predicate, PagingParameters<TEntity> pagingParameters)
    {
        try
        {
            var entities = await Repository.GetPagedAsync(predicate, pagingParameters);
            var models = PagedResult<TModel>.From(entities, entities.Items.Select(TConverter.Convert));
            return ResultContainer<PagedResult<TModel>>.CreatePassResult(models);
        }
        catch (Exception e)
        {
            return  ResultContainer<PagedResult<TModel>>.CreateFailResult(e.Message);
        }
    }

    public virtual async Task<ResultContainer<TModel>> Add(TModel entity)
    {
        try
        {
            var newEntity = await Repository.AddAsync(TConverter.Convert(entity));
            return ResultContainer<TModel>.CreatePassResult(TConverter.Convert(newEntity));
        }
        catch (ClassifiedDbException ex)
        {
            return ClassifiedResult<TModel>.FromException(ex);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Unexpected exception in {Method} for {Entity}", nameof(Add), typeof(TEntity).Name);
            return ResultContainer<TModel>.CreateFailResult("An unexpected server error occurred.");
        }
    }

    public virtual async Task<Result> Update(TModel entity)
    {
        try
        {
            await Repository.UpdateAsync(TConverter.Convert(entity));
            return Result.CreatePassResult();
        }
        catch (ClassifiedDbException ex)
        {
            return ClassifiedResult.FromException(ex);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Unexpected exception in {Method} for {Entity}", nameof(Update), typeof(TEntity).Name);
            return Result.CreateFailResult("An unexpected server error occurred.");
        }
    }

    public async Task<Result> Delete(long id)
    {
        try
        {
            await Repository.DeleteAsync(id);
            return Result.CreatePassResult();
        }
        catch (ClassifiedDbException ex)
        {
            return ClassifiedResult.FromException(ex);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Unexpected exception in {Method} for {Entity}", nameof(Delete), typeof(TEntity).Name);
            return Result.CreateFailResult("An unexpected server error occurred.");
        }
    }
}