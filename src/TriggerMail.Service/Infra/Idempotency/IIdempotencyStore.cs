namespace TriggerMail.Service.Infra.Idempotency;

public interface IIdempotencyStore
{
    bool TryGet(string alias, string key, out Guid messageId);
    void Put(string alias, string key, Guid messageId, TimeSpan ttl);
}