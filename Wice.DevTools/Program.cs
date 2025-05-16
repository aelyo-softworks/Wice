namespace Wice.DevTools;

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

        var command = CommandLine.GetArgument(0, CommandType.Unknown);
        if (command == CommandType.Unknown)
        {
            Help();
            return;
        }

        Console.WriteLine("Running command: " + command);
        switch (command)
        {
            case CommandType.UpdateWiceSamplesGalleryCode:
                UpdateWiceSamplesGalleryCode(@"..\..\..\..\Wice.Samples.Gallery");
                UpdateWiceSamplesGalleryCode(@"..\..\..\..\WiceAot.Samples.Gallery");
                break;

            default:
                throw new NotSupportedException();
        }
    }

    static void UpdateWiceSamplesGalleryCode(string csProj)
    {
        var fp = System.IO.Path.GetFullPath(csProj);
        var samplesFile = new XmlDocument();
        samplesFile.LoadXml("<samples/>");

        foreach (var path in Directory.EnumerateFiles(fp, "*.cs", SearchOption.AllDirectories).OrderBy(f => f))
        {
            var name = System.IO.Path.GetFileName(path);
            const string sampleToken = "Sample.cs";
            if (name.Length <= sampleToken.Length || !name.EndsWith(sampleToken))
                continue;

            // gather sample lines
            var lines = new List<string>();
            string tabs = null;
            var index = -1;
            var wholeRemark = false;
            foreach (var line in File.ReadAllLines(System.IO.Path.Combine(fp, path)))
            {
                if (tabs == null)
                {
                    const string tok = "public override void Layout(";
                    index = line.IndexOf(tok);
                    if (index < 0)
                        continue;

                    wholeRemark = line.IndexOf("// whole remark") >= 0;
                    tabs = line.Substring(0, index);
                    if (!wholeRemark)
                    {
                        addLine(line.Substring(index).Replace("public override ", string.Empty));
                    }
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

                    l = l.Replace("Wice.", string.Empty); //trick because some samples must use a namespace

                    if (wholeRemark)
                    {
                        var remPos = l.IndexOf("//");
                        if (remPos >= 0)
                        {
                            l = l.Substring(remPos + 2);
                        }
                    }
                    lines.Add(l);
                }
            }

            if (lines.Count == 0)
                continue;

            if (wholeRemark && lines.Count > 2)
            {
                lines.RemoveAt(0);
                lines.RemoveAt(lines.Count - 1);
            }

            var element = samplesFile.CreateElement("sample");
            samplesFile.DocumentElement.AppendChild(element);

            var nsPath = path.Substring(fp.Length + 1);
            var ns = System.IO.Path.ChangeExtension(nsPath, string.Empty);
            ns = ns.Substring(0, ns.Length - 1);
            ns = ns.Replace(System.IO.Path.DirectorySeparatorChar, '.');
            element.SetAttribute("namespace", ns);
            element.InnerText = string.Join(Environment.NewLine, lines);
        }

        var resourcesPath = System.IO.Path.Combine(fp, "Resources", "samples.xml");
        samplesFile.Save(resourcesPath);
    }

    static void Help()
    {
        Console.WriteLine(Assembly.GetEntryAssembly().GetName().Name.ToUpperInvariant() + " <command>");
        Console.WriteLine();
        Console.WriteLine("Description:");
        Console.WriteLine("    This tool is used to run a specific Wice Development command.");
        Console.WriteLine();
        Console.WriteLine("Commands:");
        Console.WriteLine("    UpdateWiceSamplesGalleryCode   Update the Wice.Samples.Gallery project and the WiceAOT.Samples.Gallery project.");

        Console.WriteLine();
    }
}
