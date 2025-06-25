using System.Text.Json;
using System.Text;
using TelegramBotBase.Exceptions;
using System.Text.Json.Serialization;

namespace TelegramBotBase.Form;

/// <summary>
/// Base class for serializing buttons and data
/// </summary>
public class CallbackData
{
    public CallbackData()
    {

    }

    public CallbackData(string method, string value)
    {
        Method = method;
        Value = value;
    }

    [JsonPropertyName("m")] public string Method { get; set; }

    [JsonPropertyName("v")] public string Value { get; set; }

    public static string Create(string method, string value)
    {
        return new CallbackData(method, value).Serialize(true);
    }

    /// <summary>
    ///     Serializes data to json string
    /// </summary>
    /// <returns></returns>
    public string Serialize(bool throwExceptionOnOverflow = false)
    {
        var s = string.Empty;

        s = JsonSerializer.Serialize(this);

        //Is data over 64 bytes ?
        int byte_count = Encoding.UTF8.GetByteCount(s);
        if (throwExceptionOnOverflow && byte_count > Constants.Telegram.MaxCallBackDataBytes)
        {
            throw new CallbackDataTooLongException(byte_count);
        }

        return s;
    }

    /// <summary>
    ///     Deserializes data from json string
    /// </summary>
    /// <param name="data"></param>
    /// <returns></returns>
    public static CallbackData Deserialize(string data)
    {
        return JsonSerializer.Deserialize<CallbackData>(data);
    }

    /// <summary>
    /// Attempts to deserialize the specified raw data into a <see cref="CallbackData"/> object.
    /// </summary>
    /// <param name="raw_data">The raw data string to deserialize.</param>
    /// <param name="data">When this method returns, contains the deserialized <see cref="CallbackData"/> object if the operation succeeds;
    /// otherwise, <see langword="null"/>. This parameter is passed uninitialized.</param>
    /// <returns><see langword="true"/> if the raw data was successfully deserialized; otherwise, <see langword="false"/>.</returns>
    public static bool TryDeserialize(string raw_data, out CallbackData data)
    {
        data = null;

        if (string.IsNullOrWhiteSpace(raw_data))
            return false;

        if (raw_data[0] != '{' || raw_data[^1] != '}')
            return false;

        try
        {
            data = Deserialize(raw_data);
            return data != null;
        }
        catch (JsonException)
        {
            // Ungültiges JSON-Format
        }


        return false;
    }

    public static implicit operator string(CallbackData callbackData) => callbackData.Serialize(true);
}
