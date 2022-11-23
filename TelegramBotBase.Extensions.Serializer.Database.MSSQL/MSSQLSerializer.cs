using TelegramBotBase.Interfaces;
using TelegramBotBase.Builder.Interfaces;
using System;
using TelegramBotBase.Base;
using TelegramBotBase.Args;
using TelegramBotBase.Form;
using Microsoft.Data.SqlClient;
using System.Data;

namespace TelegramBotBase.Extensions.Serializer.Database.MSSQL
{
    public class MSSQLSerializer : IStateMachine
    {
        public Type FallbackStateForm { get; set; }
        public string ConnectionString { get; }
        public String TablePrefix { get; set; }

        /// <summary>
        /// Will initialize the state machine.
        /// </summary>
        /// <param name="file">Path of the file and name where to save the session details.</param>
        /// <param name="fallbackStateForm">Type of Form which will be saved instead of Form which has <seealso cref="Attributes.IgnoreState"/> attribute declared. Needs to be subclass of <seealso cref="Form.FormBase"/>.</param>
        /// <param name="overwrite">Declares of the file could be overwritten.</param>
        public MSSQLSerializer(String ConnectionString, String tablePrefix = "tgb_", Type fallbackStateForm = null)
        {
            if (ConnectionString is null)
            {
                throw new ArgumentNullException(nameof(ConnectionString));
            }

            this.ConnectionString = ConnectionString;

            this.TablePrefix = tablePrefix;

            this.FallbackStateForm = fallbackStateForm;

            if (this.FallbackStateForm != null && !this.FallbackStateForm.IsSubclassOf(typeof(FormBase)))
            {
                throw new ArgumentException("FallbackStateForm is not a subclass of FormBase");
            }
        }

        public StateContainer LoadFormStates()
        {
            var sc = new StateContainer();

            using (var connection = new SqlConnection(ConnectionString))
            {
                connection.Open();

                var command = connection.CreateCommand();
                command.CommandText = "SELECT deviceId, deviceTitle, FormUri, QualifiedName FROM " + TablePrefix + "devices_sessions";

                var dataTable = new DataTable();
                using (var dataAdapter = new SqlDataAdapter(command))
                {
                    dataAdapter.Fill(dataTable);

                    foreach (DataRow r in dataTable.Rows)
                    {
                        var se = new StateEntry()
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

                        var data_command = connection.CreateCommand();
                        data_command.CommandText = "SELECT [key], value, type FROM " + TablePrefix + "devices_sessions_data WHERE deviceId = @deviceId";
                        data_command.Parameters.Add(new SqlParameter("@deviceId", r["deviceId"]));

                        var data_table = new DataTable();
                        using (var dataAdapter2 = new SqlDataAdapter(data_command))
                        {
                            dataAdapter2.Fill(data_table);

                            foreach (DataRow r2 in data_table.Rows)
                            {
                                var key = r2["key"].ToString();
                                var type = Type.GetType(r2["type"].ToString());

                                var value = Newtonsoft.Json.JsonConvert.DeserializeObject(r2["value"].ToString(), type);

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
                var clear_command = connection.CreateCommand();

                clear_command.CommandText = $"DELETE FROM {TablePrefix}devices_sessions_data";

                clear_command.ExecuteNonQuery();

                clear_command.CommandText = $"DELETE FROM {TablePrefix}devices_sessions";

                clear_command.ExecuteNonQuery();

                //Prepare new session commands
                var session_command = connection.CreateCommand();
                var data_command = connection.CreateCommand();

                session_command.CommandText = "INSERT INTO " + TablePrefix + "devices_sessions (deviceId, deviceTitle, FormUri, QualifiedName) VALUES (@deviceId, @deviceTitle, @FormUri, @QualifiedName)";
                session_command.Parameters.Add(new SqlParameter("@deviceId", ""));
                session_command.Parameters.Add(new SqlParameter("@deviceTitle", ""));
                session_command.Parameters.Add(new SqlParameter("@FormUri", ""));
                session_command.Parameters.Add(new SqlParameter("@QualifiedName", ""));

                data_command.CommandText = "INSERT INTO " + TablePrefix + "devices_sessions_data (deviceId, [key], value, type) VALUES (@deviceId, @key, @value, @type)";
                data_command.Parameters.Add(new SqlParameter("@deviceId", ""));
                data_command.Parameters.Add(new SqlParameter("@key", ""));
                data_command.Parameters.Add(new SqlParameter("@value", ""));
                data_command.Parameters.Add(new SqlParameter("@type", ""));

                //Store session data in database
                foreach (var state in container.States)
                {
                    session_command.Parameters["@deviceId"].Value = state.DeviceId;
                    session_command.Parameters["@deviceTitle"].Value = state.ChatTitle ?? "";
                    session_command.Parameters["@FormUri"].Value = state.FormUri;
                    session_command.Parameters["@QualifiedName"].Value = state.QualifiedName;

                    session_command.ExecuteNonQuery();

                    foreach (var data in state.Values)
                    {
                        data_command.Parameters["@deviceId"].Value = state.DeviceId;
                        data_command.Parameters["@key"].Value = data.Key;

                        var type = data.Value.GetType();
                        
                        if (type.IsPrimitive || type.Equals(typeof(string)))
                        {
                            data_command.Parameters["@value"].Value = data.Value;
                        }
                        else
                        {
                            data_command.Parameters["@value"].Value = Newtonsoft.Json.JsonConvert.SerializeObject(data.Value);
                        }
                        
                        data_command.Parameters["@type"].Value = type.AssemblyQualifiedName;

                        data_command.ExecuteNonQuery();
                    }

                }

                connection.Close();
            }


        }
    }
}