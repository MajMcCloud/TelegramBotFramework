using System;
using TelegramBotBase.Builder;

namespace TelegramBotBase.Extensions.Serializer.Database.MSSQL
{
    public static class BotBaseBuilderExtensions
    {
        /// <summary>
        ///     Uses an Microsoft SQL Server Database to save and restore sessions.
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="connectionString"></param>
        /// <param name="tablePrefix"></param>
        /// <param name="fallbackForm"></param>
        /// <returns></returns>
        public static BotBaseBuilder UseSqlDatabase(this BotBaseBuilder builder,
                                                             string connectionString, Type fallbackForm = null,
                                                             string tablePrefix = "tgb_")
        {
            var serializer = new MssqlSerializer(connectionString, tablePrefix, fallbackForm);

            return builder.UseSerialization(serializer);
        }


        /// <summary>
        ///     Uses an Microsoft SQL Server Database to save and restore sessions.
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="hostOrIP"></param>
        /// <param name="userId"></param>
        /// <param name="password"></param>
        /// <param name="databaseName"></param>
        /// <param name="tablePrefix"></param>
        /// <param name="fallbackForm"></param>
        /// <returns></returns>
        public static BotBaseBuilder UseSqlDatabase(this BotBaseBuilder builder, string hostOrIP,
                                                             string databaseName, string userId, string password,
                                                             Type fallbackForm = null, string tablePrefix = "tgb_")
        {
            var connectionString =
                $"Server={hostOrIP}; Database={databaseName}; User Id={userId}; Password={password}; TrustServerCertificate=true;";

            var serializer = new MssqlSerializer(connectionString, tablePrefix, fallbackForm);

            return builder.UseSerialization(serializer);
        }

        /// <summary>
        ///     Uses an Microsoft SQL Server Database with Windows Authentication to save and restore sessions.
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="hostOrIP"></param>
        /// <param name="databaseName"></param>
        /// <param name="tablePrefix"></param>
        /// <param name="fallbackForm"></param>
        /// <returns></returns>
        public static BotBaseBuilder UseSqlDatabase(this BotBaseBuilder builder, string hostOrIP,
                                                             string databaseName, bool integratedSecurity = true,
                                                             Type fallbackForm = null, string tablePrefix = "tgb_")
        {
            if (!integratedSecurity)
            {
                throw new ArgumentOutOfRangeException();
            }

            var connectionString =
                $"Server={hostOrIP}; Database={databaseName}; Integrated Security=true; TrustServerCertificate=true;";

            var serializer = new MssqlSerializer(connectionString, tablePrefix, fallbackForm);

            return builder.UseSerialization(serializer);
        }
    }
}