using System.Collections.Generic;
using System.Text;
using System.Threading;
using UnityEngine;
using UnityEngine.Networking;

namespace SteelHorse.Framework.Services.Networking
{
    public class ApiClient : MonoBehaviour, IApiClient
    {
        private const string JsonContentType = "application/json";

        [SerializeField] private ApiConfig _config;

        public Awaitable<ApiResponse> GetAsync(string endpoint, IDictionary<string, string> headers = null, CancellationToken cancellationToken = default)
        {
            return SendRequestAsync(UnityWebRequest.Get(BuildUrl(endpoint)), headers, null, cancellationToken);
        }

        public Awaitable<ApiResponse> PostAsync(string endpoint, string jsonBody, IDictionary<string, string> headers = null, CancellationToken cancellationToken = default)
        {
            return SendRequestAsync(BuildBodyRequest(UnityWebRequest.kHttpVerbPOST, endpoint, jsonBody), headers, JsonContentType, cancellationToken);
        }

        public Awaitable<ApiResponse> PutAsync(string endpoint, string jsonBody, IDictionary<string, string> headers = null, CancellationToken cancellationToken = default)
        {
            return SendRequestAsync(BuildBodyRequest(UnityWebRequest.kHttpVerbPUT, endpoint, jsonBody), headers, JsonContentType, cancellationToken);
        }

        public Awaitable<ApiResponse> DeleteAsync(string endpoint, IDictionary<string, string> headers = null, CancellationToken cancellationToken = default)
        {
            var request = UnityWebRequest.Delete(BuildUrl(endpoint));
            request.downloadHandler = new DownloadHandlerBuffer();
            return SendRequestAsync(request, headers, null, cancellationToken);
        }

        private string BuildUrl(string endpoint)
        {
            string baseUrl = _config.BaseUrl.TrimEnd('/');
            string normalizedEndpoint = endpoint.StartsWith("/") ? endpoint : "/" + endpoint;
            return baseUrl + normalizedEndpoint;
        }

        private UnityWebRequest BuildBodyRequest(string verb, string endpoint, string jsonBody)
        {
            var request = new UnityWebRequest(BuildUrl(endpoint), verb);
            byte[] bodyBytes = Encoding.UTF8.GetBytes(jsonBody ?? string.Empty);
            request.uploadHandler = new UploadHandlerRaw(bodyBytes);
            request.downloadHandler = new DownloadHandlerBuffer();
            return request;
        }

        private async Awaitable<ApiResponse> SendRequestAsync(UnityWebRequest request, IDictionary<string, string> headers, string defaultContentType, CancellationToken cancellationToken)
        {
            using (request)
            {
                if (defaultContentType != null && (headers == null || !headers.ContainsKey("Content-Type")))
                    request.SetRequestHeader("Content-Type", defaultContentType);

                if (headers != null)
                {
                    foreach (var header in headers)
                        request.SetRequestHeader(header.Key, header.Value);
                }

                var completionSource = new AwaitableCompletionSource();
                using (cancellationToken.Register(() =>
                {
                    request.Abort();
                    completionSource.TrySetCanceled();
                }))
                {
                    request.SendWebRequest().completed += _ => completionSource.TrySetResult();
                    await completionSource.Awaitable;
                }

                return BuildResponse(request);
            }
        }

        private ApiResponse BuildResponse(UnityWebRequest request)
        {
            switch (request.result)
            {
                case UnityWebRequest.Result.Success:
                    return ApiResponse.Ok(request.responseCode, request.downloadHandler.text);

                case UnityWebRequest.Result.ProtocolError:
                    // Server responded with a non-2xx status; the body may still contain
                    // a JSON error payload, so it is preserved even though Success is false.
                    return ApiResponse.HttpError(request.responseCode, request.downloadHandler.text, request.error);

                default: // ConnectionError, DataProcessingError
                    return ApiResponse.NetworkError(request.error);
            }
        }
    }
}
