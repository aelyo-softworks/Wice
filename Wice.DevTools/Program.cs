using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Xml;
using Wice.Utilities;

namespace Wice.DevTools
{
    class Program
    {
        static void Main(string[] args)
        {
            if (Debugger.IsAttached)
            {
                SafeMain(args);
            }
            else
            {
                try
                {
                    SafeMain(args);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }
        }

        static void SafeMain(string[] args)
        {
            Console.OutputEncoding = Encoding.UTF8;
            Console.WriteLine("Wice - Development Tools - Copyright (C) 2020-" + DateTime.Now.Year + " Aelyo Softworks. All rights reserved.");
            Console.WriteLine();
            if (CommandLine.HelpRequested || args.Length == 0)
            {
                Help();
                return;
            }

            var type = CommandLine.GetArgument(0, CommandType.Unknown);
            if (type == CommandType.Unknown)
            {
                Help();
                return;
            }

            switch (type)
            {
                case CommandType.UpdateDirectNCore:
                    UpdateDirectNCore();
                    break;

                case CommandType.UpdateWiceCore:
                    UpdateWiceCore();
                    break;

                default:
                    throw new NotSupportedException();
            }
        }

        static void UpdateWiceCore()
        {
            var source = new CSharpProject(@"..\..\..\Wice\Wice.csproj");
            var target = new CSharpProject(@"..\..\..\WiceCore\WiceCore.csproj");
            UpdateCoreProject(source, target, @"..\Wice\");
        }

        static void UpdateDirectNCore()
        {
            var source = new CSharpProject(@"..\..\..\DirectN\DirectN.csproj");
            var target = new CSharpProject(@"..\..\..\DirectNCore\DirectNCore.csproj");
            UpdateCoreProject(source, target, @"..\DirectN\");
        }

        static bool AddFile(string path)
        {
            var ext = System.IO.Path.GetExtension(path).ToLowerInvariant();
            return ext == ".cs" || ext == ".resx" || ext == ".png";
        }

        static string GetAction(string path)
        {
            var ext = System.IO.Path.GetExtension(path).ToLowerInvariant();
            switch (ext)
            {
                case ".cs":
                    return "Compile";

                case ".resx":
                case ".png":
                    return "EmbeddedResource";

                default:
                    return "None";
            }
        }

        static void UpdateCoreProject(CSharpProject source, CSharpProject target, string relativePath)
        {
            var implicits = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            foreach (var file in source.ImplicitIncludedFilePaths)
            {
                if (AddFile(file))
                {
                    implicits.Add(relativePath + file);
                }
            }

            var existings = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            var dic = new Dictionary<string, XmlElement>();

            foreach (var include in target.ItemGroupIncludes)
            {
                var file = target.GetFileInclude(include);
                if (file == null)
                    continue;

                if (!implicits.Remove(file))// remove already added
                {
                    existings.Add(file);
                    dic.Add(file, include);
                }
            }

            // presume we have at least one included compile item
            var compileItemGroup = target.ItemGroupIncludes.FirstOrDefault(i => i.Name == "Compile")?.ParentNode;
            if (compileItemGroup == null)
            {
                compileItemGroup = target.Document.CreateElement("ItemGroup");
                target.Project.AppendChild(compileItemGroup);
            }

            // add nodes in source but not in target
            foreach (var imp in implicits)
            {
                var action = GetAction(imp);
                var element = target.Document.CreateElement(action);
                compileItemGroup.AppendChild(element);

                element.SetAttribute("Include", imp);
                element.SetAttribute("Link", imp.Substring(relativePath.Length));
                existings.Remove(imp);
                Console.WriteLine("Adding node: " + element.OuterXml);
            }

            // remove node in target that are not in source anymore
            foreach (var existing in existings)
            {
                var inc = dic[existing];
                inc.ParentNode.RemoveChild(inc);
                Console.WriteLine("Removing node: " + inc.OuterXml);
            }
            target.Save();
        }

        static void Help()
        {
            Console.WriteLine(Assembly.GetEntryAssembly().GetName().Name.ToUpperInvariant() + " <command>");
            Console.WriteLine();
            Console.WriteLine("Description:");
            Console.WriteLine("    This tool is used to run a specific Wice Development command.");
            Console.WriteLine();
            Console.WriteLine("Commands:");
            Console.WriteLine("    UpdateDirectNCore    Update the DirectNCore project from the DirectN project.");
            Console.WriteLine("    UpdateWiceCore    Update the WiceCore project from the Wice project.");
            Console.WriteLine();
        }
    }
}
