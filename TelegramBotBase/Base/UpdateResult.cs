using Telegram.Bot.Types;
using TelegramBotBase.Interfaces;

namespace TelegramBotBase.Base;

public class UpdateResult : ResultBase
{
    public UpdateResult(Update rawData, IDeviceSession device)
    {
        RawData = rawData;
        Device = device;
    }

    /// <summary>
    ///     Returns the Device/ChatId
    /// </summary>
    public override long DeviceId =>
        RawData?.Message?.Chat?.Id
        ?? RawData?.BusinessConnection?.UserChatId 
        ?? RawData?.BusinessMessage?.Chat?.Id 
        ?? RawData?.EditedBusinessMessage?.Chat?.Id
        ?? RawData?.EditedMessage?.Chat?.Id
        ?? RawData?.CallbackQuery?.Message?.Chat?.Id
        ?? Device?.DeviceId
        ?? RawData?.MyChatMember?.From?.Id
        ?? 0;

    public Update RawData { get; set; }

    public override Message Message =>
        RawData?.Message
        ?? RawData?.EditedMessage
        ?? RawData.BusinessMessage 
        ?? RawData.EditedBusinessMessage 
        ?? RawData?.ChannelPost
        ?? RawData?.EditedChannelPost
        ?? RawData?.CallbackQuery?.Message;
}
