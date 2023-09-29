using System;
using System.Data;
using Microsoft.Data.SqlClient;
using Newtonsoft.Json;
using TelegramBotBase.Args;
using TelegramBotBase.Base;
using TelegramBotBase.Form;
using TelegramBotBase.Interfaces;

namespace TelegramBotBase.Extensions.Serializer.Database.MSSQL
{
    public class MssqlSerializer : IStateMachine
    {
        /// <summary>
        ///     Will initialize the state machine.
        /// </summary>
        /// <param name="file">Path of the file and name where to save the session details.</param>
        /// <param name="fallbackStateForm">
        ///     Type of Form which will be saved instead of Form which has
        ///     <seealso cref="Attributes.IgnoreState" /> attribute declared. Needs to be subclass of
        ///     <seealso cref="Form.FormBase" />.
        /// </param>
        /// <param name="overwrite">Declares of the file could be overwritten.</param>
        public MssqlSerializer(string connectionString, string tablePrefix = "tgb_", Type fallbackStateForm = null)
        {
            ConnectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));

            TablePrefix = tablePrefix;

            FallbackStateForm = fallbackStateForm;

            if (FallbackStateForm != null && !FallbackStateForm.IsSubclassOf(typeof(FormBase)))
            {
                throw new ArgumentException($"{nameof(FallbackStateForm)} is not a subclass of FormBase");
            }
        }

        public string ConnectionString { get; }
        public string TablePrefix { get; set; }
        public Type FallbackStateForm { get; set; }

        public StateContainer LoadFormStates()
        {
            var sc = new StateContainer();

            using (var connection = new SqlConnection(ConnectionString))
            {
                connection.Open();

                var command = connection.CreateCommand();
                command.CommandText = "SELECT deviceId, deviceTitle, FormUri, QualifiedName FROM " + TablePrefix +
                                      "devices_sessions";

                var dataTable = new DataTable();
                using (var dataAdapter = new SqlDataAdapter(command))
                {
                    dataAdapter.Fill(dataTable);

                    foreach (DataRow r in dataTable.Rows)
                    {
                        var se = new StateEntry
                        {
                            DeviceId = (long)r["deviceId"],
                            ChatTitle = r["deviceTitle"].ToString(),
                            FormUri = r["FormUri"].ToString(),
                            QualifiedName = r["QualifiedName"].ToString()
                        };

                        sc.States.Add(se);

                        if (se.DeviceId > 0)
                        {
                            sc.ChatIds.Add(se.DeviceId);
                        }
                        else
                        {
                            sc.GroupIds.Add(se.DeviceId);
                        }

                        var command2 = connection.CreateCommand();
                        command2.CommandText = "SELECT [key], value, type FROM " + TablePrefix +
                                               "devices_sessions_data WHERE deviceId = @deviceId";
                        command2.Parameters.Add(new SqlParameter("@deviceId", r["deviceId"]));

                        var dataTable2 = new DataTable();
                        using (var dataAdapter2 = new SqlDataAdapter(command2))
                        {
                            dataAdapter2.Fill(dataTable2);

                            foreach (DataRow r2 in dataTable2.Rows)
                            {
                                var key = r2["key"].ToString();
                                var type = Type.GetType(r2["type"].ToString());

                                var value = JsonConvert.DeserializeObject(r2["value"].ToString(), type);

                                se.Values.Add(key, value);
                            }
                        }
                    }
                }


                connection.Close();
            }

            return sc;
        }

        public void SaveFormStates(SaveStatesEventArgs e)
        {
            var container = e.States;

            using (var connection = new SqlConnection(ConnectionString))
            {
                connection.Open();

                //Cleanup old Session data
                var clearCommand = connection.CreateCommand();

                clearCommand.CommandText = $"DELETE FROM {TablePrefix}devices_sessions_data";

                clearCommand.ExecuteNonQuery();

                clearCommand.CommandText = $"DELETE FROM {TablePrefix}devices_sessions";

                clearCommand.ExecuteNonQuery();

                //Prepare new session commands
                var sessionCommand = connection.CreateCommand();
                var dataCommand = connection.CreateCommand();

                sessionCommand.CommandText = "INSERT INTO " + TablePrefix +
                                             "devices_sessions (deviceId, deviceTitle, FormUri, QualifiedName) VALUES (@deviceId, @deviceTitle, @FormUri, @QualifiedName)";
                sessionCommand.Parameters.Add(new SqlParameter("@deviceId", ""));
                sessionCommand.Parameters.Add(new SqlParameter("@deviceTitle", ""));
                sessionCommand.Parameters.Add(new SqlParameter("@FormUri", ""));
                sessionCommand.Parameters.Add(new SqlParameter("@QualifiedName", ""));

                dataCommand.CommandText = "INSERT INTO " + TablePrefix +
                                          "devices_sessions_data (deviceId, [key], value, type) VALUES (@deviceId, @key, @value, @type)";
                dataCommand.Parameters.Add(new SqlParameter("@deviceId", ""));
                dataCommand.Parameters.Add(new SqlParameter("@key", ""));
                dataCommand.Parameters.Add(new SqlParameter("@value", ""));
                dataCommand.Parameters.Add(new SqlParameter("@type", ""));

                //Store session data in database
                foreach (var state in container.States)
                {
                    sessionCommand.Parameters["@deviceId"].Value = state.DeviceId;
                    sessionCommand.Parameters["@deviceTitle"].Value = state.ChatTitle ?? "";
                    sessionCommand.Parameters["@FormUri"].Value = state.FormUri;
                    sessionCommand.Parameters["@QualifiedName"].Value = state.QualifiedName;

                    sessionCommand.ExecuteNonQuery();

                    foreach (var data in state.Values)
                    {
                        dataCommand.Parameters["@deviceId"].Value = state.DeviceId;
                        dataCommand.Parameters["@key"].Value = data.Key;

                        var type = data.Value.GetType();

                        if (type.IsPrimitive || type.Equals(typeof(string)))
                        {
                            dataCommand.Parameters["@value"].Value = data.Value;
                        }
                        else
                        {
                            dataCommand.Parameters["@value"].Value = JsonConvert.SerializeObject(data.Value);
                        }

                        dataCommand.Parameters["@type"].Value = type.AssemblyQualifiedName;

                        dataCommand.ExecuteNonQuery();
                    }
                }

                connection.Close();
            }
        }
    }
}