﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace TelegramBotBase
{

    [Generator(LanguageNames.CSharp)]
    public class TelegramDeviceExtensionGenerator : IIncrementalGenerator
    {

        public void Initialize(IncrementalGeneratorInitializationContext context)
        {
            var provider = context.SyntaxProvider.CreateSyntaxProvider(
                predicate: (c, _) => c is ClassDeclarationSyntax,
                transform: (n, _) => (ClassDeclarationSyntax)n.Node)
                .Where(a => a is not null);


            var compilation = context.CompilationProvider;

            context.RegisterSourceOutput(compilation, (spc, source) => Execute(spc, source));

        }

        private void Execute(SourceProductionContext context, Compilation compilation)
        {
            //if (!Debugger.IsAttached) Debugger.Launch();

            StringBuilder sb = new StringBuilder();
            sb.AppendLine();

            //Search for reference library
            var telegram_package = compilation.References.FirstOrDefault(a => a.Display != null && a.Display.Contains("Telegram.Bot"));
            if (telegram_package == null)
                return;

            var assemblySymbol = compilation.GetAssemblyOrModuleSymbol(telegram_package) as IAssemblySymbol;

            if (assemblySymbol == null)
                return;

            if (assemblySymbol.Name != "Telegram.Bot")
                return;

            //Get class which includes the existing methods
            var apiClass = assemblySymbol.GetTypeByMetadataName("Telegram.Bot.TelegramBotClientExtensions");
            if (apiClass == null)
                return;

            //Get existing list of methods
            var methods = apiClass.GetMembers().OfType<IMethodSymbol>().ToList();

            foreach (var method in methods)
            {
                if (!method.Parameters.Any(a => a.Type.Name == "ITelegramBotClient"))
                    continue;

                if (!method.Parameters.Any(a => a.Type.Name == "ChatId"))
                    continue;

                if (method.Name == ".ctor") 
                    continue;

                String parameters = "";
                String subCallParameters = "";
                foreach (var par in method.Parameters)
                {
                    if (par.Name == "botClient")
                        continue;

                    if (!string.IsNullOrEmpty(parameters))
                    {
                        parameters += ", ";
                    }

                    if (!string.IsNullOrEmpty(subCallParameters))
                    {
                        subCallParameters += ", ";
                    }

                    if (par.Name != "chatId")
                    {
                        subCallParameters += $"{par.Name}";
                        parameters += $"{par.Type.ToDisplayString()} {par.Name}";
                    }
                    else
                    {
                        subCallParameters += $"device.DeviceId";
                    }

                }


                var returnStatement = "";

                if (method.ReturnType is INamedTypeSymbol namedType && namedType.IsGenericType && namedType.ConstructedFrom.Name == "Task" && namedType.ContainingNamespace.ToDisplayString() == "System.Threading.Tasks")
                {
                    returnStatement = "return await";
                }
                else if (method.ReturnType.Name == "Task" && method.ReturnType.ContainingNamespace.ToDisplayString() == "System.Threading.Tasks")
                {
                    returnStatement = "await";
                }
                else if (method.ReturnsVoid)
                {
                    returnStatement = "";
                }
                else
                {
                    returnStatement = "return ";
                }

                String tmp = GenerateMethod(method, parameters, subCallParameters, returnStatement);

                sb.Append(tmp);

            }




            //The generated source
            var sourceCode = $$"""
            using System;
            using System.Threading.Tasks;
            using TelegramBotBase.Sessions;
            using Telegram.Bot;
            using Telegram.Bot.Extensions;
            using Telegram.Bot.Requests;
            using Telegram.Bot.Types.Enums;
            using Telegram.Bot.Types.InlineQueryResults;
            using Telegram.Bot.Types.Payments;
            using Telegram.Bot.Types.ReplyMarkups;
            using File = Telegram.Bot.Types.File;

            #nullable enable
            namespace TelegramBotBase;
        
            public static class DeviceExtensions
            {
                {{sb.ToString()}}
            }
            
            """;

            //Cleanup
            sourceCode = sourceCode.Replace("System.Threading.Tasks.", "");


            context.AddSource("DeviceExtensions.g.cs", SourceText.From(sourceCode, Encoding.UTF8));

        }

        private static String GenerateMethod(IMethodSymbol? method, string parameters, string subCallParameters, string returnStatement)
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendLine($"    public static async {method.ReturnType.ToDisplayString()} {method.Name}(this DeviceSession device, {parameters})");

            sb.AppendLine($"    {{");

            sb.AppendLine($"        {returnStatement} device.Client.TelegramClient.{method.Name}({subCallParameters});");

            sb.AppendLine($"    }}");

            sb.AppendLine();

            return sb.ToString();
        }
    }
}