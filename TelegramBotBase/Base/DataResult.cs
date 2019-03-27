using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot.Types;
using Telegram.Bot.Types.InputFiles;

namespace TelegramBotBase.Base
{
    /// <summary>
    /// Returns a class to manage attachments within messages.
    /// </summary>
    public class DataResult : ResultBase
    {
        public Telegram.Bot.Args.MessageEventArgs RawMessageData { get; set; }

        public Contact Contact
        {
            get
            {
                return this.Message.Contact;
            }
        }

        public Location Location
        {
            get
            {
                return this.Message.Location;
            }
        }

        public Document Document
        {
            get
            {
                return this.Message.Document;
            }
        }

        public Audio Audio
        {
            get
            {
                return this.Message.Audio;
            }
        }

        public Video Video
        {
            get
            {
                return this.Message.Video;
            }
        }

        public PhotoSize[] Photos
        {
            get
            {
                return this.Message.Photo;
            }
        }

        public Telegram.Bot.Types.Enums.MessageType Type
        {
            get
            {
                return this.RawMessageData?.Message?.Type ?? Telegram.Bot.Types.Enums.MessageType.Unknown;
            }
        }

        /// <summary>
        /// Returns the FileId of the first reachable element.
        /// </summary>
        public String FileId
        {
            get
            {
                return (this.Document?.FileId ??
                        this.Audio?.FileId ??
                        this.Video?.FileId ??
                        this.Photos.FirstOrDefault()?.FileId);
            }
        }


        public DataResult(Telegram.Bot.Args.MessageEventArgs rawdata)
        {
            this.RawMessageData = rawdata;
            this.Message = rawdata.Message;
        }

        public DataResult(MessageResult message)
        {
            this.RawMessageData = message.RawMessageData;
            this.Message = message.Message;

            
        }

        public async Task<InputOnlineFile> DownloadDocument()
        {
            var encryptedContent = new System.IO.MemoryStream(this.Document.FileSize);
            var file = await this.Client.TelegramClient.GetInfoAndDownloadFileAsync(this.Document.FileId, encryptedContent);
            
            return new InputOnlineFile(encryptedContent, this.Document.FileName);
        }

        public async Task<InputOnlineFile> DownloadVideo()
        {
            var encryptedContent = new System.IO.MemoryStream(this.Video.FileSize);
            var file = await this.Client.TelegramClient.GetInfoAndDownloadFileAsync(this.Video.FileId, encryptedContent);

            return new InputOnlineFile(encryptedContent, "");
        }

        public async Task<InputOnlineFile> DownloadPhoto(int index)
        {
            var photo = this.Photos[index];
            var encryptedContent = new System.IO.MemoryStream(photo.FileSize);
            var file = await this.Client.TelegramClient.GetInfoAndDownloadFileAsync(photo.FileId, encryptedContent);
            
            return new InputOnlineFile(encryptedContent, "");
        }

    }
}
