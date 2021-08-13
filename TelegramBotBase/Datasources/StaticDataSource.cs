using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TelegramBotBase.Datasources
{
    public class StaticDataSource<T> : Interfaces.IDataSource<T>
    {
        List<T> Data { get; set; }

        public StaticDataSource()
        {

        }

        public StaticDataSource(List<T> data)
        {
            this.Data = data;
        }


        public int Count
        {
            get
            {
                return Data.Count;
            }
        }

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
}
