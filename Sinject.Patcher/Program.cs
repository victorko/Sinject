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
                Patcher.ProcessPatchesFile(args[0]);
            }
            catch (Exception ex)
            {
                Console.WriteLine("ERROR:\r\n" + ex.ToString());
            }
        }
    }
}
