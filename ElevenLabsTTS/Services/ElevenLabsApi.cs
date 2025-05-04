using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using Newtonsoft.Json;
using ElevenLabsTTS.Models;
using System.Windows.Forms;

namespace ElevenLabsTTS.Services
{
    public class ElevenLabsApi
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiKey;
        private const string BaseUrl = "https://api.elevenlabs.io/v1";

        public ElevenLabsApi(string apiKey)
        {
            _apiKey = apiKey;
            _httpClient = new HttpClient();
            _httpClient.DefaultRequestHeaders.Add("xi-api-key", _apiKey);
        }

        public async Task<List<Voice>> GetVoicesAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync($"{BaseUrl}/voices");
                response.EnsureSuccessStatusCode();
                var content = await response.Content.ReadAsStringAsync();
                var result = JsonConvert.DeserializeObject<VoicesResponse>(content);
                return result?.Voices ?? new List<Voice>();
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public async Task<byte[]> TextToSpeechAsync(string text, string voiceId, string model, double stability, double speed)
        {
            try
            {
                var request = new
                {
                    text = text,
                    model_id = model,
                    voice_settings = new
                    {
                        stability = stability,
                        similarity_boost = speed
                    }
                };

                var json = JsonConvert.SerializeObject(request);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync($"{BaseUrl}/text-to-speech/{voiceId}", content);
                response.EnsureSuccessStatusCode();
                return await response.Content.ReadAsByteArrayAsync();
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        private class VoicesResponse
        {
            [JsonProperty("voices")]
            public List<Voice> Voices { get; set; } = new List<Voice>();
        }
    }
} 