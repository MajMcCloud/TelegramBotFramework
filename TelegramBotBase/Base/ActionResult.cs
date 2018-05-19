using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TelegramBotBase.Base
{
    //public class ActionResult : ResultBase
    //{
    //    public Telegram.Bot.Args.CallbackQueryEventArgs RawCallbackData { get; set; }

    //    public override long DeviceId
    //    {
    //        get
    //        {
    //            return this.RawCallbackData?.CallbackQuery.Message?.Chat.Id ?? 0;
    //        }
    //    }

    //    public String Command
    //    {
    //        get
    //        {
    //            return this.RawCallbackData.CallbackQuery.Message.Text ?? "";
    //        }
    //    }

    //    public String RawData
    //    {
    //        get
    //        {
    //            return this.RawCallbackData.CallbackQuery.Data;
    //        }
    //    }

    //    public T GetData<T>()
    //        where T : class
    //    {
    //        T cd = null;
    //        try
    //        {
    //            cd = Newtonsoft.Json.JsonConvert.DeserializeObject<T>(this.RawData);

    //            return cd;
    //        }
    //        catch
    //        {

    //        }

    //        return null;
    //    }

    //    /// <summary>
    //    /// Bestätigt den Erhalt der Aktion.
    //    /// </summary>
    //    /// <param name="message"></param>
    //    /// <returns></returns>
    //    public async Task ConfirmAction(String message = "")
    //    {
    //        try
    //        {
    //            await this.Client.TelegramClient.AnswerCallbackQueryAsync(this.RawCallbackData.CallbackQuery.Id, message);
    //        }
    //        catch
    //        {

    //        }
    //    }

    //    public override async Task DeleteMessage()
    //    {
    //        try
    //        {
    //            await this.Client.TelegramClient.DeleteMessageAsync(this.DeviceId, this.RawCallbackData.CallbackQuery.Message.MessageId);
    //        }
    //        catch
    //        {

    //        }
    //    }

    //    public ActionResult(Telegram.Bot.Args.CallbackQueryEventArgs callback)
    //    {
    //        this.RawCallbackData = callback;
    //        this.Message = callback.CallbackQuery.Message;
    //    }

    //}
}
