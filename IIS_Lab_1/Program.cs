using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace IIS_Lab_1
{
    internal class Program
    {
        private static readonly string HF_API_URL = "https://router.huggingface.co/hf-inference/models/distilbert/distilbert-base-uncased-finetuned-sst-2-english";
        //private static readonly string HF_API_TOKEN = "";

        static async Task Main(string[] args)
        {
            Console.WriteLine("Виберіть варіант:");
            Console.WriteLine("1 - Ввести свій текст");
            Console.WriteLine("2 - Використати тестовий текст");

            string choice = Console.ReadLine();
            string text = "";

            switch (choice)
            {
                case "1":
                    Console.WriteLine("Введіть текст для аналізу:");
                    text = Console.ReadLine();
                    break;
                case "2":
                    text = @"Programming involves writing code, testing it, and fixing errors.
                            It requires focus, logic, and patience.";
                    Console.WriteLine($"Використано тестовий текст: {text}");
                    break;
                default:
                    Console.WriteLine("Невірний вибір. Використано тестовий текст за замовчуванням.");
                    text = "I love programming! It is so much fun.";
                    break;
            }

            var result = await AnalyzeSentiment(text);
            Console.WriteLine("Результат аналізу сентименту:");
            Console.WriteLine(result);
        }

        private static async Task<string> AnalyzeSentiment(string text)
        {
            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", HF_API_TOKEN);

                var payload = new { inputs = text };
                var jsonPayload = JsonConvert.SerializeObject(payload);

                var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");
                var response = await client.PostAsync(HF_API_URL, content);
                var responseString = await response.Content.ReadAsStringAsync();

                try
                {
                    // Парсимо JSON
                    var jsonArray = JArray.Parse(responseString);
                    var scores = jsonArray[0]; // Беремо перший масив об'єктів

                    // Знаходимо елемент з найбільшою ймовірністю
                    JToken top = null;
                    double maxScore = -1;
                    foreach (var item in scores)
                    {
                        double score = item.Value<double>("score");
                        if (score > maxScore)
                        {
                            maxScore = score;
                            top = item;
                        }
                    }

                    if (top != null)
                    {
                        string label = top.Value<string>("label");
                        double score = top.Value<double>("score");
                        return $"{label} (ймовірність: {score:F2})";
                    }
                }
                catch (Exception ex)
                {
                    return $"Помилка обробки результату: {ex.Message}";
                }

                return "Не вдалося визначити сентимент.";
            }
        }
    }
}
