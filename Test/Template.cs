using Sinject.Runtime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Test
{
    public class Template
    {
        public int Foo(string s, object o)
        {
            var args = new object[] { this, s, o };
            object result;
            if (Stubs.CallStub("Test.Template", "Foo", args, out result))
            {
                return (int)result;
            }

            return 10;
        }

        public string Bar
        {
            get
            {
                var args = new object[] { this };
                object result;
                if (Stubs.CallStub("Test.Template", "get_Bar", args, out result))
                {
                    return (string)result;
                }

                return "sss";
            }
            set
            {
                var args = new object[] { this };
                object result;
                if (Stubs.CallStub("Test.Template", "get_Bar", args, out result))
                {
                    return;
                }
            }
        }

        public bool Yey(int i, ref object o, out string s, out int j)
        {
            var args = new object[] { this, i, o, null, null };
            object result;
            if (Stubs.CallStub("Test.Template", "Yey", args, out result))
            {
                o = args[2];
                s = (string)args[3];
                j = (int)args[4];
                return (bool)result;
            }

            o = new object();
            s = "asddf";
            j = 15;
            return false;
        }
    }
}
