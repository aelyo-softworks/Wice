using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using DirectN;

namespace Wice.DevTools
{
    // trying to use MsBuild programmatically is now total hell
    // so we do this ourselves (we don't need super powerful stuff anyway)
    public class MsBuildProject
    {
        public const string MsBuildNamespaceUri = "http://schemas.microsoft.com/developer/msbuild/2003";

        public static readonly XmlNamespaceManager NamespaceManager = new XmlNamespaceManager(new NameTable());

        static MsBuildProject()
        {
            NamespaceManager.AddNamespace("p", MsBuildNamespaceUri);
        }

        public MsBuildProject(string filePath)
        {
            if (filePath == null)
                throw new ArgumentNullException(nameof(filePath));

            ProjectFilePath = System.IO.Path.GetFullPath(filePath);
            ProjectDirectoryPath = System.IO.Path.GetDirectoryName(ProjectFilePath);
            Document = new XmlDocument();
            Document.Load(ProjectFilePath);

            Project = Document.SelectSingleNode("p:Project", NamespaceManager) as XmlElement;
            if (Project == null)
            {
                Project = Document.SelectSingleNode("Project[@Sdk='Microsoft.NET.Sdk']") as XmlElement;
                if (Project == null)
                    throw new ArgumentException(nameof(filePath), "File at '" + ProjectFilePath + "' is not a recognized as an MsBuild project.");

                ProjectType = MsBuildProjectType.NetSdk;
            }
            else
            {
                ProjectType = MsBuildProjectType.MsBuild;
            }
        }

        public string ProjectFilePath { get; }
        public string ProjectDirectoryPath { get; }
        public XmlDocument Document { get; }
        public MsBuildProjectType ProjectType { get; }
        public XmlElement Project { get; }

        public void RemoveInclude(string filePath)
        {
            if (filePath == null)
                throw new ArgumentNullException(nameof(filePath));

            var remove = GetIncludeElement(filePath);
            if (remove != null)
            {
                remove.ParentNode.RemoveChild(remove);
            }
        }

        public XmlElement GetIncludeElement(string filePath)
        {
            if (filePath == null)
                throw new ArgumentNullException(nameof(filePath));

            return ItemGroupIncludes.FirstOrDefault(e => GetIncludeFilePath(e).EqualsIgnoreCase(filePath));
        }

        public static string GetIncludeFilePath(XmlElement element)
        {
            if (element == null)
                return null;

            return element.GetAttribute("Include").Nullify();
        }

        public void Save() => Document.Save(ProjectFilePath);

        public virtual string GetFileInclude(XmlElement element)
        {
            if (element == null)
                return null;

            var include = GetIncludeFilePath(element);
            if (include == null)
                return null;

            return include;
        }

        public XmlElement GetPropertyGroup(string condition)
        {
            if (condition == null)
                throw new ArgumentNullException(nameof(condition));

            switch (ProjectType)
            {
                case MsBuildProjectType.NetSdk:
                    return Project.SelectSingleNode("PropertyGroup[@Condition=\"" + condition + "\"]") as XmlElement;

                case MsBuildProjectType.MsBuild:
                    return Project.SelectSingleNode("p:PropertyGroup[@Condition=\"" + condition + "\"]", NamespaceManager) as XmlElement;

                default:
                    throw new NotSupportedException();
            }
        }

        public void SetProperty(string groupCondition, string name, string text)
        {
            if (groupCondition == null)
                throw new ArgumentNullException(nameof(groupCondition));

            var pg = GetPropertyGroup(groupCondition);
            SetProperty(ProjectType, pg, name, text);
        }

        public static void SetProperty(MsBuildProjectType projectType, XmlElement propertyGroup, string name, string text)
        {
            if (propertyGroup == null)
                throw new ArgumentNullException(nameof(propertyGroup));

            if (name == null)
                throw new ArgumentNullException(nameof(name));

            var prop = projectType == MsBuildProjectType.NetSdk ? propertyGroup.SelectSingleNode(name) : propertyGroup.SelectSingleNode("p:" + name, NamespaceManager);
            if (prop == null)
            {
                prop = propertyGroup.OwnerDocument.CreateElement(name, MsBuildNamespaceUri);
                propertyGroup.AppendChild(prop);
            }

            prop.InnerText = text;
        }

        public IEnumerable<XmlElement> ItemGroupIncludes
        {
            get
            {
                foreach (var itemGroup in ItemGroups)
                {
                    foreach (var include in itemGroup.SelectNodes("*[@Include]").OfType<XmlElement>())
                    {
                        yield return include;
                    }
                }
            }
        }

        public IEnumerable<XmlElement> ItemGroups
        {
            get
            {
                foreach (var itemGroup in ProjectType == MsBuildProjectType.NetSdk ? Project.SelectNodes("ItemGroup").OfType<XmlElement>() : Project.SelectNodes("p:ItemGroup", NamespaceManager).OfType<XmlElement>())
                {
                    yield return itemGroup;
                }
            }
        }

        public IEnumerable<string> IncludedFilePaths
        {
            get
            {
                foreach (var include in ItemGroupIncludes)
                {
                    var file = GetFileInclude(include);
                    if (file != null)
                        yield return file;
                }
            }
        }

        public IEnumerable<string> ImplicitIncludedFilePaths
        {
            get
            {
                foreach (var include in System.IO.Directory.EnumerateFiles(ProjectDirectoryPath, "*.*", System.IO.SearchOption.AllDirectories))
                {
                    var relPath = include.Substring(ProjectDirectoryPath.Length + 1);
                    var ext = System.IO.Path.GetExtension(relPath).ToLowerInvariant();
                    if (ext.EndsWith("proj", StringComparison.OrdinalIgnoreCase) || ext == ".suo" || ext == ".user")
                        continue;

                    var relDir = System.IO.Path.GetDirectoryName(relPath);
                    var relDirSegments = relDir.Split(System.IO.Path.DirectorySeparatorChar);
                    if (relDirSegments[0].StartsWith(".")) // .vs .git, etc.
                        continue;

                    if (relDirSegments[0].EqualsIgnoreCase("obj") || relDirSegments[0].EqualsIgnoreCase("bin"))
                        continue;

                    yield return relPath;
                }
            }
        }
    }
}
