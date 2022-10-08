using System;

namespace TelegramBotBase.Tools;

public static class Arrays
{
    public static T[] Shift<T>(T[] array, int positions)
    {
        var copy = new T[array.Length];
        Array.Copy(array, 0, copy, array.Length - positions, positions);
        Array.Copy(array, positions, copy, 0, array.Length - positions);
        return copy;
    }
}