using System;
using System.IO;
using TelegramBotBase.Builder;
using TelegramBotBase.Builder.Interfaces;

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
        public static ILanguageSelectionStage UseNewtonsoftJson(this ISessionSerializationStage builder)
        {
            var path = Path.Combine(Directory.GetCurrentDirectory(), "states.json");
            
            builder.UseNewtonsoftJson(path);

            return builder as BotBaseBuilder;
        }

        /// <summary>
        ///     Using the complex version of .Net JSON, which can serialize all objects.
        ///     Saves in application directory.
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static ILanguageSelectionStage UseNewtonsoftJson(this ISessionSerializationStage builder, String path)
        {
            var _stateMachine = new NewtonsoftJsonStateMachine(path);

            builder.UseSerialization(_stateMachine);

            return builder as BotBaseBuilder;
        }
    }
}