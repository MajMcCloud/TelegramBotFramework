using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot.Types.ReplyMarkups;
using TelegramBotBase.Base;

namespace TelegramBotBase.Form
{
    /// <summary>
    /// Base class for an buttons array
    /// </summary>
    public class ButtonForm
    {
        List<List<ButtonBase>> Buttons = new List<List<ButtonBase>>();


        public IReplyMarkup Markup { get; set; }

        public ControlBase DependencyControl { get; set; }

        public ButtonForm()
        {

        }

        public ButtonForm(ControlBase control)
        {
            this.DependencyControl = control;
        }

        public void AddButtonRow(IEnumerable<ButtonBase> row)
        {
            Buttons.Add(row.ToList());
        }

        public void AddButtonRow(params ButtonBase[] row)
        {
            AddButtonRow(row.ToList());
        }

        public static T[][] SplitTo<T>(IEnumerable<T> items, int itemsPerRow = 2)
        {
            T[][] splitted = default(T[][]);

            try
            {
                var t = items.Select((a, index) => new { a, index })
                             .GroupBy(a => a.index / itemsPerRow)
                             .Select(a => a.Select(b => b.a).ToArray()).ToArray();

                splitted = t;
            }
            catch
            {

            }

            return splitted;
        }

        public int Count
        {
            get
            {
                return this.Buttons.Select(a => a.ToArray()).Aggregate((a, b) => a.Union(b).ToArray()).Length;
            }
        }

        /// <summary>
        /// Add buttons splitted in the amount of columns (i.e. 2 per row...)
        /// </summary>
        /// <param name="buttons"></param>
        /// <param name="buttonsPerRow"></param>
        public void AddSplitted(IEnumerable<ButtonBase> buttons, int buttonsPerRow = 2)
        {
            var sp = SplitTo<ButtonBase>(buttons, buttonsPerRow);

            foreach (var bl in sp)
            {
                AddButtonRow(bl);
            }
        }

        public List<ButtonBase> ToList()
        {
            return this.Buttons.Aggregate((a, b) => a.Union(b).ToList());
        }

        public InlineKeyboardButton[][] ToInlineButtonArray()
        {
            var ikb = this.Buttons.Select(a => a.Select(b => b.ToInlineButton(this)).ToArray()).ToArray();

            return ikb;
        }

        public KeyboardButton[][] ToReplyButtonArray()
        {
            var ikb = this.Buttons.Select(a => a.Select(b => b.ToKeyboardButton(this)).ToArray()).ToArray();

            return ikb;
        }

        public static implicit operator InlineKeyboardMarkup(ButtonForm form)
        {
            if (form == null)
                return null;

            InlineKeyboardMarkup ikm = new InlineKeyboardMarkup(form.ToInlineButtonArray());

            return ikm;
        }

        public static implicit operator ReplyKeyboardMarkup(ButtonForm form)
        {
            if (form == null)
                return null;

            ReplyKeyboardMarkup ikm = new ReplyKeyboardMarkup(form.ToReplyButtonArray());

            return ikm;
        }
    }
}
