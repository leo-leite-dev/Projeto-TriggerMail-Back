namespace TriggerMail.Core.Application.Ports.Persistence;

public interface IUnitOfWork
{
    Task<int> SaveChangesAsync(CancellationToken ct);
}