using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace TelegramBotBase.States.Converter
{
    public class DictionaryObjectJsonConverter : JsonConverter<Dictionary<string, object>>
    {
        public override Dictionary<string, object> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var dictionary = new Dictionary<string, object>();
            var jsonDocument = JsonDocument.ParseValue(ref reader);

            foreach (var element in jsonDocument.RootElement.EnumerateObject())
            {
                switch (element.Value.ValueKind)
                {
                    case JsonValueKind.String:
                        dictionary[element.Name] = element.Value.GetString();
                        break;
                    case JsonValueKind.Number:

                        if (element.Value.TryGetInt32(out var number))
                        {
                            dictionary[element.Name] = number;
                            continue;
                        }

                        if (element.Value.TryGetInt64(out long l))
                            dictionary[element.Name] = l;
                        else
                            dictionary[element.Name] = element.Value.GetDouble();
                        break;
                    case JsonValueKind.True:
                    case JsonValueKind.False:
                        dictionary[element.Name] = element.Value.GetBoolean();
                        break;
                    case JsonValueKind.Object:
                        dictionary[element.Name] = JsonSerializer.Deserialize<Dictionary<string, object>>(element.Value.GetRawText(), options);
                        break;
                    case JsonValueKind.Array:

                        dictionary[element.Name] = HandleArray(element.Value);

                        break;
                    default:
                        dictionary[element.Name] = element.Value.GetRawText();
                        break;
                }
            }

            return dictionary;
        }



        private object HandleArray(JsonElement jsonArray)
        {
            // Hier wird geprüft, ob alle Elemente einer bestimmten Art angehören (z. B. int, string)
            if (jsonArray.GetArrayLength() > 0)
            {
                var firstElement = jsonArray[0];
                switch (firstElement.ValueKind)
                {
                    case JsonValueKind.Number:
                        // Prüfen, ob alle Elemente ganze Zahlen sind
                        var isIntArray = true;
                        var isLongArray = false;

                        foreach (var element in jsonArray.EnumerateArray())
                        {
                            if (!element.TryGetInt32(out _))
                            {
                                isIntArray = false;
                                isLongArray = true;
                                if (!element.TryGetInt64(out _))
                                {
                                    isLongArray = false;
                                    break;
                                }
                            }
                        }
                        if (isIntArray)
                        {
                            var list = new List<int>();
                            foreach (var element in jsonArray.EnumerateArray())
                            {
                                list.Add(element.GetInt32());
                            }
                            return list;
                        }
                        else if (isLongArray)
                        {
                            var list = new List<long>();
                            foreach (var element in jsonArray.EnumerateArray())
                            {
                                list.Add(element.GetInt64());
                            }
                            return list;
                        }
                        else
                        {
                            var list = new List<double>();
                            foreach (var element in jsonArray.EnumerateArray())
                            {
                                list.Add(element.GetDouble());
                            }
                            return list;
                        }
                    case JsonValueKind.String:
                        var stringList = new List<string>();
                        foreach (var element in jsonArray.EnumerateArray())
                        {
                            stringList.Add(element.GetString());
                        }
                        return stringList;
                    case JsonValueKind.True:
                    case JsonValueKind.False:
                        var boolList = new List<bool>();
                        foreach (var element in jsonArray.EnumerateArray())
                        {
                            boolList.Add(element.GetBoolean());
                        }
                        return boolList;
                    default:
                        // Fallback: Liste von Objekten (z. B. wenn es sich um komplexe Objekte handelt)
                        var objectList = new List<object>();
                        foreach (var element in jsonArray.EnumerateArray())
                        {
                            objectList.Add(JsonSerializer.Deserialize<object>(element.GetRawText()));
                        }
                        return objectList;
                }
            }

            // Leeres Array als Liste von Objekten zurückgeben
            return new List<object>();
        }

        public override void Write(Utf8JsonWriter writer, Dictionary<string, object> value, JsonSerializerOptions options)
        {
            writer.WriteStartObject();

            foreach (var kvp in value)
            {
                writer.WritePropertyName(kvp.Key);
                JsonSerializer.Serialize(writer, kvp.Value, kvp.Value?.GetType() ?? typeof(object), options);
            }

            writer.WriteEndObject();
        }
    }
}