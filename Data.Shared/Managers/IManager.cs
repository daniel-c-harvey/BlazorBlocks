using System.Linq.Expressions;
using Models.Shared.Common;
using Models.Shared.Entities;
using Models.Shared.Models;
using NetBlocks.Models;

namespace Data.Shared.Managers;

public interface IManager<TEntity, TModel>
where TEntity : class, IEntity
where TModel : class, IModel
{
    Task<ResultContainer<bool>> Exists(TModel entity);
    Task<ResultContainer<TModel>> GetById(long id);
    Task<ResultContainer<IEnumerable<TModel>>> Get(Expression<Func<TEntity, bool>>? predicate = null);
    Task<ResultContainer<int>> GetPageCount(Expression<Func<TEntity, bool>> predicate,  PagingParameters<TEntity> pagingParameters);
    Task<ResultContainer<PagedResult<TModel>>> GetPage(Expression<Func<TEntity, bool>> predicate, PagingParameters<TEntity> pagingParameters);
    Task<ResultContainer<TModel>> Add(TModel model);
    Task<Result> Update(TModel entity);
    Task<Result> Delete(long id);
}