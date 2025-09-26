using Newtonsoft.Json;
using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

class Program
{
    private static readonly string apiToken = "YOUR_HUGGINGFACE_API_TOKEN";
    private static readonly string model = "classla/xlm-roberta-base-multilingual-text-genre-classifier";

    static async Task Main(string[] args)
    {
        string text = "In a distant future, humanity explores the depths of space...";
        var result = await ClassifyGenre(text);
        Console.WriteLine(result);
    }

    static async Task<string> ClassifyGenre(string text)
    {
        using (var client = new HttpClient())
        {
            client.DefaultRequestHeaders.Add("Authorization", $"Bearer {apiToken}");

            var payload = new
            {
                inputs = text
            };

            string jsonPayload = JsonConvert.SerializeObject(payload);

            var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

            var response = await client.PostAsync($"https://api-inference.huggingface.co/models/{model}", content);
            response.EnsureSuccessStatusCode();

            string result = await response.Content.ReadAsStringAsync();
            return result;
        }
    }
}
