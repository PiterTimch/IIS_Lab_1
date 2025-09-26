using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Linq;

class Program
{
    
    private static readonly string model = "classla/xlm-roberta-base-multilingual-text-genre-classifier";

    private static readonly Dictionary<string, List<GenreResult>> cache = new Dictionary<string, List<GenreResult>>();

    static async Task Main(string[] args)
    {
        var texts = new[]
        {
            "In a distant future, humanity explores the depths of space...",
            "Breaking news: the stock market has seen a significant drop today.",
            "This manual explains how to install the software step by step.",
            "Check out our new product launch – limited offer only today!"
        };

        var allResults = new List<GenreAnalysis>();

        foreach (var text in texts)
        {
            var results = await GetOrClassify(text);
            allResults.Add(new GenreAnalysis(text, results));

            Console.WriteLine($"\n Text: {text}");
            foreach (var r in results.OrderByDescending(x => x.Score))
            {
                Console.WriteLine($"   - {r.Label}: {r.Score:P2}");
            }
        }

        Console.WriteLine("\n📊 Overall statistics:");

        var stats = allResults
            .SelectMany(a => a.Results.OrderByDescending(r => r.Score).Take(1)) 
            .GroupBy(r => r.Label)
            .Select(g => new { Label = g.Key, Count = g.Count() })
            .OrderByDescending(x => x.Count);

        foreach (var s in stats)
        {
            Console.WriteLine($"- {s.Label}: {s.Count} texts");
        }
    }

    static async Task<List<GenreResult>> GetOrClassify(string text)
    {
        if (cache.ContainsKey(text))
            return cache[text];

        var results = await ClassifyGenre(text);
        cache[text] = results;
        return results;
    }

    static async Task<List<GenreResult>> ClassifyGenre(string text)
    {
        using (var client = new HttpClient())
        {
            client.DefaultRequestHeaders.Add("Authorization", $"Bearer {apiToken}");

            var payload = new { inputs = text };
            string jsonPayload = JsonConvert.SerializeObject(payload);

            var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

            var response = await client.PostAsync(
                $"https://api-inference.huggingface.co/models/{model}", content);

            response.EnsureSuccessStatusCode();
            string result = await response.Content.ReadAsStringAsync();

            var parsed = JsonConvert.DeserializeObject<List<List<GenreResult>>>(result);

            return parsed?[0] ?? new List<GenreResult>();
        }
    }
}

public class GenreResult
{
    [JsonProperty("label")]
    public string Label { get; set; }

    [JsonProperty("score")]
    public double Score { get; set; }
}

public class GenreAnalysis
{
    public string Text { get; }
    public List<GenreResult> Results { get; }

    public GenreAnalysis(string text, List<GenreResult> results)
    {
        Text = text;
        Results = results;
    }
}
