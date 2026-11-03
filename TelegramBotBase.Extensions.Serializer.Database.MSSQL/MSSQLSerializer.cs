using System;
using System.Data;
using System.Globalization;
using Microsoft.Data.SqlClient;
using Newtonsoft.Json;
using TelegramBotBase.Args;
using TelegramBotBase.Base;
using TelegramBotBase.Form;
using TelegramBotBase.Interfaces;

namespace TelegramBotBase.Extensions.Serializer.Database.MSSQL
{
    /// <summary>
    ///     Saves and restores sessions in a Microsoft SQL Server database. Supports per device access
    ///     (<see cref="IDeviceStateMachine" />), which is used together with session limits.
    /// </summary>
    public class MssqlSerializer : IDeviceStateMachine
    {
        /// <summary>
        ///     Will initialize the state machine.
        /// </summary>
        /// <param name="connectionString">The connection string to the database.</param>
        /// <param name="tablePrefix">The prefix of the session tables.</param>
        /// <param name="fallbackStateForm">
        ///     Type of Form which will be saved instead of Form which has
        ///     <seealso cref="Attributes.IgnoreState" /> attribute declared. Needs to be subclass of
        ///     <seealso cref="Form.FormBase" />.
        /// </param>
        public MssqlSerializer(string connectionString, string tablePrefix = "tgb_", Type fallbackStateForm = null)
        {
            ConnectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));

            TablePrefix = tablePrefix;

            FallbackStateForm = fallbackStateForm;

            if (FallbackStateForm != null && !FallbackStateForm.IsSubclassOf(typeof(FormBase)))
            {
                throw new ArgumentException($"{nameof(FallbackStateForm)} is not a subclass of {nameof(FormBase)}");
            }
        }

        public string ConnectionString { get; }
        public string TablePrefix { get; set; }
        public Type FallbackStateForm { get; set; }

        private string SessionsTableName => TablePrefix + "devices_sessions";

        private string DataTableName => TablePrefix + "devices_sessions_data";

        public StateContainer LoadFormStates()
        {
            var sc = new StateContainer();

            using (var connection = new SqlConnection(ConnectionString))
            {
                connection.Open();

                var command = connection.CreateCommand();
                command.CommandText = $"SELECT deviceId, deviceTitle, FormUri, QualifiedName FROM {SessionsTableName}";

                var sessions = Fill(command);

                foreach (DataRow r in sessions.Rows)
                {
                    var se = ReadEntry(r);

                    LoadValues(connection, se);

                    sc.States.Add(se);
                }
            }

            return sc;
        }

        public void SaveFormStates(SaveStatesEventArgs e)
        {
            using (var connection = new SqlConnection(ConnectionString))
            {
                connection.Open();

                using (var transaction = connection.BeginTransaction())
                {
                    //Cleanup old Session data
                    var clearCommand = connection.CreateCommand();
                    clearCommand.Transaction = transaction;

                    clearCommand.CommandText = $"DELETE FROM {DataTableName}";
                    clearCommand.ExecuteNonQuery();

                    clearCommand.CommandText = $"DELETE FROM {SessionsTableName}";
                    clearCommand.ExecuteNonQuery();

                    //Store session data in database
                    foreach (var state in e.States.States)
                    {
                        InsertState(connection, transaction, state);
                    }

                    transaction.Commit();
                }
            }
        }

        public void SaveFormState(StateEntry state)
        {
            if (state == null)
            {
                throw new ArgumentNullException(nameof(state));
            }

            using (var connection = new SqlConnection(ConnectionString))
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

        public StateEntry LoadFormState(long deviceId)
        {
            using (var connection = new SqlConnection(ConnectionString))
            {
                connection.Open();

                var command = connection.CreateCommand();
                command.CommandText =
                    $"SELECT deviceId, deviceTitle, FormUri, QualifiedName FROM {SessionsTableName} WHERE deviceId = @deviceId";
                command.Parameters.Add(new SqlParameter("@deviceId", SqlDbType.BigInt) { Value = deviceId });

                var sessions = Fill(command);
                if (sessions.Rows.Count == 0)
                {
                    return null;
                }

                var se = ReadEntry(sessions.Rows[0]);

                LoadValues(connection, se);

                return se;
            }
        }

        public void RemoveFormState(long deviceId)
        {
            using (var connection = new SqlConnection(ConnectionString))
            {
                connection.Open();

                using (var transaction = connection.BeginTransaction())
                {
                    DeleteState(connection, transaction, deviceId);

                    transaction.Commit();
                }
            }
        }

        private static DataTable Fill(SqlCommand command)
        {
            var table = new DataTable();
            using (var adapter = new SqlDataAdapter(command))
            {
                adapter.Fill(table);
            }

            return table;
        }

        private static StateEntry ReadEntry(DataRow r)
        {
            return new StateEntry
            {
                DeviceId = (long)r["deviceId"],
                ChatTitle = r["deviceTitle"].ToString(),
                FormUri = r["FormUri"].ToString(),
                QualifiedName = r["QualifiedName"].ToString()
            };
        }

        private void LoadValues(SqlConnection connection, StateEntry se)
        {
            var command = connection.CreateCommand();
            command.CommandText = $"SELECT [key], value, type FROM {DataTableName} WHERE deviceId = @deviceId";
            command.Parameters.Add(new SqlParameter("@deviceId", SqlDbType.BigInt) { Value = se.DeviceId });

            foreach (DataRow r in Fill(command).Rows)
            {
                var type = Type.GetType(r["type"].ToString());

                //Skip values of types which don't exist (anymore)
                if (type == null)
                {
                    continue;
                }

                se.Values[r["key"].ToString()] = DeserializeValue(r["value"].ToString(), type);
            }
        }

        private void DeleteState(SqlConnection connection, SqlTransaction transaction, long deviceId)
        {
            var command = connection.CreateCommand();
            command.Transaction = transaction;
            command.Parameters.Add(new SqlParameter("@deviceId", SqlDbType.BigInt) { Value = deviceId });

            command.CommandText = $"DELETE FROM {DataTableName} WHERE deviceId = @deviceId";
            command.ExecuteNonQuery();

            command.CommandText = $"DELETE FROM {SessionsTableName} WHERE deviceId = @deviceId";
            command.ExecuteNonQuery();
        }

        private void InsertState(SqlConnection connection, SqlTransaction transaction, StateEntry state)
        {
            var sessionCommand = connection.CreateCommand();
            sessionCommand.Transaction = transaction;
            sessionCommand.CommandText =
                $"INSERT INTO {SessionsTableName} (deviceId, deviceTitle, FormUri, QualifiedName) VALUES (@deviceId, @deviceTitle, @FormUri, @QualifiedName)";
            sessionCommand.Parameters.Add(new SqlParameter("@deviceId", SqlDbType.BigInt) { Value = state.DeviceId });
            sessionCommand.Parameters.Add(new SqlParameter("@deviceTitle", SqlDbType.NVarChar, 512) { Value = state.ChatTitle ?? "" });
            sessionCommand.Parameters.Add(new SqlParameter("@FormUri", SqlDbType.NVarChar, 512) { Value = state.FormUri ?? "" });
            sessionCommand.Parameters.Add(new SqlParameter("@QualifiedName", SqlDbType.NVarChar, 512) { Value = state.QualifiedName ?? "" });
            sessionCommand.ExecuteNonQuery();

            if (state.Values == null)
            {
                return;
            }

            var dataCommand = connection.CreateCommand();
            dataCommand.Transaction = transaction;
            dataCommand.CommandText =
                $"INSERT INTO {DataTableName} (deviceId, [key], value, type) VALUES (@deviceId, @key, @value, @type)";
            dataCommand.Parameters.Add(new SqlParameter("@deviceId", SqlDbType.BigInt) { Value = state.DeviceId });
            dataCommand.Parameters.Add(new SqlParameter("@key", SqlDbType.NVarChar, 512));
            dataCommand.Parameters.Add(new SqlParameter("@value", SqlDbType.NVarChar, -1));
            dataCommand.Parameters.Add(new SqlParameter("@type", SqlDbType.NVarChar, 512));

            foreach (var data in state.Values)
            {
                //Nothing to restore for missing values
                if (data.Value == null)
                {
                    continue;
                }

                dataCommand.Parameters["@key"].Value = data.Key;
                dataCommand.Parameters["@value"].Value = SerializeValue(data.Value);
                dataCommand.Parameters["@type"].Value = data.Value.GetType().AssemblyQualifiedName;

                dataCommand.ExecuteNonQuery();
            }
        }

        /// <summary>
        ///     Serializes a state value as JSON.
        /// </summary>
        internal static string SerializeValue(object value)
        {
            return JsonConvert.SerializeObject(value);
        }

        /// <summary>
        ///     Deserializes a state value. Also reads values written by earlier versions, which stored strings and
        ///     primitives as plain text instead of JSON.
        /// </summary>
        internal static object DeserializeValue(string value, Type type)
        {
            try
            {
                return JsonConvert.DeserializeObject(value, type);
            }
            catch (JsonException)
            {
                if (type == typeof(string))
                {
                    return value;
                }

                if (type == typeof(bool) && (value == "0" || value == "1"))
                {
                    return value == "1";
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
