using HackerNewsBambooOpenApi.Converters;
using System.Text.Json.Serialization;

namespace HackerNewsBambooOpenApi.Models
{
    public class HackerNewsItem
    {
        public int Id { get; init; }

        [JsonPropertyName("parent")]
        public int ParentId { get; init; }

        public int Score { get; init; }

        public string Type { get; init; } = null!;

        [JsonConverter(typeof(UnixTimeConverter))]
        public DateTime Time { get; init; }

        public string Title { get; init; } = null!;

        public string? Text { get; init; }

        [JsonPropertyName("by")]
        public string Author { get; init; } = null!;

        public bool Deleted { get; init; }

        public bool Dead { get; init; }

        public int Poll { get; init; }

        public int[] Kids { get; init; } = [];

        public int[] Parts { get; init; } = [];

        public string Url { get; init; } = null!;

        public int Descendants { get; init; }
    }
}
