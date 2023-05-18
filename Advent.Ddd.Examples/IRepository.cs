namespace Advent.Ddd.Examples;

public interface IRepository<T, TKey>
{
    T Get(TKey key);
    void SaveChanges();
}


public interface IRepository<T> : IRepository<T, Guid>
{
}
