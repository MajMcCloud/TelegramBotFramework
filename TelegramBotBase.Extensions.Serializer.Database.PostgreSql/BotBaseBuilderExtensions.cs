using System;
using TelegramBotBase.Builder;
using TelegramBotBase.Builder.Interfaces;

namespace TelegramBotBase.Extensions.Serializer.Database.PostgreSql
{
    /// <summary>
    /// Provides extension methods for configuring the use of PostgreSQL Server Database for session serialization.
    /// </summary>
    public static class BotBaseBuilderExtensions
    {
        /// <summary>
        ///     Uses an PostgreSQL Server Database to save and restore sessions.
        /// </summary>
        /// <param name="builder">The session serialization stage builder.</param>
        /// <param name="connectionString">The connection string to the PostgreSQL database.</param>
        /// <param name="fallbackForm">The fallback form type.</param>
        /// <param name="tablePrefix">The prefix for database table names (default is "tgb_").</param>
        /// <returns>The language selection stage builder.</returns>
        public static ILanguageSelectionStage UsePostgreSqlDatabase(
            this ISessionSerializationStage builder,
            string connectionString, Type fallbackForm = null, 
            string tablePrefix = "tgb_")
        {
            var serializer = new PostgreSqlSerializer(connectionString, tablePrefix, fallbackForm);

            builder.UseSerialization(serializer);

            return builder as BotBaseBuilder;
        }


        /// <summary>
        ///     Uses an PostgreSQL Server Database to save and restore sessions.
        /// </summary>
        /// <param name="builder">The session serialization stage builder.</param>
        /// <param name="hostOrIp">The host or IP address of the PostgreSQL server.</param>
        /// <param name="port">The port number for the PostgreSQL server.</param>
        /// <param name="databaseName">The name of the PostgreSQL database.</param>
        /// <param name="userId">The user ID for connecting to the PostgreSQL server.</param>
        /// <param name="password">The password for connecting to the PostgreSQL server.</param>
        /// <param name="fallbackForm">The fallback form type.</param>
        /// <param name="tablePrefix">The prefix for database table names (default is "tgb_").</param>
        /// <returns>The language selection stage builder.</returns>
        public static ILanguageSelectionStage UsePostgreSqlDatabase(
            this ISessionSerializationStage builder,
            string hostOrIp, string port, 
            string databaseName, string userId, 
            string password, Type fallbackForm = null,
            string tablePrefix = "tgb_")
        {
            var connectionString = $"Host={hostOrIp};Port={port};Database={databaseName};Username={userId};Password={password}";

            var serializer = new PostgreSqlSerializer(connectionString, tablePrefix, fallbackForm);

            builder.UseSerialization(serializer);

            return builder as BotBaseBuilder;
        }

        /// <summary>
        ///     Uses an PostgreSQL Server Database with Windows Authentication to save and restore sessions.
        /// </summary>
        /// <param name="builder">The session serialization stage builder.</param>
        /// <param name="hostOrIp">The host or IP address of the PostgreSQL server.</param>
        /// <param name="port">The port number for the PostgreSQL server.</param>
        /// <param name="databaseName">The name of the PostgreSQL database.</param>
        /// <param name="integratedSecurity">A flag indicating whether to use Windows Authentication (true) or not (false).</param>
        /// <param name="fallbackForm">The fallback form type.</param>
        /// <param name="tablePrefix">The prefix for database table names (default is "tgb_").</param>
        /// <returns>The language selection stage builder.</returns>
        public static ILanguageSelectionStage UsePostgreSqlDatabase(
            this ISessionSerializationStage builder,
            string hostOrIp, string port, 
            string databaseName, bool integratedSecurity = true, 
            Type fallbackForm = null, string tablePrefix = "tgb_")
        {
            if (!integratedSecurity)
            {
                throw new ArgumentOutOfRangeException();
            }

            var connectionString = $"Host={hostOrIp};Port={port};Database={databaseName};Integrated Security=true;";

            var serializer = new PostgreSqlSerializer(connectionString, tablePrefix, fallbackForm);

            builder.UseSerialization(serializer);

            return builder as BotBaseBuilder;
        }
    }
}
