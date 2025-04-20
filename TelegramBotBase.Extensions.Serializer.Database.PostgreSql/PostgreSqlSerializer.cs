using Npgsql;
using System;
using System.Data;
using NpgsqlTypes;
using TelegramBotBase.Args;
using TelegramBotBase.Base;
using TelegramBotBase.Form;
using TelegramBotBase.Interfaces;

namespace TelegramBotBase.Extensions.Serializer.Database.PostgreSql
{
    /// <summary>
    /// Represents a PostgreSQL implementation of the <see cref="IStateMachine"/> for saving and loading form states.
    /// </summary>
    public class PostgreSqlSerializer : IStateMachine
    {
        private readonly string insertIntoSessionSql;
        private readonly string insertIntoSessionsDataSql;
        private readonly string selectAllDevicesSessionsSql;
        private readonly string selectAllDevicesSessionsDataSql;

        /// <summary>
        /// Initializes a new instance of the <see cref="PostgreSqlSerializer"/> class.
        /// </summary>
        /// <param name="connectionString">The connection string to the PostgreSQL database.</param>
        /// <param name="tablePrefix">The prefix for database table names (default is "tgb_").</param>
        /// <param name="fallbackStateForm">The fallback state form type.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="connectionString"/> is null.</exception>
        /// <exception cref="ArgumentException">Thrown when <paramref name="fallbackStateForm"/> is not a subclass of <see cref="FormBase"/>.</exception>
        public PostgreSqlSerializer(string connectionString, string tablePrefix = "tgb_", Type fallbackStateForm = null)
        {
            ConnectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));

            TablePrefix = tablePrefix;

            FallbackStateForm = fallbackStateForm;

            if (FallbackStateForm != null && !FallbackStateForm.IsSubclassOf(typeof(FormBase)))
            {
                throw new ArgumentException($"{nameof(FallbackStateForm)} is not a subclass of {nameof(FormBase)}");
            }

            insertIntoSessionSql = "INSERT INTO " + TablePrefix +
                                   "devices_sessions (deviceId, deviceTitle, \"FormUri\", \"QualifiedName\") VALUES (@deviceId, @deviceTitle, @FormUri, @QualifiedName)";
            insertIntoSessionsDataSql = "INSERT INTO " + TablePrefix + "devices_sessions_data (deviceId, key, value, type) VALUES (@deviceId, @key, @value, @type)";

            selectAllDevicesSessionsSql = "SELECT * FROM " + TablePrefix + "devices_sessions";
            selectAllDevicesSessionsDataSql = "SELECT * FROM " + TablePrefix + "devices_sessions_data WHERE deviceId = @deviceId";
        }

        /// <summary>
        /// Gets the connection string to the PostgreSQL database.
        /// </summary>
        public string ConnectionString { get; }

        /// <summary>
        /// Gets or sets the table name prefix for database tables.
        /// </summary>
        public string TablePrefix { get; set; }

        /// <summary>
        /// Gets or sets the fallback state form type.
        /// </summary>
        public Type FallbackStateForm { get; set; }

        /// <inheritdoc/>
        /// <summary>
        /// Saves form states to the PostgreSQL database.
        /// </summary>
        /// <param name="e">The <see cref="SaveStatesEventArgs"/> containing the states to be saved.</param>
        public void SaveFormStates(SaveStatesEventArgs e)
        {
            var container = e.States;

            //Cleanup old Session data
            Cleanup();

            using (var connection = new NpgsqlConnection(ConnectionString))
            {
                connection.Open();

                //Store session data in database
                foreach (var state in container.States)
                {
                    using (var sessionCommand = connection.CreateCommand())
                    {
                        sessionCommand.CommandText = insertIntoSessionSql;

                        sessionCommand.Parameters.Add(new NpgsqlParameter("@deviceId", NpgsqlDbType.Bigint){Value = state.DeviceId });
                        sessionCommand.Parameters.Add(new NpgsqlParameter("@deviceTitle", DbType.StringFixedLength){Value = state.ChatTitle ?? string.Empty});
                        sessionCommand.Parameters.Add(new NpgsqlParameter("@FormUri", DbType.StringFixedLength) {Value = state.FormUri});
                        sessionCommand.Parameters.Add(new NpgsqlParameter("@QualifiedName", DbType.StringFixedLength){Value = state.QualifiedName });

                        sessionCommand.ExecuteNonQuery();
                    }
                }
            }

            using (var connection = new NpgsqlConnection(ConnectionString))
            {
                connection.Open();

                foreach (var state in container.States)
                {
                    SaveSessionsData(state, connection);
                }
            }
        }

        /// <inheritdoc/>
        /// <summary>
        /// Loads form states from the PostgreSQL database.
        /// </summary>
        /// <returns>A <see cref="StateContainer"/> containing the loaded form states.</returns>
        public StateContainer LoadFormStates()
        {
            var stateContainer = new StateContainer();

            using (var connection = new NpgsqlConnection(ConnectionString))
            {
                connection.Open();

                using (var sessionCommand = connection.CreateCommand())
                {
                    sessionCommand.CommandText = selectAllDevicesSessionsSql;

                    var sessionTable = new DataTable();
                    using (var dataAdapter = new NpgsqlDataAdapter(sessionCommand))
                    {
                        dataAdapter.Fill(sessionTable);

                        foreach (DataRow row in sessionTable.Rows)
                        {
                            var stateEntry = new StateEntry
                            {
                                DeviceId = (long)row["deviceId"],
                                ChatTitle = row["deviceTitle"].ToString(),
                                FormUri = row["FormUri"].ToString(),
                                QualifiedName = row["QualifiedName"].ToString()
                            };

                            stateContainer.States.Add(stateEntry);

                            if (stateEntry.DeviceId > 0)
                            {
                                stateContainer.ChatIds.Add(stateEntry.DeviceId);
                            }
                            else
                            {
                                stateContainer.GroupIds.Add(stateEntry.DeviceId);
                            }

                            LoadDataTable(connection, row, stateEntry);
                        }
                    }
                }
            }

            return stateContainer;
        }

        /// <summary>
        /// Cleans up old session data in the PostgreSQL database.
        /// </summary>
        private void Cleanup()
        {
            using (var connection = new NpgsqlConnection(ConnectionString))
            {
                connection.Open();

                using (var clearCommand = connection.CreateCommand())
                {
                    clearCommand.CommandText = $"DELETE FROM {TablePrefix}devices_sessions_data";
                    clearCommand.ExecuteNonQuery();
                }

                using (var clearCommand = connection.CreateCommand())
                {
                    clearCommand.CommandText = $"DELETE FROM {TablePrefix}devices_sessions";
                    clearCommand.ExecuteNonQuery();
                }
            }
        }

        /// <summary>
        /// Saves session data to the PostgreSQL database.
        /// </summary>
        /// <param name="state">The state entry containing session data to be saved.</param>
        /// <param name="connection">The NpgsqlConnection used for the database interaction.</param>
        private void SaveSessionsData(StateEntry state, NpgsqlConnection connection)
        {
            foreach (var data in state.Values)
            {
                using (var dataCommand = connection.CreateCommand())
                {
                    dataCommand.CommandText = insertIntoSessionsDataSql;

                    dataCommand.Parameters.Add(new NpgsqlParameter("@deviceId", NpgsqlDbType.Bigint) { Value = state.DeviceId });
                    dataCommand.Parameters.Add(new NpgsqlParameter("@key", DbType.StringFixedLength) { Value = data.Key });

                    var type = data.Value.GetType();

                    if (type.IsPrimitive || type == typeof(string))
                    {
                        dataCommand.Parameters.Add(new NpgsqlParameter("@value", NpgsqlDbType.Text) { Value = data.Value });
                    }
                    else
                    {
                        var json = System.Text.Json.JsonSerializer.Serialize(data.Value);
                        dataCommand.Parameters.Add(new NpgsqlParameter("@value", NpgsqlDbType.Text) { Value = json });
                    }

                    dataCommand.Parameters.Add(new NpgsqlParameter("@type", DbType.StringFixedLength) { Value = type.AssemblyQualifiedName });

                    dataCommand.ExecuteNonQuery();
                }
            }
        }

        /// <summary>
        /// Loads session data from the PostgreSQL database.
        /// </summary>
        /// <param name="connection">The NpgsqlConnection used for the database interaction.</param>
        /// <param name="row">The DataRow representing a session entry in the main sessions table.</param>
        /// <param name="stateEntry">The StateEntry object to which session data will be loaded.</param>
        private void LoadDataTable(NpgsqlConnection connection, DataRow row, StateEntry stateEntry)
        {
            using (var sessionCommand = connection.CreateCommand())
            {
                sessionCommand.CommandText = selectAllDevicesSessionsDataSql;
                sessionCommand.Parameters.Add(new NpgsqlParameter("@deviceId", row["deviceId"]));

                var dataCommandTable = new DataTable();
                using (var npgSqlDataAdapter = new NpgsqlDataAdapter(sessionCommand))
                {
                    npgSqlDataAdapter.Fill(dataCommandTable);

                    foreach (DataRow dataRow in dataCommandTable.Rows)
                    {
                        var key = dataRow["key"].ToString();
                        var type = Type.GetType(dataRow["type"].ToString());

                        var value = System.Text.Json.JsonSerializer.Deserialize(dataRow["value"].ToString(), type);

                        stateEntry.Values.Add(key, value);
                    }
                }

            }
        }
    }
}