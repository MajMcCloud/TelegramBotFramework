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
        return new CallbackData(method, value).Serialize();
    }

    /// <summary>
    ///     Serializes data to json string
    /// </summary>
    /// <returns></returns>
    public string Serialize(bool throwExceptionOnOverflow = false)
    {
        var s = "";
        try
        {
            s = JsonConvert.SerializeObject(this);
        }
        catch
        {
        }

#if DEBUG

        //Data is over 64 bytes
        if(throwExceptionOnOverflow && Encoding.UTF8.GetByteCount(s) > Constants.Telegram.MaxCallBackDataBytes)
        {
            throw new CallbackDataTooLongException();
        }

#endif

        return s;
    }

    /// <summary>
    ///     Deserializes data from json string
    /// </summary>
    /// <param name="data"></param>
    /// <returns></returns>
    public static CallbackData Deserialize(string data)
    {
        CallbackData cd = null;
        try
        {
            cd = JsonConvert.DeserializeObject<CallbackData>(data);

            return cd;
        }
        catch
        {
        }

        return null;
    }

    public static implicit operator string(CallbackData callbackData) => callbackData.Serialize(true);
}
