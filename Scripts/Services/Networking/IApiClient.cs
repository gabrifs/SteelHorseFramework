using System.Collections.Generic;
using System.Threading;
using UnityEngine;

namespace SteelHorse.Framework.Services.Networking
{
    public interface IApiClient
    {
        Awaitable<ApiResponse> GetAsync(string endpoint, IDictionary<string, string> headers = null, CancellationToken cancellationToken = default);
        Awaitable<ApiResponse> PostAsync(string endpoint, string jsonBody, IDictionary<string, string> headers = null, CancellationToken cancellationToken = default);
        Awaitable<ApiResponse> PutAsync(string endpoint, string jsonBody, IDictionary<string, string> headers = null, CancellationToken cancellationToken = default);
        Awaitable<ApiResponse> DeleteAsync(string endpoint, IDictionary<string, string> headers = null, CancellationToken cancellationToken = default);
    }
}
