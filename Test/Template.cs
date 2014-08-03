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
    }
}
