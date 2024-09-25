using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace TelegramBotBase.States.Converter
{
    public class JsonTypeConverter : JsonConverter<object>
    {

        public override object Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var jsonDocument = JsonDocument.ParseValue(ref reader);
            var type = Type.GetType(jsonDocument.RootElement.GetProperty("$type").GetString());
            var jsonObject = jsonDocument.RootElement.GetRawText();
            return JsonSerializer.Deserialize(jsonObject, type, options);
        }

        public override void Write(Utf8JsonWriter writer, object value, JsonSerializerOptions options)
        {
            writer.WriteStartObject();
            writer.WriteString("$type", value.GetType().AssemblyQualifiedName);

            var json = JsonSerializer.Serialize(value, value.GetType(), options);
            using var jsonDoc = JsonDocument.Parse(json);
            foreach (var property in jsonDoc.RootElement.EnumerateObject())
            {
                property.WriteTo(writer);
            }

            writer.WriteEndObject();
        }


    }
}
