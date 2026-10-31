using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using TelegramBotBase.SourceGenerators;

namespace TelegramBotBase
{
    /// <summary>
    /// Wraps every Telegram.Bot client method so it goes through the request dispatcher
    /// (throttling, 429 retry and priority):
    /// <list type="bullet">
    /// <item>Chat methods (a <c>chatId</c> parameter) become <c>IDeviceSession</c> extensions, with <c>chatId</c> = <c>DeviceId</c></item>
    /// <item>User methods (a <c>userId</c> parameter, no <c>chatId</c>) become <c>IDeviceSession</c> extensions, with <c>userId</c> = <c>DeviceId</c>; private chats only</item>
    /// <item>Everything else becomes an <c>IRequestDispatcher</c> extension</item>
    /// </list>
    /// </summary>
    [Generator(LanguageNames.CSharp)]
    public class TelegramDeviceExtensionGenerator : IIncrementalGenerator
    {
        static XmlDocumentationLoader? xml = null;

        /// <summary>Not regular API requests: update polling, file downloads and bot lifecycle</summary>
        private static readonly HashSet<string> Excluded = new()
        {
            "ReceiveAsync", "StartReceiving", "GetUpdates", "DropPendingUpdates",
            "GetInfoAndDownloadFile", "DownloadAndDecryptPassportFileAsync",
            "Close", "LogOut"
        };

        /// <summary>Their <c>userId</c> is the managed bot's id, not the chat user, so it must stay a parameter</summary>
        private static readonly HashSet<string> NotDeviceUser = new()
        {
            "GetManagedBotToken", "ReplaceManagedBotToken", "GetManagedBotAccessSettings", "SetManagedBotAccessSettings"
        };

        /// <summary>Default priority per method; anything not listed is Normal</summary>
        private static readonly Dictionary<string, string> DefaultPriority = new()
        {
            ["AnswerCallbackQuery"] = "Critical",
            ["AnswerInlineQuery"] = "Critical",
            ["AnswerPreCheckoutQuery"] = "Critical",
            ["AnswerShippingQuery"] = "Critical",
            ["AnswerWebAppQuery"] = "Critical",
            ["EditMessageText"] = "UI",
            ["EditMessageCaption"] = "UI",
            ["EditMessageMedia"] = "UI",
            ["EditMessageReplyMarkup"] = "UI",
            ["DeleteMessage"] = "UI",
            ["DeleteMessages"] = "UI",
            ["SendChatAction"] = "UI",
        };

        private enum Target
        {
            DeviceChat,
            DeviceUser,
            Dispatcher
        }

        public void Initialize(IncrementalGeneratorInitializationContext context)
        {
            context.RegisterSourceOutput(context.CompilationProvider, (spc, source) => Execute(spc, source));
        }

        private void Execute(SourceProductionContext context, Compilation compilation)
        {
            //Search for reference library
            var telegram_package = compilation.References.FirstOrDefault(a => a.Display != null && a.Display.Contains("Telegram.Bot"));
            if (telegram_package == null)
                return;

            //Load only once
            if (xml == null)
            {
                xml = new XmlDocumentationLoader();
                xml.ReadEmbeddedXml("Telegram.Bot.xml");
            }

            if (compilation.GetAssemblyOrModuleSymbol(telegram_package) is not IAssemblySymbol assemblySymbol)
                return;

            if (assemblySymbol.Name != "Telegram.Bot")
                return;

            //Get class which includes the existing methods
            var apiClass = assemblySymbol.GetTypeByMetadataName("Telegram.Bot.TelegramBotClientExtensions");
            if (apiClass == null)
                return;

            var device = new StringBuilder();
            var dispatcher = new StringBuilder();
            var signatures = new HashSet<string>();
            var skipped = new List<string>();

            foreach (var method in apiClass.GetMembers().OfType<IMethodSymbol>())
            {
                if (method.MethodKind != MethodKind.Ordinary || method.IsGenericMethod)
                    continue;

                if (!method.Parameters.Any(a => a.Type.Name == "ITelegramBotClient"))
                    continue;

                if (Excluded.Contains(method.Name) || ReturnStatement(method) == null)
                    continue;

                var chatId = method.Parameters.FirstOrDefault(p => p.Name == "chatId");
                var userId = method.Parameters.FirstOrDefault(p => p.Name == "userId" && p.Type.SpecialType == SpecialType.System_Int64);

                var targets = new List<Target>();
                if (chatId != null)
                {
                    targets.Add(Target.DeviceChat);
                }
                else if (userId != null && !NotDeviceUser.Contains(method.Name))
                {
                    targets.Add(Target.DeviceUser);
                }

                foreach (var target in targets)
                {
                    var substituted = target switch
                    {
                        Target.DeviceChat => "chatId",
                        Target.DeviceUser => "userId",
                        _ => null
                    };

                    var receiver = target == Target.Dispatcher ? "IRequestDispatcher" : "IDeviceSession";

                    //Removing a parameter can make two overloads identical; keep the first
                    var signature = receiver + "." + method.Name + "(" + string.Join(",",
                        method.Parameters.Where(p => p.Name != "botClient" && p.Name != substituted)
                              .Select(p => p.Type.ToDisplayString())) + ")";

                    if (!signatures.Add(signature))
                    {
                        skipped.Add(signature);
                        continue;
                    }

                    var code = GenerateMethod(method, target, substituted);
                    (target == Target.Dispatcher ? dispatcher : device).Append(code);
                }
            }

            var skippedComment = skipped.Count == 0
                ? ""
                : "// Skipped (identical signature to an earlier overload once the mapped parameter is removed):\n"
                  + string.Join("\n", skipped.Select(s => "//   " + s)) + "\n";

            context.AddSource("DeviceExtensions.g.cs", SourceText.From(WrapClass("DeviceExtensions", device.ToString(), skippedComment), Encoding.UTF8));
            context.AddSource("DispatcherExtensions.g.cs", SourceText.From(WrapClass("DispatcherExtensions", dispatcher.ToString(), ""), Encoding.UTF8));
        }

        private static string WrapClass(string className, string body, string header)
        {
            var sourceCode = $$"""
            using System;
            using System.Threading.Tasks;
            using TelegramBotBase.Interfaces;
            using TelegramBotBase.RequestDispatchers;
            using TelegramBotBase.Sessions;
            using Telegram.Bot;
            using Telegram.Bot.Extensions;
            using Telegram.Bot.Requests;
            using Telegram.Bot.Types.Enums;
            using Telegram.Bot.Types.InlineQueryResults;
            using Telegram.Bot.Types.Payments;
            using Telegram.Bot.Types.ReplyMarkups;

            #nullable enable
            namespace TelegramBotBase;

            {{header}}
            public static class {{className}}
            {
            {{body}}
            }

            """;

            //Cleanup
            return sourceCode.Replace("System.Threading.Tasks.", "");
        }

        private static string? ReturnStatement(IMethodSymbol method)
        {
            if (method.ReturnType is not INamedTypeSymbol type || type.ContainingNamespace.ToDisplayString() != "System.Threading.Tasks")
                return null;

            if (type.Name != "Task")
                return null;

            return type.IsGenericType ? "return await" : "await";
        }

        private string GenerateMethod(IMethodSymbol method, Target target, string? substituted)
        {
            var receiverName = target == Target.Dispatcher ? "dispatcher" : "device";
            var receiverType = target == Target.Dispatcher ? "IRequestDispatcher" : "IDeviceSession";
            var priority = DefaultPriority.TryGetValue(method.Name, out var p) ? p : "Normal";

            var parameters = new List<string> { $"this {receiverType} {receiverName}" };
            var arguments = new List<string>();

            foreach (var par in method.Parameters)
            {
                if (par.Name == "botClient")
                    continue;

                if (par.Name == substituted)
                {
                    arguments.Add("device.DeviceId");
                    continue;
                }

                arguments.Add(par.Name);
                parameters.Add($"{par.Type.ToDisplayString()} {par.Name}{DefaultValue(par)}");
            }

            parameters.Add($"RequestPriority priority = RequestPriority.{priority}");

            var hasCancellationToken = method.Parameters.Any(a => a.Name == "cancellationToken");
            var ct = hasCancellationToken ? "cancellationToken" : "default";

            var removedDocs = new List<string> { "botClient" };
            if (substituted != null)
                removedDocs.Add(substituted);

            var sb = new StringBuilder();
            sb.Append(CleanupXMLComments(xml?.GetDocumentationLinesForSymbol(method), removedDocs.ToArray()));
            sb.AppendLine(target == Target.Dispatcher
                ? "    /// <param name=\"dispatcher\">Request dispatcher the call is sent through</param>"
                : "    /// <param name=\"device\">Device session</param>");
            sb.AppendLine("    /// <param name=\"priority\">Order in which this request is served relative to other waiting requests</param>");

            if (target == Target.DeviceUser)
                sb.AppendLine("    /// <exception cref=\"InvalidOperationException\">Thrown when the device is a group or channel, since its id is not a user id</exception>");

            if (method.GetAttributes().Any(a => a.AttributeClass?.ToDisplayString() == "System.ObsoleteAttribute"))
                sb.AppendLine("    [Obsolete]");

            sb.AppendLine($"    public static async {method.ReturnType.ToDisplayString()} {method.Name}({string.Join(", ", parameters)})");
            sb.AppendLine("    {");

            if (target == Target.DeviceUser)
            {
                sb.AppendLine("        if (device.DeviceId <= 0)");
                sb.AppendLine($"            throw new InvalidOperationException($\"{method.Name} needs a private chat session, but device {{device.DeviceId}} is not a user\");");
                sb.AppendLine();
            }

            sb.AppendLine($"        {ReturnStatement(method)} {receiverName}.Dispatch(tgClient => tgClient.{method.Name}({string.Join(", ", arguments)}), {ct}, priority);");
            sb.AppendLine("    }");
            sb.AppendLine();
            sb.AppendLine();

            return sb.ToString();
        }

        private static string DefaultValue(IParameterSymbol par)
        {
            if (!par.HasExplicitDefaultValue)
                return "";

            var value = par.ExplicitDefaultValue;

            switch (value)
            {
                case null:
                    return " = default";
                case string s:
                    return $" = \"{s.Replace("\\", "\\\\").Replace("\"", "\\\"")}\"";
                case bool b:
                    return b ? " = true" : " = false";
                case char c:
                    return $" = '{c}'";
            }

            //Enums: cast so non-zero defaults stay valid
            var type = par.Type is INamedTypeSymbol { OriginalDefinition.SpecialType: SpecialType.System_Nullable_T } nullable
                ? nullable.TypeArguments[0]
                : par.Type;

            if (type.TypeKind == TypeKind.Enum)
                return $" = ({type.ToDisplayString()}){Convert.ToString(value, CultureInfo.InvariantCulture)}";

            return value switch
            {
                float f => $" = {f.ToString("R", CultureInfo.InvariantCulture)}f",
                double d => $" = {d.ToString("R", CultureInfo.InvariantCulture)}d",
                decimal m => $" = {m.ToString(CultureInfo.InvariantCulture)}m",
                _ => $" = {Convert.ToString(value, CultureInfo.InvariantCulture)}"
            };
        }

        private static string CleanupXMLComments(string? raw, params string[] to_remove)
        {
            if (raw == null)
                return string.Empty;

            var lines = raw.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);

            var sb = new StringBuilder();

            foreach (var line in lines)
            {
                if (to_remove.Any(a => line.Contains($"<param name=\"{a}\">")))
                    continue;

                sb.AppendLine(line);
            }

            return sb.ToString();
        }
    }
}
