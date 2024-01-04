using IronSoftware.Drawing;
using SixLabors.ImageSharp;
using System.IO;
using System.Threading.Tasks;
using Telegram.Bot.Types;
using TelegramBotBase.Form;
using TelegramBotBase.Sessions;
using static IronSoftware.Drawing.AnyBitmap;
using SKImage = SixLabors.ImageSharp.Image;

namespace TelegramBotBase.Extensions.Images.IronSoftware
{
    public static class ImageExtensions
    {
        public static Stream ToStream(this AnyBitmap image, ImageFormat format)
        {
            var stream = new MemoryStream();
            image.ExportStream(stream, format);
            stream.Position = 0;
            return stream;
        }

        public static async Task<Stream> ToStream(this SKImage image)
        {
            var stream = new MemoryStream();
            await image.SaveAsPngAsync(stream);
            stream.Position = 0;
            return stream;
        }

        /// <summary>
        ///     Sends an image
        /// </summary>
        /// <param name="image"></param>
        /// <param name="name"></param>
        /// <param name="buttons"></param>
        /// <param name="replyTo"></param>
        /// <param name="disableNotification"></param>
        /// <returns></returns>
        public static async Task<Message> SendPhoto(this DeviceSession session, AnyBitmap image, string name,
                                                    string caption, ButtonForm buttons = null, int replyTo = 0,
                                                    bool disableNotification = false)
        {
            using (var fileStream = ToStream(image, ImageFormat.Png))
            {
                var fts = InputFile.FromStream(fileStream, name);
                
                return await session.SendPhoto(fts, caption, buttons, replyTo, disableNotification);
            }
        }

        /// <summary>
        ///     Sends an image
        /// </summary>
        /// <param name="image"></param>
        /// <param name="name"></param>
        /// <param name="buttons"></param>
        /// <param name="replyTo"></param>
        /// <param name="disableNotification"></param>
        /// <returns></returns>
        public static async Task<Message> SendPhoto(this DeviceSession session, SKImage image, string name,
                                                    string caption, ButtonForm buttons = null, int replyTo = 0,
                                                    bool disableNotification = false)
        {
            using (var fileStream = await ToStream(image))
            {
                var fts = InputFile.FromStream(fileStream, name);

                return await session.SendPhoto(fts, caption, buttons, replyTo, disableNotification);
            }
        }
    }
}