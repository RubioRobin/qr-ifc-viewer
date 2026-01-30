using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using QrIfcPlugin.Models;

namespace QrIfcPlugin.Services
{
    /// <summary>
    /// API request/response models
    /// </summary>
    public class CreateTokenRequest
    {
        [JsonProperty("projectSlug")]
        public string ProjectSlug { get; set; } = string.Empty;

        [JsonProperty("ifcGlobalId")]
        public string IfcGlobalId { get; set; } = string.Empty;

        [JsonProperty("modelVersion")]
        public string? ModelVersion { get; set; }

        [JsonProperty("expiryDays")]
        public int? ExpiryDays { get; set; }
    }

    public class CreateTokenResponse
    {
        [JsonProperty("viewerUrl")]
        public string ViewerUrl { get; set; } = string.Empty;
    }

    /// <summary>
    /// Service for communicating with the backend API
    /// </summary>
    public class ApiService
    {
        private readonly HttpClient _httpClient;
        private readonly PluginSettings _settings;

        public ApiService(PluginSettings settings)
        {
            _settings = settings;
            _httpClient = new HttpClient
            {
                BaseAddress = new Uri(settings.ApiBaseUrl),
                Timeout = TimeSpan.FromSeconds(30)
            };
        }

        /// <summary>
        /// Create a viewer token for an element
        /// </summary>
        public async Task<string> CreateTokenAsync(string ifcGlobalId, string? modelVersion = null)
        {
            var request = new CreateTokenRequest
            {
                ProjectSlug = _settings.ProjectSlug,
                IfcGlobalId = ifcGlobalId,
                ModelVersion = modelVersion ?? _settings.DefaultModelVersion,
                ExpiryDays = _settings.ExpiryDays
            };

            var json = JsonConvert.SerializeObject(request);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            try
            {
                var response = await _httpClient.PostAsync("/api/tokens", content);
                
                if (!response.IsSuccessStatusCode)
                {
                    var errorBody = await response.Content.ReadAsStringAsync();
                    throw new ApiException(
                        $"API returned {response.StatusCode}: {errorBody}",
                        (int)response.StatusCode
                    );
                }

                var responseJson = await response.Content.ReadAsStringAsync();
                var tokenResponse = JsonConvert.DeserializeObject<CreateTokenResponse>(responseJson);

                if (tokenResponse == null || string.IsNullOrEmpty(tokenResponse.ViewerUrl))
                {
                    throw new ApiException("Invalid response from API", 500);
                }

                return tokenResponse.ViewerUrl;
            }
            catch (HttpRequestException ex)
            {
                throw new ApiException($"Network error: {ex.Message}", 0, ex);
            }
            catch (TaskCanceledException ex)
            {
                throw new ApiException("Request timed out", 0, ex);
            }
        }

        /// <summary>
        /// Test connection to the API
        /// </summary>
        public async Task<bool> TestConnectionAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync("/health");
                return response.IsSuccessStatusCode;
            }
            catch
            {
                return false;
            }
        }
    }

    /// <summary>
    /// Custom exception for API errors
    /// </summary>
    public class ApiException : Exception
    {
        public int StatusCode { get; }

        public ApiException(string message, int statusCode) : base(message)
        {
            StatusCode = statusCode;
        }

        public ApiException(string message, int statusCode, Exception innerException) 
            : base(message, innerException)
        {
            StatusCode = statusCode;
        }
    }
}
