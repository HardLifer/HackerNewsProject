using System.Text.Json;
using System.Text.Json.Serialization;

namespace HackerNewsBambooOpenApi.Converters
{
    public class UnixTimeConverter : JsonConverter<DateTime>
    {
        private static readonly DateTime _epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        public override DateTime Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (!reader.TryGetInt64(out long value))
            {
                throw new InvalidOperationException("The 'Time' value of HackerNews Json object isn't correct. Check an API field value type.");
            }

            return _epoch.AddMilliseconds(value);
        }

        public override void Write(Utf8JsonWriter writer, DateTime value, JsonSerializerOptions options)
        {
            writer.WriteRawValue(((DateTime)value - _epoch).TotalMilliseconds.ToString());
        }
    }
}
