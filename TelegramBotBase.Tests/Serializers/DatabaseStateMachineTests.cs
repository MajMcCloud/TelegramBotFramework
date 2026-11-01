using Microsoft.Data.SqlClient;
using Npgsql;
using TelegramBotBase.Args;
using TelegramBotBase.Base;
using TelegramBotBase.Extensions.Serializer.Database.MSSQL;
using TelegramBotBase.Extensions.Serializer.Database.PostgreSql;
using TelegramBotBase.Interfaces;
using TelegramBotBase.Tests.Infrastructure;

namespace TelegramBotBase.Tests.Serializers;

/// <summary>
///     Runs the database serializers against real servers. Skipped unless a connection string is given:
///     <c>TGB_TEST_MSSQL</c> for SQL Server, <c>TGB_TEST_POSTGRES</c> for PostgreSQL.
///     Every test creates its own tables (unique prefix) and drops them afterwards.
/// </summary>
public abstract class DatabaseStateMachineTests : IDisposable
{
    private static readonly TimeSpan Idle = TimeSpan.FromMinutes(30);

    protected DatabaseStateMachineTests(string connectionStringVariable)
    {
        ConnectionString = Environment.GetEnvironmentVariable(connectionStringVariable);
        Prefix = "t" + Guid.NewGuid().ToString("N").Substring(0, 12) + "_";

        if (ConnectionString != null)
        {
            Execute(CreateTablesSql());
        }
    }

    protected string ConnectionString { get; }

    protected string Prefix { get; }

    public void Dispose()
    {
        if (ConnectionString != null)
        {
            Execute(DropTablesSql());
        }
    }

    protected abstract string CreateTablesSql();

    protected abstract string DropTablesSql();

    /// <summary>
    ///     Inserts a data row the way earlier versions did: string values unquoted.
    /// </summary>
    protected abstract string InsertLegacyRowsSql(long deviceId);

    protected abstract IDeviceStateMachine CreateMachine();

    protected abstract void Execute(string sql);

    private void RequireDatabase()
    {
        Skip.If(ConnectionString == null, "No database connection string set.");
    }

    private static StateEntry Entry(long deviceId, string note, int counter)
    {
        var entry = new StateEntry
        {
            DeviceId = deviceId,
            ChatTitle = "Chat " + deviceId,
            FormUri = typeof(StatefulForm).FullName,
            QualifiedName = typeof(StatefulForm).AssemblyQualifiedName
        };

        entry.Values["$Note"] = note;
        entry.Values["counter"] = counter;
        entry.Values["flag"] = true;
        entry.Values["missing"] = null;

        return entry;
    }

    [SkippableFact]
    public void SaveAndLoadFormState_RoundTripsOneDevice()
    {
        RequireDatabase();
        var machine = CreateMachine();

        machine.SaveFormState(Entry(-1001234567890, "group note", 7));

        var loaded = machine.LoadFormState(-1001234567890);

        Assert.NotNull(loaded);
        Assert.Equal(-1001234567890, loaded.DeviceId);
        Assert.Equal("Chat -1001234567890", loaded.ChatTitle);
        Assert.Equal(typeof(StatefulForm).AssemblyQualifiedName, loaded.QualifiedName);
        Assert.Equal("group note", loaded.Values["$Note"]);
        Assert.Equal(7, loaded.Values["counter"]);
        Assert.Equal(true, loaded.Values["flag"]);
        Assert.False(loaded.Values.ContainsKey("missing"));
    }

    [SkippableFact]
    public void LoadFormState_OfUnknownDevice_ReturnsNull()
    {
        RequireDatabase();

        Assert.Null(CreateMachine().LoadFormState(404));
    }

    [SkippableFact]
    public void SaveFormState_ReplacesPreviousState()
    {
        RequireDatabase();
        var machine = CreateMachine();

        machine.SaveFormState(Entry(1, "first", 1));
        machine.SaveFormState(Entry(1, "second", 2));

        var loaded = machine.LoadFormState(1);
        Assert.Equal("second", loaded.Values["$Note"]);
        Assert.Equal(2, loaded.Values["counter"]);

        var all = machine.LoadFormStates();
        Assert.Single(all.States);
        Assert.Equal(3, all.States[0].Values.Count);
    }

    [SkippableFact]
    public void RemoveFormState_RemovesOnlyThatDevice()
    {
        RequireDatabase();
        var machine = CreateMachine();

        machine.SaveFormState(Entry(1, "one", 1));
        machine.SaveFormState(Entry(2, "two", 2));

        machine.RemoveFormState(1);
        machine.RemoveFormState(3);

        Assert.Null(machine.LoadFormState(1));
        Assert.Equal("two", machine.LoadFormState(2).Values["$Note"]);
    }

    [SkippableFact]
    public void BulkSave_ReplacesAllStates()
    {
        RequireDatabase();
        var machine = CreateMachine();

        machine.SaveFormState(Entry(1, "old", 1));

        var container = new StateContainer();
        container.States.Add(Entry(2, "two", 2));
        container.States.Add(Entry(3, "three", 3));
        machine.SaveFormStates(new SaveStatesEventArgs(container));

        var states = machine.LoadFormStates().States.ToDictionary(a => a.DeviceId);

        Assert.Equal(new long[] { 2, 3 }, states.Keys.OrderBy(a => a));
        Assert.Equal("three", states[3].Values["$Note"]);
        Assert.Equal(3, states[3].Values["counter"]);
    }

    [SkippableFact]
    public void LoadFormState_ReadsRowsOfEarlierVersions()
    {
        RequireDatabase();

        Execute(InsertLegacyRowsSql(5));

        var loaded = CreateMachine().LoadFormState(5);

        Assert.Equal("legacy text", loaded.Values["$Note"]);
    }

    [SkippableFact]
    public async Task Bot_EvictsToDatabase_RestoresAndDiscards()
    {
        RequireDatabase();
        var machine = CreateMachine();
        var bot = TestBot.Create(typeof(StatefulForm), machine, l => l.IdleTimeout = Idle);

        await bot.Send(1);
        var form = bot.ActiveForm<StatefulForm>(1);
        form.Note = "persisted";
        form.Counter = 41;

        bot.Advance(TimeSpan.FromDays(1));
        Assert.Equal(1, await bot.Sessions.EvictSessions());

        var saved = machine.LoadFormState(1);
        Assert.NotNull(saved);
        Assert.Equal(41, saved.Values["counter"]);

        await bot.Send(1);
        var restored = bot.ActiveForm<StatefulForm>(1);
        Assert.Equal("persisted", restored.Note);
        Assert.Equal(41, restored.Counter);
        Assert.Single(bot.Began);

        // Saving all sessions updates the active one and keeps others.
        machine.SaveFormState(Entry(2, "evicted earlier", 2));
        restored.Counter = 42;
        await bot.Sessions.SaveSessionStates();

        Assert.Equal(42, machine.LoadFormState(1).Values["counter"]);
        Assert.NotNull(machine.LoadFormState(2));

        await bot.Sessions.EndSessionAsync(1);
        Assert.Null(machine.LoadFormState(1));

        Assert.Empty(bot.Errors);
    }
}

public sealed class MssqlStateMachineTests : DatabaseStateMachineTests
{
    public MssqlStateMachineTests() : base("TGB_TEST_MSSQL")
    {
    }

    protected override string CreateTablesSql()
    {
        return $@"
CREATE TABLE {Prefix}devices_sessions (
    deviceId bigint NOT NULL PRIMARY KEY,
    deviceTitle nvarchar(512) NOT NULL,
    FormUri nvarchar(512) NOT NULL,
    QualifiedName nvarchar(512) NOT NULL);
CREATE TABLE {Prefix}devices_sessions_data (
    Id uniqueidentifier NOT NULL DEFAULT newid() PRIMARY KEY,
    deviceId bigint NOT NULL,
    [key] nvarchar(512) NOT NULL,
    value nvarchar(max) NOT NULL,
    type nvarchar(512) NOT NULL);
CREATE INDEX IX_{Prefix}data_deviceId ON {Prefix}devices_sessions_data (deviceId);";
    }

    protected override string DropTablesSql()
    {
        return $"DROP TABLE {Prefix}devices_sessions_data; DROP TABLE {Prefix}devices_sessions;";
    }

    protected override string InsertLegacyRowsSql(long deviceId)
    {
        return $@"
INSERT INTO {Prefix}devices_sessions (deviceId, deviceTitle, FormUri, QualifiedName)
    VALUES ({deviceId}, '', 'Legacy', 'Legacy');
INSERT INTO {Prefix}devices_sessions_data (deviceId, [key], value, type)
    VALUES ({deviceId}, '$Note', 'legacy text', '{typeof(string).AssemblyQualifiedName}');";
    }

    protected override IDeviceStateMachine CreateMachine()
    {
        return new MssqlSerializer(ConnectionString, Prefix);
    }

    protected override void Execute(string sql)
    {
        using var connection = new SqlConnection(ConnectionString);
        connection.Open();

        using var command = new SqlCommand(sql, connection);
        command.ExecuteNonQuery();
    }
}

public sealed class PostgreSqlStateMachineTests : DatabaseStateMachineTests
{
    public PostgreSqlStateMachineTests() : base("TGB_TEST_POSTGRES")
    {
    }

    protected override string CreateTablesSql()
    {
        return $@"
CREATE TABLE {Prefix}devices_sessions (
    deviceId bigint NOT NULL PRIMARY KEY,
    deviceTitle character varying(512) NOT NULL,
    ""FormUri"" character varying(512) NOT NULL,
    ""QualifiedName"" character varying(512) NOT NULL);
CREATE TABLE {Prefix}devices_sessions_data (
    Id uuid NOT NULL DEFAULT gen_random_uuid() PRIMARY KEY,
    deviceId bigint NOT NULL,
    key character varying(512) NOT NULL,
    ""value"" text NOT NULL,
    ""type"" character varying(512) NOT NULL);
CREATE INDEX IX_{Prefix}data_deviceId ON {Prefix}devices_sessions_data (deviceId);";
    }

    protected override string DropTablesSql()
    {
        return $"DROP TABLE {Prefix}devices_sessions_data; DROP TABLE {Prefix}devices_sessions;";
    }

    protected override string InsertLegacyRowsSql(long deviceId)
    {
        return $@"
INSERT INTO {Prefix}devices_sessions (deviceId, deviceTitle, ""FormUri"", ""QualifiedName"")
    VALUES ({deviceId}, '', 'Legacy', 'Legacy');
INSERT INTO {Prefix}devices_sessions_data (deviceId, key, value, type)
    VALUES ({deviceId}, '$Note', 'legacy text', '{typeof(string).AssemblyQualifiedName}');";
    }

    protected override IDeviceStateMachine CreateMachine()
    {
        return new PostgreSqlSerializer(ConnectionString, Prefix);
    }

    protected override void Execute(string sql)
    {
        using var connection = new NpgsqlConnection(ConnectionString);
        connection.Open();

        using var command = new NpgsqlCommand(sql, connection);
        command.ExecuteNonQuery();
    }
}
