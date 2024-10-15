﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Reflection.Metadata;
using System.Resources;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using TelegramBotBase.SourceGenerators;

namespace TelegramBotBase
{

    [Generator(LanguageNames.CSharp)]
    public class TelegramDeviceExtensionGenerator : IIncrementalGenerator
    {
        static XmlDocumentationLoader xml;


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



            //Load only once
            if (xml == null)
            {
                xml = new XmlDocumentationLoader();
                xml.ReadEmbeddedXml("Telegram.Bot.xml");
            }

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

                    if (par.Name == "chatId")
                    {
                        subCallParameters += $"device.DeviceId";
                        continue;
                    }

                    subCallParameters += $"{par.Name}";
                    parameters += $"{par.Type.ToDisplayString()} {par.Name}";

                    if (par.HasExplicitDefaultValue)
                    {
                        var defaultValue = par.ExplicitDefaultValue;

                        // Handle specific default value cases
                        if (defaultValue == null)
                        {
                            if(par.Name == "cancellationToken")
                            {
                                parameters += " = default";
                            }
                            else
                            {
                                parameters += " = null";
                            }

                            
                        }
                        else if (defaultValue is string)
                        {
                            parameters += $" = \"{defaultValue}\""; // Add quotes around string default values
                        }
                        else if (defaultValue is bool)
                        {
                            parameters += $" = {defaultValue.ToString().ToLower()}"; // Use lower case for booleans (true/false)
                        }
                        else if (defaultValue is char)
                        {
                            parameters += $" = '{defaultValue}'"; // Use single quotes for char
                        }
                        else
                        {
                            parameters += $" = {defaultValue}"; // General case for other types (numbers, enums, etc.)
                        }
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

        /// <summary>
        /// Test
        /// </summary>
        /// <param name="method"></param>
        /// <param name="parameters"></param>
        /// <param name="subCallParameters"></param>
        /// <param name="returnStatement"></param>
        /// <returns></returns>
        private String GenerateMethod(IMethodSymbol? method, string parameters, string subCallParameters, string returnStatement)
        {
            //Adding xml comments from embedded xml file (Workaround)
            String xml_comments = xml?.GetDocumentationLinesForSymbol(method);

            StringBuilder sb = new StringBuilder();

            sb.AppendLine(xml_comments);

            sb.AppendLine($"    public static async {method.ReturnType.ToDisplayString()} {method.Name}(this DeviceSession device, {parameters})");

            sb.AppendLine($"    {{");

            sb.AppendLine($"        {returnStatement} device.Client.TelegramClient.{method.Name}({subCallParameters});");

            sb.AppendLine($"    }}");

            sb.AppendLine();

            sb.AppendLine();

            return sb.ToString();
        }
    }
}