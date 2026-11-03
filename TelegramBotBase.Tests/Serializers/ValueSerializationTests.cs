using TelegramBotBase.Extensions.Serializer.Database.MSSQL;
using TelegramBotBase.Extensions.Serializer.Database.PostgreSql;

namespace TelegramBotBase.Tests.Serializers;

/// <summary>
///     Value conversion of the database serializers, no database needed.
/// </summary>
public class ValueSerializationTests
{
    public static IEnumerable<object[]> Values()
    {
        yield return new object[] { "hello" };
        yield return new object[] { "" };
        yield return new object[] { "123" };
        yield return new object[] { "with \"quotes\" and ünïcode" };
        yield return new object[] { 42 };
        yield return new object[] { -9_000_000_000L };
        yield return new object[] { true };
        yield return new object[] { false };
        yield return new object[] { 1.5d };
        yield return new object[] { DayOfWeek.Friday };
        yield return new object[] { new DateTime(2026, 11, 1, 12, 30, 0, DateTimeKind.Utc) };
        yield return new object[] { new List<int> { 1, 2, 3 } };
    }

    [Theory]
    [MemberData(nameof(Values))]
    public void Mssql_RoundTripsValues(object value)
    {
        var serialized = MssqlSerializer.SerializeValue(value);

        Assert.Equal(value, MssqlSerializer.DeserializeValue(serialized, value.GetType()));
    }

    [Theory]
    [MemberData(nameof(Values))]
    public void PostgreSql_RoundTripsValues(object value)
    {
        var serialized = PostgreSqlSerializer.SerializeValue(value);

        Assert.Equal(value, PostgreSqlSerializer.DeserializeValue(serialized, value.GetType()));
    }

    /// <summary>
    ///     Earlier versions stored strings and primitives unquoted (SQL Server converting them to text).
    /// </summary>
    [Theory]
    [InlineData("hello", typeof(string), "hello")]
    [InlineData("hello world", typeof(string), "hello world")]
    [InlineData("5", typeof(int), 5)]
    [InlineData("1", typeof(bool), true)]
    [InlineData("0", typeof(bool), false)]
    [InlineData("True", typeof(bool), true)]
    [InlineData("1.5", typeof(double), 1.5d)]
    public void Mssql_ReadsLegacyValues(string stored, Type type, object expected)
    {
        Assert.Equal(expected, MssqlSerializer.DeserializeValue(stored, type));
    }

    [Theory]
    [InlineData("hello", typeof(string), "hello")]
    [InlineData("5", typeof(int), 5)]
    [InlineData("True", typeof(bool), true)]
    public void PostgreSql_ReadsLegacyValues(string stored, Type type, object expected)
    {
        Assert.Equal(expected, PostgreSqlSerializer.DeserializeValue(stored, type));
    }
}
