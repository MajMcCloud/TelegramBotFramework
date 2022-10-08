using System.Linq;
using Telegram.Bot.Types.Enums;

namespace TelegramBotBase.Markdown;

/// <summary>
///     https://core.telegram.org/bots/api#markdownv2-style
/// </summary>
public static class Generator
{
    public static ParseMode OutputMode { get; set; } = ParseMode.Markdown;

    /// <summary>
    ///     Generates a link with title in Markdown or HTML
    /// </summary>
    /// <param name="url"></param>
    /// <param name="title"></param>
    /// <param name="tooltip"></param>
    /// <returns></returns>
    public static string Link(this string url, string title = null, string tooltip = null)
    {
        return OutputMode switch
        {
            ParseMode.Markdown => "[" + (title ?? url) + "](" + url + " " + (tooltip ?? "") + ")",
            ParseMode.Html => $"<a href=\"{url}\" title=\"{tooltip ?? ""}\">{title ?? ""}</b>",
            _ => url
        };
    }

    /// <summary>
    ///     Returns a Link to the User, title is optional.
    /// </summary>
    /// <param name="userId"></param>
    /// <param name="title"></param>
    /// <returns></returns>
    public static string MentionUser(this int userId, string title = null)
    {
        return Link("tg://user?id=" + userId, title);
    }

    /// <summary>
    ///     Returns a Link to the User, title is optional.
    /// </summary>
    /// <param name="userId"></param>
    /// <param name="title"></param>
    /// <returns></returns>
    public static string MentionUser(this string username, string title = null)
    {
        return Link("tg://user?id=" + username, title);
    }

    /// <summary>
    ///     Returns a bold text in Markdown or HTML
    /// </summary>
    /// <param name="text"></param>
    /// <returns></returns>
    public static string Bold(this string text)
    {
        return OutputMode switch
        {
            ParseMode.Markdown => "*" + text + "*",
            ParseMode.Html => "<b>" + text + "</b>",
            _ => text
        };
    }

    /// <summary>
    ///     Returns a strike through in Markdown or HTML
    /// </summary>
    /// <param name="text"></param>
    /// <returns></returns>
    public static string Strikesthrough(this string text)
    {
        return OutputMode switch
        {
            ParseMode.Markdown => "~" + text + "~",
            ParseMode.Html => "<s>" + text + "</s>",
            _ => text
        };
    }

    /// <summary>
    ///     Returns a italic text in Markdown or HTML
    /// </summary>
    /// <param name="text"></param>
    /// <returns></returns>
    public static string Italic(this string text)
    {
        return OutputMode switch
        {
            ParseMode.Markdown => "_" + text + "_",
            ParseMode.Html => "<i>" + text + "</i>",
            _ => text
        };
    }

    /// <summary>
    ///     Returns a underline text in Markdown or HTML
    /// </summary>
    /// <param name="text"></param>
    /// <returns></returns>
    public static string Underline(this string text)
    {
        return OutputMode switch
        {
            ParseMode.Markdown => "__" + text + "__",
            ParseMode.Html => "<u>" + text + "</u>",
            _ => text
        };
    }

    /// <summary>
    ///     Returns a monospace text in Markdown or HTML
    /// </summary>
    /// <param name="text"></param>
    /// <returns></returns>
    public static string Monospace(this string text)
    {
        return OutputMode switch
        {
            ParseMode.Markdown => "`" + text + "`",
            ParseMode.Html => "<code>" + text + "</code>",
            _ => text
        };
    }

    /// <summary>
    ///     Returns a multi monospace text in Markdown or HTML
    /// </summary>
    /// <param name="text"></param>
    /// <returns></returns>
    public static string MultiMonospace(this string text)
    {
        return OutputMode switch
        {
            ParseMode.Markdown => "```" + text + "```",
            ParseMode.Html => "<pre>" + text + "</pre>",
            _ => text
        };
    }

    /// <summary>
    ///     Escapes all characters as stated in the documentation: https://core.telegram.org/bots/api#markdownv2-style
    /// </summary>
    /// <param name="text"></param>
    /// <returns></returns>
    public static string MarkdownV2Escape(this string text, params char[] toKeep)
    {
        var toEscape = new[]
            { '_', '*', '[', ']', '(', ')', '~', '`', '>', '#', '+', '-', '=', '|', '{', '}', '.', '!' };

        return text.EscapeAll(toEscape.Where(a => !toKeep.Contains(a)).Select(a => a.ToString()).ToArray());
    }

    public static string EscapeAll(this string seed, string[] chars, char escapeCharacter = '\\')
    {
        return chars.Aggregate(seed, (str, cItem) => str.Replace(cItem, escapeCharacter + cItem));
    }
}