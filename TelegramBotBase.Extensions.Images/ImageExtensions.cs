using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Threading.Tasks;
using Telegram.Bot.Types;
using Telegram.Bot.Types.InputFiles;
using TelegramBotBase.Form;
using TelegramBotBase.Sessions;

namespace TelegramBotBase.Extensions.Images
{
    public static class ImageExtensions
    {
        public static Stream ToStream(this Image image, ImageFormat format)
        {
            var stream = new MemoryStream();
            image.Save(stream, format);
            stream.Position = 0;
            return stream;
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
        public static async Task<Message> SendPhoto(this DeviceSession session, Image image, string name, string caption, ButtonForm buttons = null, int replyTo = 0, bool disableNotification = false)
        {
            using (var fileStream = ToStream(image, ImageFormat.Png))
            {
                var fts = new InputOnlineFile(fileStream, name);

                return await session.SendPhoto(fts, caption: caption, buttons, replyTo, disableNotification);
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
        public static async Task<Message> SendPhoto(this DeviceSession session, Bitmap image, string name, string caption, ButtonForm buttons = null, int replyTo = 0, bool disableNotification = false)
        {
            using (var fileStream = ToStream(image, ImageFormat.Png))
            {
                var fts = new InputOnlineFile(fileStream, name);

                return await session.SendPhoto(fts, caption: caption, buttons, replyTo, disableNotification);
            }
        }
    }
}
