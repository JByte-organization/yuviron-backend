namespace Yuviron.Application.Abstractions.Data.Contexts;

public interface IDataContext : IUnitOfWork
{
    void Add<T>(T entity) where T : class;
    void Remove<T>(T entity) where T : class;
    void Update<T>(T entity) where T : class;
    void AddRange<T>(IEnumerable<T> entities) where T : class;
    void RemoveRange<T>(IEnumerable<T> entities) where T : class;
    void UpdateRange<T>(IEnumerable<T> entities) where T : class;
}
