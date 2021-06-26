using System.Collections.Generic;
using System.Xml;
using DirectN;

namespace Wice.DevTools
{
    public class CSharpProject : MsBuildProject
    {
        private static readonly HashSet<string> _includes = new HashSet<string> {
            "Compile",
            "EmbeddedResource",
            "Content",
            "Page",
            "None",
        };

        public CSharpProject(string filePath)
            : base(filePath)
        {
        }

        public override string GetFileInclude(XmlElement element)
        {
            if (element == null)
                return null;

            if (!_includes.Contains(element.LocalName))
                return null;

            return base.GetFileInclude(element);
        }

        public void RemoveStrongName()
        {
            var sign = ProjectType == MsBuildProjectType.NetSdk ? Project.SelectSingleNode("PropertyGroup/SignAssembly") : Project.SelectSingleNode("p:PropertyGroup/p:SignAssembly", NamespaceManager);
            if (sign != null && sign.InnerText.EqualsIgnoreCase("true"))
            {
                sign.ParentNode.ParentNode.RemoveChild(sign.ParentNode);
            }

            var keyFile = ProjectType == MsBuildProjectType.NetSdk ? Project.SelectSingleNode("PropertyGroup/AssemblyOriginatorKeyFile") : Project.SelectSingleNode("p:PropertyGroup/p:AssemblyOriginatorKeyFile", NamespaceManager);
            if (keyFile != null)
            {
                var name = keyFile.InnerText;
                if (!string.IsNullOrWhiteSpace(name))
                {
                    RemoveInclude(name);
                }
            }
        }
    }
}
