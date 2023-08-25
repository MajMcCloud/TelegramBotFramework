using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using TelegramBotBase.Controls.Hybrid;
using TelegramBotBase.DataSources;
using TelegramBotBase.Form;

namespace TelegramBotBase.Example.Tests.DataSources;

public class CustomDataSource : ButtonFormDataSource
{
    public List<string> Countries = new() { "Country 1", "Country 2", "Country 3" };

    public CustomDataSource()
    {
        LoadData();
    }

    public override int Count => Countries.Count;

    public override int ColumnCount => 1;

    public override int RowCount => Count;

    /// <summary>
    ///     This method has the example purpose of creating and loading some example data.
    ///     When using a database you do not need this kind of method.
    /// </summary>
    private void LoadData()
    {
        //Exists data source? Read it
        if (File.Exists(AppContext.BaseDirectory + "countries.json"))
        {
            try
            {
                var list = JsonConvert.DeserializeObject<List<string>>(File.ReadAllText("countries.json"));


                Countries = list;
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

            var tmp = JsonConvert.SerializeObject(countries);

            File.WriteAllText(AppContext.BaseDirectory + "countries.json", tmp);
        }
        catch
        {
        }
    }

    public override ButtonRow ItemAt(int index)
    {
        var item = Countries.ElementAt(index);
        if (item == null)
        {
            return new ButtonRow();
        }

        return Render(item);
    }

    public override List<ButtonRow> ItemRange(int start, int count)
    {
        var items = Countries.Skip(start).Take(count);

        var lst = new List<ButtonRow>();
        foreach (var c in items)
        {
            lst.Add(Render(c));
        }

        return lst;
    }

    public override List<ButtonRow> AllItems()
    {
        var lst = new List<ButtonRow>();
        foreach (var c in Countries)
        {
            lst.Add(Render(c));
        }

        return lst;
    }

    public override ButtonForm PickItems(int start, int count, string filter = null)
    {
        var rows = ItemRange(start, count);

        var lst = new ButtonForm();
        foreach (var c in rows)
        {
            lst.AddButtonRow(c);
        }

        return lst;
    }

    public override ButtonForm PickAllItems(string filter = null)
    {
        var rows = AllItems();

        var bf = new ButtonForm();

        bf.AddButtonRows(rows);

        return bf;
    }

    public override int CalculateMax(string filter = null)
    {
        if (filter == null)
        {
            return Countries.Count;
        }

        return Countries.Where(a => a.IndexOf(filter, StringComparison.InvariantCultureIgnoreCase) != -1).Count();
    }

    public override ButtonRow Render(object data)
    {
        if (!(data is string s))
        {
            return new ButtonRow(new ButtonBase("Empty", "zero"));
        }

        return new ButtonRow(new ButtonBase(s, s));
    }
}
