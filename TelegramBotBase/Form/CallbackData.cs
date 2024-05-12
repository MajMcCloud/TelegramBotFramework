using Newtonsoft.Json;
using System.Text;
using TelegramBotBase.Exceptions;

namespace TelegramBotBase.Form;

/// <summary>
///     Base class for serializing buttons and data
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

    [JsonProperty("m")] public string Method { get; set; }

    [JsonProperty("v")] public string Value { get; set; }

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

        s = JsonConvert.SerializeObject(this);

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
        return JsonConvert.DeserializeObject<CallbackData>(data);
    }

    public static implicit operator string(CallbackData callbackData) => callbackData.Serialize(true);
}
