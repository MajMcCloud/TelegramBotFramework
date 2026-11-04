using System;
using System.IO;
using TelegramBotBase.Builder;

namespace TelegramBotBase.Extensions.Serializer.Legacy.NewtonsoftJson
{
    public static class BotBaseBuilderExtensions
    {

        /// <summary>
        ///     Using the complex version of .Net JSON, which can serialize all objects.
        ///     Saves in application directory.
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static BotBaseBuilder UseNewtonsoftJson(this BotBaseBuilder builder)
        {
            var path = Path.Combine(Directory.GetCurrentDirectory(), "states.json");
            
            return builder.UseNewtonsoftJson(path);
        }

        /// <summary>
        ///     Using the complex version of .Net JSON, which can serialize all objects.
        ///     Saves in application directory.
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static BotBaseBuilder UseNewtonsoftJson(this BotBaseBuilder builder, String path)
        {
            var _stateMachine = new NewtonsoftJsonStateMachine(path);

            return builder.UseSerialization(_stateMachine);
        }
    }
}