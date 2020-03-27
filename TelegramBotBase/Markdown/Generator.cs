using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot.Types.Enums;

namespace TelegramBotBase.Markdown
{
    public static class Generator
    {
        public static ParseMode OutputMode { get; set; } = ParseMode.Markdown;

        /// <summary>
        /// Generates a link with title in Markdown or HTML
        /// </summary>
        /// <param name="url"></param>
        /// <param name="title"></param>
        /// <param name="tooltip"></param>
        /// <returns></returns>
        public static String Link(this String url, String title = null, String tooltip = null)
        {
            switch (OutputMode)
            {
                case ParseMode.Markdown:
                    return "[" + (title ?? url) + "](" + url + " " + (tooltip ?? "") + ")";
                case ParseMode.Html:
                    return $"<a href=\"{url}\" title=\"{tooltip ?? ""}\">{title ?? ""}</b>";
            }
            return url;
        }

        /// <summary>
        /// Returns a Link to the User, title is optional.
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="title"></param>
        /// <returns></returns>
        public static String MentionUser(this long userId, String title = null)
        {
            return Link("tg://user?id=" + userId.ToString(), title);
        }

        /// <summary>
        /// Returns a bold text in Markdown or HTML
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        public static String Bold(this String text)
        {
            switch (OutputMode)
            {
                case ParseMode.Markdown:
                    return "*" + text + "*";
                case ParseMode.Html:
                    return "<b>" + text + "</b>";
            }
            return text;
        }

        /// <summary>
        /// Returns a strike through in Markdown or HTML
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        public static String Strikesthrough(this String text)
        {
            switch (OutputMode)
            {
                case ParseMode.Markdown:
                    return "~" + text + "~";
                case ParseMode.Html:
                    return "<s>" + text + "</s>";
            }
            return text;
        }

        /// <summary>
        /// Returns a italic text in Markdown or HTML
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        public static String Italic(this String text)
        {
            switch (OutputMode)
            {
                case ParseMode.Markdown:
                    return "_" + text + "_";
                case ParseMode.Html:
                    return "<i>" + text + "</i>";
            }
            return text;
        }

        /// <summary>
        /// Returns a underline text in Markdown or HTML
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        public static String Underline(this String text)
        {
            switch (OutputMode)
            {
                case ParseMode.Markdown:
                    return "__" + text + "__";
                case ParseMode.Html:
                    return "<u>" + text + "</u>";
            }
            return text;
        }

        /// <summary>
        /// Returns a monospace text in Markdown or HTML
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        public static String Monospace(this String text)
        {
            switch (OutputMode)
            {
                case ParseMode.Markdown:
                    return "`" + text + "`";
                case ParseMode.Html:
                    return "<code>" + text + "</code>";
            }
            return text;
        }

        /// <summary>
        /// Returns a multi monospace text in Markdown or HTML
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        public static String MultiMonospace(this String text)
        {
            switch (OutputMode)
            {
                case ParseMode.Markdown:
                    return "```" + text + "```";
                case ParseMode.Html:
                    return "<pre>" + text + "</pre>";
            }
            return text;
        }
    }
}
