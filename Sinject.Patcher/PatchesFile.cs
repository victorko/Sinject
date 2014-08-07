using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Sinject.Patcher
{
    public class PatchedAssembly
    {
        public string Name { get; private set; }

        public ICollection<PatchedType> Types { get; private set; }

        public PatchedAssembly(string name, ICollection<PatchedType> types)
        {
            this.Name = name;
            this.Types = types;
        }
    }

    public class PatchedType
    {
        public string Name { get; private set; }

        public ICollection<string> Methods { get; private set; }

        public PatchedType(string name, ICollection<string> methods)
        {
            this.Name = name;
            this.Methods = methods;
        }
    }

    public static class PatchesFile
    {
        public static ICollection<PatchedAssembly> Load(string filePath)
        {
            var root = XElement.Load(filePath);
            var result = new List<PatchedAssembly>();
            foreach (var assembly in root.Elements("assembly"))
            {
                var assemblyName = assembly.Attribute("name").Value;
                var types = new List<PatchedType>();
                foreach (var type in assembly.Elements("type"))
                {
                    var typeName = type.Attribute("name").Value;
                    var methods = type.Value.Split(',').Select(token => token.Trim()).ToList();
                    types.Add(new PatchedType(typeName, methods));
                }
                result.Add(new PatchedAssembly(assemblyName, types));
            }
            return result;
        }
    }
}
