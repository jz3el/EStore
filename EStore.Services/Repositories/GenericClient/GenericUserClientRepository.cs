using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using EStore.Services.Interfaces.GenericClient;
using Microsoft.AspNetCore.Http;

namespace EStore.Services.Repositories.GenericClient
{
    public class GenericUserClientRepository : IGenericUserClientRepository
    {
        private readonly HttpClient _client;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public GenericUserClientRepository(HttpClient client, IHttpContextAccessor httpContextAccessor)
        {
            _client = client;
            _httpContextAccessor = httpContextAccessor;
        }

        private static readonly JsonSerializerOptions jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };

        // ------------------------------------------------
        // Helper → Add Authorization token to outgoing API
        // ------------------------------------------------
        private void ForwardToken(HttpRequestMessage req)
        {
            var token = _httpContextAccessor.HttpContext?
                .Request
                .Headers["Authorization"]
                .ToString();

            if (!string.IsNullOrWhiteSpace(token))
            {
                req.Headers.Authorization = AuthenticationHeaderValue.Parse(token);
            }
        }

        // -------------------- GET ------------------------
        public async Task<T> GetAsync<T>(string address)
        {
            var req = new HttpRequestMessage(HttpMethod.Get, address);
            ForwardToken(req);

            var res = await _client.SendAsync(req);
            var json = await res.Content.ReadAsStringAsync();

            return JsonSerializer.Deserialize<T>(json, jsonOptions)!;
        }

        public async Task<T> GetAsync<T>(string address, dynamic payload)
        {
            var req = new HttpRequestMessage(HttpMethod.Post, address);
            ForwardToken(req);

            req.Content = new StringContent(
                JsonSerializer.Serialize(payload),
                Encoding.UTF8,
                "application/json"
            );

            var res = await _client.SendAsync(req);
            var json = await res.Content.ReadAsStringAsync();

            return JsonSerializer.Deserialize<T>(json, jsonOptions)!;
        }

        public async Task<T> GetWithId<T>(string address)
        {
            var req = new HttpRequestMessage(HttpMethod.Get, address);
            ForwardToken(req);

            var res = await _client.SendAsync(req);
            var json = await res.Content.ReadAsStringAsync();

            return JsonSerializer.Deserialize<T>(json, jsonOptions)!;
        }

        // -------------------- POST -----------------------
        public async Task<TResponse> PostAsAsync<TResponse>(string address, dynamic payload)
        {
            var req = new HttpRequestMessage(HttpMethod.Post, address);
            ForwardToken(req);

            req.Content = new StringContent(
                JsonSerializer.Serialize(payload),
                Encoding.UTF8,
                "application/json"
            );

            var res = await _client.SendAsync(req);
            var json = await res.Content.ReadAsStringAsync();

            return JsonSerializer.Deserialize<TResponse>(json, jsonOptions)!;
        }

        public async Task<TResponse> PostAsAsync<TResponse>(string address)
        {
            var req = new HttpRequestMessage(HttpMethod.Post, address);
            ForwardToken(req);

            var res = await _client.SendAsync(req);
            var json = await res.Content.ReadAsStringAsync();

            return JsonSerializer.Deserialize<TResponse>(json, jsonOptions)!;
        }

        // -------------------- PUT ------------------------
        public async Task<TResponse> UpdateAsync<TResponse>(string address, dynamic payload)
        {
            var req = new HttpRequestMessage(HttpMethod.Put, address);
            ForwardToken(req);

            req.Content = new StringContent(
                JsonSerializer.Serialize(payload),
                Encoding.UTF8,
                "application/json"
            );

            var res = await _client.SendAsync(req);
            var json = await res.Content.ReadAsStringAsync();

            return JsonSerializer.Deserialize<TResponse>(json, jsonOptions)!;
        }

        // -------------------- DELETE ----------------------
        public async Task<TResponse> DeleteAsync<TResponse>(string address)
        {
            var req = new HttpRequestMessage(HttpMethod.Delete, address);
            ForwardToken(req);

            var res = await _client.SendAsync(req);
            var json = await res.Content.ReadAsStringAsync();

            return JsonSerializer.Deserialize<TResponse>(json, jsonOptions)!;
        }
    }
}
