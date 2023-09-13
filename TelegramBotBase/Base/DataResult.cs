using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace TelegramBotBase.Base;

/// <summary>
///     Returns a class to manage attachments within messages.
/// </summary>
public class DataResult : ResultBase
{
    public DataResult(UpdateResult update)
    {
        UpdateData = update;
        Device = update.Device;
    }

    //public Telegram.Bot.Args.MessageEventArgs RawMessageData { get; set; }

    public UpdateResult UpdateData { get; set; }


    public Contact Contact => Message.Contact;

    public Location Location => Message.Location;

    public Document Document => Message.Document;

    public Audio Audio => Message.Audio;

    public Video Video => Message.Video;

    public PhotoSize[] Photos => Message.Photo;


    public MessageType Type => Message?.Type ?? MessageType.Unknown;

    public override Message Message => UpdateData?.Message;

    /// <summary>
    ///     Returns the FileId of the first reachable element.
    /// </summary>
    public string FileId =>
        Document?.FileId ??
        Audio?.FileId ??
        Video?.FileId ??
        Photos.FirstOrDefault()?.FileId;


    public async Task<InputFileStream> DownloadDocument()
    {
        var encryptedContent = new MemoryStream();
        encryptedContent.SetLength(Document.FileSize.Value);
        var file = await Device.Client.TelegramClient.GetInfoAndDownloadFileAsync(Document.FileId,
                       encryptedContent);

        return InputFile.FromStream(encryptedContent, Document.FileName);
    }


    /// <summary>
    ///     Downloads a file and saves it to the given path.
    /// </summary>
    /// <param name="path"></param>
    /// <returns></returns>
    public async Task DownloadDocument(string path)
    {
        var file = await Device.Client.TelegramClient.GetFileAsync(Document.FileId);
        var fs = new FileStream(path, FileMode.Create);
        await Device.Client.TelegramClient.DownloadFileAsync(file.FilePath, fs);
        fs.Close();
        fs.Dispose();
    }

    /// <summary>
    ///     Downloads the document and returns an byte array.
    /// </summary>
    /// <returns></returns>
    public async Task<byte[]> DownloadRawDocument()
    {
        var ms = new MemoryStream();
        await Device.Client.TelegramClient.GetInfoAndDownloadFileAsync(Document.FileId, ms);
        return ms.ToArray();
    }

    /// <summary>
    ///     Downloads  a document and returns it as string. (txt,csv,etc) Default encoding ist UTF8.
    /// </summary>
    /// <returns></returns>
    public async Task<string> DownloadRawTextDocument()
    {
        return await DownloadRawTextDocument(Encoding.UTF8);
    }

    /// <summary>
    ///     Downloads  a document and returns it as string. (txt,csv,etc)
    /// </summary>
    /// <returns></returns>
    public async Task<string> DownloadRawTextDocument(Encoding encoding)
    {
        var ms = new MemoryStream();
        await Device.Client.TelegramClient.GetInfoAndDownloadFileAsync(Document.FileId, ms);

        ms.Position = 0;

        var sr = new StreamReader(ms, encoding);

        return sr.ReadToEnd();
    }

    public async Task<InputFileStream> DownloadVideo()
    {
        var encryptedContent = new MemoryStream();
        encryptedContent.SetLength(Video.FileSize.Value);
        var file = await Device.Client.TelegramClient.GetInfoAndDownloadFileAsync(Video.FileId, encryptedContent);

        return InputFile.FromStream(encryptedContent, "");
    }

    public async Task DownloadVideo(string path)
    {
        var file = await Device.Client.TelegramClient.GetFileAsync(Video.FileId);
        var fs = new FileStream(path, FileMode.Create);
        await Device.Client.TelegramClient.DownloadFileAsync(file.FilePath, fs);
        fs.Close();
        fs.Dispose();
    }

    public async Task<InputFileStream> DownloadAudio()
    {
        var encryptedContent = new MemoryStream();
        encryptedContent.SetLength(Audio.FileSize.Value);
        var file = await Device.Client.TelegramClient.GetInfoAndDownloadFileAsync(Audio.FileId, encryptedContent);

        return InputFile.FromStream(encryptedContent, "");
    }

    public async Task DownloadAudio(string path)
    {
        var file = await Device.Client.TelegramClient.GetFileAsync(Audio.FileId);
        var fs = new FileStream(path, FileMode.Create);
        await Device.Client.TelegramClient.DownloadFileAsync(file.FilePath, fs);
        fs.Close();
        fs.Dispose();
    }

    public async Task<InputFileStream> DownloadPhoto(int index)
    {
        var photo = Photos[index];
        var encryptedContent = new MemoryStream();
        encryptedContent.SetLength(photo.FileSize.Value);
        var file = await Device.Client.TelegramClient.GetInfoAndDownloadFileAsync(photo.FileId, encryptedContent);

        return InputFile.FromStream(encryptedContent, "");
    }

    public async Task DownloadPhoto(int index, string path)
    {
        var photo = Photos[index];
        var file = await Device.Client.TelegramClient.GetFileAsync(photo.FileId);
        var fs = new FileStream(path, FileMode.Create);
        await Device.Client.TelegramClient.DownloadFileAsync(file.FilePath, fs);
        fs.Close();
        fs.Dispose();
    }
}