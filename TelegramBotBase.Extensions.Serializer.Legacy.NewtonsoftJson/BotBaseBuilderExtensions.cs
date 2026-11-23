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
        /// <param name="builder">The bot builder.</param>
        /// <returns></returns>
        public static IBotBaseBuilder UseNewtonsoftJson(this IBotBaseBuilder builder)
        {
            var path = Path.Combine(Directory.GetCurrentDirectory(), "states.json");
            
            return builder.UseNewtonsoftJson(path);
        }

        /// <summary>
        ///     Using the complex version of .Net JSON, which can serialize all objects.
        ///     Saves in application directory.
        /// </summary>
        /// <param name="builder">The bot builder.</param>
        /// <param name="path">The path to the session state file.</param>
        /// <returns></returns>
        public static IBotBaseBuilder UseNewtonsoftJson(this IBotBaseBuilder builder, String path)
        {
            var _stateMachine = new NewtonsoftJsonStateMachine(path);

            return builder.UseSerialization(_stateMachine);
        }
    }
}
