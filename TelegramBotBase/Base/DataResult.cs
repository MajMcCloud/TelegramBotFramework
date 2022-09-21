using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.InputFiles;

namespace TelegramBotBase.Base
{
    /// <summary>
    /// Returns a class to manage attachments within messages.
    /// </summary>
    public class DataResult : ResultBase
    {

        //public Telegram.Bot.Args.MessageEventArgs RawMessageData { get; set; }

        public UpdateResult UpdateData { get; set; }


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
                return this.Message?.Type ?? Telegram.Bot.Types.Enums.MessageType.Unknown;
            }
        }

        public override Message Message
        {
            get
            {
                return this.UpdateData?.Message;
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

        public DataResult(UpdateResult update)
        {
            this.UpdateData = update;
        }


        public async Task<InputOnlineFile> DownloadDocument()
        {
            var encryptedContent = new System.IO.MemoryStream();
            encryptedContent.SetLength(this.Document.FileSize.Value);
            var file = await Device.Client.TelegramClient.GetInfoAndDownloadFileAsync(this.Document.FileId, encryptedContent);

            return new InputOnlineFile(encryptedContent, this.Document.FileName);
        }


        /// <summary>
        /// Downloads a file and saves it to the given path.
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public async Task DownloadDocument(String path)
        {
            var file = await Device.Client.TelegramClient.GetFileAsync(this.Document.FileId);
            FileStream fs = new FileStream(path, FileMode.Create);
            await Device.Client.TelegramClient.DownloadFileAsync(file.FilePath, fs);
            fs.Close();
            fs.Dispose();
        }

        /// <summary>
        /// Downloads the document and returns an byte array.
        /// </summary>
        /// <returns></returns>
        public async Task<byte[]> DownloadRawDocument()
        {
            MemoryStream ms = new MemoryStream();
            await Device.Client.TelegramClient.GetInfoAndDownloadFileAsync(this.Document.FileId, ms);
            return ms.ToArray();
        }

        /// <summary>
        /// Downloads  a document and returns it as string. (txt,csv,etc) Default encoding ist UTF8.
        /// </summary>
        /// <returns></returns>
        public async Task<String> DownloadRawTextDocument()
        {
            return await DownloadRawTextDocument(Encoding.UTF8);
        }

        /// <summary>
        /// Downloads  a document and returns it as string. (txt,csv,etc)
        /// </summary>
        /// <returns></returns>
        public async Task<String> DownloadRawTextDocument(Encoding encoding)
        {
            MemoryStream ms = new MemoryStream();
            await Device.Client.TelegramClient.GetInfoAndDownloadFileAsync(this.Document.FileId, ms);

            ms.Position = 0;

            var sr = new StreamReader(ms, encoding);

            return sr.ReadToEnd();
        }

        public async Task<InputOnlineFile> DownloadVideo()
        {
            var encryptedContent = new System.IO.MemoryStream();
            encryptedContent.SetLength(this.Video.FileSize.Value);
            var file = await Device.Client.TelegramClient.GetInfoAndDownloadFileAsync(this.Video.FileId, encryptedContent);

            return new InputOnlineFile(encryptedContent, "");
        }

        public async Task DownloadVideo(String path)
        {
            var file = await Device.Client.TelegramClient.GetFileAsync(this.Video.FileId);
            FileStream fs = new FileStream(path, FileMode.Create);
            await Device.Client.TelegramClient.DownloadFileAsync(file.FilePath, fs);
            fs.Close();
            fs.Dispose();
        }

        public async Task<InputOnlineFile> DownloadAudio()
        {
            var encryptedContent = new System.IO.MemoryStream();
            encryptedContent.SetLength(this.Audio.FileSize.Value);
            var file = await Device.Client.TelegramClient.GetInfoAndDownloadFileAsync(this.Audio.FileId, encryptedContent);

            return new InputOnlineFile(encryptedContent, "");
        }

        public async Task DownloadAudio(String path)
        {
            var file = await Device.Client.TelegramClient.GetFileAsync(this.Audio.FileId);
            FileStream fs = new FileStream(path, FileMode.Create);
            await Device.Client.TelegramClient.DownloadFileAsync(file.FilePath, fs);
            fs.Close();
            fs.Dispose();
        }

        public async Task<InputOnlineFile> DownloadPhoto(int index)
        {
            var photo = this.Photos[index];
            var encryptedContent = new System.IO.MemoryStream();
            encryptedContent.SetLength(photo.FileSize.Value);
            var file = await Device.Client.TelegramClient.GetInfoAndDownloadFileAsync(photo.FileId, encryptedContent);

            return new InputOnlineFile(encryptedContent, "");
        }

        public async Task DownloadPhoto(int index, String path)
        {
            var photo = this.Photos[index];
            var file = await Device.Client.TelegramClient.GetFileAsync(photo.FileId);
            FileStream fs = new FileStream(path, FileMode.Create);
            await Device.Client.TelegramClient.DownloadFileAsync(file.FilePath, fs);
            fs.Close();
            fs.Dispose();
        }

    }
}
