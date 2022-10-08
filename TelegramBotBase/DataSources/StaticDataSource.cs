using System.Collections.Generic;
using System.Linq;
using TelegramBotBase.Interfaces;

namespace TelegramBotBase.DataSources;

public class StaticDataSource<T> : IDataSource<T>
{
    public StaticDataSource()
    {
    }

    public StaticDataSource(List<T> data)
    {
        Data = data;
    }

    private List<T> Data { get; }


    public int Count => Data.Count;

    public T ItemAt(int index)
    {
        return Data[index];
    }

    public List<T> ItemRange(int start, int count)
    {
        return Data.Skip(start).Take(count).ToList();
    }

    public List<T> AllItems()
    {
        return Data;
    }
}