using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using TelegramBotBase.Controls.Hybrid;
using TelegramBotBase.Datasources;
using TelegramBotBase.Form;

namespace TelegramBotBaseTest.Tests.Datasources
{
    public class CustomDataSource : ButtonFormDataSource
    {

        public List<String> Countries = new List<string>() { "Country 1", "Country 2", "Country 3" };

        public CustomDataSource()
        {
            loadData();
        }

        /// <summary>
        /// This method has the example purpose of creating and loading some example data.
        /// When using a database you do not need this kind of method.
        /// </summary>
        private void loadData()
        {
            //Exists data source? Read it
            if (File.Exists(AppContext.BaseDirectory + "countries.json"))
            {
                try
                {
                    var List = Newtonsoft.Json.JsonConvert.DeserializeObject<List<String>>(File.ReadAllText("countries.json"));


                    Countries = List;
                }
                catch
                {

                }


                return;
            }

            //If not, create it
            try
            {
                var countries = CultureInfo.GetCultures(CultureTypes.SpecificCultures).Select(a => a.DisplayName).ToList();

                Countries = countries;

                var tmp = Newtonsoft.Json.JsonConvert.SerializeObject(countries);

                File.WriteAllText( AppContext.BaseDirectory + "countries.json", tmp);
            }
            catch
            {

            }

        }

        public override ButtonRow ItemAt(int index)
        {
            var item = Countries.ElementAt(index);
            if (item == null)
                return new ButtonRow();

            return Render(item);
        }

        public override List<ButtonRow> ItemRange(int start, int count)
        {
            var items = Countries.Skip(start).Take(count);

            List<ButtonRow> lst = new List<ButtonRow>();
            foreach (var c in items)
            {
                lst.Add(Render(c));
            }

            return lst;
        }

        public override List<ButtonRow> AllItems()
        {
            List<ButtonRow> lst = new List<ButtonRow>();
            foreach (var c in Countries)
            {
                lst.Add(Render(c));
            }
            return lst;
        }

        public override ButtonForm PickItems(int start, int count, string filter = null)
        {
            List<ButtonRow> rows = ItemRange(start, count);

            ButtonForm lst = new ButtonForm();
            foreach (var c in rows)
            {
                lst.AddButtonRow(c);
            }
            return lst;
        }

        public override ButtonForm PickAllItems(string filter = null)
        {
            List<ButtonRow> rows = AllItems();

            ButtonForm bf = new ButtonForm();

            bf.AddButtonRows(rows);

            return bf;
        }

        public override int CalculateMax(string filter = null)
        {
            if (filter == null)
                return Countries.Count;

            return Countries.Where(a => a.IndexOf(filter, StringComparison.InvariantCultureIgnoreCase) != -1).Count();
        }

        public override ButtonRow Render(object data)
        {
            var s = data as String;
            if (s == null)
                return new ButtonRow(new ButtonBase("Empty", "zero"));

            return new ButtonRow(new ButtonBase(s, s));
        }

        public override int Count
        {
            get
            {
                return Countries.Count;
            }
        }

        public override int ColumnCount
        {
            get
            {
                return 1;
            }
        }

        public override int RowCount
        {
            get
            {
                return this.Count;
            }
        }

    }
}
