using System;
using System.IO;
using System.Linq;
using System.Xml.Linq;

namespace Sinject.Patcher
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length == 0)
            {
                Console.WriteLine("USAGE:\r\nSinject.Patcher.exe <patch definitions xml file>");
                return;
            }

            try
            {
                var root = XElement.Load(args[0]);
                foreach (var assembly in root.Elements("assembly"))
                {
                    var assemblyPath = assembly.Attribute("path").Value;
                    var patcher = new Patcher(assemblyPath);
                    foreach (var type in assembly.Elements("type"))
                    {
                        var typeName = type.Attribute("name").Value;
                        var methods = type.Value.Split(',').Select(token => token.Trim());
                        foreach (var methodName in methods)
                        {
                            patcher.Patch(typeName, methodName);
                        }
                    }
                    patcher.Save(assemblyPath);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("ERROR:\r\n" + ex.ToString());
            }
        }
    }
}
