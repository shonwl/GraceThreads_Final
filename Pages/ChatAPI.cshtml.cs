using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Configuration;
using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace GraceThreads.Pages
{
    [IgnoreAntiforgeryToken]
    public class ChatAPIModel : PageModel
    {
        private readonly IConfiguration _config;

        public ChatAPIModel(IConfiguration config)
        {
            _config = config;
        }

        public async Task<IActionResult> OnPostAsync([FromBody] ChatMessageRequest request)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.Message))
            {
                return BadRequest(new { reply = "Message cannot be empty." });
            }

            var apiKey = _config["Gemini:ApiKey"]?.Trim();
            if (string.IsNullOrEmpty(apiKey))
            {
                return Content(JsonSerializer.Serialize(new { reply = "Configuration Error: Gemini API key is missing." }), "application/json");
            }

            using var client = new HttpClient();
            // THE FIX: Switch to the high-volume Flash-Lite model to bypass the 503 traffic jam
            var url = $"https://generativelanguage.googleapis.com/v1beta/models/gemini-3.1-flash-lite-preview:generateContent?key={apiKey}";

            var systemInstruction = "You are the interactive sizing assistant for Grace Threads, an Christian themed clothing brand. Your only job is to guide customers to their correct shirt size (S, M, L, XL, 2XL).\n\n" +
                                    "Our official Size Chart:\n" +
                                    "- S: Length = 26 in, Width = 23 in, Sleeve = 9.5 in\n" +
                                    "- M: Length = 27 in, Width = 24 in, Sleeve = 9.75 in\n" +
                                    "- L: Length = 28 in, Width = 25 in, Sleeve = 10 in\n" +
                                    "- XL: Length = 29 in, Width = 26 in, Sleeve = 10.25 in\n" +
                                    "- 2XL: Length = 30 in, Width = 27 in, Sleeve = 10.5 in\n\n" +
                                    "Interactive Rules:\n" +
                                    "1. GREETINGS: If the user says 'Hello', 'Hi', or greets you, respond with a welcoming, Friendly greeting and prompt them for their chest width, garment length, or sleeve length preferences.\n" +
                                    "2. AMBIGUOUS INPUTS: If they provide vague statements (like 'my sizing is 5'), clarify nicely that your sizes are calibrated using inches for Width, Length, and Sleeve. Ask them to provide those parameters or check a well-fitting shirt they own.\n" +
                                    "3. IN-BETWEEN SIZES: Recommend sizing up for an oversized streetwear look or down for a standard true-to-size fit if they fall directly between measurements.\n" +
                                    "4. STRICT GUARDRAIL: If they ask about ANYTHING outside of clothing dimensions, sizing advice, or fabric fit, you must politely decline by saying: 'I am only trained to help you lock down your perfect fit! For other questions regarding orders or logistics, please reach out via our Contact page.'\n\n" +
                                    "Style: Keep it conversational, crisp, friendly, and brief. Never generate huge blocks of text.";

            var payload = new
            {
                system_instruction = new { parts = new[] { new { text = systemInstruction } } },
                contents = new[]
                {
                    new { role = "user", parts = new[] { new { text = request.Message } } }
                }
            };

            var content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");

            try
            {
                var response = await client.PostAsync(url, content);
                if (response.IsSuccessStatusCode)
                {
                    var responseString = await response.Content.ReadAsStringAsync();
                    using var jsonDoc = JsonDocument.Parse(responseString);
                    
                    var replyText = jsonDoc.RootElement
                        .GetProperty("candidates")[0]
                        .GetProperty("content")
                        .GetProperty("parts")[0]
                        .GetProperty("text")
                        .GetString();

                    return Content(JsonSerializer.Serialize(new { reply = replyText }), "application/json");
                }
                else if (response.StatusCode == System.Net.HttpStatusCode.ServiceUnavailable)
                {
                    return Content(JsonSerializer.Serialize(new { reply = "Our sizing assistant is helping a lot of customers right now! Give me just a few seconds and try sending your measurements again." }), "application/json");
                }
                else
                {
                    var rawError = await response.Content.ReadAsStringAsync();
                    return Content(JsonSerializer.Serialize(new { reply = $"Google API Error ({response.StatusCode}): {rawError}" }), "application/json");
                }
            }
            catch (Exception ex)
            {
                return Content(JsonSerializer.Serialize(new { reply = $"Connection Exception: {ex.Message}" }), "application/json");
            }
        }
    }

    public class ChatMessageRequest
    {
        public string Message { get; set; }
    }
}