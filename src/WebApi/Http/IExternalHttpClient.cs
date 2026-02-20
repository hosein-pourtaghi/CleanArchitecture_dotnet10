using System.Net.Http;
using System.Threading.Tasks;

namespace WebApi.Http;

public interface IExternalHttpClient
{
    Task<HttpResponseMessage> GetAsync(System.Uri relativeUrl, CancellationToken cancellationToken = default);
    Task<HttpResponseMessage> PostAsync<T>(System.Uri relativeUrl, T content, CancellationToken cancellationToken = default);
}
