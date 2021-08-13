using System;
using System.Collections.Generic;
using System.Text;
using TelegramBotBase.Controls.Hybrid;

namespace TelegramBotBase.Interfaces
{
    public interface IDataSource<T>
    {

        /// <summary>
        /// Returns the amount of items within this source.
        /// </summary>
        /// <returns></returns>
        int Count { get; }


        /// <summary>
        /// Returns the item at the specific index.
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        T ItemAt(int index);


        /// <summary>
        /// Get all items from this source within this range.
        /// </summary>
        /// <param name="start"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        List<T> ItemRange(int start, int count);


        /// <summary>
        /// Gets a list of all items of this datasource.
        /// </summary>
        /// <returns></returns>
        List<T> AllItems();


    }
}
