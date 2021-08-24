﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Xml;
using Wice.Utilities;
using IOPath = System.IO.Path;

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
                case CommandType.UpdateDirectN:
                    var directNPath = CommandLine.GetNullifiedArgument(1);
                    if (directNPath == null)
                    {
                        Help();
                        return;
                    }

                    UpdateDirectN(directNPath);
                    break;

                case CommandType.UpdateDirectNCore:
                    UpdateDirectNCore();
                    break;

                case CommandType.UpdateWiceCore:
                    UpdateWiceCore();
                    break;

                case CommandType.UpdateWiceCoreTests:
                    UpdateWiceCoreTests();
                    break;

                case CommandType.UpdateWiceCoreSamplesGallery:
                    UpdateWiceCoreSamplesGallery();
                    break;

                case CommandType.UpdateWiceSamplesGalleryCode:
                    UpdateWiceSamplesGalleryCode();
                    UpdateWiceCoreSamplesGallery();
                    break;

                default:
                    throw new NotSupportedException();
            }
        }

        static void UpdateWiceSamplesGalleryCode()
        {
            var source = new CSharpProject(@"..\..\..\Wice.Samples.Gallery\Wice.Samples.Gallery.csproj");
            var samplesFile = new XmlDocument();
            samplesFile.LoadXml("<samples/>");

            foreach (var path in source.IncludedFilePaths)
            {
                var name = IOPath.GetFileName(path);
                const string sampleToken = "Sample.cs";
                if (name.Length <= sampleToken.Length || !name.EndsWith(sampleToken))
                    continue;

                // gather sample lines
                var lines = new List<string>();
                string tabs = null;
                var index = -1;
                foreach (var line in File.ReadAllLines(IOPath.Combine(source.ProjectDirectoryPath, path)))
                {
                    if (tabs == null)
                    {
                        const string tok = "public override void Layout(";
                        index = line.IndexOf(tok);
                        if (index < 0)
                            continue;

                        tabs = line.Substring(0, index);
                        addLine(line.Substring(index).Replace("public override ", string.Empty));
                    }
                    else
                    {
                        if (line.Length > index)
                        {
                            addLine(line.Substring(index));
                        }
                        else
                        {
                            addLine(line);
                        }
                        if (line.TrimEnd() == (tabs + "}"))
                            break;
                    }

                    void addLine(string l)
                    {
                        if (l.IndexOf("remove from display", StringComparison.OrdinalIgnoreCase) >= 0)
                            return;

                        l = l.Replace("new Wice.", "new "); //trick because some samples must use a namespace
                        lines.Add(l);
                    }
                }

                if (lines.Count == 0)
                    continue;

                var element = samplesFile.CreateElement("sample");
                samplesFile.DocumentElement.AppendChild(element);
                var ns = IOPath.ChangeExtension(path, string.Empty);
                ns = ns.Substring(0, ns.Length - 1);
                ns = ns.Replace(IOPath.DirectorySeparatorChar, '.');
                element.SetAttribute("namespace", ns);
                element.InnerText = string.Join(Environment.NewLine, lines);
            }

            var resourcesPath = IOPath.Combine(source.ProjectDirectoryPath, "Resources", "samples.xml");
            samplesFile.Save(resourcesPath);
        }

        static void UpdateDirectN(string directNPath)
        {
            var target = new CSharpProject(@"..\..\..\DirectN\DirectN.csproj");
            foreach (var file in target.ImplicitIncludedFilePaths)
            {
                if (!file.StartsWith(@"Generated\", StringComparison.OrdinalIgnoreCase))
                    continue;

                var sourcePath = IOPath.Combine(directNPath, file);
                var destinationPath = IOPath.Combine(target.ProjectDirectoryPath, file);
                IOUtilities.FileOverwrite(sourcePath, destinationPath);
                Console.WriteLine("Copied " + sourcePath + " => " + destinationPath);
            }
        }

        static void UpdateWiceCore()
        {
            var source = new CSharpProject(@"..\..\..\Wice\Wice.csproj");
            var target = new CSharpProject(@"..\..\..\WiceCore\WiceCore.csproj");
            UpdateCoreProject(source, target, @"..\Wice\", false);
        }

        static void UpdateDirectNCore()
        {
            var source = new CSharpProject(@"..\..\..\DirectN\DirectN.csproj");
            var target = new CSharpProject(@"..\..\..\DirectNCore\DirectNCore.csproj");
            UpdateCoreProject(source, target, @"..\DirectN\", true); // netstandard => implicit
        }

        static void UpdateWiceCoreTests()
        {
            var source = new CSharpProject(@"..\..\..\Wice.Tests\Wice.Tests.csproj");
            var target = new CSharpProject(@"..\..\..\WiceCore.Tests\WiceCore.Tests.csproj");
            UpdateCoreProject(source, target, @"..\Wice.Tests\", false);
        }

        static void UpdateWiceCoreSamplesGallery()
        {
            var source = new CSharpProject(@"..\..\..\Wice.Samples.Gallery\Wice.Samples.Gallery.csproj");
            var target = new CSharpProject(@"..\..\..\WiceCore.Samples.Gallery\WiceCore.Samples.Gallery.csproj");
            UpdateCoreProject(source, target, @"..\Wice.Samples.Gallery\", false);
        }

        static bool AddFile(string path)
        {
            var ext = IOPath.GetExtension(path).ToLowerInvariant();
            return ext == ".cs" ||
                ext == ".resx" ||
                ext == ".png" || ext == ".jpg" || ext == ".jpeg" || ext == ".ico" || ext == ".svg" ||
                ext == ".manifest" ||
                ext == ".rtf" ||
                ext == ".xml";
        }

        static string GetAction(string path)
        {
            var ext = IOPath.GetExtension(path).ToLowerInvariant();
            switch (ext)
            {
                case ".cs":
                    return "Compile";

                case ".resx":
                case ".png":
                case ".jpg":
                case ".jpeg":
                case ".xml":
                case ".rtf":
                case ".svg":
                    return "EmbeddedResource";

                case ".ico":
                    return "Content";

                case ".manifest":
                    return "None";

                default:
                    throw new NotSupportedException();
            }
        }

        static void UpdateCoreProject(CSharpProject source, CSharpProject target, string relativePath, bool useImplicits)
        {
            var implicits = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            foreach (var file in useImplicits ? source.ImplicitIncludedFilePaths : source.IncludedFilePaths)
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

                var link = imp.Substring(relativePath.Length);
                while (link.StartsWith(@"..\"))
                {
                    link = string.Join(@"\", link.Split('\\').Skip(2));
                }

                element.SetAttribute("Link", link);
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
            Console.WriteLine("    UpdateDirectN <path>           Update the Wice DirectN project from the public github DirectN project.");
            Console.WriteLine("    UpdateDirectNCore              Update the DirectNCore project from the DirectN project.");
            Console.WriteLine("    UpdateWiceCore                 Update the WiceCore project from the Wice project.");
            Console.WriteLine("    UpdateWiceCoreTests            Update the WiceCore.Tests project from the Wice.Tests project.");
            Console.WriteLine("    UpdateWiceCoreSamplesGallery   Update the WiceCore.Samples.Gallery project from the Wice.Samples.Gallery project.");
            Console.WriteLine("    UpdateWiceSamplesGalleryCode   Update the Wice.Samples.Gallery project and the WiceCore.Samples.Gallery project.");

            Console.WriteLine();
        }
    }
}
