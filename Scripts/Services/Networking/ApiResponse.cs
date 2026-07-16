using System;
using UnityEngine;

namespace SteelHorse.Framework.Services.Networking
{
    public class ApiResponse
    {
        public bool Success { get { return _success; } }
        public long StatusCode { get { return _statusCode; } }
        public string RawBody { get { return _rawBody; } }
        public string ErrorMessage { get { return _errorMessage; } }

        private readonly bool _success;
        private readonly long _statusCode;
        private readonly string _rawBody;
        private readonly string _errorMessage;

        private ApiResponse(bool success, long statusCode, string rawBody, string errorMessage)
        {
            _success = success;
            _statusCode = statusCode;
            _rawBody = rawBody;
            _errorMessage = errorMessage;
        }

        internal static ApiResponse Ok(long statusCode, string rawBody)
        {
            return new ApiResponse(true, statusCode, rawBody, null);
        }

        internal static ApiResponse HttpError(long statusCode, string rawBody, string errorMessage)
        {
            return new ApiResponse(false, statusCode, rawBody, errorMessage);
        }

        internal static ApiResponse NetworkError(string errorMessage)
        {
            return new ApiResponse(false, 0, null, errorMessage);
        }

        public T ParseAs<T>() where T : class
        {
            if (string.IsNullOrEmpty(_rawBody))
                return null;

            try
            {
                return JsonUtility.FromJson<T>(_rawBody);
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[ApiResponse] Failed to parse body as {typeof(T).Name}: {e.Message}");
                return null;
            }
        }
    }
}
