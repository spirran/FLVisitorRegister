public interface IVisitorRepository
{
    Task<int> InsertAsync(Visitor v, CancellationToken ct = default);
}