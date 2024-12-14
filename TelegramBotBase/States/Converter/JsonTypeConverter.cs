using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace TelegramBotBase.States.Converter
{
    public class JsonTypeConverter : JsonConverter<object>
    {

        public override object Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            using (JsonDocument doc = JsonDocument.ParseValue(ref reader))
            {
                return ParseElement(doc.RootElement, options, typeof(object));
            }

        }

        private object ParseElement(JsonElement element, JsonSerializerOptions options, Type expected_type)
        {
            // Wenn das Element ein JSON-Objekt ist
            if (element.ValueKind == JsonValueKind.Object)
            {
                // Wenn es einen $type enthält, dann den Typ dynamisch instanziieren
                if (element.TryGetProperty("$type", out JsonElement typeElement))
                {
                    string typeName = typeElement.GetString();
                    Type type = Type.GetType(typeName);

                    if (type == null)
                        throw new InvalidOperationException($"Typ '{typeName}' konnte nicht gefunden werden.");

                    // Liste verarbeiten, falls $values enthalten ist
                    if (element.TryGetProperty("$values", out JsonElement valuesElement))
                    {
                        return ParseArray(valuesElement, type);
                    }
                    else if (expected_type.IsGenericType && expected_type.GetGenericTypeDefinition() == typeof(Dictionary<,>))
                    {
                        var dictionary = new Dictionary<string, object>();
                        foreach (var property in element.EnumerateObject())
                        {
                            dictionary[property.Name] = ParseElement(property.Value, options, typeof(object));
                        }

                        return dictionary;
                    }
                    else
                    {
                        var instance = type.GetConstructor(new Type[] { }).Invoke(new object[] { });
                        var properties = type.GetProperties();

                        foreach (var p in properties)
                        {
                            if (!element.TryGetProperty(p.Name, out JsonElement value))
                                continue;

                            switch (value.ValueKind)
                            {
                                case JsonValueKind.Number:

                                    if (p.PropertyType == typeof(int))
                                    {
                                        if (value.TryGetInt32(out int i))
                                        {
                                            p.SetValue(instance, i);
                                        }
                                    }
                                    else if (p.PropertyType == typeof(long))
                                    {
                                        if (value.TryGetInt64(out long l))
                                        {
                                            p.SetValue(instance, l);
                                        }

                                    }

                                    break;
                                case JsonValueKind.String:

                                    p.SetValue(instance, value.ToString());

                                    break;

                                default:

                                    //if (p.PropertyType.IsGenericType && p.PropertyType.GetGenericTypeDefinition() == typeof(Dictionary<,>))
                                    //{
                                    //    var dictionary = new Dictionary<string, object>();
                                    //    foreach (var property in element.EnumerateObject())
                                    //    {
                                    //        dictionary[property.Name] = ParseElement(property.Value, options, p.PropertyType);
                                    //    }

                                    //    p.SetValue(instance, dictionary);
                                    //}
                                    //else
                                    //{

                                    var obj = ParseElement(value, options, p.PropertyType);

                                    if (obj == null)
                                        continue;

                                    try
                                    {
                                        p.SetValue(instance, obj);
                                    }
                                    catch
                                    {

                                    }
                                    //}


                                    break;
                            }



                        }

                        return instance;

                        // Objekt dynamisch deserialisieren
                        //return JsonSerializer.Deserialize(element.GetRawText(), type, options);
                    }
                }
                else
                {
                    // Falls kein $type vorhanden ist, als Dictionary verarbeiten
                    var dictionary = new Dictionary<string, object>();
                    foreach (var property in element.EnumerateObject())
                    {
                        dictionary[property.Name] = ParseElement(property.Value, options, typeof(object));
                    }
                    return dictionary;
                }
            }
            // Wenn das Element ein Array ist
            else if (element.ValueKind == JsonValueKind.Array)
            {
                return ParseArray(element, typeof(object));
            }
            // Primitive Typen (String, Number, Bool)
            else if (element.ValueKind == JsonValueKind.Number)
            {

                if (expected_type == typeof(Int64))
                {
                    if (element.TryGetInt64(out long l))
                        return l;
                }
                else if (expected_type == typeof(Int32))
                {
                    if (element.TryGetInt32(out int i))
                        return i;
                }

                if (element.TryGetInt32(out int i2))
                    return i2;

                return 0;
            }
            else if (element.ValueKind == JsonValueKind.String)
            {
                return element.ToString();
            }
            else
            {

#if NETCOREAPP3_1
                return JsonSerializer.Deserialize(element.ToString(), typeof(object), options);
#else
                return element.Deserialize<object>(options);
#endif

            }
        }

        private object ParseElement(JsonElement element)
        {

            return null;
        }

        private object ParseArray(JsonElement arrayElement, Type listType)
        {
            // Wenn der Typ eine generische Liste ist
            if (listType.IsGenericType && listType.GetGenericTypeDefinition() == typeof(List<>))
            {
                Type elementType = listType.GetGenericArguments()[0];
                var list = (IList)Activator.CreateInstance(typeof(List<>).MakeGenericType(elementType));
                foreach (var item in arrayElement.EnumerateArray())
                {
                    list.Add(ParseElement(item, new JsonSerializerOptions(), elementType));
                }
                return list;
            }
            else
            {
                // Standardmäßig als List<object>
                var list = new List<object>();
                foreach (var item in arrayElement.EnumerateArray())
                {
                    list.Add(ParseElement(item, new JsonSerializerOptions(), typeof(object)));
                }
                return list;
            }
        }

        public override void Write(Utf8JsonWriter writer, object value, JsonSerializerOptions options)
        {
            WriteWithTypeInfo(writer, value, options);

        }

        private void WriteWithTypeInfo(Utf8JsonWriter writer, object value, JsonSerializerOptions options)
        {
            if (value == null)
            {
                writer.WriteNullValue();
                return;
            }

            Type type = value.GetType();

            // Primitives und grundlegende Typen (int, string, bool, etc.)
            if (type.IsPrimitive || type == typeof(string) || type == typeof(decimal))
            {
                JsonSerializer.Serialize(writer, value, type, options);
            }
            // Behandlung für Sammlungen
            else if (value is IList list)
            {
                writer.WriteStartObject();
                writer.WriteString("$type", type.AssemblyQualifiedName);
                writer.WritePropertyName("$values");
                writer.WriteStartArray();

                foreach (var item in list)
                {
                    WriteWithTypeInfo(writer, item, options);
                }

                writer.WriteEndArray();
                writer.WriteEndObject();
            }
            // Behandlung für Dictionaries
            else if (value is IDictionary dictionary)
            {
                writer.WriteStartObject();
                writer.WriteString("$type", type.AssemblyQualifiedName);

                foreach (DictionaryEntry entry in dictionary)
                {
                    writer.WritePropertyName(entry.Key.ToString());
                    WriteWithTypeInfo(writer, entry.Value, options);
                }

                writer.WriteEndObject();
            }
            // Komplexe Objekte
            else if (value is Enum e)
            {

                int eValue = (int)value;

                writer.WriteNumberValue(eValue);

            }
            else
            {
                writer.WriteStartObject();
                writer.WriteString("$type", type.AssemblyQualifiedName);

                foreach (var property in type.GetProperties())
                {
                    if (property.CanRead)
                    {
                        var propValue = property.GetValue(value);
                        writer.WritePropertyName(property.Name);
                        WriteWithTypeInfo(writer, propValue, options);
                    }
                }

                writer.WriteEndObject();
            }
        }
    }
}
