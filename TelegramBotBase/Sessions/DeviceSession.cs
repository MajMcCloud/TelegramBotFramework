using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.InputFiles;
using Telegram.Bot.Types.ReplyMarkups;
using TelegramBotBase.Base;
using TelegramBotBase.Form;

namespace TelegramBotBase.Sessions
{
    /// <summary>
    /// Base class for a device/chat session
    /// </summary>
    public class DeviceSession
    {
        /// <summary>
        /// Device or chat id
        /// </summary>
        public long DeviceId { get; set; }

        public DateTime LastAction { get; set; }

        public FormBase ActiveForm { get; set; }

        public int LastMessage { get; set; }

        private MessageClient Client
        {
            get
            {
                return this.ActiveForm.Client;
            }
        }

        public EventHandlerList __Events = new EventHandlerList();

        private static object __evMessageSent = new object();

        public DeviceSession()
        {
            this.LastMessage = 0;
        }

        public DeviceSession(long DeviceId)
        {
            this.DeviceId = DeviceId;
        }

        public DeviceSession(long DeviceId, FormBase StartForm)
        {
            this.DeviceId = DeviceId;
            this.ActiveForm = StartForm;
            this.ActiveForm.Device = this;
        }

        /// <summary>
        /// Edits the text message
        /// </summary>
        /// <param name="messageId"></param>
        /// <param name="text"></param>
        /// <param name="buttons"></param>
        /// <returns></returns>
        public async Task<Message> Edit(int messageId, String text, ButtonForm buttons = null)
        {
            if (this.ActiveForm == null)
                return null;

            InlineKeyboardMarkup markup = null;
            if (buttons != null)
            {
                markup = buttons;
            }

            try
            {
                var m = await this.Client.TelegramClient.EditMessageTextAsync(this.DeviceId, messageId, text, replyMarkup: markup);
                return m;
            }
            catch
            {
                
            }


            return null;
        }

        /// <summary>
        /// Sends a simple text message
        /// </summary>
        /// <param name="text"></param>
        /// <param name="buttons"></param>
        /// <param name="replyTo"></param>
        /// <param name="disableNotification"></param>
        /// <returns></returns>
        public async Task<Message> Send(String text, ButtonForm buttons = null, int replyTo = 0, bool disableNotification = false)
        {
            if (this.ActiveForm == null)
                return null;

            InlineKeyboardMarkup markup = null;
            if (buttons != null)
            {
                markup = buttons;
            }

            Message m = null;

            try
            {
                m = await (this.Client.TelegramClient.SendTextMessageAsync(this.DeviceId, text, replyToMessageId: replyTo, replyMarkup: markup, disableNotification: disableNotification));
            }
            catch (Telegram.Bot.Exceptions.ApiRequestException ex)
            {
                return null;
            }
            catch
            {
                return null;
            }


            OnMessageSent(new MessageSentEventArgs(m.MessageId, m));

            return m;
        }

        /// <summary>
        /// Sends a simple text message
        /// </summary>
        /// <param name="text"></param>
        /// <param name="markup"></param>
        /// <param name="replyTo"></param>
        /// <param name="disableNotification"></param>
        /// <returns></returns>
        public async Task<Message> Send(String text, InlineKeyboardMarkup markup, int replyTo = 0, bool disableNotification = false)
        {
            if (this.ActiveForm == null)
                return null;

            Message m = null;

            try
            {
                m = await (this.Client.TelegramClient.SendTextMessageAsync(this.DeviceId, text, replyToMessageId: replyTo, replyMarkup: markup, disableNotification: disableNotification));
            }
            catch (Telegram.Bot.Exceptions.ApiRequestException ex)
            {
                return null;
            }
            catch
            {
                return null;
            }

            OnMessageSent(new MessageSentEventArgs(m.MessageId, m));

            return m;
        }

        /// <summary>
        /// Sends an image
        /// </summary>
        /// <param name="file"></param>
        /// <param name="buttons"></param>
        /// <param name="replyTo"></param>
        /// <param name="disableNotification"></param>
        /// <returns></returns>
        public async Task<Message> SendPhoto(InputOnlineFile file, ButtonForm buttons = null, int replyTo = 0, bool disableNotification = false)
        {
            if (this.ActiveForm == null)
                return null;

            InlineKeyboardMarkup markup = null;
            if (buttons != null)
            {
                markup = buttons;
            }

            Message m = null;

            try
            {
                m = await this.Client.TelegramClient.SendPhotoAsync(this.DeviceId, file, replyToMessageId: replyTo, replyMarkup: markup, disableNotification: disableNotification);
            }
            catch (Telegram.Bot.Exceptions.ApiRequestException ex)
            {
                return null;
            }
            catch
            {
                return null;
            }

            OnMessageSent(new MessageSentEventArgs(m.MessageId, m));

            return m;
        }

        /// <summary>
        /// Sends an image
        /// </summary>
        /// <param name="image"></param>
        /// <param name="name"></param>
        /// <param name="buttons"></param>
        /// <param name="replyTo"></param>
        /// <param name="disableNotification"></param>
        /// <returns></returns>
        public async Task<Message> SendPhoto(Image image, String name, ButtonForm buttons = null, int replyTo = 0, bool disableNotification = false)
        {
            using (var fileStream = Tools.Images.ToStream(image, ImageFormat.Png))
            {
                InputOnlineFile fts = new InputOnlineFile(fileStream, name);

                var m = await SendPhoto(fts, buttons, replyTo, disableNotification);

                return m;
            }
        }

        /// <summary>
        /// Sends an image
        /// </summary>
        /// <param name="image"></param>
        /// <param name="name"></param>
        /// <param name="buttons"></param>
        /// <param name="replyTo"></param>
        /// <param name="disableNotification"></param>
        /// <returns></returns>
        public async Task<Message> SendPhoto(Bitmap image, String name, ButtonForm buttons = null, int replyTo = 0, bool disableNotification = false)
        {
            using (var fileStream = Tools.Images.ToStream(image, ImageFormat.Png))
            {
                InputOnlineFile fts = new InputOnlineFile(fileStream, name);

                var m = await SendPhoto(fts, buttons, replyTo, disableNotification);

                return m;
            }
        }

        /// <summary>
        /// Sends an document
        /// </summary>
        /// <param name="filename"></param>
        /// <param name="document"></param>
        /// <param name="caption"></param>
        /// <param name="buttons"></param>
        /// <param name="replyTo"></param>
        /// <param name="disableNotification"></param>
        /// <returns></returns>
        public async Task<Message> SendDocument(String filename, byte[] document, String caption = "", ButtonForm buttons = null, int replyTo = 0, bool disableNotification = false)
        {
            MemoryStream ms = new MemoryStream(document);

            InputOnlineFile fts = new InputOnlineFile(ms, filename);

            var m = await SendDocument(fts, caption, buttons, replyTo, disableNotification);

            return m;
        }

        /// <summary>
        /// Sends an document
        /// </summary>
        /// <param name="document"></param>
        /// <param name="caption"></param>
        /// <param name="buttons"></param>
        /// <param name="replyTo"></param>
        /// <param name="disableNotification"></param>
        /// <returns></returns>
        public async Task<Message> SendDocument(InputOnlineFile document, String caption = "", ButtonForm buttons = null, int replyTo = 0, bool disableNotification = false)
        {
            InlineKeyboardMarkup markup = null;
            if (buttons != null)
            {
                markup = buttons;
            }

            var m = await this.Client.TelegramClient.SendDocumentAsync(this.DeviceId, document, caption, replyMarkup: markup, disableNotification: disableNotification, replyToMessageId: replyTo);

            OnMessageSent(new MessageSentEventArgs(m.MessageId, m));

            return m;
        }

        /// <summary>
        /// Set a chat action (showed to the user)
        /// </summary>
        /// <param name="action"></param>
        /// <returns></returns>
        public async Task SetAction(ChatAction action)
        {
            await this.Client.TelegramClient.SendChatActionAsync(this.DeviceId, action);
        }

        /// <summary>
        /// Deletes a message
        /// </summary>
        /// <param name="messageId"></param>
        /// <returns></returns>
        public virtual async Task<bool> DeleteMessage(int messageId = -1)
        {
            try
            {
                await this.Client.TelegramClient.DeleteMessageAsync(this.DeviceId, messageId);

                return true;
            }
            catch(ApiRequestException ex)
            {

            }

            return false;
        }

        /// <summary>
        /// Deletes the given message
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        public virtual async Task<bool> DeleteMessage(Message message)
        {
            return await DeleteMessage(message.MessageId);
        }

        /// <summary>
        /// Eventhandler for sent messages
        /// </summary>
        public event EventHandler<MessageSentEventArgs> MessageSent
        {
            add
            {
                this.__Events.AddHandler(__evMessageSent, value);
            }
            remove
            {
                this.__Events.RemoveHandler(__evMessageSent, value);
            }
        }


        public void OnMessageSent(MessageSentEventArgs e)
        {
            (this.__Events[__evMessageSent] as EventHandler<MessageSentEventArgs>)?.Invoke(this, e);
        }

    }
}
