using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TelegramBotBase.Builder;
using TelegramBotBase.Builder.Interfaces;

namespace TelegramBotBase.Extensions.Serializer.Database.MSSQL
{
    public static class BotBaseBuilderExtensions
    {

        /// <summary>
        /// Uses an Microsoft SQL Server Database to save and restore sessions.
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="ConnectionString"></param>
        /// <param name="tablePrefix"></param>
        /// <param name="fallbackForm"></param>
        /// <returns></returns>
        public static ILanguageSelectionStage UseSQLDatabase(this ISessionSerializationStage builder, String ConnectionString, Type fallbackForm = null, String tablePrefix = "tgb_")
        {
            var serializer = new MSSQLSerializer(ConnectionString, tablePrefix, fallbackForm);

            builder.UseSerialization(serializer);

            return builder as BotBaseBuilder;
        }


        /// <summary>
        /// Uses an Microsoft SQL Server Database to save and restore sessions.
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="HostOrIP"></param>
        /// <param name="UserId"></param>
        /// <param name="Password"></param>
        /// <param name="DatabaseName"></param>
        /// <param name="tablePrefix"></param>
        /// <param name="fallbackForm"></param>
        /// <returns></returns>
        public static ILanguageSelectionStage UseSQLDatabase(this ISessionSerializationStage builder, String HostOrIP, String DatabaseName, String UserId, String Password, Type fallbackForm = null, String tablePrefix = "tgb_")
        {
            var connectionString = $"Server={HostOrIP}; Database={DatabaseName}; User Id={UserId}; Password={Password}; TrustServerCertificate=true;";

            var serializer = new MSSQLSerializer(connectionString, tablePrefix, fallbackForm);

            builder.UseSerialization(serializer);

            return builder as BotBaseBuilder;
        }

        /// <summary>
        /// Uses an Microsoft SQL Server Database with Windows Authentication to save and restore sessions.
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="HostOrIP"></param>
        /// <param name="DatabaseName"></param>
        /// <param name="tablePrefix"></param>
        /// <param name="fallbackForm"></param>
        /// <returns></returns>
        public static ILanguageSelectionStage UseSQLDatabase(this ISessionSerializationStage builder, String HostOrIP, String DatabaseName, bool IntegratedSecurity = true, Type fallbackForm = null, String tablePrefix = "tgb_")
        {
            if (!IntegratedSecurity)
                throw new ArgumentOutOfRangeException();

            var connectionString = $"Server={HostOrIP}; Database={DatabaseName}; Integrated Security=true; TrustServerCertificate=true;";

            var serializer = new MSSQLSerializer(connectionString, tablePrefix, fallbackForm);

            builder.UseSerialization(serializer);

            return builder as BotBaseBuilder;
        }
    }
}
