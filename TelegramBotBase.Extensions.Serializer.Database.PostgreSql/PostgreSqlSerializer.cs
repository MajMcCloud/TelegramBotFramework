using Npgsql;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text.Json;
using NpgsqlTypes;
using TelegramBotBase.Args;
using TelegramBotBase.Base;
using TelegramBotBase.Form;
using TelegramBotBase.Interfaces;

namespace TelegramBotBase.Extensions.Serializer.Database.PostgreSql
{
    /// <summary>
    /// Represents a PostgreSQL implementation of the <see cref="IStateMachine"/> for saving and loading form states.
    /// Supports per device access (<see cref="IDeviceStateMachine"/>), which is used together with session limits.
    /// </summary>
    public class PostgreSqlSerializer : IDeviceStateMachine
    {
        private readonly string insertIntoSessionSql;
        private readonly string insertIntoSessionsDataSql;
        private readonly string selectAllDevicesSessionsSql;
        private readonly string selectDeviceSessionSql;
        private readonly string selectDeviceSessionsDataSql;
        private readonly string deleteAllSessionsSql;
        private readonly string deleteAllSessionsDataSql;
        private readonly string deleteDeviceSessionSql;
        private readonly string deleteDeviceSessionsDataSql;

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

            var sessions = TablePrefix + "devices_sessions";
            var data = TablePrefix + "devices_sessions_data";

            insertIntoSessionSql = "INSERT INTO " + sessions +
                                   " (deviceId, deviceTitle, \"FormUri\", \"QualifiedName\") VALUES (@deviceId, @deviceTitle, @FormUri, @QualifiedName)";
            insertIntoSessionsDataSql = "INSERT INTO " + data + " (deviceId, key, value, type) VALUES (@deviceId, @key, @value, @type)";

            selectAllDevicesSessionsSql = "SELECT deviceId, deviceTitle, \"FormUri\", \"QualifiedName\" FROM " + sessions;
            selectDeviceSessionSql = selectAllDevicesSessionsSql + " WHERE deviceId = @deviceId";
            selectDeviceSessionsDataSql = "SELECT key, value, type FROM " + data + " WHERE deviceId = @deviceId";

            deleteAllSessionsSql = "DELETE FROM " + sessions;
            deleteAllSessionsDataSql = "DELETE FROM " + data;
            deleteDeviceSessionSql = deleteAllSessionsSql + " WHERE deviceId = @deviceId";
            deleteDeviceSessionsDataSql = deleteAllSessionsDataSql + " WHERE deviceId = @deviceId";
        }

        /// <summary>
        /// Gets the connection string to the PostgreSQL database.
        /// </summary>
        public string ConnectionString { get; }

        /// <summary>
        /// Gets or sets the table name prefix for database tables.
        /// </summary>
        /// <remarks>Changing it after construction has no effect, the statements are built in the constructor.</remarks>
        public string TablePrefix { get; set; }

        /// <summary>
        /// Gets or sets the fallback state form type.
        /// </summary>
        public Type FallbackStateForm { get; set; }

        /// <inheritdoc/>
        /// <summary>
        /// Saves form states to the PostgreSQL database, replacing all stored states.
        /// </summary>
        /// <param name="e">The <see cref="SaveStatesEventArgs"/> containing the states to be saved.</param>
        public void SaveFormStates(SaveStatesEventArgs e)
        {
            using (var connection = new NpgsqlConnection(ConnectionString))
            {
                connection.Open();

                using (var transaction = connection.BeginTransaction())
                {
                    //Cleanup old Session data
                    Execute(connection, transaction, deleteAllSessionsDataSql, null);
                    Execute(connection, transaction, deleteAllSessionsSql, null);

                    //Store session data in database
                    foreach (var state in e.States.States)
                    {
                        InsertState(connection, transaction, state);
                    }

                    transaction.Commit();
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

                foreach (var stateEntry in ReadEntries(connection, selectAllDevicesSessionsSql, null))
                {
                    LoadValues(connection, stateEntry);

                    stateContainer.States.Add(stateEntry);
                }
            }

            return stateContainer;
        }

        /// <inheritdoc/>
        public void SaveFormState(StateEntry state)
        {
            if (state == null)
            {
                throw new ArgumentNullException(nameof(state));
            }

            using (var connection = new NpgsqlConnection(ConnectionString))
            {
                connection.Open();

                using (var transaction = connection.BeginTransaction())
                {
                    DeleteState(connection, transaction, state.DeviceId);

                    InsertState(connection, transaction, state);

                    transaction.Commit();
                }
            }
        }

        /// <inheritdoc/>
        public StateEntry LoadFormState(long deviceId)
        {
            using (var connection = new NpgsqlConnection(ConnectionString))
            {
                connection.Open();

                var entries = ReadEntries(connection, selectDeviceSessionSql, deviceId);
                if (entries.Count == 0)
                {
                    return null;
                }

                LoadValues(connection, entries[0]);

                return entries[0];
            }
        }

        /// <inheritdoc/>
        public void RemoveFormState(long deviceId)
        {
            using (var connection = new NpgsqlConnection(ConnectionString))
            {
                connection.Open();

                using (var transaction = connection.BeginTransaction())
                {
                    DeleteState(connection, transaction, deviceId);

                    transaction.Commit();
                }
            }
        }

        private static void Execute(NpgsqlConnection connection, NpgsqlTransaction transaction, string sql, long? deviceId)
        {
            using (var command = new NpgsqlCommand(sql, connection, transaction))
            {
                if (deviceId != null)
                {
                    command.Parameters.Add(new NpgsqlParameter("@deviceId", NpgsqlDbType.Bigint) { Value = deviceId.Value });
                }

                command.ExecuteNonQuery();
            }
        }

        private void DeleteState(NpgsqlConnection connection, NpgsqlTransaction transaction, long deviceId)
        {
            Execute(connection, transaction, deleteDeviceSessionsDataSql, deviceId);
            Execute(connection, transaction, deleteDeviceSessionSql, deviceId);
        }

        /// <summary>
        /// Inserts the session row and its data rows.
        /// </summary>
        private void InsertState(NpgsqlConnection connection, NpgsqlTransaction transaction, StateEntry state)
        {
            using (var sessionCommand = new NpgsqlCommand(insertIntoSessionSql, connection, transaction))
            {
                sessionCommand.Parameters.Add(new NpgsqlParameter("@deviceId", NpgsqlDbType.Bigint) { Value = state.DeviceId });
                sessionCommand.Parameters.Add(new NpgsqlParameter("@deviceTitle", NpgsqlDbType.Varchar) { Value = state.ChatTitle ?? string.Empty });
                sessionCommand.Parameters.Add(new NpgsqlParameter("@FormUri", NpgsqlDbType.Varchar) { Value = state.FormUri ?? string.Empty });
                sessionCommand.Parameters.Add(new NpgsqlParameter("@QualifiedName", NpgsqlDbType.Varchar) { Value = state.QualifiedName ?? string.Empty });

                sessionCommand.ExecuteNonQuery();
            }

            if (state.Values == null)
            {
                return;
            }

            foreach (var data in state.Values)
            {
                //Nothing to restore for missing values
                if (data.Value == null)
                {
                    continue;
                }

                using (var dataCommand = new NpgsqlCommand(insertIntoSessionsDataSql, connection, transaction))
                {
                    dataCommand.Parameters.Add(new NpgsqlParameter("@deviceId", NpgsqlDbType.Bigint) { Value = state.DeviceId });
                    dataCommand.Parameters.Add(new NpgsqlParameter("@key", NpgsqlDbType.Varchar) { Value = data.Key });
                    dataCommand.Parameters.Add(new NpgsqlParameter("@value", NpgsqlDbType.Text) { Value = SerializeValue(data.Value) });
                    dataCommand.Parameters.Add(new NpgsqlParameter("@type", NpgsqlDbType.Varchar) { Value = data.Value.GetType().AssemblyQualifiedName });

                    dataCommand.ExecuteNonQuery();
                }
            }
        }

        private static List<StateEntry> ReadEntries(NpgsqlConnection connection, string sql, long? deviceId)
        {
            var entries = new List<StateEntry>();

            using (var command = new NpgsqlCommand(sql, connection))
            {
                if (deviceId != null)
                {
                    command.Parameters.Add(new NpgsqlParameter("@deviceId", NpgsqlDbType.Bigint) { Value = deviceId.Value });
                }

                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        entries.Add(new StateEntry
                        {
                            DeviceId = reader.GetInt64(0),
                            ChatTitle = reader.GetString(1),
                            FormUri = reader.GetString(2),
                            QualifiedName = reader.GetString(3)
                        });
                    }
                }
            }

            return entries;
        }

        /// <summary>
        /// Loads the data rows of a session into its state entry.
        /// </summary>
        private void LoadValues(NpgsqlConnection connection, StateEntry stateEntry)
        {
            using (var command = new NpgsqlCommand(selectDeviceSessionsDataSql, connection))
            {
                command.Parameters.Add(new NpgsqlParameter("@deviceId", NpgsqlDbType.Bigint) { Value = stateEntry.DeviceId });

                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var type = Type.GetType(reader.GetString(2));

                        //Skip values of types which don't exist (anymore)
                        if (type == null)
                        {
                            continue;
                        }

                        stateEntry.Values[reader.GetString(0)] = DeserializeValue(reader.GetString(1), type);
                    }
                }
            }
        }

        /// <summary>
        /// Serializes a state value as JSON.
        /// </summary>
        internal static string SerializeValue(object value)
        {
            return JsonSerializer.Serialize(value, value.GetType());
        }

        /// <summary>
        /// Deserializes a state value. Also reads values written by earlier versions, which stored strings as plain
        /// text instead of JSON.
        /// </summary>
        internal static object DeserializeValue(string value, Type type)
        {
            try
            {
                return JsonSerializer.Deserialize(value, type);
            }
            catch (JsonException)
            {
                if (type == typeof(string))
                {
                    return value;
                }

                if (type.IsPrimitive)
                {
                    return Convert.ChangeType(value, type, CultureInfo.InvariantCulture);
                }

                throw;
            }
        }
    }
}
