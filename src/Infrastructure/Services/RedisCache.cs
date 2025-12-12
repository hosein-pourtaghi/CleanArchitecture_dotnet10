using StackExchange.Redis;
namespace Infrastructure.Services;
public interface ICacheService { Task SetAsync(string key, byte[] value, TimeSpan? expiry=null); Task<byte[]?> GetAsync(string key); }
public class RedisCacheService : ICacheService
{
    private readonly IDatabase _db;
    public RedisCacheService(IConfiguration cfg)
    {
        var conn = ConnectionMultiplexer.Connect(cfg["Redis:Connection"] ?? "localhost:6379");
        _db = conn.GetDatabase();
    }
    public async Task SetAsync(string key, byte[] value, TimeSpan? expiry=null) => await _db.StringSetAsync(key, value, expiry);
    public async Task<byte[]?> GetAsync(string key) { var v = await _db.StringGetAsync(key); if (v.IsNull) return null; return (byte[]?)v; }
}
