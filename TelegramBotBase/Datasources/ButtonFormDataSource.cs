using System;
using System.Collections.Generic;
using System.Text;
using TelegramBotBase.Controls.Hybrid;
using TelegramBotBase.Form;
using TelegramBotBase.Interfaces;

namespace TelegramBotBase.Datasources
{
    public class ButtonFormDataSource : Interfaces.IDataSource<ButtonRow>
    {
        public virtual ButtonForm ButtonForm
        {
            get
            {
                return __buttonform;
            }
            set
            {
                __buttonform = value;
            }
        }

        private ButtonForm __buttonform = null;

        public ButtonFormDataSource()
        {
            __buttonform = new ButtonForm();
        }

        public ButtonFormDataSource(ButtonForm bf)
        {
            __buttonform = bf;
        }


        /// <summary>
        /// Returns the amount of rows existing.
        /// </summary>
        /// <returns></returns>
        public virtual int Count => ButtonForm.Count;


        /// <summary>
        /// Returns the amount of rows.
        /// </summary>
        public virtual int RowCount => ButtonForm.Rows;

        /// <summary>
        /// Returns the maximum amount of columns.
        /// </summary>
        public virtual int ColumnCount => ButtonForm.Cols;

        /// <summary>
        /// Returns the row with the specific index.
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public virtual ButtonRow ItemAt(int index)
        {
            return ButtonForm[index];
        }

        public virtual List<ButtonRow> ItemRange(int start, int count)
        {
            return ButtonForm.GetRange(start, count);
        }

        public virtual List<ButtonRow> AllItems()
        {
            return ButtonForm.ToArray();
        }

        public virtual ButtonForm PickItems(int start, int count, String filter = null)
        {
            ButtonForm bf = new ButtonForm();
            ButtonForm dataForm = null;

            if (filter == null)
            {
                dataForm = ButtonForm.Duplicate();
            }
            else
            {
                dataForm = ButtonForm.FilterDuplicate(filter, true);
            }

            for (int i = 0; i < count; i++)
            {
                int it = start + i;

                if (it > dataForm.Rows - 1)
                    break;

                bf.AddButtonRow(dataForm[it]);
            }

            return bf;
        }

        public virtual ButtonForm PickAllItems(String filter = null)
        {
            if (filter == null)
                return ButtonForm.Duplicate();


            return ButtonForm.FilterDuplicate(filter, true);
        }

        public virtual Tuple<ButtonRow, int> FindRow(String text, bool useText = true)
        {
            return ButtonForm.FindRow(text, useText);
        }

        /// <summary>
        /// Returns the maximum items of this data source.
        /// </summary>
        /// <param name="filter"></param>
        /// <returns></returns>
        public virtual int CalculateMax(String filter = null)
        {
            return PickAllItems(filter).Rows;
        }

        public virtual ButtonRow Render(object data)
        {
            return data as ButtonRow;
        }


        public static implicit operator ButtonFormDataSource(ButtonForm bf)
        {
            return new ButtonFormDataSource(bf);
        }

        public static implicit operator ButtonForm(ButtonFormDataSource ds)
        {
            return ds.ButtonForm;
        }

    }
}
