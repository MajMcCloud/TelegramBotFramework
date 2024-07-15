using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Xml.Linq;
using System.Xml.XPath;

namespace TelegramBotBase.SourceGenerators
{
    public class XmlDocumentationLoader
    {
        XDocument xDocument;

        public string GetDocumentationLinesForSymbol(ISymbol symbol)
        {
            var docElement = xDocument?.Descendants("member")
                .FirstOrDefault(e => e.Attribute("name")?.Value == GetDocumentationCommentId(symbol));


            StringBuilder sb = new StringBuilder();

            XNode first = docElement.FirstNode;
            do
            {
                sb.AppendLine(first.ToString());



                first = first.NextNode;

            } 
            while (first.NextNode != null);

            var lines = sb.ToString().Split('\n');

            sb = new StringBuilder();

            foreach (var line in lines)
            {
                if (line == "")
                    continue;

                sb.AppendLine($"    /// {line.Trim()}");


            }


            return sb.ToString().Trim();
        }

        private string GetDocumentationCommentId(ISymbol symbol)
        {
            // Returns the documentation comment ID for a symbol
            return symbol.GetDocumentationCommentId();
        }

        public XDocument ReadEmbeddedXml(string resourceName)
        {
            // Get the assembly where the resource is embedded
            Assembly assembly = Assembly.GetExecutingAssembly();

            // Construct the full resource name
            string fullResourceName = $"{assembly.GetName().Name}.Resources.{resourceName}";

            var names = assembly.GetManifestResourceNames();

            if (!names.Contains(fullResourceName))
                return null;

            // Open a stream to the embedded resource
            using (Stream stream = assembly.GetManifestResourceStream(fullResourceName))
            {
                if (stream == null)
                {
                    //throw new FileNotFoundException("Resource not found", fullResourceName);
                    return null;
                }

                xDocument = XDocument.Load(stream);
                // Load the stream into an XDocument
                return xDocument;
            }
        }
    }
}
